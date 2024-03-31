﻿// Copyright © 2019-2023 Simon Knight
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

using System.Windows.Forms;

namespace VaultSync
{
    public partial class IgnorePatternForm : Form
    {
        public IgnorePatternForm()
        {
            InitializeComponent();
        }

        public string IgnorePattern { get => filePattern.Text; set => filePattern.Text = value; }
    }
}
