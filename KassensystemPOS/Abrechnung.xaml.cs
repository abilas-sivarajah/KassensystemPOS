using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Printing;
using Spire.Pdf;

namespace KassensystemPOS
{
    public partial class Abrechnung : Page
    {
        public class RechnungAnzeige
        {
            public int     RechnungsID  { get; set; }
            public decimal BruttoBetrag { get; set; }
            public DateTime Datum       { get; set; }
        }

        decimal umsatz;
        decimal barSumme;
        decimal karteSumme;
        int anzahlVerkaeufe;
        int anzahlStornos;
        List<Transaktionen> rechnungen;

        public Abrechnung()
        {
            InitializeComponent();
            // Kein Auto-Refresh-Timer mehr. Page_Loaded laeuft beim Anzeigen der Seite.
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Clear();
            rechnungen = await DB.GetOffeneTransaktionenAsync();

            if (rechnungen.Count == 0)
            {
                // Beim Anzeigen der Seite keine MessageBox: einfach leere Liste lassen.
                // Die Meldung kommt nur beim Klick auf "Abrechnen" (Abbrechnen_Click).
                return;
            }

            foreach (var item in rechnungen)
            {
                decimal brutto = Math.Round(item.BruttoBetrag ?? 0m, 2);
                umsatz += brutto;

                // Aufteilung nach Zahlart (Stornos sind negativ und ziehen sich korrekt ab)
                if (item.Zahlart == "Karte") karteSumme += brutto;
                else                          barSumme   += brutto; // Standard/Null = Bar

                if (item.StornoVon != null) anzahlStornos++;
                else                        anzahlVerkaeufe++;

                lv_Rechnungen.Items.Add(new RechnungAnzeige
                {
                    RechnungsID  = item.RechnungsID,
                    BruttoBetrag = brutto,
                    Datum        = (DateTime)item.RechnungsDatum
                });
            }

            lbl_Datum.Content  = DateTime.Now.ToShortDateString();
            lbl_Umsatz.Content = Euro(umsatz);
            lbl_Bar.Content    = Euro(barSumme);
            lbl_Karte.Content  = Euro(karteSumme);
            lbl_Anzahl.Content = anzahlVerkaeufe.ToString();
            lbl_Storno.Content = anzahlStornos.ToString();
        }

        private static string Euro(decimal d) => d.ToString("0.00", CultureInfo.CurrentCulture) + " €";

        public void Clear()
        {
            lv_Rechnungen.Items.Clear();
            umsatz = 0; barSumme = 0; karteSumme = 0;
            anzahlVerkaeufe = 0; anzahlStornos = 0;
            lbl_Datum.Content  = DateTime.Now.ToShortDateString();
            lbl_Umsatz.Content = Euro(0);
            lbl_Bar.Content    = Euro(0);
            lbl_Karte.Content  = Euro(0);
            lbl_Anzahl.Content = "0";
            lbl_Storno.Content = "0";
        }

        private async void Abbrechnen_Click(object sender, RoutedEventArgs e)
        {
            if (rechnungen == null || rechnungen.Count < 1)
            {
                MessageBox.Show("Keine offenen Transaktionen vorhanden");
                return;
            }

            // Anlegen + Zuordnen in EINER Transaktion (alles oder nichts).
            var ids = rechnungen.Select(r => r.RechnungsID).ToList();
            await DB.AbrechnungDurchfuehrenAsync(umsatz, DateTime.Now, ids);

            int von = rechnungen[0].RechnungsID;
            int bis = rechnungen[rechnungen.Count - 1].RechnungsID;

            var firma = await DB.GetUnternehmensDatenAsync();
            string firmaName = firma?.Name ?? "";

            string datei = Path.Combine(Path.GetTempPath(), "Kassenabschluss.pdf");
            using (var fs = new FileStream(datei, FileMode.Create))
            {
                var doc = new Document(PageSize.A6);
                var paragraph = new iTextSharp.text.Paragraph();
                PdfWriter.GetInstance(doc, fs);
                doc.Open();
                paragraph.Add("Z-BON / Kassenabschluss\n");
                paragraph.Add($"{firmaName}\n\n");
                paragraph.Add($"Datum: {DateTime.Now:dd.MM.yyyy HH:mm}\n");
                paragraph.Add($"Rechnungen: {von} bis {bis}\n");
                paragraph.Add("-------------------------------\n");
                paragraph.Add($"Verkaeufe: {anzahlVerkaeufe}   Stornos: {anzahlStornos}\n\n");
                paragraph.Add($"Bar:    {Euro(barSumme)}\n");
                paragraph.Add($"Karte:  {Euro(karteSumme)}\n");
                paragraph.Add("-------------------------------\n");
                paragraph.Add($"GESAMT: {Euro(umsatz)}\n\n");
                doc.Add(paragraph);
                doc.Close();
            }

            try
            {
                var settings = new PrinterSettings();
                using (var pdf = new Spire.Pdf.PdfDocument())
                {
                    pdf.LoadFromFile(datei);
                    pdf.PrintSettings.PrinterName = settings.PrinterName;
                    pdf.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Der Abschluss konnte nicht gedruckt werden:\n\n" + ex.Message,
                                "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Clear();
        }
    }
}
