using System;
using System.Collections.Generic;
using UnityEngine;
using Everlight.Tales.Data;

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
        private Dictionary<string, Sprite> _lookup;

        private void OnEnable() => _lookup = null;
        private void OnValidate() => _lookup = null;

        public Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key) || _entries == null) return null;
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                foreach (Entry entry in _entries)
                    if (entry != null && !string.IsNullOrEmpty(entry.Key)) _lookup[entry.Key] = entry.Sprite;
            }
            return _lookup.TryGetValue(key, out Sprite sprite) ? sprite : null;
        }

        public Sprite Part(PartType part) => Get("part:" + part) ?? Get("ICO-060");
        public Sprite Object(string id, bool form, bool known = true) => known
            ? Get("object:" + id) ?? Get(form ? "ICO-061" : "ICO-060")
            : Get(form ? "ICO-061" : "ICO-060");
        public Sprite CurrentPart(PartType part, string formId) => Get("object:" + formId) ?? Part(part);
        public Sprite Material(string id, string source = "")
        {
            if (id != "MT-006") return Get("material:" + id) ?? Get("ICO-059");
            return Get("source:" + source) ?? Get("ICO-057-generic");
        }
        public Sprite Event(EventKind kind) => Get("event:" + kind);
    }
}
