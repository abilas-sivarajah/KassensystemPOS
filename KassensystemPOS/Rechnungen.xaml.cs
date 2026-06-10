using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KassensystemPOS
{
    public partial class Rechnungen : Page
    {
        List<Transaktionen> rechnungen;

        public Rechnungen()
        {
            InitializeComponent();
        }

        public async void Laden()
        {
            rechnungen = await DB.GetAlleTransaktionenAsync();
            dataGrid.DataContext = rechnungen;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rechnungen == null) return;

            if (textBox.Text != "" && int.TryParse(textBox.Text, out int id))
                dataGrid.DataContext = rechnungen.Where(x => x.RechnungsID == id).ToList();
            else
                dataGrid.DataContext = rechnungen;
        }

        private async void btn_Storno_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGrid.SelectedItem is Transaktionen sel))
            {
                MessageBox.Show("Bitte zuerst eine Rechnung in der Liste auswählen.");
                return;
            }

            // Eine Storno-Buchung selbst darf nicht erneut storniert werden.
            if (sel.StornoVon != null)
            {
                MessageBox.Show("Diese Zeile ist selbst eine Storno-Buchung.");
                return;
            }
            // Bereits stornierte Rechnung nicht doppelt stornieren.
            if (rechnungen != null && rechnungen.Any(x => x.StornoVon == sel.RechnungsID))
            {
                MessageBox.Show("Diese Rechnung wurde bereits storniert.");
                return;
            }

            var antwort = MessageBox.Show(
                $"Rechnung {sel.RechnungsID} stornieren?\n\n" +
                "Es wird eine Gegenbuchung mit negativem Betrag erzeugt.\n" +
                "Die Originalbuchung bleibt unverändert erhalten.",
                "Storno", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (antwort != MessageBoxResult.Yes) return;

            await DB.StorniereTransaktionAsync(sel);
            MessageBox.Show("Storno wurde gebucht.");
            Laden();
        }
    }
}
