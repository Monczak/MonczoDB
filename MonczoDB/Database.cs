using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace MonczoDB
{
    [Serializable]
    public class Database
    {
        public List<string> columns;
        public List<DBRecord> records;

        public Dictionary<string, Type> columnTypes;

        public List<string> addedColumns, removedColumns;

        public Database()
        {
            Initialize();
        }
        public Database(List<string> columns)
        {
            Initialize();
            SetColumns(columns);
        }

        public void Initialize()
        {
            records = new List<DBRecord>();

            addedColumns = new List<string>();
            removedColumns = new List<string>();
        }

        public List<string> GetColumns()
        {
            return columns;
        }

        public void SetColumns(List<string> cols)
        {
            columns = cols;
        }

        public void AddColumn(string col)
        {
            if (addedColumns == null) addedColumns = new List<string>();

            if (columns.Contains(col))
                throw new Exception("Cannot add existing column");

            columns.Add(col);
            addedColumns.Add(col);
        }

        public void InsertColumn(string col, int index)
        {
            if (addedColumns == null) addedColumns = new List<string>();

            if (columns.Contains(col))
                throw new Exception("Cannot add existing column");

            columns.Insert(index, col);
            addedColumns.Add(col);
        }

        public void RemoveColumn(string col)
        {
            if (removedColumns == null) removedColumns = new List<string>();

            columns.Remove(col);
            removedColumns.Add(col);
        }

        public void RemoveColumnAt(int index)
        {
            if (removedColumns == null) removedColumns = new List<string>();

            removedColumns.Add(columns[index]);
            columns.RemoveAt(index);
        }

        public Task RenameColumn(string oldName, string newName)
        {
            if (columns.Contains(newName))
            {
                throw new Exception($"Column {newName} already exists!");
            }

            Task task = Task.Factory.StartNew(() =>
            {
                foreach (DBRecord record in records)
                {
                    dynamic value = record.Get(oldName);
                    record.fields.Remove(oldName);
                    record.fields[newName] = value;
                }

                columns[columns.IndexOf(oldName)] = newName;
            });

            return task;
        }

        public void InsertRecord(int index, params dynamic[] values)
        {
            records.Insert(index, new DBRecord(columns, values));
        }

        public void AddRecord(params dynamic[] values)
        {
            records.Add(new DBRecord(columns, values));
        }

        public void RemoveRecordAt(int index)
        {
            records.RemoveAt(index);
        }

        public Task<BitArray> GetFilterMaskAsync<T>(string column, Func<T, bool> Predicate)
        {
            BitArray result = new BitArray(records.Count);
            Task<BitArray> task = Task<BitArray>.Factory.StartNew(() =>
            {
                for (int i = 0; i < records.Count; i++)
                    result[i] = Predicate(records[i].Get(column));
                return result;
            });
            return task;
        }

        public List<DBRecord> FilterBy<T>(string column, Func<T, bool> Predicate)
        {
            return records.Where(r => Predicate(r.Get(column))).ToList();
        }

        public Task<List<DBRecord>> SortByAsync(string column, SortingDirection direction)
        {
            List<DBRecord> sortedRecords = new List<DBRecord>(records);
            Task<List<DBRecord>> task = Task<List<DBRecord>>.Factory.StartNew(() =>
            {
                RecordComparer comparer = new RecordComparer(column, direction);
                sortedRecords.Sort(comparer);
                return sortedRecords;
            });

            return task;
        }

        public Task SerializeAsync(Stream stream)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                using (stream)
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(gZipStream, this);
                    }
                }
                stream.Close();
            });

            return task;
        }

        public static Task<Database> DeserializeAsync(Stream stream)
        {
            Database result = new Database();

            Task<Database> task = Task<Database>.Factory.StartNew(() =>
            {
                using (stream)
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        result = (Database)formatter.Deserialize(gZipStream);
                    }
                }
                stream.Close();
                return result;
            });

            return task;
        }

        public Task UpdateRecords()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                if (addedColumns != null)
                {
                    foreach (string addedCol in addedColumns)
                    {
                        foreach (DBRecord record in records)
                        {
                            record.fields.Add(addedCol, null);
                        }
                    }
                    addedColumns.Clear();
                }

                if (removedColumns != null)
                {
                    foreach (string removedCol in removedColumns)
                    {
                        foreach (DBRecord record in records)
                        {
                            record.fields.Remove(removedCol);
                        }
                    }
                    removedColumns.Clear();
                }
            });

            return task;
        }
    }
}
