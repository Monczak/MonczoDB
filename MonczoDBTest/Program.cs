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

                db.SortBy("Name", SortingDirection.Ascending);

                Console.WriteLine(string.Join("\t", db.GetColumns()));

                foreach (DBRecord record in db.records)
                {
                    foreach (dynamic val in record.GetValues())
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

                db.SetColumns(new List<string> { "Id", "Name", "Last Name", "Orders" });

                db.AddRecord(0, "John", "Doe", 69);
                db.AddRecord(1, "Jebediah", "Kerman", 420);
                db.AddRecord(2, "Blahus", "Maximus", 1);
                db.AddRecord(3, "Grant", "Sanderson", 1729);
                db.AddRecord(4, "Rambox", "Rambox", 11);
                db.AddRecord(5, "Compr", "Thebest", 1337);

                db.Serialize(File.OpenWrite("test.dat"));
            }

            Console.ReadKey();
        }
    }
}
