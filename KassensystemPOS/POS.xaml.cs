using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace KassensystemPOS
{
    public class RechnungListe
    {
        public int     ArtikelNummer { get; set; }
        public string  ArtikelBez   { get; set; }
        public int     Menge        { get; set; }
        public decimal NettoPreis   { get; set; }
        public decimal Steuer       { get; set; }
        public decimal BruttoPreis  { get; set; }
    }

    public partial class POS : Page
    {
        public bool Tb_wechsel = false;
        int     kategorienr    = 0;
        int     anzahl_Artikel = 0;
        decimal gesamt_preis   = 0.00m;
        DispatcherTimer _refreshTimer;

        public List<int>     AlleMengen         = new List<int>();
        public List<int>     AlleArtikelnummern = new List<int>();
        public List<string>  AlleArtikelnamen   = new List<string>();
        public List<decimal> AlleNettoPreise    = new List<decimal>();
        public List<decimal> AlleSteuern        = new List<decimal>();
        public List<decimal> AlleBruttopreise   = new List<decimal>();

        public POS()
        {
            InitializeComponent();
        }

        public void ClearALL()
        {
            lv_kasse.Items.Clear();
            AlleArtikelnamen.Clear();
            AlleArtikelnummern.Clear();
            AlleBruttopreise.Clear();
            AlleMengen.Clear();
            AlleNettoPreise.Clear();
            AlleSteuern.Clear();
            anzahl_Artikel = 0;
            gesamt_preis   = 0;
            lbl_Anzahlartikel.Content = anzahl_Artikel.ToString();
            lbl_totalpreis.Content    = gesamt_preis.ToString();
        }

        public void TB_Clear()
        {
            tb_EAN.Clear();
            tb_menge.Clear();
        }

        public void Add_Artikel(object sender, RoutedEventArgs e)
        {
            var name = (string)((Button)sender).Content;
            var rechnungsartikel = DB.GetArtikelByName(name);
            if (rechnungsartikel.Count == 0) return;

            ArtikelZuRechnungHinzufuegen(rechnungsartikel[0]);
        }

        private void ArtikelZuRechnungHinzufuegen(Artikel art)
        {
            int menge = tb_menge.Text != "" ? Int32.Parse(tb_menge.Text) : 1;

            double  steuersatz  = (double)art.Steuersatz / 100;
            decimal steuer      = Math.Round((decimal)(art.Nettopreis * (decimal)steuersatz) * menge, 2);
            decimal netto       = Math.Round((decimal)art.Nettopreis * menge, 2);
            decimal brutto      = Math.Round(netto + steuer, 2);

            var rechnung = new RechnungListe
            {
                ArtikelBez    = art.Artikelbez,
                ArtikelNummer = art.Artikelnr,
                Menge         = menge,
                NettoPreis    = netto,
                Steuer        = steuer,
                BruttoPreis   = brutto
            };

            lv_kasse.Items.Add(rechnung);
            anzahl_Artikel++;
            AlleArtikelnamen.Add(rechnung.ArtikelBez);
            AlleArtikelnummern.Add(rechnung.ArtikelNummer);
            AlleMengen.Add(rechnung.Menge);
            AlleNettoPreise.Add(rechnung.NettoPreis);
            AlleBruttopreise.Add(rechnung.BruttoPreis);
            AlleSteuern.Add(rechnung.Steuer);

            gesamt_preis += brutto;
            lbl_totalpreis.Content    = gesamt_preis;
            lbl_Anzahlartikel.Content = anzahl_Artikel;
            TB_Clear();
        }

        private void CreateButton(int kategorieNr)
        {
            btn_List.Children.Clear();
            var artikel = DB.GetArtikelByKategorie(kategorieNr);
            foreach (var item in artikel)
            {
                var b = new Button
                {
                    Content = item.Artikelbez,
                    Width   = 150,
                    Height  = 100
                };
                b.Click += new RoutedEventHandler(Add_Artikel);
                btn_List.Children.Add(b);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lbl_Anzahlartikel.Content = anzahl_Artikel;
            lbl_totalpreis.Content    = gesamt_preis;

            LadeKategorienUndButtons();

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(10);
            _refreshTimer.Tick += (s, _) => RefreshButtons();
            _refreshTimer.Start();
        }

        private void LadeKategorienUndButtons()
        {
            cb_kategorie.Items.Clear();
            btn_List.Children.Clear();

            var kategorien = DB.GetAlleKategorien();
            foreach (var item in kategorien)
                cb_kategorie.Items.Add(item.Kategoriename);

            if (kategorien.Count > 0)
            {
                cb_kategorie.SelectedIndex = 0;
                CreateButton(kategorien[0].Kategorie_Nr);
            }
        }

        private void RefreshButtons()
        {
            // Aktuelle Kategorie merken
            string aktuelleKat = cb_kategorie.SelectedValue as string;

            cb_kategorie.Items.Clear();
            var kategorien = DB.GetAlleKategorien();
            foreach (var item in kategorien)
                cb_kategorie.Items.Add(item.Kategoriename);

            // Vorherige Auswahl wiederherstellen, sonst erste Kategorie
            if (aktuelleKat != null && cb_kategorie.Items.Contains(aktuelleKat))
                cb_kategorie.SelectedValue = aktuelleKat;
            else if (cb_kategorie.Items.Count > 0)
                cb_kategorie.SelectedIndex = 0;

            var kat = DB.GetKategorieByName(cb_kategorie.SelectedValue as string);
            if (kat != null)
                CreateButton(kat.Kategorie_Nr);
        }

        private void cb_kategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_kategorie.SelectedIndex >= 0)
            {
                var kat = DB.GetKategorieByName((string)cb_kategorie.SelectedValue);
                if (kat != null)
                    CreateButton(kat.Kategorie_Nr);
            }
        }

        private void EingabeZahlBtn(object sender, RoutedEventArgs e)
        {
            if (Tb_wechsel)
                tb_menge.Text += (string)((Button)sender).Content;
            else
                tb_EAN.Text += (string)((Button)sender).Content;
        }

        private void btn_CE_Click(object sender, RoutedEventArgs e)
        {
            TB_Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string eingabe = tb_EAN.Text;
            if (!int.TryParse(eingabe, out int artikelnr))
            {
                MessageBox.Show("Bitte nur Zahlen eingeben (Artikelnummer)");
                TB_Clear();
                return;
            }

            var art = DB.GetArtikelByNummer(artikelnr);
            if (art == null)
            {
                MessageBox.Show("Artikelnummer existiert nicht");
                TB_Clear();
                return;
            }

            ArtikelZuRechnungHinzufuegen(art);
        }

        private void tb_menge_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tb_wechsel = true;
        }

        private void tb_EAN_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tb_wechsel = false;
        }

        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            if (anzahl_Artikel >= 1 && lv_kasse.SelectedIndex >= 0)
            {
                int delIndex = lv_kasse.SelectedIndex;
                lv_kasse.Items.RemoveAt(delIndex);
                gesamt_preis -= AlleBruttopreise[delIndex];
                AlleNettoPreise.RemoveAt(delIndex);
                AlleSteuern.RemoveAt(delIndex);
                AlleBruttopreise.RemoveAt(delIndex);
                lbl_totalpreis.Content = gesamt_preis;
                anzahl_Artikel--;
                lbl_Anzahlartikel.Content = anzahl_Artikel;
            }
        }

        private void btn_ManuAdd_Click(object sender, RoutedEventArgs e)
        {
            new ManuelleEingabe(this).Show();
        }

        private void btn_zahlung_Click(object sender, RoutedEventArgs e)
        {
            new Zahlungsabschluss(this).Show();
        }
    }
}
