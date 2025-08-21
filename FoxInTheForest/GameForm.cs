using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace FoxInTheForest
{
    public enum CardSuit
    {
        Bell,
        Key,
        Moon
    }

    // Card class
    public class Card
    {
        public CardSuit Suit { get; set; }
        public int Value { get; set; }
        public string DisplayName => $"{Suit} {Value}";

        public Card(CardSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
    
    public partial class GameForm : Form
    {
        private int winThreshold;
        private int currentPlayer;
        private int player1Points;
        private int player2Points;
    private string playedCard = "";
    private string? lastPlayedCard;
    private string? lastPlayedCardImagePath;
        private string currentDecree;
        private string moonPicturePath = System.IO.Path.GetFullPath("moon.png");
        private string keyPicturePath = System.IO.Path.GetFullPath("key.png");
        private string bellPicturePath = System.IO.Path.GetFullPath("bell.png");
        private List<Card> player1Hand;
        private List<Card> player2Hand;
        private List<Card> drawPile;

        public GameForm(
            int winThreshold,
            int playerNumber = 1,
            List<Card>? player1Hand = null,
            List<Card>? player2Hand = null,
            List<Card>? drawPile = null,
            int player1Points = 0,
            int player2Points = 0,
            string? currentDecree = null,
            string? lastPlayedCard = null,
            string? lastPlayedCardImagePath = null
        )
        {
            this.winThreshold = winThreshold;
            this.currentPlayer = playerNumber;
            this.player1Points = player1Points;
            this.player2Points = player2Points;
            this.currentDecree = currentDecree ?? "";
            this.player1Hand = player1Hand ?? new List<Card>();
            this.player2Hand = player2Hand ?? new List<Card>();
            this.drawPile = drawPile ?? new List<Card>();
            this.lastPlayedCard = lastPlayedCard;
            this.lastPlayedCardImagePath = lastPlayedCardImagePath;
            if (this.player1Hand.Count == 0 || this.player2Hand.Count == 0 || this.drawPile.Count == 0)
            {
                ShuffleCards();
            }
            InitializeComponent();
            this.FormClosing += GameForm_FormClosing;
        }

        private ListBox? player1ListBox;
        private Label? cardInfoLabel;
        private Button? playCardButton;
        private Label? playedCardLabel;
        private PictureBox? playedCardPictureBox;

        private void ShowSwapPlayerScreenAndSwitch(int nextPlayerNumber)
        {
            using (var swapForm = new SwapPlayerForm())
            {
                if (swapForm.ShowDialog() == DialogResult.OK)
                {
                    // Determine the image path for the played card
                    string? imagePath = null;
                    if (!string.IsNullOrEmpty(playedCard))
                    {
                        if (playedCard.StartsWith("Bell"))
                            imagePath = bellPicturePath;
                        else if (playedCard.StartsWith("Key"))
                            imagePath = keyPicturePath;
                        else if (playedCard.StartsWith("Moon"))
                            imagePath = moonPicturePath;
                    }
                    var nextForm = new GameForm(
                        winThreshold,
                        nextPlayerNumber,
                        new List<Card>(player1Hand),
                        new List<Card>(player2Hand),
                        new List<Card>(drawPile),
                        player1Points,
                        player2Points,
                        currentDecree,
                        playedCard, // pass the last played card text
                        imagePath // pass the last played card image path
                    );
                    this.Hide();
                    nextForm.ShowDialog();
                    this.Close();
                }
            }
        }

        private void InitializeComponent()
        {
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = $"Fox in the Forest - Playing to {winThreshold} points";

            Label info = new Label();
            info.Text = $"Game started! Playing to {winThreshold} points.";
            info.Location = new System.Drawing.Point(50, 20);
            info.Size = new System.Drawing.Size(400, 30);
            info.Font = new Font("Segoe UI", 12F);

            if (player1Hand.Count == 0 || player2Hand.Count == 0 || drawPile.Count == 0)
            {
                ShuffleCards();
            }

            // --- Add ListBox for Player 1 ---
            player1ListBox = new ListBox();
            player1ListBox.Location = new System.Drawing.Point(50, 60);
            player1ListBox.Size = new System.Drawing.Size(200, 300);
            player1ListBox.Font = new Font("Segoe UI", 10F);
            player1ListBox.SelectedIndexChanged += Player1ListBox_SelectedIndexChanged;

            this.Controls.Add(info);
            this.Controls.Add(player1ListBox);

            // --- Add Label for card info ---
            cardInfoLabel = new Label();
            cardInfoLabel.Location = new System.Drawing.Point(50, 370);
            cardInfoLabel.Size = new System.Drawing.Size(200, 200);
            cardInfoLabel.Font = new Font("Segoe UI", 10F);
            cardInfoLabel.BorderStyle = BorderStyle.FixedSingle;
            cardInfoLabel.Text = "Select a card to see info.";

            this.Controls.Add(cardInfoLabel);

            // --- Add play card button ---
            playCardButton = new Button();
            playCardButton.Text = "Play Card";
            playCardButton.Location = new System.Drawing.Point(650, 540);
            playCardButton.Size = new System.Drawing.Size(100, 30);
            playCardButton.Click += PlayCardButton_Click;

            this.Controls.Add(playCardButton);

            // --- Add label for current decree ---
            Label decreeLabel = new Label();
            decreeLabel.Location = new System.Drawing.Point(275, 60);
            decreeLabel.Size = new System.Drawing.Size(175, 30);
            decreeLabel.Font = new Font("Segoe UI", 12F);
            decreeLabel.BorderStyle = BorderStyle.FixedSingle;
            decreeLabel.Text = $"Decree: {currentDecree}";

            this.Controls.Add(decreeLabel);

            // --- Add score board ---
            Label scoreBoardLabel = new Label();
            scoreBoardLabel.Location = new System.Drawing.Point(475, 60);
            scoreBoardLabel.Size = new System.Drawing.Size(100, 30);
            scoreBoardLabel.Font = new Font("Segoe UI", 12F);
            scoreBoardLabel.BorderStyle = BorderStyle.FixedSingle;
            scoreBoardLabel.Text = $"Score: {player1Points} - {player2Points}";

            this.Controls.Add(scoreBoardLabel);

            // --- Add current player ---
            Label currentPlayerLabel = new Label();
            currentPlayerLabel.Location = new System.Drawing.Point(600, 60);
            currentPlayerLabel.Size = new System.Drawing.Size(150, 30);
            currentPlayerLabel.Font = new Font("Segoe UI", 12F);
            currentPlayerLabel.BorderStyle = BorderStyle.FixedSingle;
            currentPlayerLabel.Text = $"Current Player: {currentPlayer}";

            this.Controls.Add(currentPlayerLabel);
            // --- Played card ---
            playedCardLabel = new Label();
            playedCardLabel.Location = new System.Drawing.Point(275, 100);
            playedCardLabel.Size = new System.Drawing.Size(175, 253);
            playedCardLabel.Font = new Font("Segoe UI", 12F);
            playedCardLabel.BorderStyle = BorderStyle.FixedSingle;
            if (!string.IsNullOrEmpty(lastPlayedCard))
                playedCardLabel.Text = $"Played Card: {lastPlayedCard}";
            else
                playedCardLabel.Text = $"No Card Played";

            this.Controls.Add(playedCardLabel);

            playedCardPictureBox = new PictureBox();
            playedCardPictureBox.Location = new System.Drawing.Point(288, 150);
            playedCardPictureBox.Size = new System.Drawing.Size(150, 150);
            playedCardPictureBox.BorderStyle = BorderStyle.None;
            playedCardPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Show last played card image if available
            if (!string.IsNullOrEmpty(lastPlayedCardImagePath) && System.IO.File.Exists(lastPlayedCardImagePath))
            {
                playedCardPictureBox.ImageLocation = lastPlayedCardImagePath;
                playedCardPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                playedCardPictureBox.Update();
            }
            else
            {
                playedCardPictureBox.Image = null;
            }

            this.Controls.Add(playedCardPictureBox);
            playedCardPictureBox.BringToFront();

            // Add a label above the table
            Label pointsTableTitle = new Label();
            pointsTableTitle.Text = "Both players get points at the end of the round based on the amount of fights won.";
            pointsTableTitle.Location = new System.Drawing.Point(475, 100);
            pointsTableTitle.Size = new System.Drawing.Size(275, 30);
            pointsTableTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pointsTableTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            pointsTableTitle.AutoEllipsis = true;
            this.Controls.Add(pointsTableTitle);

            // Add a static table using TableLayoutPanel
            TableLayoutPanel pointsTable = new TableLayoutPanel();
            pointsTable.Location = new System.Drawing.Point(475, 130);
            pointsTable.Size = new System.Drawing.Size(275, 223);
            pointsTable.ColumnCount = 2;
            pointsTable.RowCount = 7;
            pointsTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            pointsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            pointsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            pointsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            for (int i = 1; i < 7; i++) pointsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            pointsTable.Font = new Font("Segoe UI", 12F);

            // Header
            pointsTable.Controls.Add(new Label() { Text = "Fights", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold) }, 0, 0);
            pointsTable.Controls.Add(new Label() { Text = "Points", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold) }, 1, 0);
            // Rows
            pointsTable.Controls.Add(new Label() { Text = "0-3", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 1);
            pointsTable.Controls.Add(new Label() { Text = "6", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 1);
            pointsTable.Controls.Add(new Label() { Text = "4", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 2);
            pointsTable.Controls.Add(new Label() { Text = "1", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 2);
            pointsTable.Controls.Add(new Label() { Text = "5", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 3);
            pointsTable.Controls.Add(new Label() { Text = "2", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 3);
            pointsTable.Controls.Add(new Label() { Text = "6", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 4);
            pointsTable.Controls.Add(new Label() { Text = "3", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 4);
            pointsTable.Controls.Add(new Label() { Text = "7-9", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 5);
            pointsTable.Controls.Add(new Label() { Text = "6", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 5);
            pointsTable.Controls.Add(new Label() { Text = "10-13", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, 6);
            pointsTable.Controls.Add(new Label() { Text = "0", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, 6);

            this.Controls.Add(pointsTable);


            // Show the hand for the current player
            DisplayHand(currentPlayer == 1 ? player1Hand : player2Hand, player1ListBox);
        }

        private void Player1ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (player1ListBox != null && player1ListBox.SelectedIndex >= 0)
            {
                var hand = currentPlayer == 1 ? player1Hand : player2Hand;
                Card selectedCard = hand[player1ListBox.SelectedIndex];
                if (selectedCard.Value % 2 == 1) // special cards: 1,3,5,7,9,11
                {
                    if (selectedCard.Value == 1)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the Swan, when this card is played and you lose the fight you will start the next one.";
                    }
                    else if (selectedCard.Value == 3)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the Fox, when this card is played you may swap the decree card with one from your hand.";
                    }
                    else if (selectedCard.Value == 5)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the woodcutter, when this card is played you draw a card from the draw pile and afterwards you remove a card from your hand.";
                    }
                    else if (selectedCard.Value == 7)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the treasure, the winner of this fight get an extra point for every 7 played.";
                    }
                    else if (selectedCard.Value == 9)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the witch, if the fight contains only one witch this card will work as the symbol of the decree card.";
                    }
                    else if (selectedCard.Value == 11)
                    {
                        if (cardInfoLabel != null)
                            cardInfoLabel.Text = $"Special card: {selectedCard}. This is the king, when this card is played the opponent needs to play their corresponding 1 or highest corresponding card.";
                    }
                }
                else
                {
                    if (cardInfoLabel != null)
                        cardInfoLabel.Text = $"Normal card: {selectedCard}.";
                }
            }
        }

        private void PlayCardButton_Click(object? sender, EventArgs e)
        {
            if (player1ListBox != null && player1ListBox.SelectedIndex >= 0)
            {
                var hand = currentPlayer == 1 ? player1Hand : player2Hand;
                Card selectedCard = hand[player1ListBox.SelectedIndex];
                playedCard = selectedCard.ToString();
                hand.RemoveAt(player1ListBox.SelectedIndex);
                DisplayHand(hand, player1ListBox);

                if (playedCardLabel != null && playedCardLabel.Text == "No Card Played")
                {
                    playedCardLabel.Text = $"Played Card: {playedCard}";

                    if (playedCardPictureBox != null)
                    {
                        if (playedCard.StartsWith("Bell"))
                        {
                            if (!System.IO.File.Exists(bellPicturePath))
                            {
                                MessageBox.Show($"Image not found: {bellPicturePath}", "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                playedCardPictureBox.ImageLocation = bellPicturePath;
                                playedCardPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                                playedCardPictureBox.Update();
                            }
                        }
                        else if (playedCard.StartsWith("Key"))
                        {
                            if (!System.IO.File.Exists(keyPicturePath))
                            {
                                MessageBox.Show($"Image not found: {keyPicturePath}", "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                playedCardPictureBox.ImageLocation = keyPicturePath;
                                playedCardPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                                playedCardPictureBox.Update();
                            }
                        }
                        else if (playedCard.StartsWith("Moon"))
                        {
                            if (!System.IO.File.Exists(moonPicturePath))
                            {
                                MessageBox.Show($"Image not found: {moonPicturePath}", "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                playedCardPictureBox.ImageLocation = moonPicturePath;
                                playedCardPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                                playedCardPictureBox.Update();
                            }
                        }
                        else
                        {
                            playedCardPictureBox.Image = null; // Clear the image if it's a normal card
                            playedCardPictureBox.Update();
                        }
                    }
                }
                int nextPlayer = (currentPlayer == 1) ? 2 : 1;
                this.Hide();
                ShowSwapPlayerScreenAndSwitch(nextPlayer);
            }
        }

        private void DisplayHand(List<Card> hand, ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (Card card in hand)
            {
                listBox.Items.Add(card.ToString());
            }
        }

        private Random rng = new Random();

        private void ShuffleCards()
        {
            // Create the deck
            List<Card> deck = new List<Card>();

            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                for (int value = 1; value <= 11; value++)
                {
                    deck.Add(new Card(suit, value));
                }
            }

            // Shuffle the full deck
            Shuffle(deck);

            // Deal 13 cards to each player
            player1Hand = deck.Take(13).ToList();
            player2Hand = deck.Skip(13).Take(13).ToList();

            // Remaining cards form the draw pile
            drawPile = deck.Skip(26).ToList();

            // Shuffle draw pile just in case
            Shuffle(drawPile);

            currentDecree = drawPile.FirstOrDefault()?.ToString() ?? "";
            drawPile.RemoveAt(0);
        }

        // Generic Fisher-Yates shuffle
        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T temp = list[k];
                list[k] = list[n];
                list[n] = temp;
            }
        }

        private void GameForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
