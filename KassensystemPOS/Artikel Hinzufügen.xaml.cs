using System;
using System.Threading.Tasks;
using System.Windows;

namespace KassensystemPOS
{
    public partial class Artikel_Hinzufügen : Window
    {
        public Artikel_Hinzufügen()
        {
            InitializeComponent();
            _ = LadeKategorienAsync(); // Kategorien im Hintergrund laden
        }

        private async Task LadeKategorienAsync()
        {
            // ComboBox zeigt "Kategoriename", liefert per SelectedValue die "Kategorie_Nr".
            cb_kategorie.ItemsSource = await DB.GetAlleKategorienAsync();
        }

        private async void Artikel_Add(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tb_Artikelnummer.Text, out int artikelnr))
            {
                MessageBox.Show("Die Artikelnummer muss eine Zahl sein");
                return;
            }

            if (cb_kategorie.SelectedValue == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen");
                return;
            }
            int kategorieNr = (int)cb_kategorie.SelectedValue;

            var a = new Artikel
            {
                Artikelnr  = artikelnr,
                Artikelbez = tb_Artikelbez.Text,
                Nettopreis = Convert.ToDecimal(tb_Artikelpreis.Text),
                Kategorie  = kategorieNr,
                Steuersatz = int.Parse(tb_Steuersatz.Text)
            };

            await DB.ArtikelHinzufuegenAsync(a);
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
