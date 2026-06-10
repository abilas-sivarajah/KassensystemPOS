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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            aktuell = await DB.GetUnternehmensDatenAsync();

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

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (aktuell == null) return;
            await DB.UnternehmensDatenAktualisierenAsync(aktuell);
            MessageBox.Show("Aktualisieren erfolgt");
        }

        private async void button_add_Click(object sender, RoutedEventArgs e)
        {
            if (aktuell == null) return;
            await DB.UnternehmensDatenHinzufuegenAsync(aktuell);
            MessageBox.Show("Anlegen erfolgt");
            Page_Loaded(null, null);
        }

        private async void button_del_Click(object sender, RoutedEventArgs e)
        {
            await DB.AlleUnternehmensDatenLoeschenAsync();
            MessageBox.Show("Löschen erfolgt");
            Page_Loaded(null, null);
        }
    }
}
