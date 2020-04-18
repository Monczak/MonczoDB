using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MonczoDB
{
    [Serializable]
    public class DBRecord
    {
        private Dictionary<string, dynamic> fields;

        public DBRecord(List<string> columns)
        {
            CreateEmpty(columns);
        }

        public DBRecord(List<string> columns, params dynamic[] values)
        {
            Create(columns, values);
        }

        public void CreateEmpty(List<string> columns)
        {
            fields = new Dictionary<string, dynamic>();
            foreach (string column in columns)
            {
                fields.Add(column, null);
            }
        }

        public void Create(List<string> columns, params dynamic[] values)
        {
            fields = new Dictionary<string, dynamic>();
            for (int i = 0; i < columns.Count; i++)
                fields.Add(columns[i], values[i]);
        }

        public List<dynamic> GetValues()
        {
            return fields.Values.ToList();
        }

        public T Get<T>(string column)
        {
            return (T)fields[column];
        }

        public dynamic Get(string column)
        {
            return fields[column];
        }

        public void Set(string column, dynamic value)
        {
            fields[column] = value;
        }
    }
}
