namespace FoxInTheForest
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;

        private Label GameInfoLabel;
        private TextBox GameWinThresholdInputBox;
        private Button StartGameButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GameInfoLabel = new System.Windows.Forms.Label();
            this.GameWinThresholdInputBox = new System.Windows.Forms.TextBox();
            this.StartGameButton = new System.Windows.Forms.Button();

            this.SuspendLayout();

            this.GameInfoLabel.Size = new System.Drawing.Size(200, 60);
            this.GameInfoLabel.AutoEllipsis = true;
            this.GameInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.GameInfoLabel.AutoSize = false;
            this.GameInfoLabel.Text = "Welcome to Fox in the Forest!" +
                "\nSet your win threshold and start the game.";
            this.GameInfoLabel.Name = "GameInfoLabel";
            this.GameInfoLabel.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GameInfoLabel.Location = new Point(
                (this.ClientSize.Width - this.GameInfoLabel.Width) / 2,
                40
            );

            this.GameWinThresholdInputBox.Size = new System.Drawing.Size(200, 30);
            this.GameWinThresholdInputBox.Name = "GameWinThresholdInputBox";
            this.GameWinThresholdInputBox.PlaceholderText = "21";
            this.GameWinThresholdInputBox.TextChanged += new System.EventHandler(this.GameWinThresholdInputBox_TextChanged);
            this.GameWinThresholdInputBox.Location = new Point(
                (this.ClientSize.Width - this.GameWinThresholdInputBox.Width) / 2,
                120
            );

            this.StartGameButton.Size = new System.Drawing.Size(200, 40);
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            this.StartGameButton.Location = new Point(
                (this.ClientSize.Width - this.StartGameButton.Width) / 2,
                180
            );

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 450);
            this.Controls.Add(this.GameInfoLabel);
            this.Controls.Add(this.GameWinThresholdInputBox);
            this.Controls.Add(this.StartGameButton);
            this.Name = "Form1";
            this.Text = "Fox in the Forest";

            this.ResumeLayout(false);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        #endregion
    }
}