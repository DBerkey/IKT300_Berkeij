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
        private int currentPlayer = 1;
        private int player1Points = 0;
        private int player2Points = 0;
        private string playedCard = "";
        private string currentDecree = "";

        public GameForm(int winThreshold)
        {
            this.winThreshold = winThreshold;
            InitializeComponent();
            this.FormClosing += GameForm_FormClosing;
        }

        private ListBox? player1ListBox;
        private Label? cardInfoLabel;
        private Button? playCardButton;

        private void InitializeComponent()
        {
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = $"Fox in the Forest - Playing to {winThreshold} points";

            Label info = new Label();
            info.Text = $"Game started! Playing to {winThreshold} points.";
            info.Location = new System.Drawing.Point(50, 20);
            info.Size = new System.Drawing.Size(400, 30);
            info.Font = new Font("Segoe UI", 12F);

            ShuffleCards();

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
            Label playedCardLabel = new Label();
            playedCardLabel.Location = new System.Drawing.Point(275, 100);
            playedCardLabel.Size = new System.Drawing.Size(175, 253);
            playedCardLabel.Font = new Font("Segoe UI", 12F);
            playedCardLabel.BorderStyle = BorderStyle.FixedSingle;
            playedCardLabel.Text = $"No Card Played";

            this.Controls.Add(playedCardLabel);

            // Show the hand
            DisplayHand(player1Hand, player1ListBox);
        }

        private void Player1ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (player1ListBox.SelectedIndex >= 0)
            {
                Card selectedCard = player1Hand[player1ListBox.SelectedIndex];
                if (selectedCard.Value % 2 == 1) // special cards: 1,3,5,7,9,11
                {
                    if (selectedCard.Value == 1)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the Swan, when this card is played and you lose the fight you will start the next one.";
                    }
                    else if (selectedCard.Value == 3)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the Fox, when this card is played you may swap the decree card with one from your hand.";
                    }
                    else if (selectedCard.Value == 5)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the woodcutter, when this card is played you draw a card from the draw pile and afterwards you remove a card from your hand.";
                    }
                    else if (selectedCard.Value == 7)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the treasure, the winner of this fight get an extra point for every 7 played.";
                    }
                    else if (selectedCard.Value == 9)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the witch, if the fight contains only one witch this card will work as the symbol of the decree card.";
                    }
                    else if (selectedCard.Value == 11)
                    {
                        cardInfoLabel.Text = $"Special card: {selectedCard}. This is the king, when this card is played the opponent needs to play their corresponding 1 or highest corresponding card.";
                    }
                }
                else
                {
                    cardInfoLabel.Text = $"Normal card: {selectedCard}.";
                }
            }
        }

        private void PlayCardButton_Click(object? sender, EventArgs e)
        {
            if (player1ListBox.SelectedIndex >= 0)
            {
                Card selectedCard = player1Hand[player1ListBox.SelectedIndex];
                playedCard = selectedCard.ToString();
                player1Hand.RemoveAt(player1ListBox.SelectedIndex);
                DisplayHand(player1Hand, player1ListBox);
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


        private List<Card> player1Hand = new List<Card>();
        private List<Card> player2Hand = new List<Card>();
        private List<Card> drawPile = new List<Card>();
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