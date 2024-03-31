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
// along with VaultSync.  If not, see<https://www.gnu.org/licenses/>.using System;

using System.Windows.Forms;

namespace VaultSync
{
    public class ImageCache
    {
        public static ImageList SmallThumbnail = new ImageList();
        public static ImageList LargeThumbnail = new ImageList();

        public static void Init()
        {
            // Default is 8 bit and it has intermittent issues with transparency on system icons
            SmallThumbnail.ColorDepth = ColorDepth.Depth32Bit;
            LargeThumbnail.ColorDepth = ColorDepth.Depth32Bit;

        }
    }
}
