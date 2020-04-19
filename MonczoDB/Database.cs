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
        private List<string> columns;
        public List<DBRecord> records;

        public Dictionary<string, Type> columnTypes;

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
        }

        public List<string> GetColumns()
        {
            return columns;
        }

        public void SetColumns(List<string> cols)
        {
            columns = cols;
            columnTypes = new Dictionary<string, Type>(cols.Count);
        }

        public void AddColumn(string col)
        {
            columns.Append(col);
            columnTypes.Add(col, null);
        }

        public void RemoveColumn(string col)
        {
            columns.Remove(col);
            columnTypes.Remove(col);
        }

        public void RemoveColumnAt(int index)
        {
            columnTypes.Remove(columns[index]);
            columns.RemoveAt(index);
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
    }
}
