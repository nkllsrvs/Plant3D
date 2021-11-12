using Setup.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Setup
{
    public partial class FrmInstallSuccessfull : Form
    {
        public FrmInstallPath FrmInstallPath { get; set; }
        public FrmInstallSuccessfull()
        {
            InitializeComponent();
           

        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            FrmInstallPath.FrmIntroduction.Close();
        }        

        private void BtnPrior_Click(object sender, EventArgs e)
        {
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
           

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void FrmInstallSuccessfull_Load(object sender, EventArgs e)
        {
            
        }
    }
}
