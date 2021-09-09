using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plant3D
{
    public partial class FormVALE : Form
    {
        bool display = true;
        bool toggleLight = true;
        public FormVALE()
        {
            InitializeComponent();
        }

        private void buttonSelection_Click(object sender, EventArgs e)
        {
            buttonSelection.BackColor = Color.LightGreen;
            buttonSelection.Text = "Instruments";

            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
            //Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            List<PromptEntityResult> Instruments = new List<PromptEntityResult>();
            PromptEntityResult result;

            bool loop = true;
            while (loop)
            {
                result = ed.GetEntity("\nSelecione um  Instrumento: ");
                StringCollection eKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "RelatedTo",
                    "Class",
                    "ClassName",
                    "TagFormatName",
                    "ObjectClass"
                };
                StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(result.ObjectId), eKeys, true);
                //var className = dlm.GetObjectClassname(result.ObjectId);
                //var type = dlm.GetType();
                //var typeResult = result.ObjectId.GetType();
                //var allProperties = dlm.GetAllProperties(dlm.FindAcPpRowId(result.ObjectId), true);
                //DBObject obj = db.TransactionManager.StartTransaction().GetObject(result.ObjectId, OpenMode.ForRead);
                //db.TransactionManager.StartTransaction().Commit();

                if (result.Status == PromptStatus.OK)
                {
                    if (Instruments.Contains(result))
                        MessageBox.Show("O instrumento já foi selecionado!!");
                    else
                    {
                        Instruments.Add(result);
                    }
                }
                DialogResult messageBox = MessageBox.Show("Deseja selecionar outro Instrumento?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (messageBox == DialogResult.No)
                    break;
            }
            buttonSelection.BackColor = Color.Blue;
            buttonSelection.Text = "Equipment";

            PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento: ");
            if (equipment.Status == PromptStatus.OK)
            {
                int equipmentRowId = dlm.FindAcPpRowId(equipment.ObjectId);
                StringCollection eKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "RelatedTo",
                    "Class",
                    "ClassName"

                };
                StringCollection eVals = dlm.GetProperties(equipmentRowId, eKeys, true);
                foreach (PromptEntityResult entityResult in Instruments)
                {
                    int instrumentRowId = dlm.FindAcPpRowId(entityResult.ObjectId);
                    StringCollection iKeys = new StringCollection
                    {
                        "Description",
                        "Tag",
                        "RelatedTo"
                    };
                    StringCollection iVals = dlm.GetProperties(instrumentRowId, iKeys, true);

                    iVals[2] = eVals[1];

                    db.StartTransaction();
                    ListViewItem item = new ListViewItem(instrumentRowId.ToString());
                    item.SubItems.Add(iVals[1]);
                    listView.Items.Add(item);
                    dlm.SetProperties(entityResult.ObjectId, iKeys, iVals);
                    db.CommitTransaction();
                }
            }
            buttonSelection.BackColor = Color.Gray;
            buttonSelection.Text = "Selection";

        }

        private void buttonRelatedTo_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FormVALE_Load(object sender, EventArgs e)
        {
            buttonSelection.Text = "Selection";
        }
    }
}