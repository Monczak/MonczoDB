using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonczoDB;
using MonczoDBInterface.Core;

namespace MonczoDBInterface
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

        public void UpdateStatusText(string text)
        {
            StatusText.Text = text;
        }

        private async void FileNewBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void FileLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Filter = "MonczoDB Databases (*.dat)|*.dat|All Files|*.*"
            };

            string filePath = null;
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    filePath = fileDialog.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }

            UpdateStatusText($"Loading {System.IO.Path.GetFileName(filePath)}...");
            await DBWrapper.LoadFromFile(filePath);
            UpdateStatusText("Ready");
        }

        private async void FileSaveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void FileSaveAsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataInsertRecordBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataDeleteRecordBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataInsertColumnBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataDeleteColumnBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataSortAscendingBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataSortDescendingBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DataFilterBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
