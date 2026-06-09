using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace KassensystemPOS
{
    public partial class Artikelverwaltung : Page
    {
        ListCollectionView displaylist;

        public Artikelverwaltung()
        {
            InitializeComponent();
            // Liste einmal beim Oeffnen laden. Kein Auto-Refresh-Timer mehr,
            // sonst wuerde die Auswahl zuruecksetzen und die Textboxen
            // beim Bearbeiten staendig geleert werden.
            Refresh();
        }

        public void Refresh()
        {
            displaylist = new ListCollectionView(DB.GetAlleArtikel());
            parentGrid.DataContext = displaylist;
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tb_artikelnr.Text, out int nr)) return;
            DB.ArtikelLoeschen(nr);
            MessageBox.Show("Löschen erfolgt");
            Refresh();
        }

        private void btn_update_Click(object sender, RoutedEventArgs e)
        {
            var a = (Artikel)displaylist.CurrentItem;
            if (a == null) return;
            DB.ArtikelAktualisieren(a);
            MessageBox.Show("Aktualisieren erfolgt");
            Refresh();
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            new Artikel_Hinzufügen().Show();
        }
    }
}
