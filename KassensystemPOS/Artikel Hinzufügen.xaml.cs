using System;
using System.Windows;

namespace KassensystemPOS
{
    public partial class Artikel_Hinzufügen : Window
    {
        public Artikel_Hinzufügen()
        {
            InitializeComponent();
        }

        private void AddArtikel_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in DB.GetAlleKategorien())
                cb_kategorie.Items.Add(item.Kategoriename);
        }

        private void Artikel_Add(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tb_Artikelnummer.Text, out int artikelnr))
            {
                MessageBox.Show("Die Artikelnummer muss eine Zahl sein");
                return;
            }

            var kat = DB.GetKategorieByName((string)cb_kategorie.SelectedValue);
            if (kat == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen");
                return;
            }

            var a = new Artikel
            {
                Artikelnr  = artikelnr,
                Artikelbez = tb_Artikelbez.Text,
                Nettopreis = Convert.ToDecimal(tb_Artikelpreis.Text),
                Kategorie  = kat.Kategorie_Nr,
                Steuersatz = int.Parse(tb_Steuersatz.Text)
            };

            DB.ArtikelHinzufuegen(a);
            MessageBox.Show("Artikel erfolgreich hinzugefügt");
            ((MainWindow)Application.Current.MainWindow).Artverwaltung.Refresh();
            Tb_Clear();
        }

        public void Tb_Clear()
        {
            tb_Artikelbez.Clear();
            tb_Artikelnummer.Clear();
            tb_Artikelpreis.Clear();
            tb_Steuersatz.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
