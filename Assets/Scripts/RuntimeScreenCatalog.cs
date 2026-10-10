using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum AppLanguage
{
    English,
    Chinese
}

[CreateAssetMenu(fileName = "RuntimeScreenCatalog", menuName = "Awakening Celebrations/Runtime Screen Catalog")]
public sealed class RuntimeScreenCatalog : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        public string screenId;
        public string language;
        public string figmaRootNodeId;
        public VisualTreeAsset asset;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public bool TryGet(string screenId, AppLanguage language, out VisualTreeAsset asset)
    {
        string languageKey = language == AppLanguage.English ? "en" : "zh";
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry != null && entry.screenId == screenId && entry.language == languageKey && entry.asset != null)
            {
                asset = entry.asset;
                return true;
            }
        }

        asset = null;
        return false;
    }

    public bool TryGetEntry(string screenId, AppLanguage language, out Entry result)
    {
        string languageKey = language == AppLanguage.English ? "en" : "zh";
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry != null && entry.screenId == screenId && entry.language == languageKey && entry.asset != null)
            {
                result = entry;
                return true;
            }
        }

        result = null;
        return false;
    }

    public IEnumerable<Entry> Entries { get { return entries; } }
}
