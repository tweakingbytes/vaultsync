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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VaultSync
{
    public partial class MessageBoxForm : Form
    {
        public static DialogResult Show(string message, MessageBoxButtons buttons)
        {
            MessageBoxForm box = new MessageBoxForm(message, buttons);
            return box.ShowDialog();
        }
        public static DialogResult Show(string message)
        {
            return MessageBoxForm.Show(message, MessageBoxButtons.OK);
        }


        private MessageBoxForm(string text, MessageBoxButtons buttons)
        {
            InitializeComponent();

            // Message display is a text box so that the message can be selected for copy
            // Make it look lke an autosizing label
            message.BackColor = BackColor;
            message.Text = text;
            Graphics graphics = message.CreateGraphics();
            SizeF size = graphics.MeasureString(text, message.Font);
            graphics.Dispose();
            message.Width = (int)Math.Round(size.Width);
            if (message.Lines.Length > 2)
            {
                message.ScrollBars = ScrollBars.Vertical;
            }

            Text = Utils.ProductName;

            switch (buttons)
            {
                case MessageBoxButtons.YesNo:
                    button1.Text = Strings.Yes;
                    button1.DialogResult = DialogResult.Yes;

                    button2.Text = Strings.No;
                    button2.DialogResult = DialogResult.No;
                    Align2Buttons();
                    break;

                case MessageBoxButtons.OK:
                    button1.Text = Strings.OK;
                    button1.DialogResult = DialogResult.OK;
                    Align1Button();
                    break;

                case MessageBoxButtons.YesNoCancel:
                    button1.Text = Strings.Yes;
                    button1.DialogResult = DialogResult.Yes;

                    button2.Text = Strings.No;
                    button2.DialogResult = DialogResult.No;

                    button3.Text = Strings.Cancel;
                    button3.DialogResult = DialogResult.Cancel;

                    Align3Buttons();
                    break;


                default:
                    throw new Exception(Strings.OtherButtonsUnimplemented);
            }
        }

        private void Align1Button()
        {
            Width = Math.Max(2 * message.Left + message.Right, 2* button1.Width);
            button1.Visible = true;
            button1.Left = (Width - button1.Width) / 2;
        }

        private void Align2Buttons()
        {
            Width = Math.Max(2 * message.Left + message.Right, 4 * button1.Width);

            button1.Visible = true;
            button1.Left = (Width - 2 * button1.Width - button1.Width / 2) / 2;

            button2.Visible = true;
            button2.Left = button1.Right + button1.Width / 2;
        }

        private void Align3Buttons()
        {
            Width = Math.Max(2 * message.Left + message.Right, 5 * button1.Width);

            button1.Visible = true;
            button1.Left = (Width - 4 * button1.Width) / 2;

            button2.Visible = true;
            button2.Left = button1.Right + button1.Width / 2;

            button3.Visible = true;
            button3.Left = button2.Right + button2.Width / 2;
        }
    }
}
