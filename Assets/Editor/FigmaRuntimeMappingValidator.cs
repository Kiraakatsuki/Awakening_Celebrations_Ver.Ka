using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class FigmaRuntimeMappingValidator : IPreprocessBuildWithReport
{
    private const string ManifestPath = "Assets/Figma/Awakening-Celebrations---Visual-Redesign/.figma-manifest.json";
    private const string RuntimeSnapshotPath = "Assets/Resources/RuntimeNodeBindings.json";
    private const string CatalogPath = "Assets/Resources/RuntimeScreenCatalog.asset";

    [Serializable]
    private sealed class MappingFile
    {
        public MappingScreen[] screens;
    }

    [Serializable]
    private sealed class MappingScreen
    {
        public string rootNodeId;
        public string assetName;
        public MappingNode[] nodes;
    }

    [Serializable]
    private sealed class MappingNode
    {
        public string figmaNodeId;
        public string path;
        public string tag;
        public string classes;
    }

    public int callbackOrder { get { return 0; } }

    [MenuItem("Tools/Awakening Celebrations/Validate Figma Runtime Mapping")]
    private static void ValidateFromMenu()
    {
        List<string> errors = ValidateCurrentProject();
        if (errors.Count == 0)
        {
            Debug.Log("Figma runtime mapping is current: connector manifest, runtime snapshot, catalog, and UXML assets agree.");
            return;
        }

        Debug.LogError("Figma runtime mapping validation failed:\n - " + string.Join("\n - ", errors));
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        List<string> errors = ValidateCurrentProject();
        if (errors.Count > 0)
            throw new BuildFailedException("Figma runtime mappings are stale or incomplete:\n - " + string.Join("\n - ", errors));
    }

    private static List<string> ValidateCurrentProject()
    {
        List<string> errors = new List<string>();
        if (!File.Exists(ManifestPath))
        {
            errors.Add("Connector manifest is missing: " + ManifestPath);
            return errors;
        }
        if (!File.Exists(RuntimeSnapshotPath))
        {
            errors.Add("Runtime mapping snapshot is missing: " + RuntimeSnapshotPath);
            return errors;
        }

        MappingFile manifest = JsonUtility.FromJson<MappingFile>(File.ReadAllText(ManifestPath));
        MappingFile snapshot = JsonUtility.FromJson<MappingFile>(File.ReadAllText(RuntimeSnapshotPath));
        if (manifest == null || manifest.screens == null || snapshot == null || snapshot.screens == null)
        {
            errors.Add("Manifest or runtime snapshot JSON could not be parsed.");
            return errors;
        }
        if (manifest.screens.Length != snapshot.screens.Length)
            errors.Add("Screen-row count differs (connector=" + manifest.screens.Length + ", runtime=" + snapshot.screens.Length + ").");

        Dictionary<string, MappingScreen> manifestByRoot = new Dictionary<string, MappingScreen>();
        for (int i = 0; i < manifest.screens.Length; i++)
        {
            MappingScreen screen = manifest.screens[i];
            if (screen != null && !string.IsNullOrEmpty(screen.rootNodeId))
                manifestByRoot[screen.rootNodeId] = screen;
        }

        Dictionary<string, MappingScreen> snapshotByRoot = new Dictionary<string, MappingScreen>();
        for (int i = 0; i < snapshot.screens.Length; i++)
        {
            MappingScreen screen = snapshot.screens[i];
            if (screen == null || string.IsNullOrEmpty(screen.rootNodeId) || snapshotByRoot.ContainsKey(screen.rootNodeId))
            {
                errors.Add("Runtime snapshot contains an empty or duplicate root node ID.");
                continue;
            }
            snapshotByRoot.Add(screen.rootNodeId, screen);
        }

        HashSet<string> seenNodeIds = new HashSet<string>();
        for (int i = 0; i < manifest.screens.Length; i++)
        {
            MappingScreen screen = manifest.screens[i];
            MappingScreen saved;
            if (screen == null || !snapshotByRoot.TryGetValue(screen.rootNodeId, out saved))
            {
                errors.Add("Runtime snapshot has no row for Figma root " + (screen == null ? "<null>" : screen.rootNodeId) + ".");
                continue;
            }
            if (screen.assetName != saved.assetName)
                errors.Add("Asset name changed for root " + screen.rootNodeId + ".");

            Dictionary<string, MappingNode> savedNodes = new Dictionary<string, MappingNode>();
            if (saved.nodes != null)
            {
                for (int n = 0; n < saved.nodes.Length; n++)
                    if (saved.nodes[n] != null && !string.IsNullOrEmpty(saved.nodes[n].figmaNodeId))
                        savedNodes[saved.nodes[n].figmaNodeId] = saved.nodes[n];
            }
            if (screen.nodes == null || saved.nodes == null || screen.nodes.Length != saved.nodes.Length)
                errors.Add("Node count changed for root " + screen.rootNodeId + ".");

            if (screen.nodes == null)
                continue;
            for (int n = 0; n < screen.nodes.Length; n++)
            {
                MappingNode node = screen.nodes[n];
                MappingNode runtimeNode;
                if (node == null || !seenNodeIds.Add(node.figmaNodeId))
                {
                    errors.Add("Connector manifest contains an empty or duplicate node ID.");
                    continue;
                }
                if (!savedNodes.TryGetValue(node.figmaNodeId, out runtimeNode))
                {
                    errors.Add("Runtime snapshot is missing node " + node.figmaNodeId + ".");
                    continue;
                }
                if (node.path != runtimeNode.path || node.tag != runtimeNode.tag || node.classes != runtimeNode.classes)
                    errors.Add("Node mapping changed for " + node.figmaNodeId + " (path/tag/class mismatch).");
            }

            string uxmlPath = "Assets/Figma/Awakening-Celebrations---Visual-Redesign/" + screen.assetName + ".uxml";
            if (!File.Exists(uxmlPath))
                errors.Add("Generated UXML is missing for root " + screen.rootNodeId + ": " + uxmlPath);
        }

        RuntimeScreenCatalog catalog = AssetDatabase.LoadAssetAtPath<RuntimeScreenCatalog>(CatalogPath);
        if (catalog == null)
        {
            errors.Add("Runtime screen catalog asset is missing or failed to import.");
            return errors;
        }

        HashSet<string> catalogKeys = new HashSet<string>();
        HashSet<string> catalogRoots = new HashSet<string>();
        foreach (RuntimeScreenCatalog.Entry entry in catalog.Entries)
        {
            if (entry == null || entry.asset == null || string.IsNullOrEmpty(entry.screenId) || string.IsNullOrEmpty(entry.language))
            {
                errors.Add("Runtime screen catalog has an incomplete entry.");
                continue;
            }
            string key = entry.screenId + "/" + entry.language;
            if (!catalogKeys.Add(key))
                errors.Add("Duplicate runtime screen key " + key + ".");
            catalogRoots.Add(entry.figmaRootNodeId);

            MappingScreen sourceScreen;
            if (!manifestByRoot.TryGetValue(entry.figmaRootNodeId, out sourceScreen))
            {
                errors.Add("Catalog entry " + key + " points to unknown Figma root " + entry.figmaRootNodeId + ".");
                continue;
            }
            string expectedLanguage = sourceScreen.assetName.StartsWith("TUTORIAL---", StringComparison.Ordinal)
                ? (sourceScreen.rootNodeId.StartsWith("24:", StringComparison.Ordinal) ? "en" : "zh")
                : (sourceScreen.assetName.Contains("ENGLISH") || sourceScreen.assetName.EndsWith("---EN", StringComparison.Ordinal) || sourceScreen.assetName.Contains("---EN---") ? "en" : "zh");
            if (entry.language != expectedLanguage)
                errors.Add("Catalog language mismatch for root " + entry.figmaRootNodeId + ".");

            string expectedUxmlPath = "Assets/Figma/Awakening-Celebrations---Visual-Redesign/" + sourceScreen.assetName;
            if (sourceScreen.assetName.StartsWith("TUTORIAL---", StringComparison.Ordinal) && expectedLanguage == "en")
                expectedUxmlPath += "---ENGLISH";
            expectedUxmlPath += ".uxml";
            string registeredAssetPath = AssetDatabase.GetAssetPath(entry.asset);
            if (!string.Equals(registeredAssetPath, expectedUxmlPath, StringComparison.Ordinal))
                errors.Add("Catalog asset does not match the localized source for root " + entry.figmaRootNodeId + ".");
        }
        if (catalogKeys.Count != manifest.screens.Length)
            errors.Add("Catalog entry count does not match connector screen-row count.");
        foreach (MappingScreen screen in manifest.screens)
            if (screen != null && !catalogRoots.Contains(screen.rootNodeId))
                errors.Add("Catalog has no localized runtime entry for root " + screen.rootNodeId + ".");

        return errors;
    }
}
