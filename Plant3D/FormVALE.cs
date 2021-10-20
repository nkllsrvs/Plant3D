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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plant3D
{
    public partial class FormVALE : Form
    {
        private List<ObjectId> Instruments = new List<ObjectId>();
        private Document docInstruments;
        DataLinksManager dlmInstruments;
        PnPDatabase dbInstruments;

        public FormVALE()
        {
            InitializeComponent();
        }

        private void ButtonRelatedTo_Click(object sender, EventArgs e)
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

        private void buttonEquipment_Click(object sender, EventArgs e)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento: ");
            if (equipment.Status == PromptStatus.OK)
            {
                using (var trEquipment = doc.TransactionManager.StartTransaction())
                {
                    Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                    if (ent.ToString() == "Autodesk.AutoCAD.DatabaseServices.ImpEntity" | ent.Layer == "Equipment")
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
                        foreach (ObjectId intrumentId in Instruments)
                        {
                            int instrumentRowId = dlmInstruments.FindAcPpRowId(intrumentId);
                            StringCollection iKeys = new StringCollection
                            {
                                "Description",
                                "Tag",
                                "RelatedTo"
                            };
                            StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
                            iVals[2] = eVals[1];
                            dbInstruments.StartTransaction();
                            dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                            dbInstruments.CommitTransaction();
                        }
                        MessageBox.Show("RelatedTo executado com sucesso!!");
                        foreach (ListViewItem item in listView.Items)
                            this.listView.Items.Remove(item);
                        this.listView.Items.Clear();
                        Instruments.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Objeto não é um equipamento!!");                    }
                    trEquipment.Commit();
                }
            }

        }
        private void buttonInstruments_Click(object sender, EventArgs e)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            dlmInstruments = pnidProj.DataLinksManager;
            dbInstruments = dlmInstruments.GetPnPDatabase();
            docInstruments = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = docInstruments.Editor;
            PromptEntityResult instrument;
            while (true)
            {
                instrument = ed.GetEntity("\nSelecione um  Instrumento: ");
                StringCollection eKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "RelatedTo",
                    "Class",
                    "ClassName",
                    "Status",
                    "Linetype",
                    "Color",
                    "Layer"
                };
                StringCollection eVals = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), eKeys, true);
                if (instrument.Status == PromptStatus.OK)
                {
                    using (var tr = docInstruments.TransactionManager.StartTransaction())
                    {
                        Entity ent = (Entity)tr.GetObject(instrument.ObjectId, OpenMode.ForRead);
                        if (ent.ToString() == "Autodesk.AutoCAD.DatabaseServices.ImpEntity" | ent.Layer == "Instrument")
                        {
                            if (Instruments.Contains(instrument.ObjectId))
                                MessageBox.Show("O instrumento já foi selecionado!!");
                            else
                            {
                                Instruments.Add(instrument.ObjectId);
                                Invoke((MethodInvoker)delegate
                                {
                                    StringCollection keyTag = new StringCollection { "Tag" };
                                    StringCollection valTag = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), keyTag, true);
                                    ListViewItem item = new ListViewItem(dlmInstruments.FindAcPpRowId(instrument.ObjectId).ToString());
                                    item.SubItems.Add(valTag[0]);
                                    listView.Items.Add(item);
                                });
                            }
                        }
                        else
                        {
                            MessageBox.Show("Objeto não é um instrumento!!");
                        }
                        tr.Commit();
                    }
                }
                DialogResult messageBox = MessageBox.Show("Deseja selecionar outro Instrumento?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (messageBox == DialogResult.No)
                    break;
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}