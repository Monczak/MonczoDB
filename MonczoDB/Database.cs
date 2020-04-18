using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

        public List<DBRecord> SortBy(string column, SortingDirection direction)
        {
            List<DBRecord> sortedRecords = records;

            RecordComparer comparer = new RecordComparer(column, direction);
            sortedRecords.Sort(comparer);

            return sortedRecords;
        }

        public void Serialize(Stream stream)
        {
            using (stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
            stream.Close();
        }

        public static Database Deserialize(Stream stream)
        {
            Database result = (Database)Activator.CreateInstance(typeof(Database));

            using (stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                result = (Database)formatter.Deserialize(stream);
            }

            return result;
        }
    }
}
