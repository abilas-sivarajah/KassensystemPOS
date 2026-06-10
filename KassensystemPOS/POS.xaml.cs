using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KassensystemPOS
{
    public class RechnungListe
    {
        public int     ArtikelNummer { get; set; }
        public string  ArtikelBez   { get; set; }
        public int     Menge        { get; set; }
        public decimal Einzelpreis  { get; set; }   // Netto-Preis pro Stueck
        public decimal NettoPreis   { get; set; }   // Einzelpreis * Menge
        public decimal Steuer       { get; set; }
        public decimal BruttoPreis  { get; set; }
    }

    public partial class POS : Page
    {
        // Merkt sich, welche Textbox zuletzt den Fokus hatte (Artikelnummer oder Menge).
        // Das Numpad schreibt immer in dieses Feld.
        private TextBox _aktivesFeld;

        // EINE Quelle der Wahrheit fuer den Warenkorb statt 6 parallelen Listen.
        // ObservableCollection: die ListView aktualisiert sich automatisch mit.
        public ObservableCollection<RechnungListe> Warenkorb { get; }
            = new ObservableCollection<RechnungListe>();

        public POS()
        {
            InitializeComponent();
            lv_kasse.ItemsSource = Warenkorb; // ListView folgt automatisch der Liste

            // Fokus verfolgen: sobald eine der beiden Textboxen angeklickt wird,
            // merken wir sie uns als Ziel fuer das Numpad.
            _aktivesFeld = tb_EAN;
            tb_EAN.GotFocus   += (s, e) => _aktivesFeld = tb_EAN;
            tb_menge.GotFocus += (s, e) => _aktivesFeld = tb_menge;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetzeLayout(Properties.Settings.Default.Linkshaender); // gemerkte Wahl anwenden
            await LadeKategorienUndButtonsAsync();
            AktualisiereSummen();
            tb_EAN.Focus(); // Standard: Eingabe landet zuerst in der Artikelnummer
        }

        /// <summary>
        /// Spiegelt das POS-Layout fuer Links- bzw. Rechtshaender.
        /// Rechtshaender (Standard): Artikel-Buttons links, Numpad/Warenkorb rechts.
        /// Linkshaender: umgekehrt.
        /// </summary>
        public void SetzeLayout(bool linkshaender)
        {
            if (linkshaender)
            {
                Grid.SetColumn(posGridButton,   1);
                Grid.SetColumn(posGridNumpad,   0);
                Grid.SetColumn(posGridListView, 0);
            }
            else
            {
                Grid.SetColumn(posGridButton,   0);
                Grid.SetColumn(posGridNumpad,   1);
                Grid.SetColumn(posGridListView, 1);
            }
        }

        // ---------- Warenkorb-Logik ----------

        /// <summary>
        /// Fuegt einen Posten zum Warenkorb hinzu. Wird sowohl von den Artikel-Buttons
        /// als auch von der manuellen Eingabe verwendet.
        /// </summary>
        public void ArtikelHinzufuegen(string bez, int nummer, decimal nettoEinzel,
                                       int steuersatzProzent, int menge)
        {
            decimal netto  = Math.Round(nettoEinzel * menge, 2);
            decimal steuer = Math.Round(netto * steuersatzProzent / 100m, 2);
            decimal brutto = Math.Round(netto + steuer, 2);

            Warenkorb.Add(new RechnungListe
            {
                ArtikelBez    = bez,
                ArtikelNummer = nummer,
                Menge         = menge,
                Einzelpreis   = Math.Round(nettoEinzel, 2),
                NettoPreis    = netto,
                Steuer        = steuer,
                BruttoPreis   = brutto
            });

            AktualisiereSummen();
            TB_Clear();
        }

        private void ArtikelZuRechnungHinzufuegen(Artikel art)
        {
            int menge = int.TryParse(tb_menge.Text, out int m) && m > 0 ? m : 1;
            ArtikelHinzufuegen(art.Artikelbez, art.Artikelnr,
                               art.Nettopreis ?? 0m, art.Steuersatz ?? 0, menge);
        }

        public void ClearALL()
        {
            Warenkorb.Clear();
            AktualisiereSummen();
        }

        private void AktualisiereSummen()
        {
            // Tatsaechliche Stueckzahl: Summe der Mengen, nicht Anzahl der Zeilen.
            lbl_Anzahlartikel.Content = Warenkorb.Sum(r => r.Menge);
            lbl_totalpreis.Content    = Warenkorb.Sum(r => r.BruttoPreis)
                                                 .ToString("0.00", CultureInfo.CurrentCulture);
        }

        public void TB_Clear()
        {
            tb_EAN.Clear();
            tb_menge.Clear();
        }

        // ---------- Artikel-Buttons & Kategorien ----------

        public async void Add_Artikel(object sender, RoutedEventArgs e)
        {
            var name = (string)((Button)sender).Content;
            var treffer = await DB.GetArtikelByNameAsync(name);
            if (treffer.Count == 0) return;
            ArtikelZuRechnungHinzufuegen(treffer[0]);
        }

        private async Task CreateButtonAsync(int kategorieNr)
        {
            btn_List.Children.Clear();
            foreach (var item in await DB.GetArtikelByKategorieAsync(kategorieNr))
            {
                var b = new Button { Content = item.Artikelbez, Width = 150, Height = 100 };
                b.Click += Add_Artikel;
                btn_List.Children.Add(b);
            }
        }

        private async Task LadeKategorienUndButtonsAsync()
        {
            cb_kategorie.Items.Clear();
            btn_List.Children.Clear();

            var kategorien = await DB.GetAlleKategorienAsync();
            foreach (var item in kategorien)
                cb_kategorie.Items.Add(item.Kategoriename);

            if (kategorien.Count > 0)
                cb_kategorie.SelectedIndex = 0; // loest SelectionChanged -> Buttons werden gebaut
        }

        private async void cb_kategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_kategorie.SelectedIndex < 0) return;
            var kat = await DB.GetKategorieByNameAsync((string)cb_kategorie.SelectedValue);
            if (kat != null) await CreateButtonAsync(kat.Kategorie_Nr);
        }

        // ---------- Numpad / Eingaben ----------

        private void EingabeZahlBtn(object sender, RoutedEventArgs e)
        {
            var feld = _aktivesFeld ?? tb_EAN;
            feld.Text += (string)((Button)sender).Content;
            feld.Focus();                       // Fokus bleibt im Eingabefeld
            feld.CaretIndex = feld.Text.Length; // Cursor ans Ende
        }

        private void btn_CE_Click(object sender, RoutedEventArgs e) => TB_Clear();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tb_EAN.Text, out int artikelnr))
            {
                MessageBox.Show("Bitte nur Zahlen eingeben (Artikelnummer)");
                TB_Clear();
                return;
            }

            var art = await DB.GetArtikelByNummerAsync(artikelnr);
            if (art == null)
            {
                MessageBox.Show("Artikelnummer existiert nicht");
                TB_Clear();
                return;
            }

            ArtikelZuRechnungHinzufuegen(art);
        }

        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            int i = lv_kasse.SelectedIndex;
            if (i < 0) return;
            Warenkorb.RemoveAt(i); // Eine Zeile - kann nie mehr "verrutschen"
            AktualisiereSummen();
        }

        private void btn_ManuAdd_Click(object sender, RoutedEventArgs e)
            => new ManuelleEingabe(this).Show();

        private void btn_zahlung_Click(object sender, RoutedEventArgs e) => Kassieren("Bar");
        private void btn_karte_Click(object sender, RoutedEventArgs e)   => Kassieren("Karte");

        private void Kassieren(string zahlart)
        {
            if (Warenkorb.Count == 0)
            {
                MessageBox.Show("Der Warenkorb ist leer.");
                return;
            }
            new Zahlungsabschluss(this, zahlart).Show();
        }
    }
}
