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
    public partial class FrmRemoveSuccessfull : Form
    {
        public InstallParameters InstallParameters { get; set; }
        public FrmIntroduction FrmIntroduction { get; set; }
        public FrmRemoveSuccessfull(InstallParameters installParameters)
        {
            InitializeComponent();
            InstallParameters = installParameters;         

        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.FrmIntroduction.Close();
        }        

        private void BtnPrior_Click(object sender, EventArgs e)
        {
            FrmIntroduction.Show();
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
            try
            {
                Microsoft.Win32.RegistryKey key;

                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\SGP Plus Plugin", true);

                if (key == null)
                {
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\SGP Plus Plugin");
                   
                }
                key.SetValue("InstallDate", DateTime.Now.ToString());
                key.SetValue("InstallPath", InstallParameters.InstallPath);

                key.Close();
            }
            finally { }
        }
    }
}
