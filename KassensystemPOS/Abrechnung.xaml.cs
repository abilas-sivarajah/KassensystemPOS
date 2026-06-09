using System;
using System.Collections.Generic;
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
        List<Transaktionen> rechnungen;
        DispatcherTimer _refreshTimer;

        public Abrechnung()
        {
            InitializeComponent();
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(10);
            _refreshTimer.Tick += (s, e) => Page_Loaded(null, null);
            _refreshTimer.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Clear();
            rechnungen = DB.GetOffeneTransaktionen();

            if (rechnungen.Count == 0)
            {
                MessageBox.Show("Keine offenen Transaktionen vorhanden");
                return;
            }

            foreach (var item in rechnungen)
            {
                umsatz += Math.Round((decimal)item.BruttoBetrag, 2);
                lv_Rechnungen.Items.Add(new RechnungAnzeige
                {
                    RechnungsID  = item.RechnungsID,
                    BruttoBetrag = Math.Round((decimal)item.BruttoBetrag, 2),
                    Datum        = (DateTime)item.RechnungsDatum
                });
            }

            lbl_Datum.Content  = DateTime.Now.ToShortDateString();
            lbl_Umsatz.Content = umsatz;
        }

        public void Clear()
        {
            lv_Rechnungen.Items.Clear();
            umsatz = 0;
            lbl_Datum.Content  = DateTime.Now.ToShortDateString();
            lbl_Umsatz.Content = 0;
        }

        private void Abbrechnen_Click(object sender, RoutedEventArgs e)
        {
            if (rechnungen == null || rechnungen.Count < 1)
            {
                MessageBox.Show("Keine offenen Transaktionen vorhanden");
                return;
            }

            var abr = new Abrechnungen
            {
                SollBestand      = umsatz,
                AbrechnungsDatum = DateTime.Now
            };
            int newAbrID = DB.AbrechnungHinzufuegen(abr);

            var ids = rechnungen.Select(r => r.RechnungsID).ToList();
            DB.TransaktionenAbrechnen(newAbrID, ids);

            int von = rechnungen[0].RechnungsID;
            int bis = rechnungen[rechnungen.Count - 1].RechnungsID;

            var firma = DB.GetUnternehmensDaten();
            string firmaName = firma?.Name ?? "";

            Document doc = new Document(PageSize.A6);
            var paragraph = new iTextSharp.text.Paragraph();
            PdfWriter.GetInstance(doc, new FileStream("Test.pdf", FileMode.Create));
            doc.Open();
            paragraph.Add($"Kassenabschluss {firmaName}\n\n");
            paragraph.Add($"Rechnungsnummern von {von} bis {bis}\n\n");
            paragraph.Add($"Gesamtumsatz: {umsatz}\n\n");
            paragraph.Add($"Datum: {DateTime.Now.ToShortDateString()}\n\n");
            doc.Add(paragraph);
            doc.Close();

            PrinterSettings settings = new PrinterSettings();
            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
            pdf.LoadFromFile("Test.pdf");
            pdf.PrintSettings.PrinterName = settings.PrinterName;
            pdf.Print();
            pdf.Dispose();

            Clear();
        }
    }
}
