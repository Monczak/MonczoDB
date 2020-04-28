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
using TextBox = System.Windows.Controls.TextBox;

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
        bool ctrlPressed = false;

        string selectedColumn = null;
        int selectedRecord = -1;

        bool columnChanged = false;

        bool suppressCellUpdate = false;

        bool fileHasChanges = false;

        Dictionary<Tuple<int, int>, TextBox> cellIndices;
        Dictionary<TextBox, DBCell> visibleCells;

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

        void HandleColumnTextChanged(TextBox box)
        {
            columnChanged = true;
        }

        async void HandleColumnShiftClick(TextBox box)
        {
            if (ctrlPressed)
            {
                DeselectAll();
                selectedColumn = box.Text;
                UpdateStatusText($"Selected {selectedColumn}");
                await SelectColumn(selectedColumn);
            }
        }

        async void HandleCellShiftClick(TextBox box)
        {
            // TODO: Maybe change this to Alt, so as to make selecting multiple records possible
            if (ctrlPressed)
            {
                DeselectAll();
                selectedRecord = visibleCells[box].recordID + topRecordIndex;
                UpdateStatusText($"Selected record {selectedRecord}");
                await SelectRecord(selectedRecord);
            }
        }

        void HandleCellReturn(TextBox box, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateCell(box);
            }
        }

        void HandleCellLostFocus(TextBox box)
        {
            UpdateCell(box);
        }

        private void UpdateCell(TextBox box)
        {
            if (!suppressCellUpdate)
            {
                DBCell cell = visibleCells[box];

                // Spaghetti ducktyping?
                if (box.Text.EndsWith(".0") && double.TryParse(box.Text, out double dResult))
                    DBInterface.db.records[cell.recordID + topRecordIndex].Set(cell.column, dResult);
                else if (int.TryParse(box.Text, out int iResult))
                    DBInterface.db.records[cell.recordID + topRecordIndex].Set(cell.column, iResult);
                else if (double.TryParse(box.Text, out double dResult2))
                    DBInterface.db.records[cell.recordID + topRecordIndex].Set(cell.column, dResult2);
                else
                    DBInterface.db.records[cell.recordID + topRecordIndex].Set(cell.column, box.Text);

                UpdateUnsavedChanges(true);
                Keyboard.ClearFocus();
            }
            
        }

        async void HandleColumnReturn(TextBox box, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UpdateColumn(box);
            }
        }

        async void HandleColumnLostFocus(TextBox box)
        {
            await UpdateColumn(box);
        }

        private async Task UpdateColumn(TextBox box)
        {
            if (columnChanged)
            {
                string oldName = visibleCells[box].column;

                try
                {
                    UpdateStatusText("Updating records...");
                    await DBInterface.db.RenameColumn(oldName, box.Text);
                    UpdateUnsavedChanges(true);
                    visibleCells[box].column = box.Text;
                    UpdateStatusText("Ready");

                    Keyboard.ClearFocus();
                }
                catch (Exception ex)
                {
                    UpdateStatusText(ex.Message);
                }
            }
            columnChanged = false;
            
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
                GetCellAt(recordID - topRecordIndex, i).selected = true;
            }

            await UpdateDBGrid();
        }

        public async Task SelectColumn(string column)
        {
            int colIndex = columns.IndexOf(column);
            for (int i = 0; i < Math.Min(DBInterface.db.records.Count, visibleRecords); i++)
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

            cellIndices = new Dictionary<Tuple<int, int>, TextBox>();
            visibleCells = new Dictionary<TextBox, DBCell>();

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

                var box = new TextBox()
                {
                    Text = columns[i],
                    FontSize = 14,
                    Background = new SolidColorBrush(new Color() { R = 0xdd, G = 0xdd, B = 0xdd, A = 0xff })
                };

                box.PreviewMouseDown += (obj, _) => HandleColumnShiftClick((TextBox)obj);
                box.KeyDown += (obj, e) => HandleColumnReturn((TextBox)obj, e);
                box.LostFocus += (obj, e) => HandleColumnLostFocus((TextBox)obj);

                box.TextChanged += (obj, _) => HandleColumnTextChanged((TextBox)obj);

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
                        var box = new TextBox()
                        {
                            Text = Convert.ToString(DBInterface.db.records[i].Get(columns[j])),
                            FontSize = 14
                        };

                        DBGrid.Children.Add(box);
                        cellIndices[new Tuple<int, int>(i - topRecordIndex, j)] = box;
                        Grid.SetRow(box, i - topRecordIndex + 1);
                        Grid.SetColumn(box, j);

                        box.PreviewMouseDown += (obj, _) => HandleCellShiftClick((TextBox)obj);
                        box.KeyDown += (obj, e) => HandleCellReturn((TextBox)obj, e);
                        box.LostFocus += (obj, e) => HandleCellLostFocus((TextBox)obj);

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

        private void UpdateUnsavedChanges(bool newStatus)
        {
            if (fileHasChanges = newStatus) // This is deliberate
                SetTitle($"{System.IO.Path.GetFileName(DBInterface.currentFilePath)} (*)");
            else
                SetTitle(System.IO.Path.GetFileName(DBInterface.currentFilePath));
        }

        private MessageBoxResult ShowUnsavedChangesWarning()
        {
            return MessageBox.Show("There are unsaved changes. Would you like to continue?", "Warning - Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        private async void FileNewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (fileHasChanges)
                {
                    if (ShowUnsavedChangesWarning() != MessageBoxResult.Yes)
                    {
                        isBusy = false;
                        return;
                    }
                }
                DBInterface.db = new Database();

                columns = new List<string>() { "New Column" };
                DBInterface.db.SetColumns(columns);
                DBInterface.db.AddRecordObject(new DBRecord(columns));

                isBusy = false;
                hasFileLoaded = true;

                visibleRecords = 25;
                topRecordIndex = 0;

                shiftPressed = false;
                ctrlPressed = false;

                UpdateUnsavedChanges(false);
                SetTitle(null);
                DBInterface.currentFilePath = null;

                selectedColumn = null;
                selectedRecord = -1;

                await CreateNewDBGrid();
                await UpdateDBGrid();
                isBusy = false;
            }
           
        }

        private async void FileLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;

                if (fileHasChanges)
                {
                    if (ShowUnsavedChangesWarning() != MessageBoxResult.Yes)
                    {
                        isBusy = false;
                        return;
                    }
                }

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
                    hasFileLoaded = true;
                    UpdateUnsavedChanges(false);

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

                UpdateUnsavedChanges(false);

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

                DBInterface.currentFilePath = filePath;

                UpdateUnsavedChanges(false);

                isBusy = false;
            }
            
        }

        private async void DataInsertRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (selectedRecord == -1)
                {
                    DBRecord newRecord = new DBRecord(columns);
                    DBInterface.db.AddRecordObject(newRecord);
                    topRecordIndex = Math.Max(0, DBInterface.db.records.Count - visibleRecords);
                }
                else
                {
                    DBInterface.db.InsertRecordObject(selectedRecord, new DBRecord(columns));
                }

                UpdateUnsavedChanges(true);

                DeselectAll();
                await UpdateDBGrid();

                UpdateStatusText("Ready");
                isBusy = false;
            }
        }

        private async void DataDeleteRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (selectedRecord != -1)
                {
                    DBInterface.db.RemoveRecordAt(selectedRecord);
                    DeselectAll();

                    UpdateUnsavedChanges(true);

                    await CreateNewDBGrid();
                    await UpdateDBGrid();
                    UpdateStatusText("Ready");
                }
                isBusy = false;
            }
        }

        private async void DataInsertColumnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
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

                    UpdateUnsavedChanges(true);

                    await CreateNewDBGrid();
                    await UpdateDBGrid();
                }
                catch (Exception)
                {
                    UpdateStatusText("Can't insert new column.\nTry renaming New Column.");
                }
                isBusy = false;
            }
           
        }

        private async void DataDeleteColumnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (selectedColumn != null)
                {
                    DBInterface.db.RemoveColumn(selectedColumn);

                    selectedColumn = null;

                    UpdateStatusText("Updating records...");
                    await DBInterface.db.UpdateRecords();
                    UpdateStatusText("Ready");

                    UpdateUnsavedChanges(true);

                    await CreateNewDBGrid();
                    await UpdateDBGrid();
                }
                isBusy = false;
            }
        }

        private async void DataSortAscendingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (selectedColumn != null)
                {
                    await SortTask(SortingDirection.Ascending);
                    UpdateUnsavedChanges(true);
                }
                else
                {
                    UpdateStatusText("Nothing to sort by.\nTry selecting a column.");
                }
                isBusy = false;
            }
            
        }

        private async void DataSortDescendingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                if (selectedColumn != null)
                {
                    await SortTask(SortingDirection.Descending);
                    UpdateUnsavedChanges(true);
                }
                else
                {
                    UpdateStatusText("Nothing to sort by.\nTry selecting a column.");
                }
                isBusy = false;
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
            if (selectedRecord != -1 && selectedRecord >= topRecordIndex && selectedRecord < topRecordIndex + visibleRecords)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    GetCellAt(selectedRecord - topRecordIndex, i).selected = false;
                }
            }

            topRecordIndex -= e.Delta / 120;
            topRecordIndex = Math.Min(topRecordIndex, DBInterface.db.records.Count - visibleRecords);
            topRecordIndex = Math.Max(topRecordIndex, 0);

            if (selectedRecord >= topRecordIndex && selectedRecord < topRecordIndex + visibleRecords)
                await SelectRecord(selectedRecord);

            if (DBInterface.db.records.Count >= visibleRecords)
                DBGridScrollBar.Value = (double)topRecordIndex / (DBInterface.db.records.Count - visibleRecords);

            await UpdateDBGrid();
        }

        private async void DBGridScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (selectedRecord != -1 && selectedRecord >= topRecordIndex && selectedRecord < topRecordIndex + visibleRecords)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    GetCellAt(selectedRecord - topRecordIndex, i).selected = false;
                }
            }

            int newIndex;
            try
            {
                newIndex = (int)Math.Round(e.NewValue * Math.Max(0, DBInterface.db.records.Count - visibleRecords));
            }
            catch (NullReferenceException)
            {
                return;
            }

            topRecordIndex = newIndex;

            if (selectedRecord >= topRecordIndex && selectedRecord < topRecordIndex + visibleRecords)
                await SelectRecord(selectedRecord);

            await UpdateDBGrid();
        }

        private async void Grid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shiftPressed = true;

            if (e.Key == Key.LeftCtrl)
                ctrlPressed = true;

            if (e.Key == Key.Escape)
            {
                DeselectAll();
                await UpdateDBGrid();
                UpdateStatusText("Ready");
            }
        }

        private void Grid_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shiftPressed = false;

            if (e.Key == Key.LeftCtrl)
                ctrlPressed = false;
        }
    }
}
