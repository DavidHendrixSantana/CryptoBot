using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using API_BINANCE;

namespace PruebaBinance
{
    public partial class Form1 : Form
    {
        Spot API = null;
      
        public Form1()
        {
            InitializeComponent();
            API = new Spot( Application.StartupPath );
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string puntas = API.Precios.ObtenerPuntas("ETHUSDT", 100);
            txtSalida.Text = puntas;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            API.Consultas.ObtenerReglas();
        }
    }
}
