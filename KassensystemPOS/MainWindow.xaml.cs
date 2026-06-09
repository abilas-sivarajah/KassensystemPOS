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

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            pos_page.posGridButton.SetValue(Grid.ColumnProperty, 1);
            pos_page.posGridListView.SetValue(Grid.ColumnProperty, 0);
            pos_page.posGridNumpad.SetValue(Grid.ColumnProperty, 0);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e) { }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e) { }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e) { }

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
