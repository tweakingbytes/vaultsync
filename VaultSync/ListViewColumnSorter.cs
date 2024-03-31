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

using System.Collections;
using System.Windows.Forms;

// From https://support.microsoft.com/en-au/help/319401/how-to-sort-a-listview-control-by-a-column-in-visual-c

namespace VaultSync
{

    /// This class is an implementation of the 'IComparer' interface.
    public class ListViewColumnSorter : IComparer
    {
        /// Specifies the column to be sorted
        private int ColumnToSort;
        /// Specifies the order in which to sort (i.e. 'Ascending').
        private SortOrder OrderOfSort;

        /// Class constructor.  Initializes various elements
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'Ascending'
            OrderOfSort = SortOrder.Ascending;

            // Initialize the CaseInsensitiveComparer object
             Comparitor = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult;

            // Compare the two items
            if (Comparitor is CaseInsensitiveComparer)
            {
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                compareResult = Comparitor.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
            }
            else
            {
                compareResult = Comparitor.Compare(x, y);
            }

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }

        public IComparer Comparitor  { get; set; }

    }

    // Compares the modified date column, or the path if equal
    public class DateCompare : IComparer
    {
        public int Compare(object x, object y)
        {
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;
            FileDetail detailX = (FileDetail)itemX.Tag;
            FileDetail detailY = (FileDetail)itemY.Tag;
            int result = Utils.IntCompare(detailX.ModDate - detailY.ModDate);
            if (result == 0)
            {
                result = string.Compare(detailX.Path, detailY.Path);
            }
            return result;
        }
    }

    // Compares the size column, or the path if equal
    public class SizeCompare : IComparer
    {
        public int Compare(object x, object y)
        {
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;
            FileDetail detailX = (FileDetail)itemX.Tag;
            FileDetail detailY = (FileDetail)itemY.Tag;
            int result = Utils.IntCompare(detailX.Size - detailY.Size);
            if (result == 0)
            {
                result = string.Compare(detailX.Path, detailY.Path);
            }
            return result;
        }
    }
}
