// Copyright © 2019-2023 Simon Knight
// This file is part of VaultSync.

// VaultSync is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.

// VaultSync is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with VaultSync.  If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VaultSync
{
    class ListCache
    {
        private DataConnector db;
        private Dictionary<int, ListViewItem> cache;
        private Int64 folderId; // Id of the parent folder for this list
        private string text;    // The text associated with the parent folder (i.e. the folder path)
        private int nextIndex;  // Index of the next insert point in the cache

        public SortOrder Order { get; set; }
        public int SortColumn { get; set; }

        public ListCache(DataConnector db)
        {
            this.db = db;
            cache = new Dictionary<int, ListViewItem>();
            Order = SortOrder.Ascending;
            ResetCache(0, ""); // Start at the root folder
        }

        public int ResetCache()
        {
            nextIndex = 0;
            cache.Clear();

            return db.CountContents(folderId);
        }

        public int ResetCache(Int64 folderId, string text)
        {
            this.folderId = folderId;
            this.text = text;
            return ResetCache();
        }

        // Get the item at the specified index
        public ListViewItem GetItem(int index)
        {
            if (!cache.ContainsKey(index))
            {
                // Cache miss
                CacheFetch(index, index + 5);
            }
            return cache[index];
        }

        public void CacheFetch(int index, int count)
        {
            nextIndex = index;

            db.ListFiles(BuildCacheItems, folderId, count, index, text, GetSortColumn(), GetSortOrder());
        }

        private string GetSortOrder()
        {
            string order;
            if (Order == SortOrder.Descending)
            {
                order = "desc";
            }
            else
            {
                order = "asc";
            }

            return order;
        }

        private string GetSortColumn()
        {
            // Create an appropriate comparitor for the column contents
            string sortCol;
            switch (SortColumn)
            {
                case 1:
                    sortCol = "moddate";
                    break;
                case 2:
                    sortCol = "size";
                    break;
                case 3:
                    sortCol = "host";
                    break;
                default:
                    sortCol = "path";
                    break;
            }

            return sortCol;
        }

        private void BuildCacheItems(FileDetail detail)
        {
            ListViewItem item = CreateListItem(detail);
            cache[nextIndex] = item;
            nextIndex += 1;
        }

        private ListViewItem CreateListItem(FileDetail detail)
        {
            var text = Path.GetFileName(detail.Path);
            if (text == "")
            {
                // Must be the root, so show that instead
                text = detail.Path;
            }
            ListViewItem item = new ListViewItem(text);
            item.Name = detail.Path;
            item.SubItems.Add(detail.ModDate != 0 ? DateTime.FromFileTime(detail.ModDate).ToString() : "");
            item.SubItems.Add(detail.Size != 0 ? Utils.ReadableSize(detail.Size) : "");
            item.SubItems.Add(detail.Host);
            item.Tag = detail; // The item tag holds a reference to the full file detail
            Utils.SetIcon(item);
            return item;
        }
    }
}
