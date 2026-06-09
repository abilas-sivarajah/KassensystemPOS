using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Printing;
using Spire.Pdf;

namespace KassensystemPOS
{
    public partial class Zahlungsabschluss : Window
    {
        public decimal einzahlung;
        public decimal rueckgeld;
        public decimal GesamtNetto;
        public decimal GesamtBrutto;
        public decimal GesamtSteuer;

        public List<int>     AlleMengen;
        public List<int>     AlleArtikelnummern;
        public List<string>  AlleArtikelnamen;
        public List<decimal> AlleNettoPreise;
        public List<decimal> AlleSteuern;
        public List<decimal> AlleBruttopreise;

        POS pos;

        public Zahlungsabschluss(POS pos)
        {
            this.pos = pos;
            InitializeComponent();
            AlleArtikelnamen   = pos.AlleArtikelnamen;
            AlleArtikelnummern = pos.AlleArtikelnummern;
            AlleMengen         = pos.AlleMengen;
            AlleNettoPreise    = pos.AlleNettoPreise;
            AlleSteuern        = pos.AlleSteuern;
            AlleBruttopreise   = pos.AlleBruttopreise;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < AlleNettoPreise.Count; i++)
            {
                GesamtBrutto += Math.Round(AlleBruttopreise[i], 2);
                GesamtNetto  += Math.Round(AlleNettoPreise[i],  2);
                GesamtSteuer += Math.Round(AlleSteuern[i],      2);
            }
            lbl_TotalPreisAnzeige.Content = GesamtBrutto.ToString();
            lbl_rueckgeldbetrag.Content   = GesamtBrutto.ToString();
        }

        private void txt_rueckgeldeingabe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt_rueckgeldeingabe.Text == "")
            {
                lbl_rueckgeldAnzeige.Content = "Noch zu Zahlen";
                lbl_rueckgeldbetrag.Content  = GesamtBrutto.ToString();
                return;
            }

            einzahlung = decimal.Parse(txt_rueckgeldeingabe.Text);
            rueckgeld  = GesamtBrutto - einzahlung;

            lbl_rueckgeldAnzeige.Content = rueckgeld <= 0 ? "Rückgeld:" : "Noch zu Zahlen";
            lbl_rueckgeldbetrag.Content  = rueckgeld.ToString();
        }

        private void btn_Abbruch_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Zahlung_Click(object sender, RoutedEventArgs e)
        {
            if (rueckgeld > 0) return;

            var trans = new Transaktionen
            {
                BruttoBetrag      = GesamtBrutto,
                RechnungsDatum    = DateTime.Now,
                NettoGesamtBetrag = GesamtNetto,
                Steuer            = GesamtSteuer
            };

            int rechnungsID = DB.TransaktionHinzufuegen(trans);

            MessageBox.Show("Rückgeld: " + rueckgeld.ToString());
            MessageBox.Show("Zahlung erfolgreich");
            bool ohneQuittung = toggle_OhneQuittung.IsChecked == true;
            Close();
            if (!ohneQuittung)
                RechnungDrucken(rechnungsID);
            pos.ClearALL();
        }

        public void RechnungDrucken(int rechnungsID)
        {
            var firma    = DB.GetUnternehmensDaten();
            string name  = firma?.Name       ?? "";
            string str   = firma?.Strasse    ?? "";
            int    hnr   = firma?.Hausnummer ?? 0;
            int    plz   = firma?.PLZ        ?? 0;
            string ort   = firma?.Ort        ?? "";
            long   tel   = firma?.Tel        ?? 0;
            long   steuernr = firma?.Steuernummer ?? 0;

            Document doc = new Document(PageSize.A5);
            var paragraph = new iTextSharp.text.Paragraph();
            PdfWriter.GetInstance(doc, new FileStream("Test.pdf", FileMode.Create));
            doc.Open();
            paragraph.Add($"          {name}\n\n");
            paragraph.Add($"Datum: \t {DateTime.Now.ToShortDateString()}\t-\t {DateTime.Now.ToShortTimeString()}\n");
            paragraph.Add($"Rechnungsnummer: {rechnungsID}\n");
            paragraph.Add("Menge\t Artikelnummer\t Name\t Nettopreis\n");
            paragraph.Add("---------------------------------------------------------------------\n");
            for (int i = 0; i < AlleArtikelnummern.Count; i++)
                paragraph.Add($"{AlleMengen[i]}\t{AlleArtikelnummern[i]}\t{AlleArtikelnamen[i]}\t{AlleNettoPreise[i]}\n");
            paragraph.Add("---------------------------------------------------------------------\n");
            paragraph.Add($"Netto:    {GesamtNetto}€\n");
            paragraph.Add($"Steuer:   {GesamtSteuer}€\n");
            paragraph.Add($"TOTAL:    {GesamtBrutto}€\n");
            paragraph.Add($"Gegeben:  {einzahlung}€\n");
            paragraph.Add($"Rückgeld: {rueckgeld}€\n");
            paragraph.Add($"{str} {hnr}\n{plz} {ort}\nTel: {tel}\nSteuernummer: {steuernr}\n");
            paragraph.Add("                              Auf Wiedersehen\n\n");
            doc.Add(paragraph);
            doc.Close();

            PrinterSettings settings = new PrinterSettings();
            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
            pdf.LoadFromFile("Test.pdf");
            pdf.PrintSettings.PrinterName = settings.PrinterName;
            pdf.Print();
            pdf.Dispose();
        }
    }
}
