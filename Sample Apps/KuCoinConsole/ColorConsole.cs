using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinConsole
{

    public static class ColorConsole
    {
        private static Dictionary<TableContext, List<int>> tableCache = new Dictionary<TableContext, List<int>>();

        public static Dictionary<TableContext, List<int>> TableCache
        {
            get => tableCache;
            set
            {
                tableCache = value;
            }
        }

        public static void WriteColor(string text, ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            var oldColor = Console.ForegroundColor;
            var oldBack = Console.BackgroundColor;

            Console.ForegroundColor = color;
            if (backgroundColor != null)
            {
                Console.BackgroundColor = (ConsoleColor)backgroundColor;
            }
            Console.Write(text);

            Console.ForegroundColor = oldColor;
            Console.BackgroundColor = oldBack;
        }

        public static void WriteColorLine(string text, ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            var oldColor = Console.ForegroundColor;
            var oldBack = Console.BackgroundColor;

            Console.ForegroundColor = color;
            if (backgroundColor != null)
            {
                Console.BackgroundColor = (ConsoleColor)backgroundColor;
            }

            Console.WriteLine(text);

            Console.ForegroundColor = oldColor;
            Console.BackgroundColor = oldBack;
        }


        public static List<int> GetCreateTableContext(TableContext context)
        {
            if (tableCache.ContainsKey(context))
            {
                return tableCache[context];
            }
            else
            {
                var n = new List<int>();
                tableCache.Add(context, n);

                return n;
            }
        }

        public static List<int> AutoContext(params string[] columns)
        {
            var l = new List<int>();

            foreach (var c in columns)
            {
                l.Add(c.Length + 1);
            }

            return l;
        }

        public static void WriteRow(TableContext context, params string[] columns)
        {
            List<int> cols;
            int c = columns.Length;

            if (!tableCache.ContainsKey(context))
            {
                cols = AutoContext(columns);
            }
            else
            {
                cols = new List<int>(tableCache[context]);

                if (cols.Count < c)
                {
                    var col2 = AutoContext(columns);

                    for (int j = 0; j < cols.Count; j++)
                    {
                        col2[j] = cols[j];
                    }

                    cols = col2;
                }
            }

            for (int i = 0; i < c; i++)
            {
                Write(FixedText(columns[i], cols[i]));
            }

        }



        public static void Write(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var stext = new StringBuilder();
            var sb = new StringBuilder();

            int i, c = text.Length;

            var oldColor = Console.ForegroundColor;

            for (i = 0; i < c; i++)
            {
                if (text[i] == '{')
                {
                    if (stext.Length != 0)
                    {
                        Console.Write(stext.ToString());
                        stext.Clear();
                    }

                    i++;
                    while (text[i] != '}')
                    {
                        sb.Append(text[i++]);
                    }

                    var color = sb.ToString();
                    if (color == "Reset")
                    {                        
                        Console.ResetColor();
                    }
                    else if (color == "ForegroundReset")
                    {
                        var oldBack = Console.BackgroundColor;
                        Console.ResetColor();
                        Console.BackgroundColor = oldBack;
                    }
                    else if (color == "BackgroundReset")
                    {
                        var oldFore = Console.ForegroundColor;
                        Console.ResetColor();
                        Console.ForegroundColor = oldFore;
                    }
                    else if (color.StartsWith("Background"))
                    {
                        color = color.Substring(10);
                        var val = (ConsoleColor?)typeof(ConsoleColor).GetField(color)?.GetValue(null);

                        if (val is ConsoleColor cc)
                        {
                            Console.BackgroundColor = cc;
                        }
                    }
                    else
                    {
                        var val = (ConsoleColor?)typeof(ConsoleColor).GetField(color)?.GetValue(null);

                        if (val is ConsoleColor cc)
                        {
                            Console.ForegroundColor = cc;
                        }
                    }
                    sb.Clear();
                }
                else
                {
                    stext.Append(text[i]);
                }
            }

            if (stext.Length > 0) Console.Write(stext);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteLine(string text)
        {
            Write(text + "\r\n");
        }


        public static void WriteToEdge(this StringBuilder sb, string text)
        {
            int c = RealCount(text);
            int d = Console.WindowWidth - 2;

            if (c >= d)
            {
                if (c > d) text = text.Substring(0, d);
                sb.Append(text);

                return;
            }

            sb.Append(text);
            sb.Append(new string(' ', d - c));
        }

        public static void WriteToEdgeLine(this StringBuilder sb, string text)
        {
            int c = RealCount(text);
            int d = Console.WindowWidth - 2;

            if (c >= d)
            {
                if (c > d) text = text.Substring(0, d);
                sb.Append(text);

                return;
            }

            sb.Append(text);
            sb.AppendLine(new string(' ', d - c));
        }

        public static int RealCount(string str)
        {
            int i, c = str.Length;
            int d = 0;

            for (i = 0; i < c; i++)
            {
                if (str[i] == '{')
                {
                    while (str[i] != '}')
                    {
                        i++;
                    }
                }
                else
                {
                    d++;
                }


            }

            return d;
        }

        public static string FixedText(string str, int length)
        {
            int c = RealCount(str);
            if (c > length) return str;

            return str + new string(' ', length - c);
        }


    }

    /// <summary>
    /// Token to use when formatting tables.
    /// </summary>
    public struct TableContext : IEquatable<TableContext>, IComparable<TableContext>
    {

        /// <summary>
        /// Represents an invalid table context.
        /// </summary>
        public static readonly TableContext Invalid = new TableContext()
        {
            Guid = Guid.Empty
        };

        private Guid Guid;

        public static TableContext Create()
        {
            return new TableContext()
            {
                Guid = Guid.NewGuid()
            };
        }

#if DOTNETSTD
        public override bool Equals(object obj)
#else
        public override bool Equals([NotNullWhen(true)] object obj)
#endif
        {
            if (obj is TableContext rc)
            {
                return Equals(rc);
            }
            else if (obj is Guid g)
            {
                return Guid.Equals(g);
            }
            return base.Equals(obj);
        }

        public bool Equals(TableContext obj)
        {
            return obj.Guid == Guid;
        }

        public int CompareTo(TableContext other)
        {
            return Guid.CompareTo(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public override string ToString()
        {
            return Guid.ToString("d");
        }

        public static bool operator ==(TableContext left, TableContext right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TableContext left, TableContext right)
        {
            return !left.Equals(right);
        }

    }


}
