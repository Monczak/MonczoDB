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

        public int visibleRecords = 25;
        public int topRecordIndex = 0;

        List<string> columns;

        Dictionary<Tuple<int, int>, System.Windows.Controls.TextBox> visibleCells;

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

        public async Task CreateNewDBGrid()
        {
            DBGrid.RowDefinitions.Clear();
            DBGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < visibleRecords + 1; i++)
            {
                DBGrid.RowDefinitions.Add(new RowDefinition()
                {
                    MaxHeight = 100
                });
            }

            columns = DBInterface.db.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                DBGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    MaxWidth = 100
                });

                var box = new System.Windows.Controls.TextBox()
                {
                    Text = columns[i],
                    FontSize = 14,
                    Background = new SolidColorBrush(new Color() { R = 0xdd, G = 0xdd, B = 0xdd, A = 0xff })
                };
                DBGrid.Children.Add(box);
                Grid.SetRow(box, 0);
                Grid.SetColumn(box, i);
            }
            DBGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        public async Task UpdateDBGrid()
        {
            if (visibleCells == null) visibleCells = new Dictionary<Tuple<int, int>, System.Windows.Controls.TextBox>();

            for (int i = topRecordIndex; i < Math.Min(DBInterface.db.records.Count, topRecordIndex + visibleRecords); i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    if (visibleCells.ContainsKey(new Tuple<int, int>(i - topRecordIndex, j)))
                    {
                        visibleCells[new Tuple<int, int>(i - topRecordIndex, j)].Text = Convert.ToString(DBInterface.db.records[i].Get(columns[j]));
                    }
                    else
                    {
                        var box = new System.Windows.Controls.TextBox()
                        {
                            Text = Convert.ToString(DBInterface.db.records[i].Get(columns[j])),
                            FontSize = 14
                        };
                        DBGrid.Children.Add(box);
                        visibleCells[new Tuple<int, int>(i - topRecordIndex, j)] = box;
                        Grid.SetRow(box, i - topRecordIndex + 1);
                        Grid.SetColumn(box, j);
                    }
                    
                }
            }
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

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    UpdateStatusText($"Loading {System.IO.Path.GetFileName(filePath)}...");
                    await DBInterface.LoadFromFile(filePath);
                    UpdateStatusText("Ready");
                    isBusy = false;
                    hasFileLoaded = true;

                    SetTitle(System.IO.Path.GetFileName(filePath));

                    await CreateNewDBGrid();
                    await UpdateDBGrid();
                }
                
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

        private async void DBGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                topRecordIndex++;
                topRecordIndex = Math.Min(topRecordIndex, DBInterface.db.records.Count - visibleRecords);
            }
            else if (e.Delta > 0)
            {
                topRecordIndex--;
                topRecordIndex = Math.Max(topRecordIndex, 0);
            }

            DBGridScrollBar.Value = (double)topRecordIndex / (DBInterface.db.records.Count - visibleRecords);

            await UpdateDBGrid();
        }

        private async void DBGridScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int newIndex;
            try
            {
                newIndex = (int)Math.Round(e.NewValue * (DBInterface.db.records.Count - visibleRecords));
                UpdateStatusText(newIndex.ToString());
            }
            catch (NullReferenceException)
            {
                return;
            }

            topRecordIndex = newIndex;

            await UpdateDBGrid();
        }
    }
}
