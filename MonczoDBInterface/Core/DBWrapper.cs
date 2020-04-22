using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonczoDB;

namespace MonczoDBInterface.Core
{
    public class DBWrapper
    {
        public static Database db;

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

            OnDBLoaded?.Invoke();

            return;
        }
    }
}
