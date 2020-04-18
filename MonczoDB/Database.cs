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
        public List<string> columns;
        public List<DBRecord> records;

        public void Initialize()
        {
            columns = new List<string>();
            records = new List<DBRecord>();
        }

        public void SetColumns(List<string> cols)
        {
            columns = cols;
        }

        // TODO: Convert this to insert records directly into file
        public void InsertRecord(int index, params dynamic[] values)
        {
            records.Insert(index, new DBRecord(columns, values));
        }

        public void AddRecord(params dynamic[] values)
        {
            records.Add(new DBRecord(columns, values));
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
