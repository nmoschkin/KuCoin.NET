using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Helpers
{
    public abstract class ComparingBase<T> 
    {
        protected Comparison<T> comp;
        protected IComparer<T> comparer;

        public ComparingBase(IComparer<T> comparer)
        {
            if (comparer == null)
            {
                typeof(T).GetInterfaceMap(typeof(IComparable<T>));

                comp = new Comparison<T>((x, y) =>
                {
                    if (x is IComparable<T> a && y is T b)
                    {
                        return a.CompareTo(b);
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                });
            }
            else
            {
                this.comparer = comparer;
                comp = comparer.Compare;
            }
        }

    }
}
