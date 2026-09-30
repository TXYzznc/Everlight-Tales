using System;
using System.Collections.Generic;
using UnityEngine;

namespace Everlight.Tales.UI
{
    [CreateAssetMenu(menuName = "Everlight Tales/UI/Formal Sprite Catalog", fileName = "UIFormalSpriteCatalog")]
    public sealed class UIFormalSpriteCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Key;
            public Sprite Sprite;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        public Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key) || _entries == null) return null;
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry != null && entry.Key == key) return entry.Sprite;
            }
            return null;
        }
    }
}
