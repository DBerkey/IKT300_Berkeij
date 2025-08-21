using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FoxInTheForest
{
    public class SelectCardForm : Form
    {
        private ListBox cardListBox;
        private Button okButton;
        private Button cancelButton;
        public Card? SelectedCard { get; private set; }

        public SelectCardForm(List<Card> cards, string title = "Select a card")
        {
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(300, 400);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.SelectedCard = null;

            cardListBox = new ListBox();
            cardListBox.Location = new System.Drawing.Point(20, 20);
            cardListBox.Size = new System.Drawing.Size(250, 280);
            cardListBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            foreach (var card in cards)
            {
                cardListBox.Items.Add(card);
            }
            cardListBox.SelectedIndexChanged += CardListBox_SelectedIndexChanged;
            this.Controls.Add(cardListBox);

            okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new System.Drawing.Point(40, 320);
            okButton.Size = new System.Drawing.Size(80, 30);
            okButton.Enabled = false;
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new System.Drawing.Point(160, 320);
            cancelButton.Size = new System.Drawing.Size(80, 30);
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);
        }

        private void CardListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            okButton.Enabled = cardListBox.SelectedIndex >= 0;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (cardListBox.SelectedIndex >= 0)
            {
                SelectedCard = cardListBox.SelectedItem as Card;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
