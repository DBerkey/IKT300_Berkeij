namespace FoxInTheForest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.ActiveControl = StartGameButton;
        }

        private void GameWinThresholdInputBox_TextChanged(object sender, EventArgs e)
        {
            // Validate and update the game win threshold
            if (!(int.TryParse(GameWinThresholdInputBox.Text, out int threshold)))
            {
                // Handle invalid input
                MessageBox.Show("Please enter a valid number.");
                GameWinThresholdInputBox.Text = "";
            }
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            int winThreshold = 21;
            if (int.TryParse(GameWinThresholdInputBox.Text, out int threshold))
            {
                winThreshold = threshold;
            }

            GameForm gameForm = new GameForm(winThreshold);
            gameForm.Show();
            this.Hide();
        }
    }
}
