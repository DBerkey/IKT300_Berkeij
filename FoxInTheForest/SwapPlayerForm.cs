using System;
using System.Windows.Forms;

namespace FoxInTheForest
{
    public class SwapPlayerForm : Form
    {
        public SwapPlayerForm()
        {
            this.Text = "Swap Player";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label messageLabel = new Label();
            messageLabel.Text = "Please swap with the other player";
            messageLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            messageLabel.Dock = DockStyle.Top;
            messageLabel.Height = 100;

            Button continueButton = new Button();
            continueButton.Text = "Continue";
            continueButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            continueButton.Width = 120;
            continueButton.Height = 40;
            continueButton.Top = 120;
            continueButton.Left = (this.ClientSize.Width - continueButton.Width) / 2;
            continueButton.Anchor = AnchorStyles.Top;
            continueButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            this.Controls.Add(messageLabel);
            this.Controls.Add(continueButton);
        }
    }
}
