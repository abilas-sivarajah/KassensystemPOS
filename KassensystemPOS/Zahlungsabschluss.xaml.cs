using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Printing;

namespace KassensystemPOS
{
    public partial class Zahlungsabschluss : Window
    {
        public decimal einzahlung;
        public decimal rueckgeld;
        public decimal GesamtNetto;
        public decimal GesamtBrutto;
        public decimal GesamtSteuer;

        readonly POS pos;
        readonly string _zahlart;

        // Austauschbare TSE. Heute Mock; spaeter eine echte Cloud-TSE (fiskaly o.ae.).
        private static readonly ITseService _tse = new MockTseService();

        public Zahlungsabschluss(POS pos, string zahlart)
        {
            this.pos = pos;
            this._zahlart = zahlart;
            InitializeComponent();
            Title = "Zahlung – " + zahlart;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Summen direkt aus dem Warenkorb berechnen (eine Quelle der Wahrheit).
            GesamtNetto  = pos.Warenkorb.Sum(r => r.NettoPreis);
            GesamtSteuer = pos.Warenkorb.Sum(r => r.Steuer);
            GesamtBrutto = pos.Warenkorb.Sum(r => r.BruttoPreis);
            rueckgeld    = -GesamtBrutto; // noch nichts eingezahlt

            lbl_TotalPreisAnzeige.Content = Anzeige(GesamtBrutto);
            lbl_rueckgeldbetrag.Content   = Anzeige(GesamtBrutto);
        }

        private static string Anzeige(decimal betrag)
            => betrag.ToString("0.00", CultureInfo.CurrentCulture);

        private void txt_rueckgeldeingabe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_rueckgeldeingabe.Text))
            {
                einzahlung = 0;
                rueckgeld  = -GesamtBrutto;
                lbl_rueckgeldAnzeige.Content = "Noch zu Zahlen";
                lbl_rueckgeldbetrag.Content  = Anzeige(GesamtBrutto);
                return;
            }

            // Komma wie Punkt akzeptieren.
            if (!decimal.TryParse(txt_rueckgeldeingabe.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out einzahlung))
                return;

            rueckgeld = einzahlung - GesamtBrutto; // positiv = Rueckgeld, negativ = noch offen

            lbl_rueckgeldAnzeige.Content = rueckgeld >= 0 ? "Rückgeld:" : "Noch zu Zahlen";
            lbl_rueckgeldbetrag.Content  = Anzeige(Math.Abs(rueckgeld));
        }

        private void btn_Abbruch_Click(object sender, RoutedEventArgs e) => Close();

        private async void btn_Zahlung_Click(object sender, RoutedEventArgs e)
        {
            if (pos.Warenkorb.Count == 0)
            {
                MessageBox.Show("Der Warenkorb ist leer.");
                return;
            }
            if (einzahlung < GesamtBrutto)
            {
                MessageBox.Show("Der eingezahlte Betrag reicht nicht aus.");
                return;
            }

            try
            {
                var trans = new Transaktionen
                {
                    BruttoBetrag      = GesamtBrutto,
                    RechnungsDatum    = DateTime.Now,
                    NettoGesamtBetrag = GesamtNetto,
                    Steuer            = GesamtSteuer,
                    Zahlart           = _zahlart
                };

                int rechnungsID = await DB.TransaktionHinzufuegenAsync(trans);

                // TSE signiert JEDEN Vorgang (hier Mock-Signatur).
                var tseSig = _tse.Signiere($"{rechnungsID};{Anzeige(GesamtBrutto)};{_zahlart}");

                bool ohneQuittung = toggle_OhneQuittung.IsChecked == true;
                MessageBox.Show("Zahlung erfolgreich.\nRückgeld: " + Anzeige(rueckgeld) + " €");

                if (!ohneQuittung)
                    await RechnungDruckenAsync(rechnungsID, tseSig);

                pos.ClearALL();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Die Zahlung konnte nicht gespeichert werden:\n\n" + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task RechnungDruckenAsync(int rechnungsID, TseSignatur tse)
        {
            var firma       = await DB.GetUnternehmensDatenAsync();
            string name     = firma?.Name         ?? "";
            string str      = firma?.Strasse      ?? "";
            int    hnr      = firma?.Hausnummer   ?? 0;
            int    plz      = firma?.PLZ          ?? 0;
            string ort      = firma?.Ort          ?? "";
            long   tel      = firma?.Tel          ?? 0;
            long   steuernr = firma?.Steuernummer ?? 0;

            string datei = Path.Combine(Path.GetTempPath(), "Quittung.pdf");

            using (var fs = new FileStream(datei, FileMode.Create))
            {
                var doc = new Document(PageSize.A5);
                var paragraph = new iTextSharp.text.Paragraph();
                PdfWriter.GetInstance(doc, fs);
                doc.Open();
                paragraph.Add($"          {name}\n\n");
                paragraph.Add($"Datum: \t {DateTime.Now.ToShortDateString()}\t-\t {DateTime.Now.ToShortTimeString()}\n");
                paragraph.Add($"Rechnungsnummer: {rechnungsID}\n");
                paragraph.Add("Menge\t Artikelnummer\t Name\t Nettopreis\n");
                paragraph.Add("---------------------------------------------------------------------\n");
                foreach (var r in pos.Warenkorb)
                    paragraph.Add($"{r.Menge}\t{r.ArtikelNummer}\t{r.ArtikelBez}\t{r.NettoPreis}\n");
                paragraph.Add("---------------------------------------------------------------------\n");
                paragraph.Add($"Netto:    {Anzeige(GesamtNetto)}€\n");
                paragraph.Add($"Steuer:   {Anzeige(GesamtSteuer)}€\n");
                paragraph.Add($"TOTAL:    {Anzeige(GesamtBrutto)}€\n");
                paragraph.Add($"Gegeben:  {Anzeige(einzahlung)}€\n");
                paragraph.Add($"Rückgeld: {Anzeige(rueckgeld)}€\n");
                paragraph.Add($"Zahlart:  {_zahlart}\n");
                paragraph.Add($"{str} {hnr}\n{plz} {ort}\nTel: {tel}\nSteuernummer: {steuernr}\n");
                paragraph.Add("----- TSE (Mock, nicht zertifiziert) -----\n");
                paragraph.Add($"TSE-Transaktion: {tse.TransaktionsNummer}\n");
                paragraph.Add($"Signaturzaehler: {tse.SignaturZaehler}\n");
                paragraph.Add($"Zeit: {tse.Zeitstempel:dd.MM.yyyy HH:mm:ss}\n");
                paragraph.Add($"Signatur: {tse.Signatur}\n");
                paragraph.Add("                              Auf Wiedersehen\n\n");
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
                MessageBox.Show("Die Quittung konnte nicht gedruckt werden:\n\n" + ex.Message,
                                "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
