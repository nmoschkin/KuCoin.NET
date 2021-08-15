using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Rest;
using Kucoin.NET.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuCoinConsole
{
    public partial class Form1 : Form
    {
        int FeedStateCol = 3;
        int FullNameCol = 4;
        int TimeCol = 5;
        int QueueCol = 6;

        List<ISymbolDataService> observers;

        public Timer timer1 = new Timer();

        public Form1()
        {
            Kucoin.NET.Helpers.Dispatcher.Initialize();
            InitializeComponent();
            this.Activated += Form1_Activated;

            listView1.ListViewItemSorter = Comparer<ListViewItem>.Create(new Comparison<ListViewItem>((a, b) => {

                if (a.Tag is ISymbolDataService obs1 && b.Tag is ISymbolDataService obs2)
                {
                    if (obs1.Level3Observation != null && obs2.Level3Observation != null)
                    {
                        if (obs1.Level3Observation.GrandTotal > obs2.Level3Observation.GrandTotal) return -1;
                        else if (obs1.Level3Observation.GrandTotal < obs2.Level3Observation.GrandTotal) return 1;
                        else return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }

            }));

            timer1.Interval = 100;
            timer1.Tick += Timer1_Tick;
            timer1.Enabled = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            DoUpdate();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!Dispatcher.Initialized)
            {
                Dispatcher.Initialize();
            }
            Console.SetWindowPosition(Console.WindowLeft, Console.WindowTop);
        }

        public void InitializeSymbols()
        {
            observers = new List<ISymbolDataService>(Program.Observers.Values);

            foreach (var obs in observers)
            {
                bool p = false;
                foreach (ListViewItem itemtest in listView1.Items)
                {
                    if (itemtest.Tag == obs)
                    {
                        p = true;
                        break;
                    }

                }

                if (p) continue;

                var item = listView1.Items.Add(obs.Symbol);
                var l3 = obs.Level3Observation;
                var book = l3.FullDepthOrderBook;
                item.UseItemStyleForSubItems = false;

                if (book != null)
                {
                    item.SubItems.Add(book.Asks[0].Price.ToString("#,##0.00########")).ForeColor = Color.Green;
                    item.SubItems.Add(book.Bids[0].Price.ToString("#,##0.00########")).ForeColor = Color.Red;
                }
                else
                {
                    item.SubItems.Add(("0")).ForeColor = Color.Green;
                    item.SubItems.Add(("0")).ForeColor = Color.Red;
                }

                item.SubItems.Add(l3.State.ToString());
                string bctest, curr = obs.Symbol;

                try
                {
                    bctest = Market.Instance.Symbols[obs.Symbol].BaseCurrency;
                    curr = obs.Symbol;

                    if (Market.Instance.Currencies.TryGetValue(bctest, out MarketCurrency currency))
                    {
                        curr = currency?.FullName ?? curr;
                    }
                }
                catch { }

                item.SubItems.Add(curr);

                if (book != null)
                {
                    item.SubItems.Add(book.Timestamp.ToString("g"));
                }
                else
                {
                    item.SubItems.Add("");
                }

                item.SubItems.Add(l3.QueueLength.ToString());
                item.Tag = obs;
            }

            listView1.Sort();
        }

        public void DoUpdate()
        {
            if (observers == null || observers.Count != listView1.Items.Count || observers.Count != Program.Observers.Count)
            {
                InitializeSymbols();
            }

            else
            {
                try
                {
                    listView1.BeginUpdate();

                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.Tag is ISymbolDataService obs)
                        {
                            var l3 = obs.Level3Observation;
                            var book = l3.FullDepthOrderBook;

                            if (book != null)
                            {
                                item.SubItems[0].Text = book.Asks[0].Price.ToString("#,##0.00########");
                                item.SubItems[1].Text = book.Bids[0].Price.ToString("#,##0.00########");
                                item.SubItems[TimeCol].Text = book.Timestamp.ToString("g");

                            }

                            item.SubItems[QueueCol].Text = l3.QueueLength.ToString();
                            item.SubItems[FeedStateCol].Text = l3.State.ToString();
                        }

                    }
                }
                catch
                {

                }
                finally
                {
                    listView1.Sort();
                    listView1.EndUpdate();
                }
            }
        }
    }

    class ListViewNF : System.Windows.Forms.ListView
    {
        public ListViewNF()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }
}
