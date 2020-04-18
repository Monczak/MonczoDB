using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonczoDB
{
    class RecordComparer : IComparer<DBRecord>
    {
        public string column;
        public SortingDirection direction;

        public RecordComparer(string col, SortingDirection dir)
        {
            column = col;
            direction = dir;
        }

        public int Compare(DBRecord x, DBRecord y)
        {
            return x.Get(column).CompareTo(y.Get(column)) * (direction == SortingDirection.Ascending ? 1 : -1);
        }
    }
}
