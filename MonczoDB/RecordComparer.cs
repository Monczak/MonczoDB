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
            if (x == null)
            {
                if (y == null) return 0;
                return direction == SortingDirection.Ascending ? -1 : 1;
            }
            if (y == null)
            {
                if (x == null) return 0;
                return direction == SortingDirection.Ascending ? 1 : -1;
            }

            if (x.Get(column) == null && y.Get(column) == null)
                return 0;

            if (x.Get(column) == null)
                return direction == SortingDirection.Ascending ? -1 : 1;
            if (y.Get(column) == null)
                return direction == SortingDirection.Ascending ? 1 : -1;

            if (x.Get(column).GetType() != y.Get(column).GetType()) return Convert.ToString(x.Get(column)).CompareTo(Convert.ToString(y.Get(column)));

            return x.Get(column).CompareTo(y.Get(column)) * (direction == SortingDirection.Ascending ? 1 : -1);
        }
    }
}
