using Microsoft.Win32;
using System;
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
using System.Windows.Shapes;

namespace WPFProject02_GamePuzzle
{
    /// <summary>
    /// Interaction logic for BootScreen.xaml
    /// </summary>
    public partial class BootScreen : Window
    {
        public int userChoice { get; set; } //3 - 3x3, 4 - 4x4, 5 - 5x5
        public bool isDefaultMode { get; set; } //load image from resources or from user choice
        public string userImagePath { get; set; } //string to user image 
        public string userName { get; set; } 

        public BootScreen()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxName.Text.ToString()!="")
            {
                if (radio3_3.IsChecked == true)
                {
                    userChoice = 3;
                }
                else
                if (radio4_4.IsChecked == true)
                {
                    userChoice = 4;
                }
                else
                {
                    userChoice = 5;
                }

                if (radioDefaultImage.IsChecked == true)
                {
                    isDefaultMode = true;
                }
                else
                {
                    isDefaultMode = false;
                    var screen = new OpenFileDialog();
                    if (screen.ShowDialog() == true)
                    {
                        userImagePath = screen.FileName;
                    }
                }

                userName = textBoxName.Text.ToString();

                this.DialogResult = true;
                this.Close();
            }
            else //username still not typed
            {
                MessageBox.Show("Please type your in-game name!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
