using System.Collections.Generic;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>记录本容器持有的租用代次；页面关闭或复用后不回收旧持有记录。</summary>
    public sealed class ListRowCollection
    {
        private struct Entry
        {
            public ListRowItemObject Item;
            public ulong Lease;
            public UIFormBase Form;
            public GameObject Template;
        }
        private readonly List<Entry> _entries = new List<Entry>();

        public ListRowItemObject Spawn(UIFormBase form, GameObject template, Transform root)
        {
            ListRowItemObject row = form.SpawnChildItem<ListRowItemObject>(template, root);
            row.gameObject.transform.SetAsLastSibling();
            _entries.Add(new Entry { Item = row, Lease = row.Lease, Form = form, Template = template });
            return row;
        }

        public void Clear()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry.Form != null && entry.Template != null && entry.Item.gameObject != null
                    && entry.Item.IsSpawned && entry.Item.Lease == entry.Lease)
                    entry.Form.UnspawnChildItem(entry.Template, entry.Item);
            }
            _entries.Clear();
        }
    }
}
