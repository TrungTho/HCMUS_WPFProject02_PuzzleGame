using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// class for game - controling
        /// </summary>
        class Game
        {
            public string UserName { get; set; }
            public int NumberOfScrambles { get; set; }
            public Tuple<int,int> BlankPos { get; set; }
            public bool IsDragging { get; set; }
            public Image SelectedBitmap { get; set; }
            public Point LastPosition { get; set; }
            public Point LastSelectedPosition { get; set; }
            public int MaxTime { get; set; } //second
            public string ImagePath { get; set; }
            public bool IsNewGame { get; set; } //to dectect user load game or just init a new game


            /// <summary>
            /// method to init new game before start, notice param numberOfScramble
            /// </summary>
            /// <param name="x"></param> x - coordinate of blankpos (last cell)
            /// <param name="y"></param> y - coordinate of blankpos (last cell)
            public void InitGame(int x, int y)
            {
                NumberOfScrambles = UIView.NumberOfColumns*20; //just for testing
                BlankPos = Tuple.Create(x, y);
                IsDragging = false;
                SelectedBitmap = null;
                MaxTime = 3*60; 
                IsNewGame = true;
            }

            public void Won(bool isAuto)
            {
                _countdownTimer.Stop();
                _moves.Clear();
                MessageBox.Show("You Won!!!", "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                if (isAuto==false)
                {
                    int tmpTime = 180 - _time;
                    int minute = tmpTime / 60;
                    int second = tmpTime % 60;
                    var userTime = string.Format("{0}:{1}", minute.ToString().PadLeft(2, '0'), second.ToString().PadLeft(2, '0'));
                    addHighScore(UserName,userTime);
                }
            }

            public void Lose()
            {
                MessageBox.Show("timeout!");
            }

            public bool isFinish(int[,] _matrix)
            {
                for (int i = 0; i < UIView.NumberOfRows; i++)
                {
                    for (int j = 0; j < UIView.NumberOfColumns; j++)
                    {
                        if (i != UIView.NumberOfRows - 1 || j != UIView.NumberOfColumns - 1) 
                        {
                            if (_matrix[i, j] != i * UIView.NumberOfColumns + j + 1)
                                return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// class to control constants of details in game
        /// </summary>
        public class UIView
        {
            //changeabel value
            public static int NumberOfRows { get; set; }
            public static int NumberOfColumns { get; set; }
            public static int CellsStartX { get; set; }
            public static int CellsStartY { get; set; }
            public static int PreviewImageStartX { get; set; }
            public static int PreviewImageStartY { get; set; }

            //Constant for specifications
            //public const int topOffset = 40;
            //public const int leftOffset = 40;
            public const int widthOfCell = 70;
            public const int heightOfCell = 70;
            public const int widthOfImage = 60;
            public const int heightOfImage = 60;
            public const int widthOfPreviewImage = 250;
            public const int heightOfPreviewImage = 250;
            public const string fileSave = "saveGame.txt";
            public const string highScore = "highScore.txt";
        }

        public class HighScore
        {
            public int id { get; set; }
            public string userName { get; set; }
            public string userTime { get; set; }
        }

        /*timer section*/
        static DispatcherTimer _countdownTimer;
        static int _time;
        
        /// <summary>
        /// start count downtimer
        /// </summary>
        private void startTimer()
        {
            _time = _game.MaxTime;//convert from minute to second
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = new TimeSpan(0, 0, 1);
            _countdownTimer.Tick += _countdownTimer_Tick;
            _countdownTimer.Start();
        }

        /// <summary>
        /// what happend in 1 tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _countdownTimer_Tick(object sender, EventArgs e)
        {
            if (_time!=0)
            {
                _time--;
                int minute = _time / 60;
                int second = _time % 60;
                textblockTimer.Text = string.Format("{0}:{1}",minute.ToString().PadLeft(2,'0'),second.ToString().PadLeft(2,'0'));
            }
            else
            {
                _game.Lose();
                _countdownTimer.Stop();
            }
        }

        //model init
        int[,] _matrix; //matrix of images initialized
        Image[] _image; //references to from model to UI
        Game _game; //New game initialized
        static List<string> _moves;
        static BindingList<HighScore> highScores;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           //timer's UI setup
            Canvas.SetLeft(textblockTimer, (this.Width*2/3 - textblockTimer.Width) / 2);
            Canvas.SetTop(textblockTimer, 20);

            highScores = new BindingList<HighScore>();
            loadHighScore();

            //start a new game
            newGame_Click(null, null);
        }


        /*Game prepare*/
        /// <summary>
        /// draw aqua line to separate cells
        /// </summary>
        private void drawLine()
        {
            /*UI*/
            //Draw column
            for (int i = 0; i < UIView.NumberOfRows + 1; i++)
            {
                var line = new Line
                {
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(Colors.Aqua)
                };
                canvasUI.Children.Add(line);

                line.X1 = UIView.CellsStartX + i * UIView.widthOfCell;
                line.Y1 = UIView.CellsStartY;

                line.X2 = UIView.CellsStartX + i * UIView.widthOfCell;
                line.Y2 = UIView.CellsStartY + (UIView.NumberOfColumns) * UIView.heightOfCell;
            }

            //Draw row
            for (int i = 0; i < UIView.NumberOfColumns + 1; i++)
            {
                var line = new Line
                {
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(Colors.Aqua)
                };
                canvasUI.Children.Add(line);

                line.X1 = UIView.CellsStartX;
                line.Y1 = UIView.CellsStartY + i * UIView.heightOfCell;

                line.X2 = UIView.CellsStartX + (UIView.NumberOfRows) * UIView.widthOfCell;
                line.Y2 = UIView.CellsStartY + i * UIView.heightOfCell;
            }

            //draw separator
            var separatorLine = new Line
            {
                StrokeThickness = 5,
                Stroke = new SolidColorBrush(Colors.Aqua)
            };

            canvasUI.Children.Add(separatorLine);

            separatorLine.X1 = this.Width * 2 / 3;
            separatorLine.Y1 = 0;

            separatorLine.X2 = this.Width * 2 / 3;
            separatorLine.Y2 = this.Height;


        }

        /// <summary>
        /// load image from resources to UI
        /// </summary>
        private void loadDefaultImage()
        {
            //load preview image
            string tmpPreviewName = $"DefaultImages/preview{UIView.NumberOfRows}.png";
            var previewImg = new Image();
            previewImg.Width = UIView.widthOfPreviewImage;
            previewImg.Height = UIView.heightOfPreviewImage;
            previewImg.Source = new BitmapImage(new Uri(tmpPreviewName, UriKind.Relative));
            canvasUI.Children.Add(previewImg);
            previewImg.Tag = Tuple.Create(-1, -1);

            Canvas.SetLeft(previewImg, UIView.PreviewImageStartX);
            Canvas.SetTop(previewImg, UIView.PreviewImageStartY);

            //load cell - images
            for (int i = 0; i < UIView.NumberOfRows; i++)
                for (int j = 0; j < UIView.NumberOfColumns; j++)
                {
                    if (_matrix[i, j] != 0)
                    {
                        int step = UIView.NumberOfColumns;
                        string tmpImageName = $"DefaultImages/number{_matrix[i, j]}.png";
                        var img = new Image
                        {
                            Width = UIView.widthOfImage,
                            Height = UIView.heightOfImage,
                            Source = new BitmapImage(new Uri(tmpImageName, UriKind.Relative)),
                            Name = $"image{_matrix[i,j]}"
                        };
                        canvasUI.Children.Add(img);

                        Canvas.SetLeft(img, UIView.CellsStartX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        Canvas.SetTop(img, UIView.CellsStartY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                        img.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
                        //for debug tag of control

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
            int leng = (int)source.PixelWidth;
            if (source.PixelHeight < source.PixelWidth)
                leng = (int)source.PixelHeight;

            var previewImg = new Image();
            previewImg.Width = UIView.widthOfPreviewImage;
            previewImg.Height = UIView.heightOfPreviewImage;
            previewImg.Source = source; //full picture
            //previewImg.Source = new CroppedBitmap(source,new Int32Rect(0,0,leng,leng)); //crop picture
            canvasUI.Children.Add(previewImg);
            previewImg.Tag = Tuple.Create(-1, -1);

            Canvas.SetLeft(previewImg, UIView.PreviewImageStartX);
            Canvas.SetTop(previewImg, UIView.PreviewImageStartY);

            //crop image and store in model _image
            for (int i = 0; i < UIView.NumberOfRows; i++)
            {
                for (int j = 0; j < UIView.NumberOfColumns; j++)
                {
                    if ((i != UIView.NumberOfRows - 1) || (j != UIView.NumberOfColumns - 1))
                    {
                        var h = (int)leng / UIView.NumberOfRows;
                        var w = (int)leng / UIView.NumberOfColumns;
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        var cropBitmap = new CroppedBitmap(source, rect);

                        var img = new Image
                        {
                            Stretch = Stretch.Fill,
                            Width = UIView.widthOfImage,
                            Height = UIView.heightOfImage,
                            Source = cropBitmap,
                            Name = $"image{ (i * UIView.NumberOfRows + 1 + j) }"
                        };

                        img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                        img.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
                        //for debug tag of control

                        _image[i * UIView.NumberOfRows + 1 + j] = img;
                        //img.Tag = new Tuple<int, int>(i, j);
                    }
                }
            }

            //load image with scrambled _matrix
            for (int i = 0; i < UIView.NumberOfRows; i++)
            {
                for (int j = 0; j < UIView.NumberOfColumns; j++)
                {
                    if (_matrix[i, j] != 0)
                    {
                        var img = _image[_matrix[i, j]];

                        canvasUI.Children.Add(img);
                        Canvas.SetLeft(img, UIView.CellsStartX + j * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                        Canvas.SetTop(img, UIView.CellsStartY + i * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                        img.Tag = new Tuple<int, int>(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// get number of cells, init size of cell,.. from custom dialog
        /// </summary>
        /// <param name="isNewGame"></param> new game or load game
        private void getSize(bool isNewGame)
        {
            if (isNewGame)
            {
                var screen = new BootScreen();
                if (screen.ShowDialog() == true)
                {
                    UIView.NumberOfColumns = screen.userChoice;
                    UIView.NumberOfRows = screen.userChoice;
                    _game.UserName = screen.userName;
                }
                else
                {
                    this.Close();
                }

                //detect mode 
                string resMode = "";
                if (screen.isDefaultMode == false)
                    resMode = screen.userImagePath;

                _game.ImagePath = resMode;
            }

            //set arrow button
            Canvas.SetLeft(labelControl, 519);
            Canvas.SetTop(labelControl, 470);

            Canvas.SetTop(buttonDown, this.Height * 3 / 4);
            Canvas.SetLeft(buttonDown, (this.Width * 2/ 3+ (this.Width*1/3 - buttonDown.Width)/2));

            Canvas.SetTop(buttonLeft, this.Height * 3 / 4);
            Canvas.SetLeft(buttonLeft, (this.Width * 2 / 3 + (this.Width * 1 / 3 - buttonLeft.Width) / 2) - buttonLeft.Width - 10);

            Canvas.SetTop(buttonRight, this.Height * 3 / 4);
            Canvas.SetLeft(buttonRight, (this.Width * 2 / 3 + (this.Width * 1 / 3 - buttonRight.Width) / 2) + buttonRight.Width + 10);

            Canvas.SetTop(buttonUp, this.Height * 3 / 4 - buttonUp.Height - 10);
            Canvas.SetLeft(buttonUp, (this.Width * 2 / 3 + (this.Width * 1 / 3 - buttonUp.Width) / 2));

            //set highscore board
            Canvas.SetTop(labelhighScore, 40);
            Canvas.SetLeft(labelhighScore, 489);

            Canvas.SetTop(listviewHighScore, 100);
            Canvas.SetLeft(listviewHighScore, this.Width * 2 / 3 + (this.Width * 1 / 3 - listviewHighScore.Width) / 2);

            //calculate coordinate of preview image and cells
            UIView.PreviewImageStartX = ((int)this.Width*2/3 - UIView.widthOfPreviewImage) / 2;
            UIView.PreviewImageStartY = ((int)this.Height - UIView.heightOfPreviewImage - UIView.NumberOfRows * UIView.heightOfCell) / 6 + (int)textblockTimer.Height;

            UIView.CellsStartX = ((int)this.Width*2/3 - UIView.NumberOfColumns * UIView.widthOfCell) / 2;
            UIView.CellsStartY = 2 * UIView.PreviewImageStartY - (int)textblockTimer.Height + UIView.heightOfPreviewImage;
        }

        public static void sortHighScore()
        {
            for (int i = 0; i < highScores.Count - 1; i++)
            {
                for (int j = i + 1; j < highScores.Count; j++)
                {
                    int comparison = String.Compare(highScores[i].userTime, highScores[j].userTime, comparisonType: StringComparison.OrdinalIgnoreCase);
                    if (comparison > 0)
                    {
                        var tmpItem = highScores[i];
                        highScores[i] = highScores[j];
                        highScores[j] = tmpItem;
                    }
                }
            }

            while (highScores.Count != 5)
                highScores.RemoveAt(4);

            for (int i = 0; i < highScores.Count; i++)
                highScores[i].id = i + 1;
        }

        private void loadHighScore()
        {
            try
            {
                string[] lines = File.ReadAllLines(UIView.highScore);
                int tmpID = 0;
                foreach (var line in lines)
                {
                    tmpID++;
                    string[] tokens = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var item = new HighScore
                    {
                        id = tmpID,
                        userName = tokens[0]
                    };
                    item.userTime = tokens[1];

                    highScores.Add(item);
                }

                sortHighScore();
            }
            catch (Exception e)
            {

            }

            listviewHighScore.ItemsSource = highScores;
        }

        public static void addHighScore(string userName, string userTime)
        {
            var newItem = new HighScore
            {
                id = 0,
                userTime = userTime,
                userName = userName
            };

            highScores.Add(newItem);

            sortHighScore();

            //update database
            //open file
            using (StreamWriter output = new StreamWriter(UIView.highScore))
            {
                foreach (var score in highScores)
                {
                    output.WriteLine($"{score.userName} {score.userTime}");
                }

            }
        }


        /*BUS*/
        /// <summary>
        /// funtion to find which image in coordinate (x,y) of canvasUI
        /// </summary>
        /// <param name="x"></param> x coordinate
        /// <param name="y"></param> y coordinate
        /// <returns></returns> index of image in canvasUI.Children[]
        private int findChild(int x, int y)
        {
            for (int i = 0; i < canvasUI.Children.Count; i++)
            {
                if (canvasUI.Children[i] is Image)
                {
                    var child = canvasUI.Children[i] as Image;
                    var tag = child.Tag as Tuple<int, int>;
                    if (tag.Item1 == x && tag.Item2 == y)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// function to return coordinate in matrix from mouse's cursor position
        /// </summary>
        /// <param name="position"></param> Point value
        /// <param name="i"></param> in - 0, out - coordinate in matrix
        /// <param name="j"></param> in - 0, out - coordinate in matrix
        private void getPos(Point position, ref int i, ref int j)
        {
            i = ((int)position.Y - UIView.CellsStartY) / UIView.heightOfCell;
            j = ((int)position.X - UIView.CellsStartX) / UIView.widthOfCell;
        }

        /// <summary>
        /// capture event that cursor leave window but still dragging the selected image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_game.IsDragging)
            {
                //stop dragging image
                _game.IsDragging = false;

                //get last position of selectedImage
                int x = 0, y = 0;
                getPos(_game.LastSelectedPosition, ref x, ref y);

                //set coordinate for selectedImage
                if (_game.SelectedBitmap != null)
                {
                    Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);
                }
            }
        }

        /// <summary>
        /// function to swap 2 pos in _matrix
        /// </summary>
        /// <param name="a"></param> - first pos
        /// <param name="b"></param> - second pos
        private void swapVal(Tuple<int, int> a, Tuple<int, int> b)
        {
            var tmp = _matrix[a.Item1, a.Item2];
            _matrix[a.Item1, a.Item2] = _matrix[b.Item1, b.Item2];
            _matrix[b.Item1, b.Item2] = tmp;
        }

        /// <summary>
        /// function to detect which direction that blank cell will move to next
        /// </summary>
        /// <param name="oldPos"></param> present position of blank cell
        /// <param name="newPos"></param> next position that blank cell will be moved to
        /// <returns></returns>
        private string getDirect(Tuple<int, int> oldPos, Tuple<int, int> newPos)
        {
            string res = "";

            if (oldPos.Item1 == newPos.Item1)
            {
                if (oldPos.Item2 > newPos.Item2)
                    res = "L";
                else
                    res = "R";
            }
            else
                if (oldPos.Item1 > newPos.Item1)
            {
                res = "U";
            }
            else
                res = "D";

            return res;
        }

        /// <summary>
        /// funtion to scamble the both model _matrix and UI Image
        /// </summary>
        private void scambleMatrix()
        {
            //set up basis value for matrix
            for (int i = 0; i < UIView.NumberOfRows; i++)
            {
                for (int j = 0; j < UIView.NumberOfColumns; j++)
                {
                    _matrix[i, j] = i * UIView.NumberOfRows + j + 1;
                }
            }
            _matrix[UIView.NumberOfRows - 1, UIView.NumberOfColumns - 1] = 0; //blank pos

            //scramble this matrix
            Random random = new Random();
            var tmpPos = Tuple.Create(UIView.NumberOfRows - 1, UIView.NumberOfColumns - 1);

            for (int i = 0; i < _game.NumberOfScrambles; i++)
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
                while (nextX < 0 || nextY < 0 || nextX > UIView.NumberOfRows - 1 || nextY > UIView.NumberOfColumns - 1 || (nextX == tmpPos.Item1) && nextY == tmpPos.Item2);

                _moves.Add(getDirect(tmpPos, Tuple.Create(nextX, nextY)));
                swapVal(tmpPos, Tuple.Create(nextX, nextY));
                tmpPos = Tuple.Create(nextX, nextY);
            }

            _game.BlankPos = tmpPos;
            //printToDebug();
        }


        /*Game control fuction*/
        /// <summary>
        /// Capture event cursor moving around window
        /// </summary>
        /// <param name="sender"></param> crsor
        /// <param name="e"></param> argsu
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = 0, j = 0;
            getPos(position, ref i, ref j);

            //this.Title = $"{position.X} - {position.Y}, a[{i}][{j}] , blank: [{_game.BlankPos.Item1}],[{_game.BlankPos.Item2}]";

            if (_game.IsDragging) 
            {
                var dx = position.X - _game.LastPosition.X;
                var dy = position.Y - _game.LastPosition.Y;

                var lastLeft = Canvas.GetLeft(_game.SelectedBitmap);
                var lastTop = Canvas.GetTop(_game.SelectedBitmap);
                Canvas.SetLeft(_game.SelectedBitmap, lastLeft + dx);
                Canvas.SetTop(_game.SelectedBitmap, lastTop + dy);

                _game.LastPosition = position;
            }
        }

        /// <summary>
        /// Capture event that an image was finish dragged
        /// </summary>
        /// <param name="sender"></param> selectedimage
        /// <param name="e"></param> agrs m
        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _game.IsDragging = false;
            var position = e.GetPosition(this);

            int x = 0, y = 0;
            getPos(position, ref x, ref y);

            int oldPosX = 0, oldPosY = 0;
            getPos(_game.LastSelectedPosition, ref oldPosX, ref oldPosY);

            if (Math.Abs(oldPosY - y) + Math.Abs(oldPosX - x) == 1)
            {
                if (x == _game.BlankPos.Item1 && y == _game.BlankPos.Item2)
                {

                    _moves.Add(getDirect(Tuple.Create(_game.BlankPos.Item1, _game.BlankPos.Item2), Tuple.Create(oldPosX, oldPosY)));

                    //set new coordinate for selectedImage
                    Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                    //get last position of selectedImage
                    int oldX = 0, oldY = 0;
                    getPos(_game.LastSelectedPosition, ref oldX, ref oldY);

                    //swap tag of selectedImage to blank cell that it just filled in
                    _game.SelectedBitmap.Tag = _game.BlankPos;
                    swapVal(_game.BlankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                    _game.BlankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                    //printToDebug();

                    if (_game.isFinish(_matrix))
                    {
                        _game.Won(false);
                    }
                }
            }
            else //return image to old position
            {
                //get last position of selectedImage
                getPos(_game.LastSelectedPosition, ref x, ref y);

                //set coordinate for selectedImage
                Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);
            }

            //MessageBox.Show($"{i} - {j}");
        }

        /// <summary>
        /// Capture the event that an image was just clicked
        /// </summary>
        /// <param name="sender"></param> selected image
        /// <param name="e"></param> args
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _game.IsDragging = true;
            _game.SelectedBitmap = sender as Image;
            _game.LastPosition = e.GetPosition(this);
            _game.LastSelectedPosition = e.GetPosition(this);
        }

        /// <summary>
        /// Capture the event that a arrow key was pressed and released
        /// </summary>
        /// <param name="sender"></param> window 
        /// <param name="e"></param> args
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            int nextX = _game.BlankPos.Item1, nextY = _game.BlankPos.Item2;
            bool isKeyValid = false;
            switch (e.Key)
            {
                case Key.Up:
                    nextX -= 1;
                    isKeyValid = true;
                    break;
                case Key.Down:
                    nextX += 1;
                    isKeyValid = true;
                    break;
                case Key.Left:
                    nextY -= 1;
                    isKeyValid = true;
                    break;
                case Key.Right:
                    nextY += 1;
                    isKeyValid = true;
                    break;
                default:
                    break;
            }

            if (isKeyValid)
            {
                //still not over range of matrix
                if (!(nextX < 0 || nextY < 0 || nextX > UIView.NumberOfRows - 1 || nextY > UIView.NumberOfColumns - 1))
                {
                    //add move to track and auto play (if neccessary)
                    _moves.Add(getDirect(Tuple.Create(_game.BlankPos.Item1, _game.BlankPos.Item2),Tuple.Create(nextX, nextY)));
                    //get image to set new position
                    _game.SelectedBitmap = canvasUI.Children[findChild(nextX, nextY)] as Image;

                    int x = _game.BlankPos.Item1, y = _game.BlankPos.Item2;
                    Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                    //get last position of selectedImage
                    int oldX = nextX, oldY = nextY;

                    //swap tag of selectedImage to blank cell that it just filled in
                    _game.SelectedBitmap.Tag = _game.BlankPos;
                    swapVal(_game.BlankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                    _game.BlankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                    //printToDebug();

                    if (_game.isFinish(_matrix))
                    {
                        _game.Won(false);
                    }

                    //debug
                    //textblockForDebug.Text = $"{nextX} - {nextY}";
                }
            }
        }


        /*Game options*/
        /// <summary>
        /// function to save the current game to database
        /// </summary>
        /// <param name="sender"></param> button
        /// <param name="e"></param> args
        private void saveGame_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Override last saved game?", "Save game", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //open file
                using (StreamWriter output = new StreamWriter(UIView.fileSave))
                {
                    //first line is number of rows & columns / time remain
                    output.WriteLine($"{UIView.NumberOfRows} {UIView.NumberOfColumns} {_time}");

                    //second line is mode of game: default ("") or custom image (path)
                    output.WriteLine(_game.ImagePath);

                    //third line is blank pos coordinate
                    output.WriteLine($"{_game.BlankPos.Item1} {_game.BlankPos.Item2}");

                    //fourth line is current user
                    output.WriteLine(_game.UserName);

                    //fifth line is moves had moved
                    foreach (var move in _moves)
                    {
                        output.Write($"{move} ");
                    }
                    output.WriteLine("");

                    //folowing is values of _matrix[]
                    for (int i = 0; i < UIView.NumberOfColumns; i++)
                    {
                        for (int j = 0; j < UIView.NumberOfColumns; j++)
                            output.Write($"{_matrix[i, j]} ");
                        output.WriteLine("");
                    }

                }

                MessageBox.Show("Game saved!");
            }
        }

        /// <summary>
        /// function to load the latest saved game from database to this window
        /// </summary>
        /// <param name="sender"></param> button
        private void loadGame_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Quit and Load game?", "Load Game", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //reset UI
                ResetAll();

                string[] lines = File.ReadAllLines(UIView.fileSave);

                string[] separator = { " " };

                //first line is number of rows & columns
                string[] tokens = lines[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                UIView.NumberOfRows = Int32.Parse(tokens[0]);
                UIView.NumberOfColumns = Int32.Parse(tokens[1]);
                int tmpTime = Int32.Parse(tokens[2]);

                //second line is mode of game
                _game.ImagePath = lines[1];

                //third line is blank pos coordinate of preview game
                tokens = lines[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                int tmpX = Int32.Parse(tokens[0]);
                int tmpY = Int32.Parse(tokens[1]);

                //fourth line is 
                _game.UserName = lines[3];

                //fifth line is moves had moved in previous game
                tokens = lines[4].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                _moves.Clear(); //reset moves of current game
                //add new moves to _moves
                foreach (var move in tokens)
                {
                    _moves.Add(move);
                }

                //Model
                _matrix = new int[UIView.NumberOfRows, UIView.NumberOfColumns];
                _image = new Image[UIView.NumberOfRows * UIView.NumberOfColumns];
                _game.InitGame(tmpX, tmpY);
                _game.MaxTime = tmpTime;
                getSize(false);

                int row = 0;
                for (int countLine = 5; countLine < lines.Length; countLine++)
                {
                    tokens = lines[countLine].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < tokens.Length; j++)
                    {
                        _matrix[row, j] = Int32.Parse(tokens[j]);
                    }

                    row++;
                }

                //UI
                drawLine();

                //load image 
                if (_game.ImagePath == "")
                {
                    loadDefaultImage();
                }
                else
                {
                    loadCustomImage(_game.ImagePath);
                }

                //timer
                startTimer();
            }
        }

        /// <summary>
        /// function to reset all UIs and models to restart a new game or load previous game
        /// </summary>
        private void ResetAll()
        {
            _moves = new List<string>();

            //reset UI
            const int numberOfBasisChildren = 9;//some children define in xaml
            while (canvasUI.Children.Count!=numberOfBasisChildren)
            {
                canvasUI.Children.RemoveAt(numberOfBasisChildren);
            }

            //reset timer
            if (_countdownTimer!=null)
                _countdownTimer.Stop();
        }

        /// <summary>
        /// function to start a whole new game 
        /// </summary>
        /// <param name="sender"></param> button
        private void newGame_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || (sender != null && MessageBox.Show("Start a New game?", "New Game", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes))
            {
                //reset game
                ResetAll();

                _game = new Game();
                //get number of cell to split image also path to user custom image (if neccessary)
                getSize(true);

                //Model
                _matrix = new int[UIView.NumberOfRows, UIView.NumberOfColumns];
                _image = new Image[UIView.NumberOfRows * UIView.NumberOfColumns];
                _game.InitGame(UIView.NumberOfRows - 1, UIView.NumberOfColumns - 1);

                //UI
                drawLine();

                //scramble matrix for playing 
                scambleMatrix();

                //load image 
                if (_game.ImagePath == "")
                {
                    loadDefaultImage();
                }
                else
                {
                    loadCustomImage(_game.ImagePath);
                }

                //timer
                startTimer();
            }
        }

        /// <summary>
        /// function to exit window
        /// </summary>
        /// <param name="sender"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Exit game?", "Exit", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        /// <summary>
        /// app & dev team informations 
        /// </summary>
        /// <param name="sender"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Window Programming Course\nMini Project 02 - Puzzle game\n1712798 - 1712270", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// some guide to play game
        /// </summary>
        /// <param name="sender"></param>
        private void Guide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Use mouse to drag image or use arrow key to move blank image.", "Controls", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// asynctask to demo how autoplay solve puzzle by track back scramble
        /// </summary>
        /// <returns></returns>
        private async Task Solve_ClickAsync()
        {
            int tmpCount = _moves.Count;
            int maxCount = tmpCount;
            int speedAnimation = 100;
            while (tmpCount > 0)
            {
                tmpCount--;
                int nextX = _game.BlankPos.Item1, nextY = _game.BlankPos.Item2;
                switch (_moves[tmpCount])
                {
                    case "D":
                        nextX -= 1;
                        break;
                    case "U":
                        nextX += 1;
                        break;
                    case "R":
                        nextY -= 1;
                        break;
                    case "L":
                        nextY += 1;
                        break;
                    default:
                        break;
                }

                //still not over range of matrix
                if (!(nextX < 0 || nextY < 0 || nextX > UIView.NumberOfRows - 1 || nextY > UIView.NumberOfColumns - 1))
                {
                    //get image to set new position
                    _game.SelectedBitmap = canvasUI.Children[findChild(nextX, nextY)] as Image;

                    int x = _game.BlankPos.Item1, y = _game.BlankPos.Item2;
                    Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                    //get last position of selectedImage
                    int oldX = nextX, oldY = nextY;                    

                    //swap tag of selectedImage to blank cell that it just filled in
                    _game.SelectedBitmap.Tag = _game.BlankPos;
                    swapVal(_game.BlankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                    _game.BlankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                    await Task.Delay(TimeSpan.FromMilliseconds(speedAnimation)); //wait for seeing
                }
            }


            if (_game.isFinish(_matrix))
            {
                _game.Won(true);
            }
        }

        /// <summary>
        /// funtion to solve the puzzle automatically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            Solve_ClickAsync();
        }

        private void ButtonDirect_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int nextX = _game.BlankPos.Item1, nextY = _game.BlankPos.Item2;
            bool isKeyValid = false;
            switch (button.Name)
            {
                case "buttonUp":
                    nextX -= 1;
                    isKeyValid = true;
                    break;
                case "buttonDown":
                    nextX += 1;
                    isKeyValid = true;
                    break;
                case "buttonLeft":
                    nextY -= 1;
                    isKeyValid = true;
                    break;
                case "buttonRight":
                    nextY += 1;
                    isKeyValid = true;
                    break;
                default:
                    break;
            }

            if (isKeyValid)
            {
                //still not over range of matrix
                if (!(nextX < 0 || nextY < 0 || nextX > UIView.NumberOfRows - 1 || nextY > UIView.NumberOfColumns - 1))
                {
                    //add move to track and auto play (if neccessary)
                    _moves.Add(getDirect(Tuple.Create(_game.BlankPos.Item1, _game.BlankPos.Item2), Tuple.Create(nextX, nextY)));
                    //get image to set new position
                    _game.SelectedBitmap = canvasUI.Children[findChild(nextX, nextY)] as Image;

                    int x = _game.BlankPos.Item1, y = _game.BlankPos.Item2;
                    Canvas.SetLeft(_game.SelectedBitmap, UIView.CellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.SelectedBitmap, UIView.CellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                    //get last position of selectedImage
                    int oldX = nextX, oldY = nextY;

                    //swap tag of selectedImage to blank cell that it just filled in
                    _game.SelectedBitmap.Tag = _game.BlankPos;
                    swapVal(_game.BlankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                    _game.BlankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                    //printToDebug();

                    if (_game.isFinish(_matrix))
                    {
                        _game.Won(false);
                    }
                }
            }
        }
    }
}