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
    public partial class FrmInstallPath : Form
    {
        private string versao;
        public FrmInstallSuccessfull FrmInstallSuccess { get; set; }
        public FrmIntroduction FrmIntroduction { get; set; }
        public FrmInstallPath()
        {
            InitializeComponent();

            FrmInstallSuccess = new FrmInstallSuccessfull();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.FrmIntroduction.Close();


        }

        private void BtnProcurar_Click(object sender, EventArgs e)
        {
            DialogResult d = fbDialog.ShowDialog();

            if (d == DialogResult.OK)
            {
            }
        }

        private void BtnPrior_Click(object sender, EventArgs e)
        {
            FrmIntroduction.Show();
            this.Hide();
        }




        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                Instalar();


                FrmInstallSuccess.FrmInstallPath = this;
                FrmInstallSuccess.Show();

                this.Hide();
            }
            catch (Exception Ex) {
                MessageBox.Show(Ex.Message);

            }
        }

        private void Instalar()
        {
            try
            {
                ////Dependencies
                //string dependenciesPathSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SetupDocuments\\Dependencies");

                //DirectoryInfo infoDependencies = new DirectoryInfo(dependenciesPathSource);

                //string dependenciesPathDestination = $"{txtPath.Text}\\Dependencies";

                //if (!Directory.Exists(dependenciesPathDestination)) Directory.CreateDirectory(dependenciesPathDestination);

                //foreach (var file in infoDependencies.GetFiles())
                //{
                //    string pathFileDestination = Path.Combine(dependenciesPathDestination, file.Name);
                //    if (File.Exists(pathFileDestination)) File.Delete(pathFileDestination);
                //    file.CopyTo(pathFileDestination);
                //}

                //Plugin
                string pluginPathSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"SetupDocuments\\Plant 3D\\{versao}");

                DirectoryInfo infoPlugin = new DirectoryInfo(pluginPathSource);

                string pluginPathDestination = $"{txtPath.Text}\\Plant3DValeAddin";

                if (Directory.Exists(pluginPathDestination)) Directory.Delete(pluginPathDestination, true);

                Directory.CreateDirectory(pluginPathDestination);

                CopyDirectory(infoPlugin, pluginPathDestination);

                string acadPath = @"C:\Program Files\Autodesk\AutoCAD 2022\Support\en-us\acad2022doc.lsp";

                string [] allLines = File.ReadAllLines(acadPath);
                for (int i = 0; i < allLines.Length; i++)
                {
                    if (allLines[i] == ";;;===== AutoArxLoad Arx Applications =====")
                    {
                        //allLines[i + 1] = "(command " + '"' + "_netload" + '"' + ' ' + '"' + pluginPathDestination.Replace("\\", "/") + "/Plant3D.dll" + '"' + ")";
                        allLines[i + 1] = "(command " + '"' + "_netload" + '"' + ' ' + '"' + "C:/Users/nikol/source/repos/nkllsrvs/Plant3D/Plant3D/bin/Debug" + "/Plant3D.dll" + '"' + ")";
                        allLines[i + 2] = "(command " + '"' +"_vale" + '"' + ")";
                        break;
                    }
                }
                File.WriteAllLines(acadPath, allLines);
            }

            catch (Exception Ex)
            {
                throw new Exception($"Não foi possivel realizar a instalaçã0: {Ex.Message}");
            }
        }

        private void CopyDirectory(DirectoryInfo info, string destination)
        {
            foreach (var file in info.GetFiles())
            {
                string pathFileDestination = Path.Combine(destination, file.Name);
                if (File.Exists(pathFileDestination)) File.Delete(pathFileDestination);
                file.CopyTo(pathFileDestination);
            }

            foreach (DirectoryInfo dir in info.GetDirectories())
            {
                if (Directory.Exists($"{destination}\\{dir.Name}")) Directory.Delete($"{destination}\\{dir.Name}", true);

                Directory.CreateDirectory($"{destination}\\{dir.Name}");

                CopyDirectory(dir, $"{destination}\\{dir.Name}");
            }
        }




        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void cmbVersion_SelectedValueChanged(object sender, EventArgs e)
        {

            switch (cmbVersion.SelectedItem.ToString())
            {
                case "AutoCad Plant 3D 2019":
                    versao = "2019";
                    txtPath.Text = $"C:\\Program Files\\Autodesk\\AutoCAD {versao}";
                    break;
                case "AutoCad Plant 3D 2020":
                    versao = "2020";
                    txtPath.Text = $"C:\\Program Files\\Autodesk\\AutoCAD {versao}";
                    break;
                case "AutoCad Plant 3D 2021":
                    versao = "2021";
                    txtPath.Text = $"C:\\Program Files\\Autodesk\\AutoCAD {versao}";
                    break;
                case "AutoCad Plant 3D 2022":
                    versao = "2022";
                    txtPath.Text = $"C:\\Program Files\\Autodesk\\AutoCAD {versao}";
                    break;
            }
        }

        private void cmbVersion_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
