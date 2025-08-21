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
        private int player1FightPoints;
        private int player2FightPoints;
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
            int player1FightPoints = 0,
            int player2FightPoints = 0,
            string? currentDecree = null,
            string? lastPlayedCard = null,
            string? lastPlayedCardImagePath = null,
            Card? pendingFightCard = null,
            int? pendingFightPlayer = null
        )
        {
            this.winThreshold = winThreshold;
            this.currentPlayer = playerNumber;
            this.player1Points = player1Points;
            this.player2Points = player2Points;
            this.player1FightPoints = player1FightPoints;
            this.player2FightPoints = player2FightPoints;
            this.currentDecree = currentDecree ?? "";
            this.player1Hand = player1Hand ?? new List<Card>();
            this.player2Hand = player2Hand ?? new List<Card>();
            this.drawPile = drawPile ?? new List<Card>();
            this.lastPlayedCard = lastPlayedCard;
            this.lastPlayedCardImagePath = lastPlayedCardImagePath;
            this.pendingFightCard = pendingFightCard;
            this.pendingFightPlayer = pendingFightPlayer;
            // Only shuffle and deal if both hands are null (i.e., new round), not just empty
            if (player1Hand == null && player2Hand == null)
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
        private Label? fightPointsLabel;

        private void ShowSwapPlayerScreenAndSwitch(int nextPlayerNumber, Card? pendingFightCardToPass = null, int? pendingFightPlayerToPass = null)
        {
            // Only show the swap popup if the next player is NOT the current player
            bool showPopup = nextPlayerNumber != currentPlayer;
            bool proceed = true;
            if (showPopup)
            {
                using (var swapForm = new SwapPlayerForm())
                {
                    proceed = swapForm.ShowDialog() == DialogResult.OK;
                }
            }
            if (proceed)
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
                    player1FightPoints,
                    player2FightPoints,
                    currentDecree,
                    playedCard, // pass the last played card text
                    imagePath, // pass the last played card image path
                    pendingFightCardToPass ?? this.pendingFightCard,
                    pendingFightPlayerToPass ?? this.pendingFightPlayer
                );
                this.Hide();
                nextForm.ShowDialog();
                this.Close();
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

            // Only shuffle and deal if both hands are null (i.e., new round), not just empty
            if (player1Hand == null && player2Hand == null)
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

            // --- Add fight points label ---
            fightPointsLabel = new Label();
            fightPointsLabel.Location = new System.Drawing.Point(275, 370);
            fightPointsLabel.Size = new System.Drawing.Size(175, 30);
            fightPointsLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            fightPointsLabel.BorderStyle = BorderStyle.FixedSingle;
            fightPointsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            fightPointsLabel.Text = $"Fight Points: {player1FightPoints} - {player2FightPoints}";
            this.Controls.Add(fightPointsLabel);

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

        private Card? pendingFightCard = null;
        private int? pendingFightPlayer = null;

    // Track all 7s played in the round
    private List<int> sevensPlayedByPlayer = new List<int> { 0, 0 }; // [0]=player1, [1]=player2

        private void PlayCardButton_Click(object? sender, EventArgs e)
        {
            if (player1ListBox != null && player1ListBox.SelectedIndex >= 0)
            {
                var hand = currentPlayer == 1 ? player1Hand : player2Hand;
                Card selectedCard = hand[player1ListBox.SelectedIndex];
                hand.RemoveAt(player1ListBox.SelectedIndex);
                DisplayHand(hand, player1ListBox);

                // Show card in UI
                playedCard = selectedCard.ToString();
                if (playedCardLabel != null)
                    playedCardLabel.Text = $"Played Card: {playedCard}";
                if (playedCardPictureBox != null)
                {
                    if (playedCard.StartsWith("Bell"))
                        playedCardPictureBox.ImageLocation = bellPicturePath;
                    else if (playedCard.StartsWith("Key"))
                        playedCardPictureBox.ImageLocation = keyPicturePath;
                    else if (playedCard.StartsWith("Moon"))
                        playedCardPictureBox.ImageLocation = moonPicturePath;
                    else
                        playedCardPictureBox.Image = null;
                    playedCardPictureBox.Update();
                }

                // --- Special Card Effects ---
                // Swan (1): If you lose the fight, you start the next one
                bool swanPlayed = selectedCard.Value == 1;
                // Fox (3): Swap decree with a card from hand
                bool foxPlayed = selectedCard.Value == 3;
                // Woodcutter (5): Draw a card, then discard one
                bool woodcutterPlayed = selectedCard.Value == 5;
                // Treasure (7): Track for end-of-round bonus
                bool treasurePlayed = selectedCard.Value == 7;
                // Witch (9): If only one witch, it acts as decree suit
                bool witchPlayed = selectedCard.Value == 9;
                // King (11): Opponent must play 1 or highest of that suit (not enforced here, just info)
                bool kingPlayed = selectedCard.Value == 11;

                // Track 7s played for end-of-round bonus
                if (treasurePlayed)
                {
                    if (currentPlayer == 1)
                        sevensPlayedByPlayer[0]++;
                    else
                        sevensPlayedByPlayer[1]++;
                }

                // Fox: Allow decree swap
                if (foxPlayed && hand.Count > 0)
                {
                    // Let player swap decree with a card from their hand
                    var result = MessageBox.Show("Do you want to swap the decree card with a card from your hand?", "Fox Effect", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        // Let player pick a card to swap
                        using (var selectForm = new SelectCardForm(hand, "Select a card to become the new decree."))
                        {
                            if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedCard != null)
                            {
                                // Swap decree
                                Card oldDecree = null;
                                if (!string.IsNullOrEmpty(currentDecree))
                                {
                                    var parts = currentDecree.Split(' ');
                                    if (parts.Length == 2 && Enum.TryParse<CardSuit>(parts[0], out var suit) && int.TryParse(parts[1], out var val))
                                        oldDecree = new Card(suit, val);
                                }
                                currentDecree = selectForm.SelectedCard.ToString();
                                hand.Remove(selectForm.SelectedCard);
                                if (oldDecree != null)
                                    hand.Add(oldDecree);
                                DisplayHand(hand, player1ListBox);
                                MessageBox.Show($"Decree swapped! New decree: {currentDecree}", "Fox Effect");
                            }
                        }
                    }
                }

                // Woodcutter: Draw a card, then discard one
                if (woodcutterPlayed && drawPile.Count > 0)
                {
                    Card drawn = drawPile[0];
                    drawPile.RemoveAt(0);
                    hand.Add(drawn);
                    DisplayHand(hand, player1ListBox);
                    MessageBox.Show($"Woodcutter: You drew {drawn}. Now select a card to discard.", "Woodcutter Effect");
                    using (var selectForm = new SelectCardForm(hand, "Select a card to discard."))
                    {
                        if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedCard != null)
                        {
                            hand.Remove(selectForm.SelectedCard);
                            DisplayHand(hand, player1ListBox);
                        }
                    }
                }

                // Fight logic: if this is the first card of the fight, store it and swap
                if (pendingFightCard == null)
                {
                    int nextPlayer = (currentPlayer == 1) ? 2 : 1;
                    this.Hide();
                    ShowSwapPlayerScreenAndSwitch(nextPlayer, selectedCard, currentPlayer);
                }
                else
                {
                    // Second card played, resolve fight
                    Card card1 = pendingFightPlayer == 1 ? pendingFightCard : selectedCard;
                    Card card2 = pendingFightPlayer == 1 ? selectedCard : pendingFightCard;
                    int player1 = pendingFightPlayer == 1 ? 1 : 2;
                    int player2 = (player1 == 1) ? 2 : 1;

                    // --- Witch effect: If only one witch, it acts as decree suit ---
                    bool card1Witch = card1.Value == 9;
                    bool card2Witch = card2.Value == 9;
                    CardSuit decreeSuit = Enum.TryParse<CardSuit>(currentDecree.Split(' ')[0], out var suit) ? suit : card1.Suit;
                    bool card1Decree = card1.Suit == decreeSuit;
                    bool card2Decree = card2.Suit == decreeSuit;
                    if (card1Witch ^ card2Witch) // Only one witch
                    {
                        if (card1Witch) card1Decree = true;
                        if (card2Witch) card2Decree = true;
                    }

                    // --- Fight winner logic ---
                    int winnerPlayer;
                    if (card1Decree && card2Decree)
                        winnerPlayer = (card1.Value >= card2.Value) ? player1 : player2;
                    else if (card1Decree)
                        winnerPlayer = player1;
                    else if (card2Decree)
                        winnerPlayer = player2;
                    else
                        winnerPlayer = (card1.Value >= card2.Value) ? player1 : player2;

                    if (winnerPlayer == 1)
                        player1FightPoints++;
                    else
                        player2FightPoints++;

                    // --- Swan effect: If loser played Swan, they start next fight ---
                    int nextFightStarter = winnerPlayer;
                    if (card1.Value == 1 && winnerPlayer != player1)
                        nextFightStarter = player1;
                    else if (card2.Value == 1 && winnerPlayer != player2)
                        nextFightStarter = player2;

                    // Update fight points label
                    if (fightPointsLabel != null)
                        fightPointsLabel.Text = $"Fight Points: {player1FightPoints} - {player2FightPoints}";

                    // Show message BEFORE hiding the form
                    MessageBox.Show($"Player {winnerPlayer} wins the fight!\n{card1} vs {card2}", "Fight Result");

                    // Reset for next fight
                    pendingFightCard = null;
                    pendingFightPlayer = null;
                    playedCard = "";
                    if (playedCardLabel != null) playedCardLabel.Text = "No Card Played";
                    if (playedCardPictureBox != null) { playedCardPictureBox.Image = null; playedCardPictureBox.Update(); }

                    // Check for end of round
                    if (player1Hand.Count == 0 && player2Hand.Count == 0)
                    {
                        // Calculate round points using the table
                        int p1Fights = player1FightPoints;
                        int p2Fights = player2FightPoints;
                        int p1Points = GetPointsForFights(p1Fights);
                        int p2Points = GetPointsForFights(p2Fights);

                        // Add treasure (7) bonus to the player who won the round
                        int p1Sevens = sevensPlayedByPlayer[0];
                        int p2Sevens = sevensPlayedByPlayer[1];
                        int bonus = p1Sevens + p2Sevens;
                        string treasureMsg = "";
                        if (p1Points > p2Points && bonus > 0)
                        {
                            player1Points += bonus;
                            treasureMsg = $"\nTreasure: Player 1 gets {bonus} bonus point(s) for all 7s played this round!";
                        }
                        else if (p2Points > p1Points && bonus > 0)
                        {
                            player2Points += bonus;
                            treasureMsg = $"\nTreasure: Player 2 gets {bonus} bonus point(s) for all 7s played this round!";
                        }
                        // If tie, no one gets bonus

                        player1Points += p1Points;
                        player2Points += p2Points;

                        string roundWinnerMsg = "";
                        if (p1Points > p2Points)
                            roundWinnerMsg = "Player 1 wins the round!";
                        else if (p2Points > p1Points)
                            roundWinnerMsg = "Player 2 wins the round!";
                        else
                            roundWinnerMsg = "The round is a tie!";

                        string msg = $"Round over!\nPlayer 1 fights: {p1Fights} (Points: {p1Points})\nPlayer 2 fights: {p2Fights} (Points: {p2Points})\nTotal: {player1Points} - {player2Points}{treasureMsg}\n{roundWinnerMsg}";
                        MessageBox.Show(msg, "Round Result");

                        // Check for game end
                        bool player1Wins = player1Points >= winThreshold && player1Points > player2Points;
                        bool player2Wins = player2Points >= winThreshold && player2Points > player1Points;
                        if (player1Wins || player2Wins)
                        {
                            string winnerMsg = player1Wins ? "Player 1 wins the game!" : "Player 2 wins the game!";
                            MessageBox.Show(winnerMsg + $"\nFinal Score: {player1Points} - {player2Points}", "Game Over");
                            Application.Exit();
                            return;
                        }

                        // Reset fight points and 7s for next round
                        player1FightPoints = 0;
                        player2FightPoints = 0;
                        sevensPlayedByPlayer[0] = 0;
                        sevensPlayedByPlayer[1] = 0;

                        // Winner of the round starts next round (if tie, player 1 starts)
                        int nextRoundStarter = 1;
                        if (p2Points > p1Points) nextRoundStarter = 2;

                        var nextForm = new GameForm(
                            winThreshold,
                            nextRoundStarter,
                            null,
                            null,
                            null,
                            player1Points,
                            player2Points,
                            0,
                            0,
                            null
                        );
                        this.Hide();
                        nextForm.ShowDialog();
                        this.Close();
                        return;
                    }
                    // Next fight starts with winner (or Swan loser if played)
                    this.Hide();
                    ShowSwapPlayerScreenAndSwitch(nextFightStarter, null, null);
                }
            }
        }
        // Returns points for a given number of fights won, based on the table
        private int GetPointsForFights(int fights)
        {
            if (fights <= 3) return 6;
            if (fights == 4) return 1;
            if (fights == 5) return 2;
            if (fights == 6) return 3;
            if (fights >= 7 && fights <= 9) return 6;
            if (fights >= 10 && fights <= 13) return 0;
            return 0;
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
