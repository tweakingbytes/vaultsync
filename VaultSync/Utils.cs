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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VaultSync
{
    public class Utils
    {
        public static readonly string ProductName = Strings.ProductName;
        public static int IntCompare(long cmp)
        {
            if (cmp > 0)
            {
                return 1;
            }
            else if (cmp < 0)
            {
                return -1;
            }
            return 0;
        }


        public static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_SMALLICON);

            if (shinfo.hIcon != System.IntPtr.Zero)
            {
                return GetIcon(shinfo);
            }

            // Try again without expecting the file to be there
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

            if (shinfo.hIcon != System.IntPtr.Zero)
            {
                return GetIcon(shinfo);
            }
            return null;
        }

        private static Icon GetIcon(SHFILEINFO shinfo)
        {
            Icon tempManagedRes = Icon.FromHandle(shinfo.hIcon);
            Icon icon = (Icon)tempManagedRes.Clone();
            tempManagedRes.Dispose();
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        // Set the icon key for the file type from the image cache
        public static void SetIcon(ListViewItem item)
        {
            // Key the image from the file extension if one exists, otherwise use the full path
            string key = Path.GetExtension(item.Name);
            if (string.IsNullOrEmpty(key))
            {
                key = item.Name;
            }

            // Check to see if the image collection contains an image for this key.
            if (!ImageCache.SmallThumbnail.Images.ContainsKey(key))
            {
                // If not, add the image to the image list.
                Icon iconForFile = ExtractFromPath(item.Name);
                if (iconForFile != null)
                {
                    ImageCache.SmallThumbnail.Images.Add(key, iconForFile);
                }
            }
            item.ImageIndex = ImageCache.SmallThumbnail.Images.IndexOfKey(key); // Can't use key for virtual lists
        }

        // Generate a string for a file size
        public static string ReadableSize(Int64 size)
        {
            if (size < 1024)
            {
                return size.ToString();
            }
            return string.Format(Strings.FileSizeFormat, size / 1024);
        }

        // Convert a sync type to a string
        public static string TypeString(DataConnector.SyncType type)
        {
            return type == DataConnector.SyncType.Folder ? Strings.Folder : Strings.File;
        }

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyIcon(IntPtr hIcon);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

           
    }


    // Code from https://stackoverflow.com/questions/254129/how-to-i-display-a-sort-arrow-in-the-header-of-a-list-view-column-using-c
    public static class ListViewExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            };

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            };
        };

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;

        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM lParam);

        public static void SetSortIcon(this ListView listViewControl, int columnIndex, SortOrder order)
        {
            IntPtr columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            for (int columnNumber = 0; columnNumber <= listViewControl.Columns.Count - 1; columnNumber++)
            {
                var columnPtr = new IntPtr(columnNumber);
                var item = new HDITEM
                {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    Exception e = new System.ComponentModel.Win32Exception();
                    Console.WriteLine(e.Message);
                }

                if (order != SortOrder.None && columnNumber == columnIndex)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    Exception e = new System.ComponentModel.Win32Exception();
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}