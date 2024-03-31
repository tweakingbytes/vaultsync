
namespace VaultSync
{
    partial class UpdateCheck
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FinishButton = new System.Windows.Forms.Button();
            this.Message = new System.Windows.Forms.TextBox();
            this.SiteLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // CancelButton
            // 
            this.FinishButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.FinishButton.Location = new System.Drawing.Point(12, 78);
            this.FinishButton.Name = "CancelButton";
            this.FinishButton.Size = new System.Drawing.Size(75, 23);
            this.FinishButton.TabIndex = 0;
            this.FinishButton.Text = "Cancel";
            this.FinishButton.UseVisualStyleBackColor = true;
            this.FinishButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // Message
            // 
            this.Message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Message.CausesValidation = false;
            this.Message.Location = new System.Drawing.Point(12, 12);
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Size = new System.Drawing.Size(510, 13);
            this.Message.TabIndex = 3;
            // 
            // SiteLink
            // 
            this.SiteLink.AutoSize = true;
            this.SiteLink.Location = new System.Drawing.Point(9, 42);
            this.SiteLink.Name = "SiteLink";
            this.SiteLink.Size = new System.Drawing.Size(29, 13);
            this.SiteLink.TabIndex = 4;
            this.SiteLink.TabStop = true;
            this.SiteLink.Text = "URL";
            this.SiteLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SiteLink.Visible = false;
            this.SiteLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SiteLink_LinkClicked);
            // 
            // UpdateCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 113);
            this.Controls.Add(this.SiteLink);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.FinishButton);
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateCheck";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check for Updates";
            this.Load += new System.EventHandler(this.UpdateCheck_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button FinishButton;
        private System.Windows.Forms.TextBox Message;
        private System.Windows.Forms.LinkLabel SiteLink;
    }
}