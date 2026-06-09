using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace KassensystemPOS
{
    public partial class Rechnungen : Page
    {
        List<Transaktionen> rechnungen;
        DispatcherTimer _refreshTimer;

        public Rechnungen()
        {
            InitializeComponent();
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(10);
            _refreshTimer.Tick += (s, e) => Laden();
            _refreshTimer.Start();
        }

        public void Laden()
        {
            rechnungen = DB.GetAlleTransaktionen();
            dataGrid.DataContext = rechnungen;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rechnungen == null) return;

            if (textBox.Text != "" && int.TryParse(textBox.Text, out int id))
                dataGrid.DataContext = rechnungen.Where(x => x.RechnungsID == id).ToList();
            else
                dataGrid.DataContext = rechnungen;
        }
    }
}
