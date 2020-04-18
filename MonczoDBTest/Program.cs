using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MonczoDB;

namespace MonczoDBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db;

            if (File.Exists("test.dat"))
            {
                db = Database.Deserialize(File.OpenRead("test.dat"));

                Console.WriteLine(string.Join("\t", db.columns));

                foreach (DBRecord record in db.records)
                {
                    foreach (dynamic val in record.fields.Values)
                    {
                        Console.Write(Convert.ToString(val));
                        Console.Write("\t");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                db = new Database();
                db.Initialize();

                List<string> columns = new List<string> { "Id", "Name", "Last Name", "Orders" };
                db.SetColumns(columns);

                db.AddRecord(0, "John", "Doe", 69);
                db.AddRecord(1, "Jebediah", "Kerman", 420);

                db.Serialize(File.OpenWrite("test.dat"));
            }

            Console.ReadKey();
        }
    }
}
