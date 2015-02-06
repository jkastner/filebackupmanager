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

namespace BackupUI
{
    /// <summary>
    /// Interaction logic for YesNoTopmost.xaml
    /// </summary>
    public partial class YesNoTopmost : Window
    {
        public YesNoTopmost(String title, String message)
        {
            InitializeComponent();
            Message_TextBlock.Text = message;
            Title = title;
        }

        public bool YesWasClicked { get; set; }
        public bool NoWasClicked { get; set; }
        private void YesClicked(object sender, RoutedEventArgs e)
        {
            YesWasClicked = true;
            NoWasClicked = false;
            this.Close();
        }

        private void NoClicked(object sender, RoutedEventArgs e)
        {
            NoWasClicked = true;
            YesWasClicked = false;
            this.Close();
        }


    }
}
