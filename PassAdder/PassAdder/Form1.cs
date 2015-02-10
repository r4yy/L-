using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PassAdder
{
    public partial class Form1 : Form
    {
        private StreamReader dateiReader;
        private StreamWriter dateiWriter = new StreamWriter("output.txt");

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            DialogResult dlgResult = dlgOffnen.ShowDialog();
            if (dlgResult != DialogResult.OK)
                return;

            string dateiname = dlgOffnen.FileName;
            dateiReader = new StreamReader(dateiname);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                while (dateiReader.Peek() != -1)
                {
                    string zeile = dateiReader.ReadLine();
                    zeile = zeile + ':' + zeile + txbAdd.Text;

                    dateiWriter.WriteLine(zeile);
                    //dateiWriter.Dispose();
                }

                MessageBox.Show(text: @"Done");
            }

            catch (Exception)
            {
                MessageBox.Show(text: @"Failed");
            }
            
            dateiWriter.Close();
            dateiReader.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
