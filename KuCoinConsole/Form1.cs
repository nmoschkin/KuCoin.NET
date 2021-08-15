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
        List<ISymbolDataService> observers;

        private Timer timer1 = new Timer();

        public Form1()
        {
            InitializeComponent();
            this.Activated += Form1_Activated;
            timer1.Enabled = false;
            timer1.Interval = 50;
            timer1.Tick += Timer1_Tick;
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
            timer1.Enabled = false;

            observers = new List<ISymbolDataService>(Program.Observers.Values);

            listView1.Items.Clear();

            foreach (var obs in observers)
            {
                if (obs.Level3Observation == null || obs.Level3Observation.FullDepthOrderBook == null) continue;

                var item = listView1.Items.Add(obs.Symbol);
                var l3 = obs.Level3Observation;
                var book = l3.FullDepthOrderBook;

                item.SubItems.Add(book.Asks[0].Price.ToString("#,##0.00########")).ForeColor = Color.Green;
                item.SubItems.Add(book.Bids[0].Price.ToString("#,##0.00########")).ForeColor = Color.Red;

                item.SubItems.Add(l3.QueueLength.ToString());
                string bctest = "", curr = obs.Symbol;

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
                item.SubItems.Add(book.Timestamp.ToString("g"));
                item.Tag = obs;
            }

            timer1.Enabled = true;

        }

        public void DoUpdate()
        {

            if (observers.Count != listView1.Items.Count)
            {
                InitializeSymbols();
            }

            else
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Tag is ISymbolDataService obs)
                    {
                        var l3 = obs.Level3Observation;
                        var book = l3.FullDepthOrderBook;

                        item.SubItems[0].Text = book.Asks[0].Price.ToString("#,##0.00########");
                        item.SubItems[1].Text = book.Bids[0].Price.ToString("#,##0.00########");
                        item.SubItems[3].Text = l3.QueueLength.ToString();
                        item.SubItems[4].Text = book.Timestamp.ToString("g");

                    }

                }
            }
        }

    }
}
