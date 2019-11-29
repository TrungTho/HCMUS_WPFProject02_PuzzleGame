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
            public string userName { get; set; }
            public int numberOfRounds { get; set; } //de sau :)))
            public int numberOfScrambles { get; set; }
            public Tuple<int,int> blankPos { get; set; }
            public bool isDragging { get; set; }
            public Image selectedBitmap { get; set; }
            public Point lastPosition { get; set; }
            public Point newBlankPosition { get; set; }

            /// <summary>
            /// method to init new game before start, notice param numberOfScramble
            /// </summary>
            /// <param name="x"></param> x - coordinate of blankpos (last cell)
            /// <param name="y"></param> y - coordinate of blankpos (last cell)
            public void InitGame(int x, int y)
            {
                timeOfRound = 0;
                userName = "";
                numberOfRounds = 0;
                numberOfScrambles = 2; //just for testing
                blankPos = Tuple.Create(x, y);
                isDragging = false;
                selectedBitmap = null;
                //lastPosition;
                //newBlankPos;
            }

            public bool isFinish(int[,] _matrix)
            {
                for (int i = 0; i < UIView.numberOfRows; i++)
                {
                    for (int j = 0; j < UIView.numberOfColumns; j++)
                    {
                        if (i != UIView.numberOfRows - 1 || j != UIView.numberOfColumns - 1) 
                        {
                            if (_matrix[i, j] != i * UIView.numberOfColumns + j + 1)
                                return false;
                        }
                    }
                }
                return true;
            }
        }

        //class to control constants of details in game
        public class UIView
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

        /// <summary>
        /// open dialog to get number of cells to split the image
        /// </summary>
        private void getSplit()
        {
            var screen = new BootScreen();
            if (screen.ShowDialog() == true)
            {
                UIView.numberOfColumns = screen.userChoice;
                UIView.numberOfRows = screen.userChoice;
            }
        }

        /// <summary>
        /// function to return coordinate in matrix of mouse's cursor
        /// </summary>
        /// <param name="position"></param> Point value
        /// <param name="i"></param> in - 0, out - coordinate in matrix
        /// <param name="j"></param> in - 0, out - coordinate in matrix
        private void getPos(Point position,ref int i, ref int j)
        {
            i = ((int)position.Y - UIView.startY) / UIView.heightOfCell;
            j = ((int)position.X - UIView.startX) / UIView.widthOfCell;
        }

        //model init
        int[,] _matrix; //matrix of images initialized
        Image[,] _image; //references to from model to UI
        Game _game; //New game initialized

        /// <summary>
        /// draw aqua line to separate cells
        /// </summary>
        private void drawLine()
        {
            /*UI*/
            //Draw column
            for (int i = 0; i < UIView.numberOfRows + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Aqua);
                canvasUI.Children.Add(line);

                line.X1 = UIView.startX + i * UIView.widthOfCell;
                line.Y1 = UIView.startY;

                line.X2 = UIView.startX + i * UIView.widthOfCell;
                line.Y2 = UIView.startY + (UIView.numberOfColumns) * UIView.heightOfCell;
            }

            //Draw row
            for (int i = 0; i < UIView.numberOfColumns + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Aqua);
                canvasUI.Children.Add(line);

                line.X1 = UIView.startX;
                line.Y1 = UIView.startY + i * UIView.heightOfCell;

                line.X2 = UIView.startX + (UIView.numberOfRows) * UIView.widthOfCell;
                line.Y2 = UIView.startY + i * UIView.heightOfCell;
            }

        }

        /// <summary>
        /// load image from resources to UI
        /// </summary>
        private void loadImage()
        {

            for (int i = 0; i < UIView.numberOfRows; i++)
                for (int j = 0; j < UIView.numberOfColumns; j++)
                {
                    if (_matrix[i,j]!=0)
                    {
                        int step = UIView.numberOfColumns;
                        string tmpImageName = $"DefaultImages/number{_matrix[i,j]}.png";
                        var img = new Image();
                        img.Width = UIView.widthOfImage;
                        img.Height = UIView.heightOfImage;
                        img.Source = new BitmapImage(new Uri(tmpImageName, UriKind.Relative));
                        canvasUI.Children.Add(img);

                        Canvas.SetLeft(img, UIView.startX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        Canvas.SetTop(img, UIView.startY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                        img.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
                        img.Tag = new Tuple<int, int>(i, j);
                    }
                }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //get number of cell to split image
            getSplit();

            //Model
            _matrix = new int[UIView.numberOfRows, UIView.numberOfColumns];
            _image = new Image[UIView.numberOfRows, UIView.numberOfColumns];
            _game = new Game();
            _game.InitGame(UIView.numberOfRows-1, UIView.numberOfColumns-1);


            drawLine();
            setupMatrix();
            printToDebug();
            scambleMatrix();
            loadImage();
        }

        /*Game prepare*/
        private void setupMatrix()
        {
            for (int i = 0; i < UIView.numberOfRows; i++)
                for (int j = 0; j < UIView.numberOfColumns; j++)
                    _matrix[i, j] = i*UIView.numberOfRows + j + 1;
            _matrix[UIView.numberOfRows - 1, UIView.numberOfColumns - 1] = 0;
        }
        /// <summary>
        /// function to swap 2 pos in _matrix
        /// </summary>
        /// <param name="a"></param> - first pos
        /// <param name="b"></param> - second pos
        private void swapVal(Tuple<int,int>a, Tuple<int,int>b)
        {
            var tmp = _matrix[a.Item1, a.Item2];
            _matrix[a.Item1, a.Item2] = _matrix[b.Item1, b.Item2];
            _matrix[b.Item1, b.Item2] = tmp;
        }

        /// <summary>
        /// funtion to scamble the both model _matrix and UI Image
        /// </summary>
        private void scambleMatrix()
        {
            Random random = new Random();
            var tmpPos = Tuple.Create(UIView.numberOfRows - 1, UIView.numberOfColumns - 1);

            for (int i = 0; i < _game.numberOfScrambles; i++)
            {

                //random generate to select next positon to swap
                int nextX = 0, nextY = 0;

                do
                {
                    nextX = random.Next(3) - 1;
                    nextY = random.Next(3) - 1;
                }
                while (nextX < 0 || nextY < 0 || nextX > UIView.numberOfRows - 1 || nextY > UIView.numberOfColumns - 1 || (nextX == tmpPos.Item1) && nextY == tmpPos.Item2);

                swapVal(tmpPos, Tuple.Create(nextX, nextY));
                tmpPos = Tuple.Create(nextX, nextY);
            }

            _game.blankPos = tmpPos;
            printToDebug();
        }

        /*BUS*/
        private void printToDebug()
        {
            Debug.WriteLine("----------");

            for (int i = 0; i < UIView.numberOfColumns; i++)
            {
                for (int j = 0; j < UIView.numberOfColumns; j++)
                    Debug.Write(_matrix[i, j]);
                Debug.WriteLine("");
            }

            Debug.WriteLine("----------");
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = 0, j = 0;
            getPos(position, ref i, ref j);

            this.Title = $"{position.X} - {position.Y}, a[{i}][{j}] , blank: [{_game.blankPos.Item1}],[{_game.blankPos.Item2}]";

            if (_game.isDragging) 
            {
                var dx = position.X - _game.lastPosition.X;
                var dy = position.Y - _game.lastPosition.Y;

                var lastLeft = Canvas.GetLeft(_game.selectedBitmap);
                var lastTop = Canvas.GetTop(_game.selectedBitmap);
                Canvas.SetLeft(_game.selectedBitmap, lastLeft + dx);
                Canvas.SetTop(_game.selectedBitmap, lastTop + dy);

                _game.lastPosition = position;
            }
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _game.isDragging = false;
            var position = e.GetPosition(this);

            int x = 0, y = 0;
            getPos(position, ref x, ref y);

            Canvas.SetLeft(_game.selectedBitmap, UIView.startX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
            Canvas.SetTop(_game.selectedBitmap, UIView.startY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

            //code cua thay
            //var image = sender as Image;
            //var tuple = image.Tag as Tuple<int, int>;
            //int i = tuple.Item1; 
            //int j = tuple.Item2;

            int oldX = 0, oldY = 0;
            getPos(_game.newBlankPosition, ref oldX, ref oldY);

            swapVal(_game.blankPos, Tuple.Create(oldX, oldY));
            _game.blankPos = Tuple.Create(oldX, oldY);

            printToDebug();

            if (_game.isFinish(_matrix))
            {
                MessageBox.Show("Win!!!");
            }

            //MessageBox.Show($"{i} - {j}");
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _game.isDragging = true;
            _game.selectedBitmap = sender as Image;
            _game.lastPosition = e.GetPosition(this);
            _game.newBlankPosition = e.GetPosition(this);
        }
    }
}
