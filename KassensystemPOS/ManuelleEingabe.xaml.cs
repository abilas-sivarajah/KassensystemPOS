using System.Globalization;
using System.Windows;

namespace KassensystemPOS
{
    /// <summary>
    /// Interaktionslogik für ManuelleEingabe.xaml
    /// </summary>
    public partial class ManuelleEingabe : Window
    {
        readonly POS pos;

        public ManuelleEingabe(POS pos)
        {
            this.pos = pos;
            InitializeComponent();
        }

        private void Artikel_Add(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_Artikelbez.Text))
            {
                MessageBox.Show("Bitte eine Bezeichnung eingeben.");
                return;
            }

            // Komma wie Punkt akzeptieren, unabhaengig von den Windows-Spracheinstellungen.
            if (!decimal.TryParse(tb_Artikelpreis.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal netto))
            {
                MessageBox.Show("Bitte einen gültigen Nettopreis eingeben.");
                return;
            }

            int steuersatz = int.TryParse(tb_Steuersatz.Text, out int s) ? s : 0;

            // Manueller Posten hat keine Artikelnummer -> 0
            pos.ArtikelHinzufuegen(tb_Artikelbez.Text, 0, netto, steuersatz, 1);
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
