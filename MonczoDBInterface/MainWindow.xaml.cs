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
using MessageBox = System.Windows.MessageBox;

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

        bool shiftPressed = false;

        string selectedColumn = null;
        int selectedRecord = -1;

        Dictionary<Tuple<int, int>, System.Windows.Controls.TextBox> cellIndices;
        Dictionary<System.Windows.Controls.TextBox, DBCell> visibleCells;

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

        async void HandleColumnShiftClick(System.Windows.Controls.TextBox box, MouseButtonEventArgs e)
        {
            if (shiftPressed)
            {
                DeselectAll();
                selectedColumn = box.Text;
                UpdateStatusText($"Selected {selectedColumn}");
                await SelectColumn(selectedColumn);
            }
        }

        async void HandleCellShiftClick(System.Windows.Controls.TextBox box, MouseButtonEventArgs e)
        {
            // TODO: Maybe change this to Alt, so as to make selecting multiple records possible
            if (shiftPressed)
            {
                DeselectAll();
                selectedRecord = visibleCells[box].recordID;
                UpdateStatusText($"Selected record {selectedRecord}");
                await SelectRecord(selectedRecord);
            }
        }

        void HandleCellReturn(System.Windows.Controls.TextBox box, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateCell(box);
            }
        }

        void HandleCellLostFocus(System.Windows.Controls.TextBox box)
        {
            UpdateCell(box);
        }

        private void UpdateCell(System.Windows.Controls.TextBox box)
        {
            DBCell cell = visibleCells[box];

            UpdateStatusText(box.Text);

            // Ducktyping?
            if (int.TryParse(box.Text, out int iResult))
                DBInterface.db.records[cell.recordID].Set(cell.column, iResult);
            else if (double.TryParse(box.Text, out double dResult))
                DBInterface.db.records[cell.recordID].Set(cell.column, dResult);
            else
                DBInterface.db.records[cell.recordID].Set(cell.column, box.Text);

            Keyboard.ClearFocus();
        }

        async void HandleColumnReturn(System.Windows.Controls.TextBox box, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UpdateColumn(box);
            }
        }

        async void HandleColumnLostFocus(System.Windows.Controls.TextBox box)
        {
            await UpdateColumn(box);
        }

        private async Task UpdateColumn(System.Windows.Controls.TextBox box)
        {
            string oldName = visibleCells[box].column;

            try
            {
                UpdateStatusText("Updating records...");
                await DBInterface.db.RenameColumn(oldName, box.Text);
                visibleCells[box].column = box.Text;
                UpdateStatusText("Ready");

                Keyboard.ClearFocus();
            }
            catch (Exception ex)
            {
                UpdateStatusText(ex.Message);
            }
        }

        DBCell GetCellAt(int recordID, int column)
        {
            return visibleCells[cellIndices[new Tuple<int, int>(recordID, column)]];
        }

        public void DeselectAll()
        {
            foreach (DBCell cell in visibleCells.Values) cell.selected = false;
            selectedColumn = null;
            selectedRecord = -1;
            Keyboard.ClearFocus();
        }

        public async Task SelectRecord(int recordID)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                GetCellAt(recordID, i).selected = true;
            }

            await UpdateDBGrid();
        }

        public async Task SelectColumn(string column)
        {
            int colIndex = columns.IndexOf(column);
            for (int i = 0; i < visibleRecords; i++)
            {
                GetCellAt(i, colIndex).selected = true;
            }
            await UpdateDBGrid();
        }

        public async Task CreateNewDBGrid()
        {
            GC.Collect();

            topRecordIndex = 0;

            DBGrid.RowDefinitions.Clear();
            DBGrid.ColumnDefinitions.Clear();

            DBGrid.Children.Clear();

            cellIndices = new Dictionary<Tuple<int, int>, System.Windows.Controls.TextBox>();
            visibleCells = new Dictionary<System.Windows.Controls.TextBox, DBCell>();

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

                box.PreviewMouseDown += (obj, e) => HandleColumnShiftClick((System.Windows.Controls.TextBox)obj, e);
                box.KeyDown += (obj, e) => HandleColumnReturn((System.Windows.Controls.TextBox)obj, e);
                box.LostFocus += (obj, e) => HandleColumnLostFocus((System.Windows.Controls.TextBox)obj);

                if (!visibleCells.ContainsKey(box))
                {
                    visibleCells[box] = new DBCell()
                    {
                        column = columns[i],
                        recordID = -1
                    };
                }

                DBGrid.Children.Add(box);
                Grid.SetRow(box, 0);
                Grid.SetColumn(box, i);
            }
            DBGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        public async Task UpdateDBGrid()
        {
            for (int i = topRecordIndex; i < Math.Min(DBInterface.db.records.Count, topRecordIndex + visibleRecords); i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    if (cellIndices.ContainsKey(new Tuple<int, int>(i - topRecordIndex, j)))
                    {
                        cellIndices[new Tuple<int, int>(i - topRecordIndex, j)].Text = Convert.ToString(DBInterface.db.records[i].Get(columns[j]));
                    }
                    else
                    {
                        var box = new System.Windows.Controls.TextBox()
                        {
                            Text = Convert.ToString(DBInterface.db.records[i].Get(columns[j])),
                            FontSize = 14
                        };

                        DBGrid.Children.Add(box);
                        cellIndices[new Tuple<int, int>(i - topRecordIndex, j)] = box;
                        Grid.SetRow(box, i - topRecordIndex + 1);
                        Grid.SetColumn(box, j);

                        box.PreviewMouseDown += (obj, e) => HandleCellShiftClick((System.Windows.Controls.TextBox)obj, e);
                        box.KeyDown += (obj, e) => HandleCellReturn((System.Windows.Controls.TextBox)obj, e);
                        box.LostFocus += (obj, e) => HandleCellLostFocus((System.Windows.Controls.TextBox)obj);

                        if (!visibleCells.ContainsKey(box))
                        {
                            visibleCells[box] = new DBCell()
                            {
                                column = columns[j],
                                recordID = i
                            };
                        }
                    }

                    cellIndices[new Tuple<int, int>(i - topRecordIndex, j)].Background = new SolidColorBrush() { Color = GetCellAt(i - topRecordIndex, j).selected ? Color.FromRgb(210, 210, 210) : Color.FromRgb(255, 255, 255) };
                }
            }
        } 

        private async void FileNewBtn_Click(object sender, RoutedEventArgs e)
        {
            DBInterface.db = new Database();

            isBusy = false;
            hasFileLoaded = true;

            visibleRecords = 25;
            topRecordIndex = 0;

            shiftPressed = false;

            selectedColumn = null;
            selectedRecord = -1;

            await CreateNewDBGrid();
            await UpdateDBGrid();
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

                isBusy = false;
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
            try
            {
                if (selectedColumn == null)
                {
                    DBInterface.db.AddColumn("New Column");
                }
                else
                {
                    DBInterface.db.InsertColumn("New Column", columns.IndexOf(selectedColumn));
                }

                UpdateStatusText("Updating records...");
                await DBInterface.db.UpdateRecords();
                UpdateStatusText("Ready");

                await CreateNewDBGrid();
                await UpdateDBGrid();
            }
            catch (Exception)
            {
                UpdateStatusText("Can't insert new column.\nTry renaming New Column.");
            }
        }

        private async void DataDeleteColumnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedColumn != null)
            {
                DBInterface.db.RemoveColumn(selectedColumn);

                selectedColumn = null;

                UpdateStatusText("Updating records...");
                await DBInterface.db.UpdateRecords();
                UpdateStatusText("Ready");

                await CreateNewDBGrid();
                await UpdateDBGrid();
            }
        }

        private async void DataSortAscendingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedColumn != null)
            {
                await SortTask(SortingDirection.Ascending);
            }
        }

        private async void DataSortDescendingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedColumn != null)
            {
                await SortTask(SortingDirection.Descending);
            }
        }

        // TODO: Proper exception handling
        private async Task SortTask(SortingDirection dir)
        {
            UpdateStatusText($"Sorting by {selectedColumn}...");
            try
            {
                DBInterface.db.records = await DBInterface.db.SortByAsync(selectedColumn, dir);
                UpdateStatusText("Ready");
            }
            catch (Exception)
            {
                UpdateStatusText("Cannot sort!");
            }
            await UpdateDBGrid();
        }

        private async void DataFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon!", "Can't have everything, y'know", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            }
            catch (NullReferenceException)
            {
                return;
            }

            topRecordIndex = newIndex;

            await UpdateDBGrid();
        }

        private async void Grid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shiftPressed = true;

            if (e.Key == Key.Escape)
            {
                DeselectAll();
                await UpdateDBGrid();
            }
        }

        private void Grid_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shiftPressed = false;
        }
    }
}
