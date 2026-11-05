using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Owns the UIDocument instances used by the existing screen catalog.</summary>
public sealed class ScreenDocumentManager : MonoBehaviour
{
    [Serializable]
    private sealed class DocumentSlot
    {
        public string family;
        public UIDocument document;
    }

    [SerializeField] private List<DocumentSlot> mainDocuments = new List<DocumentSlot>();
    [SerializeField] private UIDocument assistantOverlayDocument;
    private UIDocument activeMainDocument;

    public void Initialize()
    {
        activeMainDocument = null;
        for (int i = 0; i < mainDocuments.Count; i++)
        {
            UIDocument document = mainDocuments[i] == null ? null : mainDocuments[i].document;
            if (document == null)
                continue;
            document.sortingOrder = 10;
            document.enabled = false;
        }

        if (assistantOverlayDocument != null)
        {
            assistantOverlayDocument.sortingOrder = 20;
            assistantOverlayDocument.enabled = false;
        }
    }

    public UIDocument Activate(string screenId, VisualTreeAsset asset)
    {
        string family = ResolveFamily(screenId);
        UIDocument target = FindDocument(family);
        if (target == null || asset == null)
        {
            Debug.LogError("ScreenDocumentManager: No document is assigned for screen family '" + family + "'.", this);
            return null;
        }

        DeactivateOverlay();
        if (activeMainDocument != null && activeMainDocument != target)
            activeMainDocument.enabled = false;
        target.enabled = false;
        target.visualTreeAsset = asset;
        target.enabled = true;
        activeMainDocument = target;
        return target;
    }

    public void DeactivateMain()
    {
        if (activeMainDocument != null)
            activeMainDocument.enabled = false;
        activeMainDocument = null;
    }

    public VisualElement ActivateOverlay(VisualTreeAsset asset)
    {
        if (assistantOverlayDocument == null || asset == null)
            return null;
        DeactivateOverlay();
        assistantOverlayDocument.enabled = false;
        assistantOverlayDocument.visualTreeAsset = asset;
        assistantOverlayDocument.enabled = true;
        return assistantOverlayDocument.rootVisualElement;
    }

    public void DeactivateOverlay()
    {
        if (assistantOverlayDocument == null)
            return;
        assistantOverlayDocument.enabled = false;
        assistantOverlayDocument.visualTreeAsset = null;
    }

    private UIDocument FindDocument(string family)
    {
        for (int i = 0; i < mainDocuments.Count; i++)
        {
            DocumentSlot slot = mainDocuments[i];
            if (slot != null && slot.family == family)
                return slot.document;
        }
        return null;
    }

    private static string ResolveFamily(string screenId)
    {
        if (screenId == "Privacy") return "Privacy";
        if (screenId.StartsWith("TUTORIAL---", StringComparison.Ordinal)) return "Tutorial";
        if (screenId.StartsWith("ATTRACT---", StringComparison.Ordinal)) return "Attract";
        if (screenId == "COLOR-RELEASE") return "ColorRelease";
        if (screenId == "AssistantChooser") return "AssistantChooser";
        if (screenId.StartsWith("SETTINGS---", StringComparison.Ordinal)) return "Settings";
        if (screenId.StartsWith("CLOSING---", StringComparison.Ordinal)) return "Closing";
        if (screenId.StartsWith("SOUVENIR---", StringComparison.Ordinal)) return "Souvenir";
        if (screenId.Contains("---FOCUS-VIEW---")) return "Focus";
        if (screenId.Contains("---ACTIVATION---")) return "Activation";
        if (screenId.Contains("---LIVING-PORTRAIT---")) return "LivingPortrait";
        if (screenId.Contains("---RELEASE---")) return "Release";
        if (screenId.Contains("---POST-VIDEO-AI---")) return "PostVideoAssistant";
        return string.Empty;
    }
}
