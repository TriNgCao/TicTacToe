using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CaroGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Player, ImageSource> images = new()
        {
            { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
        {
            { Player.X, new ObjectAnimationUsingKeyFrames() },
            { Player.O, new ObjectAnimationUsingKeyFrames() }
        };

        private readonly DoubleAnimation fadeOutAnim = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 1,
            To = 0
        };

        private readonly DoubleAnimation fadeInAnim = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 0,
            To = 1
        };

        private readonly Image[,] imageControls = new Image[3, 3];
        private readonly GameState gameState = new GameState();
        public int Mode { get; set; }
        public int Role { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();
            SetupAnimations();

            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameReset += OnGameReset;
        }
        private void SetupAnimations()
        {
            animations[Player.X].Duration = TimeSpan.FromSeconds(0.25);
            animations[Player.O].Duration = TimeSpan.FromSeconds(0.25);

            for (int i = 0; i < 16; i++)
            {
                Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
                BitmapImage xImg = new BitmapImage(xUri);
                DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
                animations[Player.X].KeyFrames.Add(xKeyFrame);

                Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
                BitmapImage oImg = new BitmapImage(oUri);
                DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
                animations[Player.O].KeyFrames.Add(oKeyFrame);
            }
        }

        private void SetupGameGrid()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Image imageControl = new Image();
                    GameGrid.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }
        }

        private void OnMoveMade(int r, int c)
        {
            Player player = gameState.GameGrid[r, c];
            imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
            PlayerImage.Source = images[gameState.CurrentPlayer];
        }

        private async void OnGameReset()
        {
            PlayerImage.Source = images[gameState.CurrentPlayer];

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                }
            }

            await TransitionToGameScreen();
        }

        private async void OnGameEnded(GameResult result)
        {
            if (result.Winner == Player.None)
            {
                await Task.Delay(1000);
                await TransitionToEndScreen("It's a tie!", null);
            }
            else
            {
                await Task.Delay(1000);
                await ShowLine(result.WinInfo);
                await Task.Delay(1000);
                await TransitionToEndScreen("Winner:", images[result.Winner]);
            }
        }

        private (Point, Point) FindLinePoints(WinInfo win)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize / 2;

            if (win.Type == WinType.Row)
            {
                double y = margin + win.Number * squareSize;
                return (new Point(0, y), new Point(GameGrid.Width, y));
            }
            else if (win.Type == WinType.Col)
            {
                double x = margin + win.Number * squareSize;
                return (new Point(x, 0), new Point(x, GameGrid.Height));
            }
            else if (win.Type == WinType.Diagonal)
            {
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
            }

            return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }

        private async Task ShowLine(WinInfo win)
        {
            (Point start, Point end) = FindLinePoints(win);

            Line.Visibility = Visibility.Visible;
            Line.X1 = start.X;
            Line.Y1 = start.Y;

            DoubleAnimation x2Anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.25),
                From = start.X,
                To = end.X,
            };

            DoubleAnimation y2Anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.25),
                From = start.Y,
                To = end.Y,
            };

            Line.BeginAnimation(Line.X2Property, x2Anim);
            Line.BeginAnimation(Line.Y2Property, y2Anim);
            await Task.Delay(250);
        }

        private async void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(GameGrid);
            double squareSize = GameGrid.Width / 3;
            int r = (int)(clickPosition.Y / squareSize);
            int c = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(r, c);
            if (Mode == 1)
            {
                await Task.Delay(1000);
                gameState.MakeComputerMove();
            }
        }

        private void PlayAgainClick(object sender, RoutedEventArgs e)
        {
            if (gameState.GameOver)
            {
                gameState.Reset();
            }
        }

        private async Task FadeIn(UIElement elem)
        {
            elem.Visibility = Visibility.Visible;
            elem.BeginAnimation(OpacityProperty, fadeInAnim);
            await Task.Delay(fadeOutAnim.Duration.TimeSpan);
        }

        private async Task FadeOut(UIElement elem)
        {
            elem.BeginAnimation(OpacityProperty, fadeOutAnim);
            await Task.Delay(fadeOutAnim.Duration.TimeSpan);
            elem.Visibility = Visibility.Hidden;
        }

        private async Task TransitionToEndScreen(string text, ImageSource winImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source = winImage;
            await FadeIn(EndScreen);
        }

        private async Task TransitionToGameScreen()
        {
            Line.Visibility = Visibility.Hidden;
            await FadeOut(EndScreen);
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Mode == 1 && Role == 2)
            {
                await Task.Delay(500);
                gameState.MakeComputerMove();
            }
        }
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ModeWindow m = new();
            this.Hide();
            m.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}