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
        public bool isBusy = false;
        public bool hasFileLoaded = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateStatusText(string text)
        {
            StatusText.Text = text;
        }

        public void SetTitle(string text)
        {
            if (text == null)
                Title = "MonczoDB";
            else
                Title = $"MonczoDB - {text}";
        }

        private async void FileNewBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void FileLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;

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
                await DBInterface.LoadFromFile(filePath);
                UpdateStatusText("Ready");
                isBusy = false;
                hasFileLoaded = true;

                SetTitle(System.IO.Path.GetFileName(filePath));
            }
        }

        private async void FileSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy && hasFileLoaded)
            {
                isBusy = true;

                if (DBInterface.currentFilePath == null)
                {
                    FileSaveAsBtn_Click(sender, e);
                    return;
                }

                UpdateStatusText($"Saving {System.IO.Path.GetFileName(DBInterface.currentFilePath)}...");
                await DBInterface.SaveFile(DBInterface.currentFilePath);
                UpdateStatusText("Ready");

                isBusy = false;
            }
            
        }

        private async void FileSaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy && hasFileLoaded)
            {
                isBusy = true;

                FileDialog fileDialog = new SaveFileDialog
                {
                    Filter = "MonczoDB Database (*.dat)|*.dat"
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

                UpdateStatusText($"Saving as {System.IO.Path.GetFileName(filePath)}...");
                await DBInterface.SaveFile(filePath);
                UpdateStatusText("Ready");

                isBusy = false;
            }
            
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
