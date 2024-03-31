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
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace VaultSync
{
    public partial class ChangePasswordForm : Form
    {
        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private int Entropy { get; set; }
        private int Percent { get; set; }
        
        public string Password { get => password.Text; }

        private enum CharacterClass { None = 0, LowerCase = 1, UpperCase = 2, Digit = 4, Symbol = 8, }

        private void password_TextChanged(object sender, EventArgs e)
        {
            CheckPassword();
            EstimatedQuality();
        }

        // Both passwords have to be the same for OK to be enabled
        private void CheckPassword()
        {
            if (password.Text != checkPassword.Text)
            {
                checkPassword.BackColor = Color.Pink;
                OkButton.Enabled = false;
            }
            else
            {
                checkPassword.BackColor = SystemColors.Window;
                OkButton.Enabled = true;
            }
        }

        private void checkPassword_TextChanged(object sender, EventArgs e)
        {
            CheckPassword();
        }

        private void EstimatedQuality()
        {
            string[] sequence = { "~!@#$%^&*()_+", "1234567890-=", "qwertyuiop", "asdfghjkl", "zxcvbnm", "password" }; // Obvious sequences
            string text = password.Text;

            int repeatCount = 0;
            char repeatChar = '\0';
            int sequenceIndex = -1;
            string seqStr = "";

            CharacterClass charClass = GetCharacterClass(text);

            double totalSymbols = 1; // start at 1 because of the multiplys
            foreach (char c in text)
            {
                totalSymbols *= EstimatedSymbols(c, charClass); // product of total symbols

                // Track character repeats
                if (c == repeatChar)
                {
                    repeatCount += 1;
                }
                else
                {
                    // Repeat ends so reset it
                    repeatChar = c;
                    repeatCount = 0;
                }

                // Discount repeated characters
                if (repeatCount > 2)
                {
                    totalSymbols /= EstimatedSymbols(repeatChar, charClass) * (repeatCount - 2); // Discount the repeated chars
                }

                // Look for obvious sequences
                if (sequenceIndex < 0)
                {
                    // Search the sequences for the character
                    for (int i = 0; i < sequence.Length; i += 1)
                    {
                        if (sequence[i].Contains(c))
                        {
                            // Start a sequence
                            sequenceIndex = i;
                            seqStr = char.ToLower(c).ToString();
                            break;
                        }
                    }
                }
                else
                {
                    // Currently in a sequence
                    seqStr += char.ToLower(c);
                    if (!sequence[sequenceIndex].Contains(seqStr))
                    {
                        // Sequence has ended
                        sequenceIndex = -1;
                    }

                    // Discount components of obvious sequences
                    if (seqStr.Length > 2)
                    {
                        totalSymbols /= (seqStr.Length - 2) * EstimatedSymbols(seqStr[0], charClass);
                    }
                }
            }

            // Calculate the number of bits in the symbols
            Entropy = (int)Math.Floor(Math.Log(totalSymbols) / Math.Log(2)); 

            // Calculated entropy as a percentage of a 'good' one, e.g. 128 bits. This is an arbitary reference so handle > 100%
            int value = (Entropy * 100) / 128;
            Percent = value > 0 ? (value > 100 ? 100 : value) : 0;

            quality.Invalidate();
        }

        // Estimate the entropy of a character, i.e. the number of symbols in the character class
        private static double EstimatedSymbols(char c, CharacterClass charClass)
        {
            // This algorithm assumes that the attacker has guessed what character classes are in the password.
            // The more classes there are then the more symbol combinations are required in a brute force attack.
            // This also assumes that the probability of a user choosing a character from the class is equal but it won't be in practice. Compensating for this may introduce other security flaws.
            int[] symbols = {
                1,  // 0 None
                26, // 1 Lower
                26, // 2 Upper
                52, // 3 Lower & Upper
                10, // 4 Digit
                36, // 5 Lower & Digit
                36, // 6 Upper & Digit
                62, // 7 Lower & Upper & Digit
                30, // 8 Symbol 
                56, // 9 Lower & Symbol 
                56, // 10 Upper & Symbol 
                82, // 11 Lower & Upper & Symbol 
                40, // 12 Digit  & Symbol 
                66, // 13 Lower & Digit & Symbol 
                66, // 14 Upper & Digit & Symbol 
                92, // 15 Lower & Upper & Digit & Symbol
            };

            return symbols[(int)charClass]; 
        }

        // Scan for character classes
        private CharacterClass GetCharacterClass(string text)
        {
            CharacterClass result = CharacterClass.None;

            foreach (char c in password.Text)
            {
                if (char.IsDigit(c))
                {
                    result |= CharacterClass.Digit;
                }
                else if (char.IsLower(c))
                {
                    result |= CharacterClass.LowerCase;
                }
                else if (char.IsUpper(c))
                {
                    result |= CharacterClass.UpperCase;
                }
                else if (char.IsPunctuation(c))
                {
                    result |= CharacterClass.Symbol;
                }
                else if (char.IsWhiteSpace(c))
                {
                    result |= CharacterClass.Symbol;
                }
            }
            return result;
        }
         
        private void quality_Paint(object sender, PaintEventArgs e)
        {
            // Create a red to green gradient brush as big as the panel
            LinearGradientBrush linGrBrush = new LinearGradientBrush( 
               new Point(0, 0),
               new Point(quality.Width, quality.Height),
               Color.FromArgb(180, 255, 0, 0),   // Opaque red
               Color.FromArgb(180, 0, 255, 0));  // Opaque green

            // Paint a rectangle on the panel with the gradient brush, sized by the quality percentage
            Pen pen = new Pen(linGrBrush);
            e.Graphics.FillRectangle(linGrBrush, 0, 0, quality.Width * Percent / 100, quality.Height);

            // Draw the number of bits in the password, centered in the panel
            string text = string.Format(Strings.Bits, Entropy);
            SizeF textSize = e.Graphics.MeasureString(text, Font);
            SolidBrush textBrush = new SolidBrush(Color.Black);
            e.Graphics.DrawString(text, Font, textBrush, new PointF((quality.Width - textSize.Width) / 2, (quality.Height - textSize.Height) / 2));
        }
    }
}
