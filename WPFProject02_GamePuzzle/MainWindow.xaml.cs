using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        static DispatcherTimer _countdownTimer;
        private int _time;

        //class for game - controling
        class Game
        {
            public string userName { get; set; }
            public int numberOfScrambles { get; set; }
            public Tuple<int,int> blankPos { get; set; }
            public bool isDragging { get; set; }
            public Image selectedBitmap { get; set; }
            public Point lastPosition { get; set; }
            public Point lastSelectedPosition { get; set; }
            public int maxTime { get; set; } //minutes
            public string imagePath { get; set; }
            public bool isNewGame { get; set; } //to dectect user load game or just init a new game


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
                isNewGame = true;
                //lastPosition;
                //newBlankPos;
            }

            public void Won()
            {
                _countdownTimer.Stop();
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

        private void getSize(bool isNewGame)
        {
            if (isNewGame)
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

                //detect mode 
                string resMode = "";
                if (screen.isDefaultMode == false)
                    resMode = screen.userImagePath;

                _game.imagePath = resMode;
            }

            //calculate coordinate of preview image and cells
            UIView.previewImageStartX = ((int)this.Width - UIView.widthOfPreviewImage) / 2;
            UIView.previewImageStartY = ((int)this.Height - UIView.heightOfPreviewImage - UIView.numberOfRows * UIView.heightOfCell) / 6 + (int)textblockTimer.Height;

            UIView.cellsStartX = ((int)this.Width - UIView.numberOfColumns * UIView.widthOfCell) / 2;
            UIView.cellsStartY = 2 * UIView.previewImageStartY + UIView.heightOfPreviewImage;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           //timer's UI setup
            Canvas.SetLeft(textblockTimer, (this.Width - textblockTimer.Width) / 2);
            Canvas.SetTop(textblockTimer, 20);

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
                    if (_matrix[i, j] != 0)
                    {
                        int step = UIView.numberOfColumns;
                        string tmpImageName = $"DefaultImages/number{_matrix[i, j]}.png";
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
            //set up basis value for matrix
            for (int i = 0; i < UIView.numberOfRows; i++)
            {
                for (int j = 0; j < UIView.numberOfColumns; j++)
                {
                    _matrix[i, j] = i * UIView.numberOfRows + j + 1;
                }
            }
            _matrix[UIView.numberOfRows - 1, UIView.numberOfColumns - 1] = 0; //blank pos

            //scramble this matrix
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
            //printToDebug();
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

            int oldPosX = 0, oldPosY = 0;
            getPos(_game.lastSelectedPosition, ref oldPosX, ref oldPosY);

            if (Math.Abs(oldPosY - y) + Math.Abs(oldPosX - x) == 1)
            {
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

            //textblockForDebug.Text = "can't find image";
            return -1;
        }

        private void Img_KeyUp(object sender, KeyEventArgs e)
        {
            int nextX = _game.blankPos.Item1, nextY = _game.blankPos.Item2;
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
                if (!(nextX < 0 || nextY < 0 || nextX > UIView.numberOfRows - 1 || nextY > UIView.numberOfColumns - 1))
                {
                    _game.selectedBitmap = canvasUI.Children[findChild(nextX, nextY)] as Image;

                    int x = _game.blankPos.Item1, y = _game.blankPos.Item2;
                    Canvas.SetLeft(_game.selectedBitmap, UIView.cellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.selectedBitmap, UIView.cellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);

                    //get last position of selectedImage
                    int oldX = nextX, oldY = nextY;

                    //swap tag of selectedImage to blank cell that it just filled in
                    _game.selectedBitmap.Tag = _game.blankPos;
                    swapVal(_game.blankPos, Tuple.Create(oldX, oldY)); //swap value in model _matrix
                    _game.blankPos = Tuple.Create(oldX, oldY); //blankpos chage to last position of selectedImage

                    printToDebug();

                    if (_game.isFinish(_matrix))
                    {
                        _game.Won();
                    }

                    //debug
                    //textblockForDebug.Text = $"{nextX} - {nextY}";
                }
            }
        }

        //debug stuffs
        private void Img_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            var tag = img.Tag as Tuple<int, int>;
            //textblockForDebug.Text = $"tag: {tag.Item1} - {tag.Item2}";
        }

        private void saveGame_Click(object sender, RoutedEventArgs e)
        {
            //open file
            using (StreamWriter output = new StreamWriter(UIView.fileSave))
            {
                //first line is number of rows & columns
                output.WriteLine($"{UIView.numberOfRows} {UIView.numberOfColumns}");

                //second line is mode of game: default ("") or custom image (path)
                output.WriteLine(_game.imagePath);

                //third line is blank pos coordinate
                output.WriteLine($"{_game.blankPos.Item1} {_game.blankPos.Item2}");

                //folowing is values of _matrix[]
                for (int i = 0; i < UIView.numberOfColumns; i++)
                {
                    for (int j = 0; j < UIView.numberOfColumns; j++)
                        output.Write($"{_matrix[i, j]} ");
                    output.WriteLine("");
                }

            }

            MessageBox.Show("Game saved!");
        }

        private void loadGame_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Quit and Load game?", "Load Game", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {

                string[] lines = File.ReadAllLines(UIView.fileSave);

                string[] separator = { " " };

                //first line is number of rows & columns
                string[] tokens = lines[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                UIView.numberOfRows = Int32.Parse(tokens[0]);
                UIView.numberOfColumns = Int32.Parse(tokens[1]);

                //second line is mode of game
                _game.imagePath = lines[1];

                //third line is blank pos coordinate of preview game
                tokens = lines[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                int tmpX = Int32.Parse(tokens[0]);
                int tmpY = Int32.Parse(tokens[1]);

                //reset UI
                ResetAll();

                //Model
                _matrix = new int[UIView.numberOfRows, UIView.numberOfColumns];
                _image = new Image[UIView.numberOfRows * UIView.numberOfColumns];
                _game.InitGame(tmpX, tmpY);
                getSize(false);

                int row = 0;
                for (int countLine = 3; countLine < lines.Length; countLine++)
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
                if (_game.imagePath == "")
                {
                    loadDefaultImage();
                }
                else
                {
                    loadCustomImage(_game.imagePath);
                }

                //timer
                startTimer();
            }
        }

        private void ResetAll()
        {
            ////reset model
            //for (int i = 0; i < UIView.numberOfColumns; i++)
            //{
            //    for (int j = 0; j < UIView.numberOfColumns; j++)
            //    {
            //        _matrix[i, j] = 0;
            //        if (i * UIView.numberOfColumns + j + 1 != _image.Length)
            //            _image[i * UIView.numberOfColumns + j + 1] = null;
            //    }
            //}

            //reset UI
            const int numberOfBasisChildren = 2;
            while (canvasUI.Children.Count!=numberOfBasisChildren)
            {
                canvasUI.Children.RemoveAt(numberOfBasisChildren);
            }

            //reset timer
            if (_countdownTimer!=null)
                _countdownTimer.Stop();
        }

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
                _matrix = new int[UIView.numberOfRows, UIView.numberOfColumns];
                _image = new Image[UIView.numberOfRows * UIView.numberOfColumns];
                _game.InitGame(UIView.numberOfRows - 1, UIView.numberOfColumns - 1);

                //UI
                drawLine();

                //scramble matrix for playing 
                scambleMatrix();

                //load image 
                if (_game.imagePath == "")
                {
                    loadDefaultImage();
                }
                else
                {
                    loadCustomImage(_game.imagePath);
                }

                //timer
                startTimer();
            }
        }

        /// <summary>
        /// capture event that cursor leave window but still dragging the selected image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_game.isDragging)
            {
                //stop dragging image
                _game.isDragging = false;

                //get last position of selectedImage
                int x = 0, y = 0;
                getPos(_game.lastSelectedPosition, ref x, ref y);

                //set coordinate for selectedImage
                if (_game.selectedBitmap != null)
                {
                    Canvas.SetLeft(_game.selectedBitmap, UIView.cellsStartX + y * UIView.widthOfCell + (UIView.widthOfCell - UIView.widthOfImage) / 2);
                    Canvas.SetTop(_game.selectedBitmap, UIView.cellsStartY + x * UIView.heightOfCell + (UIView.heightOfCell - UIView.heightOfImage) / 2);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Window Programming Course\nMini Project 02 - Puzzle game\n1712798 - 1712270", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Guide_Click(object sender, RoutedEventArgs e)
        {
                
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
