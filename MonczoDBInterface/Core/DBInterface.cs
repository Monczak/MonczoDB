using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonczoDB;

namespace MonczoDBInterface.Core
{
    public class DBInterface
    {
        public static Database db;

        public static string currentFilePath = null;

        public delegate void OnDBLoadedDelegate();
        public static event OnDBLoadedDelegate OnDBLoaded;

        public static async Task LoadFromFile(string path)
        {
            Task<Database> loadTask;
            try
            {
                loadTask = Database.DeserializeAsync(File.OpenRead(path));
                await loadTask;
                db = loadTask.Result;
            }
            catch (FileNotFoundException e)
            {
                return;
            }
            catch (Exception e)
            {
                return;
            }

            currentFilePath = path;
            OnDBLoaded?.Invoke();
        }

        public static async Task SaveFile(string path)
        {
            Task saveTask;
            try
            {
                saveTask = db.SerializeAsync(File.OpenWrite(path));
                await saveTask;
            }
            catch (FileNotFoundException e)
            {
                return;
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}
