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
        private readonly List<Instruments> InstrumentsOld = new List<Instruments>();
        private readonly List<ObjectId> Instruments = new List<ObjectId>();
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
            if(Instruments.Count > 0)
            {
                PlantProject proj = PlantApplication.CurrentProject;
                ProjectPartCollection projParts = proj.ProjectParts;
                PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
                DataLinksManager dlm = pnidProj.DataLinksManager;
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;

                PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento ou  Linha: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    while (true)
                    {
                        using (var trEquipment = docInstruments.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                            //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                            if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                            {
                                int equipmentRowId = dlmInstruments.FindAcPpRowId(equipment.ObjectId);
                                StringCollection eKeys = new StringCollection { "Tag" };
                                StringCollection eVals = dlmInstruments.GetProperties(equipmentRowId, eKeys, true);
                                foreach (ObjectId intrumentId in Instruments)
                                {
                                    int instrumentRowId = dlmInstruments.FindAcPpRowId(intrumentId);
                                    StringCollection iKeys = new StringCollection
                                    {
                                        "Tag",
                                        "RelatedTo"
                                    };
                                    StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
                                    iVals[1] = eVals[0];
                                    dbInstruments.StartTransaction();
                                    dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                    dbInstruments.CommitTransaction();
                                }
                                MessageBox.Show("Related To executado com sucesso!!");
                                foreach (ListViewItem item in listView.Items)
                                    this.listView.Items.Remove(item);
                                this.listView.Items.Clear();
                                Instruments.Clear();
                                trEquipment.Commit();
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Selecione um equipamento ou linha!!");
                            }
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show("Lista de Instrumentos está vazia, selecione instrumentos!!");
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
                    "ClassName"
                };
                StringCollection eVals = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), eKeys, true);
                if (instrument.Status == PromptStatus.OK)
                {
                    using (var tr = docInstruments.TransactionManager.StartTransaction())
                    {
                        if (HaveRelatedToEquip(dlmInstruments.GetAllProperties(instrument.ObjectId, true)))
                        {
                            if (Instruments.Contains(instrument.ObjectId))
                                MessageBox.Show("O instrumento já foi selecionado!!");
                            else
                            {
                                Entity ent = (Entity)tr.GetObject(instrument.ObjectId, OpenMode.ForRead);
                                Instruments.Add(instrument.ObjectId);
                                Instruments iOld = new Instruments();
                                iOld.Id = ent.ObjectId;
                                iOld.Layer = ent.Layer;
                                iOld.LayerId = ent.LayerId;
                                InstrumentsOld.Add(iOld);
                                Invoke((MethodInvoker)delegate
                                {
                                    StringCollection keyTag = new StringCollection { "Tag", "RelatedTo", "Layer" };
                                    StringCollection valTag = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), keyTag, true);
                                    ListViewItem item = new ListViewItem(valTag[0]);
                                    item.SubItems.Add(valTag[1]);
                                    listView.Items.Add(item);
                                });
                            }
                        }
                        else
                        {
                            MessageBox.Show(dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), eKeys, true)[1] + " não é um instrumento!!");
                        }
                        tr.Commit();
                    }
                }
                DialogResult messageBox = MessageBox.Show("Deseja selecionar outro Instrumento?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (messageBox == DialogResult.No)
                    break;
            }
            if (!(checkBoxEquipmentOtherDWG.Checked) & Instruments.Count > 0)
            {
                PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento ou Linhas: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    while (true)
                    {
                        using (var trEquipment = docInstruments.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                            //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                            if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                            {
                                int equipmentRowId = dlmInstruments.FindAcPpRowId(equipment.ObjectId);
                                StringCollection eKeys = new StringCollection {"Tag"};
                                StringCollection eVals = dlmInstruments.GetProperties(equipmentRowId, eKeys, true);
                                foreach (ObjectId intrumentId in Instruments)
                                {
                                    int instrumentRowId = dlmInstruments.FindAcPpRowId(intrumentId);
                                    StringCollection iKeys = new StringCollection
                                    {
                                        "Tag",
                                        "RelatedTo"
                                    };
                                    StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
                                    iVals[1] = eVals[0];
                                    dbInstruments.StartTransaction();
                                    dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                    Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                    
                                    dbInstruments.CommitTransaction();
                                }
                                MessageBox.Show("Related To executado com sucesso!!");
                                foreach (ListViewItem item in listView.Items)
                                    this.listView.Items.Remove(item);
                                this.listView.Items.Clear();
                                Instruments.Clear();
                                InstrumentsOld.Clear();
                                trEquipment.Commit();
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Selecione um equipamento ou linha!!");
                            }
                        }
                    }
                }
            }
        }
        private void checkBoxEquipmentOtherDWG_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxEquipmentOtherDWG.Checked == true)
                {
                    buttonEquipment.Enabled = true;
                    buttonEquipment.BackColor = Color.LightGreen;
                }
                if (checkBoxEquipmentOtherDWG.Checked == false)
                {
                    buttonEquipment.Enabled = false;
                    buttonEquipment.BackColor = Color.Silver;
                }
            }
            catch { }
        }
        private bool HaveRelatedToEquip(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if(kvp.Key.Equals("RelatedTo"))
                    return true;
            }
            return false;
        }
        private void ReplacePropertys(Entity ent, Instruments instruments)
        {
            if(ent.ObjectId == instruments.Id)
            {
                ent.Layer = instruments.Layer;
                ent.LayerId = instruments.LayerId;
            }
        }

    }

    public class  Instruments
    {
        public ObjectId Id { get; set; }
        public String Layer { get; set; }
        public ObjectId LayerId { get; set; }
    }
}