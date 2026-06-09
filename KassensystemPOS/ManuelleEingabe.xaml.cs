using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KassensystemPOS
{
    /// <summary>
    /// Interaktionslogik für ManuelleEingabe.xaml
    /// </summary>
    public partial class ManuelleEingabe : Window
    {
        POS pos;
        public ManuelleEingabe(POS pos)
        {
            this.pos = pos;
            InitializeComponent();
        }

        private void Artikel_Add(object sender, RoutedEventArgs e)
        {
            RechnungListe r = new RechnungListe();
            r.ArtikelBez = tb_Artikelbez.Text;
            r.NettoPreis = decimal.Parse(tb_Artikelpreis.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
