using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media; // Stopwatch

namespace MemoryGame
{
    public partial class MainWindow : Window
    {
        private int _rows = 6;
        private int _columns = 6;
        private List<string> _imagePaths;
        private Button _firstCard, _secondCard;
        private int _matchedPairs = 0;
        private Stopwatch _stopwatch; // Stopwatch objektum

        public MainWindow()
        {
            InitializeComponent();
            LoadImagePaths();
            InitializeGame();
        }

        private void LoadImagePaths()
        {
            _imagePaths = new List<string>();
            for (int i = 1; i <= 8; i++)
            {
                _imagePaths.Add($"Images/image{i}.png");
            }
        }

        private void InitializeGame()
        {
            int totalCards = _rows * _columns;

            if (_imagePaths.Count < totalCards / 2)
            {
                var missingImagesCount = (totalCards / 2) - _imagePaths.Count;
                for (int i = 0; i < missingImagesCount; i++)
                {
                    _imagePaths.Add(_imagePaths[i % _imagePaths.Count]);
                }
            }

            var selectedImages = _imagePaths.Take(totalCards / 2).ToList();
            var cardImages = selectedImages.Concat(selectedImages).OrderBy(_ => Guid.NewGuid()).ToList();

            CardGrid.Children.Clear();
            CardGrid.RowDefinitions.Clear();
            CardGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < _rows; i++)
                CardGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < _columns; i++)
                CardGrid.ColumnDefinitions.Add(new ColumnDefinition());

            foreach (var imagePath in cardImages)
            {
                var cardButton = new Button
                {
                    Tag = imagePath
                };
                cardButton.Click += CardButton_Click;
                SetCardImage(cardButton, new BitmapImage(new Uri("Images/cover.png", UriKind.Relative)));

                Grid.SetRow(cardButton, CardGrid.Children.Count / _columns);
                Grid.SetColumn(cardButton, CardGrid.Children.Count % _columns);
                CardGrid.Children.Add(cardButton);
            }

            _matchedPairs = 0;

            // Indítjuk az időmérőt a játék kezdetén
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        private void StartNewGame_Click(object sender, RoutedEventArgs e)
        {
            if (SizeSelector.SelectedItem is ComboBoxItem selectedItem && int.TryParse(selectedItem.Tag.ToString(), out int gridSize))
            {
                _rows = gridSize;
                _columns = gridSize;
                InitializeGame();
            }
            else
            {
                MessageBox.Show("Hiba: Érvénytelen méret!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_firstCard != null && _secondCard != null)
                return;

            var clickedCard = sender as Button;
            if (clickedCard == null || clickedCard.Content is Image { Source: BitmapImage image } && image.UriSource.OriginalString != "Images/cover.png")
                return;

            SetCardImage(clickedCard, new BitmapImage(new Uri(clickedCard.Tag.ToString(), UriKind.Relative)));

            if (_firstCard == null)
            {
                _firstCard = clickedCard;
            }
            else
            {
                _secondCard = clickedCard;

                if (_firstCard.Tag.ToString() == _secondCard.Tag.ToString())
                {
                    _matchedPairs++;
                    _firstCard = null;
                    _secondCard = null;

                    if (_matchedPairs == (_rows * _columns) / 2)
                    {
                        // Megállítjuk az időzítőt, amikor a játékot befejezik
                        _stopwatch.Stop();
                        var elapsedTime = _stopwatch.Elapsed;
                        MessageBox.Show($"Gratulálok, nyertél!\nA játékidő: {elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}",
                            "Gratulálunk!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    timer.Tick += (_, _) =>
                    {
                        SetCardImage(_firstCard, new BitmapImage(new Uri("Images/cover.png", UriKind.Relative)));
                        SetCardImage(_secondCard, new BitmapImage(new Uri("Images/cover.png", UriKind.Relative)));
                        _firstCard = null;
                        _secondCard = null;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }

        private void SetCardImage(Button cardButton, BitmapImage image)
        {
            cardButton.Content = new Image
            {
                Source = image,
                Stretch = Stretch.UniformToFill
            };
        }
    }
}
