using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                Console.WriteLine("Loading from file...");
                db = Database.Deserialize(File.OpenRead("test.dat"));
                Console.WriteLine("Loaded!");

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

                Random random = new Random();
                for (int i = 0; i < 100; i++)
                {
                    db.AddRecord(i + 1, "Blahus", "Maximus", random.Next(100));
                }

                db.Serialize(File.OpenWrite("test.dat"));
            }

            Console.ReadKey();
        }
    }
}
