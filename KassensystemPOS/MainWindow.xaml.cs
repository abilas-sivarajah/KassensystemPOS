using System.Windows;
using System.Windows.Controls;

namespace KassensystemPOS
{
    public partial class MainWindow : Window
    {
        public Artikelverwaltung Artverwaltung;
        POS pos_page;
        Rechnungen rechnungs_page;
        Abrechnung abrechnungs_page;
        UnternehmensDatenPflege udp;

        public MainWindow()
        {
            InitializeComponent();
            udp              = new UnternehmensDatenPflege();
            rechnungs_page   = new Rechnungen();
            abrechnungs_page = new Abrechnung();
            Artverwaltung    = new Artikelverwaltung();
            pos_page         = new POS();

            frame.Content = pos_page; // POS direkt beim Start anzeigen
        }

        private void POS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = pos_page;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Artverwaltung.Refresh();
            frame.Content = Artverwaltung;
        }

        private void Layout_Rechts_Click(object sender, RoutedEventArgs e)
        {
            pos_page.SetzeLayout(false);
            Properties.Settings.Default.Linkshaender = false;
            Properties.Settings.Default.Save(); // dauerhaft merken
            frame.Content = pos_page;
        }

        private void Layout_Links_Click(object sender, RoutedEventArgs e)
        {
            pos_page.SetzeLayout(true);
            Properties.Settings.Default.Linkshaender = true;
            Properties.Settings.Default.Save(); // dauerhaft merken
            frame.Content = pos_page;
        }

        private void AlleRechnungen(object sender, RoutedEventArgs e)
        {
            rechnungs_page.Laden();
            frame.Content = rechnungs_page;
        }

        private void Abrechnung(object sender, RoutedEventArgs e)
        {
            frame.Content = abrechnungs_page;
        }

        private void unternehmn_click(object sender, RoutedEventArgs e)
        {
            frame.Content = udp;
        }
    }
}
