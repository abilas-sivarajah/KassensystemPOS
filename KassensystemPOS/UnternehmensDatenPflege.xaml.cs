using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace KassensystemPOS
{
    public partial class UnternehmensDatenPflege : Page
    {
        UnternnehmensDaten aktuell;

        public UnternehmensDatenPflege()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            aktuell = DB.GetUnternehmensDaten();

            if (aktuell != null)
            {
                this.DataContext    = aktuell;
                button_add.IsEnabled      = false;
                button_add_Copy.IsEnabled = true;
            }
            else
            {
                aktuell = new UnternnehmensDaten();
                this.DataContext    = aktuell;
                button_add.IsEnabled      = true;
                button_add_Copy.IsEnabled = false;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (aktuell == null) return;
            DB.UnternehmensDatenAktualisieren(aktuell);
            MessageBox.Show("Aktualisieren erfolgt");
        }

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            if (aktuell == null) return;
            DB.UnternehmensDatenHinzufuegen(aktuell);
            MessageBox.Show("Anlegen erfolgt");
            Page_Loaded(null, null);
        }

        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            DB.AlleUnternehmensDatenLoeschen();
            MessageBox.Show("Löschen erfolgt");
            Page_Loaded(null, null);
        }
    }
}
