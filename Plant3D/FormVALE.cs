using Autodesk.AutoCAD.ApplicationServices;
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
        public FormVALE()
        {
            InitializeComponent();
        }

        private void buttonSelection_Click(object sender, EventArgs e)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
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
                    "TagFormatName"
                };
                StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(result.ObjectId), eKeys, true);
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
                if(messageBox == DialogResult.No)
                    break;
            }
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
                    dlm.SetProperties(entityResult.ObjectId, iKeys, iVals);
                    db.CommitTransaction();
                }
            }
        }

        private void buttonRelatedTo_Click(object sender, EventArgs e)
        {

        }
    }
}