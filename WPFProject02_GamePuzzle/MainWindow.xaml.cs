using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFProject02_GamePuzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //class for game - controling
        class Game
        {
            public int timeOfRound { get; set; }
            public int userName { get; set; }
            public int numberOfRounds { get; set; } //de sau :)))
            public int timeScramble { get; set; }
            public Tuple<int,int> blankPos { get; set; }
        }

        //class to control constants of details in game
        class GameConfigs
        {
            public static int numberOfRows { get; set; }
            public static int numberOfColumns { get; set; }

            //Constant for specifications
            public const int topOffset = 40;
            public const int leftOffset = 40;
            public const int startX = 30;
            public const int startY = 30;
            public const int widthOfCell = 50;
            public const int heightOfCell = 50;
            public const int widthOfImage = 40;
            public const int heightOfImage = 40;
            public const string fileSave = "saveGame.txt";
        }

        //open dialog to get number of cells split the image
        private void getSplit()
        {
            var screen = new BootScreen();
            if (screen.ShowDialog() == true)
            {
                GameConfigs.numberOfColumns = screen.userChoice;
                GameConfigs.numberOfRows = screen.userChoice;
            }
        }

        //model init
        int[,] _matrix; //matrix of images initialized
        Image[,] _image; //references to from model to UI
        Game _game; //New game initialized

        private void drawLine()
        {
            /*UI*/
            //Draw column
            for (int i = 0; i < GameConfigs.numberOfRows + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Aqua);
                canvasUI.Children.Add(line);

                line.X1 = GameConfigs.startX + i * GameConfigs.widthOfCell;
                line.Y1 = GameConfigs.startY;

                line.X2 = GameConfigs.startX + i * GameConfigs.widthOfCell;
                line.Y2 = GameConfigs.startY + (GameConfigs.numberOfColumns) * GameConfigs.heightOfCell;
            }

            //Draw row
            for (int i = 0; i < GameConfigs.numberOfColumns + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Aqua);
                canvasUI.Children.Add(line);

                line.X1 = GameConfigs.startX;
                line.Y1 = GameConfigs.startY + i * GameConfigs.heightOfCell;

                line.X2 = GameConfigs.startX + (GameConfigs.numberOfRows) * GameConfigs.widthOfCell;
                line.Y2 = GameConfigs.startY + i * GameConfigs.heightOfCell;
            }

        }

        private void loadImage()
        {

            for (int i = 0; i < GameConfigs.numberOfRows; i++)
                for (int j = 0; j < GameConfigs.numberOfColumns; j++)
                {
                    if (_matrix[i,j]!=0)
                    {
                        int step = GameConfigs.numberOfColumns;
                        string tmpImageName = $"DefaultImages/number{_matrix[i,j]}.png";
                        var img = new Image();
                        img.Width = GameConfigs.widthOfImage;
                        img.Height = GameConfigs.heightOfImage;
                        img.Source = new BitmapImage(new Uri(tmpImageName, UriKind.Relative));
                        canvasUI.Children.Add(img);

                        Canvas.SetLeft(img, GameConfigs.startX + j * GameConfigs.widthOfCell + (GameConfigs.widthOfCell - GameConfigs.widthOfImage) / 2);
                        Canvas.SetTop(img, GameConfigs.startY + i * GameConfigs.heightOfCell + (GameConfigs.heightOfCell - GameConfigs.heightOfImage) / 2);
                    }
                }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //get number of cell to split image
            getSplit();

            //Model
            _matrix = new int[GameConfigs.numberOfRows, GameConfigs.numberOfColumns];
            _image = new Image[GameConfigs.numberOfRows, GameConfigs.numberOfColumns];

            drawLine();
            setupMatrix();
            printToDebug();
            scambleMatrix();
            loadImage();
        }

        /*Game prepare*/
        private void setupMatrix()
        {
            for (int i = 0; i < GameConfigs.numberOfRows; i++)
                for (int j = 0; j < GameConfigs.numberOfColumns; j++)
                    _matrix[i, j] = i*GameConfigs.numberOfRows + j + 1;
            _matrix[GameConfigs.numberOfRows - 1, GameConfigs.numberOfColumns - 1] = 0;
        }

        private void swapVal(Tuple<int,int>a, Tuple<int,int>b)
        {
            var tmp = _matrix[a.Item1, a.Item2];
            _matrix[a.Item1, a.Item2] = _matrix[b.Item1, b.Item2];
            _matrix[b.Item1, b.Item2] = tmp;
        }

        private void scambleMatrix()
        {
            Random random = new Random();
            var tmpPos = Tuple.Create(GameConfigs.numberOfRows - 1, GameConfigs.numberOfColumns - 1);

            for (int i = 0; i < 20; i++)
            {

                //random generate to select next positon to swap
                int nextX = 0, nextY = 0;

                do
                {
                    nextX = random.Next(3) - 1;
                    nextY = random.Next(3) - 1;
                }
                while (nextX < 0 || nextY < 0 || nextX > GameConfigs.numberOfRows - 1 || nextY > GameConfigs.numberOfColumns - 1 || (nextX == tmpPos.Item1) && nextY == tmpPos.Item2);

                swapVal(tmpPos, Tuple.Create(nextX, nextY));
                tmpPos = Tuple.Create(nextX, nextY);
            }

           // _game.blankPos = tmpPos;
            printToDebug();
        }

        private void printToDebug()
        {
            for (int i = 0; i < GameConfigs.numberOfColumns; i++)
            {
                for (int j = 0; j < GameConfigs.numberOfColumns; j++)
                    Debug.Write(_matrix[i, j]);
                Debug.WriteLine("");
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = ((int)position.Y - GameConfigs.startY) / GameConfigs.heightOfCell;
            int j = ((int)position.X - GameConfigs.startX) / GameConfigs.widthOfCell;

            this.Title = $"{position.X} - {position.Y}, a[{i}][{j}]";

        }
    }
}
