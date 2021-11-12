using Setup.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Setup
{
    public partial class FrmIntroduction : Form
    {
        public FrmInstallPath FrmInstallPath { get; set; }   


        public FrmIntroduction()
        {
            InitializeComponent();

            btnRemove.Hide();          

            FrmInstallPath = new FrmInstallPath();
        }   

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            FrmInstallPath.FrmIntroduction = this;
            FrmInstallPath.Show();

            this.Hide();
        }       

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnPrior_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
