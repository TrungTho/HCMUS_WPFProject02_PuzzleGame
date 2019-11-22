﻿using System;
using System.Collections.Generic;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //get number of cell to split image
            getSplit();

            //Model
            _matrix = new int[GameConfigs.numberOfRows, GameConfigs.numberOfColumns];
            _image = new Image[GameConfigs.numberOfRows, GameConfigs.numberOfColumns];

            /*UI*/
            //Draw column
            for (int i = 0; i < GameConfigs.numberOfRows + 1; i++)
            {
                var line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Black);
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
                line.Stroke = new SolidColorBrush(Colors.Black);
                canvasUI.Children.Add(line);

                line.X1 = GameConfigs.startX;
                line.Y1 = GameConfigs.startY + i * GameConfigs.heightOfCell;

                line.X2 = GameConfigs.startX + (GameConfigs.numberOfRows) * GameConfigs.widthOfCell;
                line.Y2 = GameConfigs.startY + i * GameConfigs.heightOfCell;
            }
        }
    }
}
