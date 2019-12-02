﻿using System;
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
using System.Windows.Threading;

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

        DispatcherTimer _countdownTimer;
        private int _time;

        //class for game - controling
        class Game
        {
            //public int timeOfRound { get; set; }
            public string userName { get; set; }
            //public int numberOfRounds { get; set; } //de sau :)))
            public int numberOfScrambles { get; set; }
            public Tuple<int,int> blankPos { get; set; }
            public bool isDragging { get; set; }
            public Image selectedBitmap { get; set; }
            public Point lastPosition { get; set; }
            public Point lastSelectedPosition { get; set; }
            public int maxTime { get; set; } //minutes

            /// <summary>
            /// method to init new game before start, notice param numberOfScramble
            /// </summary>
            /// <param name="x"></param> x - coordinate of blankpos (last cell)
            /// <param name="y"></param> y - coordinate of blankpos (last cell)
            public void InitGame(int x, int y)
            {
                //timeOfRound = 0;
                userName = "";
                //numberOfRounds = 0;
                numberOfScrambles = 20; //just for testing
                blankPos = Tuple.Create(x, y);
                isDragging = false;
                selectedBitmap = null;
                maxTime = 3;
                //lastPosition;
                //newBlankPos;
            }

            public void Won()
            {
                MessageBox.Show("Win!!!");
            }

            public void Lose()
            {
                MessageBox.Show("timeout!");
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
            //changeabel value
            public static int numberOfRows { get; set; }
            public static int numberOfColumns { get; set; }
            public static int cellsStartX { get; set; }
            public static int cellsStartY { get; set; }
            public static int previewImageStartX { get; set; }
            public static int previewImageStartY { get; set; }

            //Constant for specifications
            //public const int topOffset = 40;
            //public const int leftOffset = 40;
            public const int widthOfCell = 70;
            public const int heightOfCell = 70;
            public const int widthOfImage = 60;
            public const int heightOfImage = 60;
            public const int widthOfPreviewImage = 200;
            public const int heightOfPreviewImage = 200;
            public const string fileSave = "saveGame.txt";
        }

        private void startTimer()
        {
            _time = _game.maxTime * 60;//convert from minute to second
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = new TimeSpan(0, 0, 1);
            _countdownTimer.Tick += _countdownTimer_Tick;
            _countdownTimer.Start();
        }

        private void _countdownTimer_Tick(object sender, EventArgs e)
        {
            if (_time!=0)
            {
                _time--;
                textblockTimer.Text = string.Format($"0{_time / 60}:{_time % 60}");
            }
            else
            {
                _game.Lose();
                _countdownTimer.Stop();
            }
        }

        private string getMode()
        {
            var screen = new BootScreen();
            if (screen.ShowDialog() == true)
            {
                UIView.numberOfColumns = screen.userChoice;
                UIView.numberOfRows = screen.userChoice;
            }
            else
            {
                this.Close();
            }

            UIView.previewImageStartX = ((int)this.Width - UIView.widthOfPreviewImage) / 2;
            UIView.previewImageStartY = ((int)this.Height - UIView.heightOfPreviewImage - UIView.numberOfRows * UIView.heightOfCell) / 6;

            UIView.cellsStartX = ((int)this.Width - UIView.numberOfColumns * UIView.widthOfCell) / 2;
            UIView.cellsStartY = 2 * UIView.previewImageStartY + UIView.heightOfPreviewImage;

            //detect mode 
            string resMode = "";
            if (screen.isDefaultMode == false)
                resMode=screen.userImagePath;

            return resMode;
        }

        /// <summary>
        /// function to return coordinate in matrix of mouse's cursor
        /// </summary>
        /// <param name="position"></param> Point value
        /// <param name="i"></param> in - 0, out - coordinate in matrix
        /// <param name="j"></param> in - 0, out - coordinate in matrix
        private void getPos(Point position,ref int i, ref int j)
        {
            i = ((int)position.Y - UIView.cellsStartY) / UIView.heightOfCell;
            j = ((int)position.X - UIView.cellsStartX) / UIView.widthOfCell;
        }

        //model init
        int[,] _matrix; //matrix of images initialized
        Image[] _image; //references to from model to UI
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

                line.X1 = UIView.cellsStartX + i * UIView.widthOfCell;
                line.Y1 = UIView.cellsStartY;

                line.X2 = UIView.cellsStartX + i * UIView.widthOfCell;
                line.Y2 = UIView.cellsStartY + (UIView.numberOfColumns) * UIView.heightOfCell;
            }

            //Draw row
            for (int i = 0; i < UIView.numberOfColumns + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Aqua);
                canvasUI.Children.Add(line);

                line.X1 = UIView.cellsStartX;
                line.Y1 = UIView.cellsStartY + i * UIView.heightOfCell;

                line.X2 = UIView.cellsStartX + (UIView.numberOfRows) * UIView.widthOfCell;
                line.Y2 = UIView.cellsStartY + i * UIView.heightOfCell;
            }

        }

        /// <summary>
        /// load image from resources to UI
        /// </summary>
        private void loadDefaultImage()
        {
            //load preview image
            string tmpPreviewName = $"DefaultImages/preview{UIView.numberOfRows}.png";
            var previewImg = new Image();
            previewImg.Width = UIView.widthOfPreviewImage;
            previewImg.Height = UIView.heightOfPreviewImage;
            previewImg.Source = new BitmapImage(new Uri(tmpPreviewName, UriKind.Relative));
            canvasUI.Children.Add(previewImg);
            previewImg.Tag = Tuple.Create(-1, -1);

            Canvas.SetLeft(previewImg, UIView.previewImageStartX);
            Canvas.SetTop(previewImg, UIView.previewImageStartY);

            //load cell - images
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

                        Canvas.SetLeft(img, UIView.cellsStartX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        Canvas.SetTop(img, UIView.cellsStartY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                        img.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
                        //for debug tag of control
                        img.MouseRightButtonUp += Img_MouseRightButtonUp;

                        img.Tag = new Tuple<int, int>(i, j);
                    }
                }
        }

        /// <summary>
        /// load image from file seletion
        /// </summary>
        /// <param name="tmpPreviewName"></param> path to image user has chosen
        private void loadCustomImage(string tmpPreviewName)
        {
            //load preview image
            var source = new BitmapImage(new Uri(tmpPreviewName, UriKind.Absolute));

            //detect min dimesion
            int leng = (int)source.Width;
            if (source.Height < source.Width)
                leng = (int)source.Height;

            var previewImg = new Image();
            previewImg.Width = UIView.widthOfPreviewImage;
            previewImg.Height = UIView.heightOfPreviewImage;
            previewImg.Source = source; //full picture
            //previewImg.Source = new CroppedBitmap(source,new Int32Rect(0,0,leng,leng)); //crop picture
            canvasUI.Children.Add(previewImg);
            previewImg.Tag = Tuple.Create(-1, -1);

            Canvas.SetLeft(previewImg, UIView.previewImageStartX);
            Canvas.SetTop(previewImg, UIView.previewImageStartY);

            //crop image and store in model _image
            for (int i = 0; i < UIView.numberOfRows; i++)
            {
                for (int j = 0; j < UIView.numberOfColumns; j++)
                {
                    if ((i != UIView.numberOfRows - 1) || (j != UIView.numberOfColumns - 1))
                    {
                        var h = (int)leng / UIView.numberOfRows;
                        var w = (int)leng / UIView.numberOfColumns;
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        var cropBitmap = new CroppedBitmap(source, rect);

                        var img = new Image();
                        img.Stretch = Stretch.Fill;
                        img.Width = UIView.widthOfImage;
                        img.Height = UIView.heightOfImage;
                        img.Source = cropBitmap;
                        //canvasUI.Children.Add(img);

                        //Canvas.SetLeft(img, UIView.cellsStartX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        //Canvas.SetTop(img, UIView.cellsStartY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                        img.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
                        //for debug tag of control
                        img.MouseRightButtonUp += Img_MouseRightButtonUp;

                        _image[i * UIView.numberOfRows + 1 + j] = img;
                        //img.Tag = new Tuple<int, int>(i, j);
                    }
                }
            }

            //load image with scrambled _matrix
            for (int i = 0; i < UIView.numberOfRows; i++)
            {
                for (int j = 0; j < UIView.numberOfColumns; j++)
                {
                    if (_matrix[i, j] != 0) 
                    {
                        var img = _image[_matrix[i, j]];

                        canvasUI.Children.Add(img);
                        Canvas.SetLeft(img, UIView.cellsStartX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        Canvas.SetTop(img, UIView.cellsStartY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.Tag = new Tuple<int, int>(i, j);
                    }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //get number of cell to split image
            string isDefaultMode = getMode();

            //Model
            _matrix = new int[UIView.numberOfRows, UIView.numberOfColumns];
            _image = new Image[UIView.numberOfRows * UIView.numberOfColumns];
            _game = new Game();
            _game.InitGame(UIView.numberOfRows-1, UIView.numberOfColumns-1);

            //timer
            Canvas.SetLeft(textblockTimer, (this.Width - textblockTimer.Width) / 2);
            Canvas.SetTop(textblockTimer, 0);
            startTimer();
            
            //model
            setupMatrix();
            scambleMatrix();

            //UI
            drawLine();
            if (isDefaultMode == "")
            {
                loadDefaultImage();
            }
            else
            {
                loadCustomImage(isDefaultMode);
            }

            //debug section
            textblockForDebug.Text= this.Width.ToString();
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
                    //random step in 2 dimesion
                    do
                    {
                        nextX = random.Next(3) - 1;
                        nextY = random.Next(3) - 1;
                    }
                    while (Math.Abs(nextX) + Math.Abs(nextY) > 1);
                    
                    nextX += tmpPos.Item1;
                    nextY += tmpPos.Item2;
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

            if (x == _game.blankPos.Item1 && y == _game.blankPos.Item2)
            {
                //set new coordinate for selectedImage
                Canvas.SetLeft(_game.selectedBitmap, UIView.cellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                Canvas.SetTop(_game.selectedBitmap, UIView.cellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                //get last position of selectedImage
                int oldX = 0, oldY = 0;
                getPos(_game.lastSelectedPosition, ref oldX, ref oldY);

                //swap tag of selectedImage to blank cell that it just filled in
                _game.selectedBitmap.Tag = _game.blankPos;
                swapVal(_game.blankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                _game.blankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                //printToDebug();

                if (_game.isFinish(_matrix))
                {
                    _game.Won();
                }
            }
            else //return image to old position
            {
                //get last position of selectedImage
                getPos(_game.lastSelectedPosition, ref x, ref y);

                //set coordinate for selectedImage
                Canvas.SetLeft(_game.selectedBitmap, UIView.cellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                Canvas.SetTop(_game.selectedBitmap, UIView.cellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);
            }

            //MessageBox.Show($"{i} - {j}");
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _game.isDragging = true;
            _game.selectedBitmap = sender as Image;
            _game.lastPosition = e.GetPosition(this);
            _game.lastSelectedPosition = e.GetPosition(this);
        }

        private int findChild(int x, int y)
        {
            for (int i=0; i < canvasUI.Children.Count;i++)
            {
                if (canvasUI.Children[i] is Image)
                {
                    var child = canvasUI.Children[i] as Image;
                    var tag = child.Tag as Tuple<int, int>;
                    if (tag.Item1 == x && tag.Item2 == y)
                        return i;
                }
            }

            textblockForDebug.Text = "can't find image";
            return -1;
        }

        private void Img_KeyDown(object sender, KeyEventArgs e)
        {
            int nextX = _game.blankPos.Item1, nextY = _game.blankPos.Item2;

            switch (e.Key)
            {
                case Key.Up:
                    nextX -= 1;
                    break;
                case Key.Down:
                    nextX += 1;
                    break;
                case Key.Left:
                    nextY -= 1;
                    break;
                case Key.Right:
                    nextY += 1;
                    break;
                default:
                    break;
            }

            //still not over range of matrix
            if (!(nextX < 0 || nextY < 0 || nextX > UIView.numberOfRows - 1 || nextY > UIView.numberOfColumns - 1))
            {
                _game.selectedBitmap = canvasUI.Children[findChild(nextX, nextY)] as Image;

                int x = _game.blankPos.Item1, y = _game.blankPos.Item2;
                Canvas.SetLeft(_game.selectedBitmap, UIView.cellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                Canvas.SetTop(_game.selectedBitmap, UIView.cellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                //code cua thay
                //var image = sender as Image;
                //var tuple = image.Tag as Tuple<int, int>;
                //int i = tuple.Item1; 
                //int j = tuple.Item2;

                //get last position of selectedImage
                int oldX = nextX, oldY = nextY;

                //swap tag of selectedImage to blank cell that it just filled in
                _game.selectedBitmap.Tag = _game.blankPos;
                swapVal(_game.blankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                _game.blankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                printToDebug();

                if (_game.isFinish(_matrix))
                {
                    MessageBox.Show("Win!!!");
                }

                //debug
                textblockForDebug.Text = $"{nextX} - {nextY}";
            }

        }

        //debug stuffs
        private void Img_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            var tag = img.Tag as Tuple<int, int>;
            textblockForDebug.Text = $"tag: {tag.Item1} - {tag.Item2}";
        }
    }
}
