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

                Task<Database> loadTask = Database.DeserializeAsync(File.OpenRead("test.dat"));
                loadTask.Wait();
                db = loadTask.Result;

                Console.WriteLine("Loaded!");

                Console.WriteLine(string.Join("\t", db.GetColumns()));
                for (int i = 0; i < db.records.Count; i++)
                {
                    foreach (dynamic val in db.records[i].GetValues())
                    {
                        Console.Write(Convert.ToString(val));
                        Console.Write("\t");
                    }

                    Console.WriteLine();
                    

                }
            }
            else
            {
                Console.WriteLine("Creating a test db...");

                db = new Database();
                db.Initialize();

                db.SetColumns(new List<string> { "Id", "Name", "Last Name", "Orders" });

                Random random = new Random();
                for (int i = 0; i < 100000; i++)
                {
                    db.AddRecord(i + 1, "Blahus", "Maximus", random.Next(100000));
                }

                Console.WriteLine("Saving...");
                Task saveTask = db.SerializeAsync(File.OpenWrite("test.dat"));
                saveTask.Wait();
                Console.WriteLine("Saved!");

                Console.WriteLine("Finished writing test db!");
            }

            Console.ReadKey();
        }
    }
}
