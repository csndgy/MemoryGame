using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MemoryGame
{
    public partial class MainWindow : Window
    {
        private List<string> _imagePaths;
        private Button _firstCard, _secondCard;
        private DispatcherTimer _flipBackTimer;
        private DispatcherTimer _gameTimer;
        private int _timeElapsed;
        private BitmapImage _coverImage;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Képek elérési útjainak megadása
            _imagePaths = new List<string>
    {
        "Images/image1.png", "Images/image1.png",
        "Images/image2.png", "Images/image2.png",
        "Images/image3.png", "Images/image3.png",
        "Images/image4.png", "Images/image4.png",
        "Images/image5.png", "Images/image5.png",
        "Images/image6.png", "Images/image6.png",
        "Images/image7.png", "Images/image7.png",
        "Images/image8.png", "Images/image8.png"
    };
            _imagePaths = _imagePaths.OrderBy(x => Guid.NewGuid()).ToList();

            // Borítókép betöltése
            _coverImage = new BitmapImage(new Uri("pack://application:,,,/Images/cover.png"));

            // Táblázat feltöltése képekkel
            CardGrid.Children.Clear();
            foreach (var imagePath in _imagePaths)
            {
                Button cardButton = new Button
                {
                    Tag = imagePath,
                    Style = (Style)FindResource("CardStyle")
                };

                // Beállítjuk a borítóképet minden kártyánál alapértelmezettként
                SetCardImage(cardButton, _coverImage);

                cardButton.Click += CardButton_Click;
                CardGrid.Children.Add(cardButton);
            }

            // Időzítők beállítása
            _flipBackTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _flipBackTimer.Tick += FlipBackTimer_Tick;

            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _gameTimer.Tick += GameTimer_Tick;
            _timeElapsed = 0;
            TimerText.Text = "Eltelt idő: 0 mp";
            _gameTimer.Start();

            GameEndMessage.Visibility = Visibility.Collapsed;
            _firstCard = null;
            _secondCard = null;
        }


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            _timeElapsed++;
            TimerText.Text = $"Eltelt idő: {_timeElapsed} mp";
        }

        private void FlipBackTimer_Tick(object sender, EventArgs e)
        {
            _flipBackTimer.Stop();
            SetCardImage(_firstCard, _coverImage);
            SetCardImage(_secondCard, _coverImage);
            _firstCard = null;
            _secondCard = null;
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_flipBackTimer.IsEnabled) return;
            Button clickedCard = sender as Button;
            if (clickedCard == null || clickedCard == _firstCard) return;

            var imagePath = clickedCard.Tag.ToString();
           var cardImage = new BitmapImage(new Uri($"pack://application:,,,/{imagePath}"));

            SetCardImage(clickedCard, cardImage);

            if (_firstCard == null)
            {
                _firstCard = clickedCard;
            }
            else
            {
                _secondCard = clickedCard;
                if (_firstCard.Tag.ToString() == _secondCard.Tag.ToString())
                {
                    _firstCard.IsEnabled = false;
                    _secondCard.IsEnabled = false;
                    _firstCard = null;
                    _secondCard = null;
                    CheckForGameEnd();
                }
                else
                {
                    _flipBackTimer.Start();
                }
            }
        }
        private void SetCardImage(Button card, BitmapImage image)
        {
            card.Content = new Image
            {
                Source = image,
                Stretch = System.Windows.Media.Stretch.Fill
            };
        }


        private void CheckForGameEnd()
        {
            if (CardGrid.Children.Cast<Button>().All(b => !b.IsEnabled))
            {
                _gameTimer.Stop();
                GameEndMessage.Text = $"Gratulálunk, megtaláltad az összes párt! Idő: {_timeElapsed} mp";
                GameEndMessage.Visibility = Visibility.Visible;
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }
    }
}
