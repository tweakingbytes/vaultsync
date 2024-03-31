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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VaultSync
{
    class IgnorePatternMatcher
    {
        Regex regEx;
        string list;

        public void LoadIgnoreList(DataConnector db)
        {
            list = null;
            db.ListIgnoreItems(IgnoreAction);

            if (list != null)
            {
                regEx = new Regex(list, RegexOptions.IgnoreCase);
            }
        }

        private void IgnoreAction(string pattern)
        {
            String convertedMask = "^" + Regex.Escape(pattern).Replace("\\.", "\\.").Replace("\\*", ".*").Replace("\\?", ".") + "$";
            if (list == null)
            {
                list = convertedMask;
            }
            else
            {
                list += "|" + convertedMask;
            }
        }

        public bool IgnoreFile(string path)
        {
            if (regEx != null)
            {
                return regEx.IsMatch(path);
            }
            return false;
        }
    }
}
