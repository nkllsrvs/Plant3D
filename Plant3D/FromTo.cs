using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace Plant3D
{
    public partial class FromTo : Form
    {
        private readonly List<Instruments> LinhasOld = new List<Instruments>();
        private readonly List<ObjectId> Linhas = new List<ObjectId>();
        private Document docLines;
        DataLinksManager dlmLines;
        PnPDatabase dbLines;
        int countRTE = 0;

        public FromTo()
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
            if (Linhas.Count > 0)
            {
                PlantProject proj = PlantApplication.CurrentProject;
                ProjectPartCollection projParts = proj.ProjectParts;
                PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
                DataLinksManager dlm = pnidProj.DataLinksManager;
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                doc.LockDocument();
                PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento ou  Linha: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countRTE > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais instrumentos com RelatedToEquip já preenchido, deseja substituir a propriedade?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (var trEquipment = doc.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                            //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                            if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                            {
                                int equipmentRowId = dlmLines.FindAcPpRowId(equipment.ObjectId);
                                StringCollection eKeys = new StringCollection { "Tag" };
                                StringCollection eVals = dlmLines.GetProperties(equipmentRowId, eKeys, true);
                                foreach (ObjectId intrumentId in Linhas)
                                {
                                    int instrumentRowId = dlmLines.FindAcPpRowId(intrumentId);
                                    StringCollection iKeys = new StringCollection
                                    {
                                        "Tag",
                                        "Pipe Run To",
                                        "Pipe Run From"
                                    };
                                    StringCollection iVals = dlmLines.GetProperties(instrumentRowId, iKeys, true);
                                    if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                    {
                                        if (String.IsNullOrEmpty(iVals[1]))
                                        {
                                            iVals[1] = eVals[0];
                                            dbLines.StartTransaction();
                                            dlmLines.SetProperties(intrumentId, iKeys, iVals);
                                            Transaction trInstrumenst = docLines.TransactionManager.StartTransaction();
                                            Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                            ReplacePropertys(entEdited, LinhasOld);
                                            trInstrumenst.Commit();
                                            dbLines.CommitTransaction();
                                        }
                                    }
                                    else
                                    {
                                        iVals[1] = eVals[0];
                                        dbLines.StartTransaction();
                                        dlmLines.SetProperties(intrumentId, iKeys, iVals);
                                        Transaction trInstrumenst = docLines.TransactionManager.StartTransaction();
                                        Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                        ReplacePropertys(entEdited, LinhasOld);
                                        trInstrumenst.Commit();
                                        dbLines.CommitTransaction();
                                    }
                                }

                                MessageBox.Show("From To executado com sucesso!!");
                                foreach (ListViewItem item in listView.Items)
                                    this.listView.Items.Remove(item);
                                this.listView.Items.Clear();
                                Linhas.Clear();
                                LinhasOld.Clear();
                                trEquipment.Commit();
                                countRTE = 0;
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Selecione um equipamento ou linha!!");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Equipamento selecionado inválido!");
                }
            }
            else
            {
                MessageBox.Show("Lista de Instrumentos está vazia, selecione instrumentos!!");
            }
        }
        private void buttonLine_Click(object sender, EventArgs e)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            dlmLines = pnidProj.DataLinksManager;
            dbLines = dlmLines.GetPnPDatabase();
            docLines = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = docLines.Editor;
            PromptEntityResult line;
            docLines.LockDocument();

            while (true)
            {
                line = ed.GetEntity("\nSelecione uma linha: ");
                StringCollection iKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "Pipe Run To",
                    "Pipe Run From",
                    "ClassName"
                };
                StringCollection iVals = dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), iKeys, true);
                if (line.Status == PromptStatus.OK)
                {
                    using (DocumentLock doclock = docLines.LockDocument())
                    {
                        using (var trLine = docLines.TransactionManager.StartTransaction())
                        {
                            Entity ent1 = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);

                            if (ent1.Id.ObjectClass.DxfName == "SLINE")
                            {
                                Entity ent = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);
                                Linhas.Add(line.ObjectId);
                                Instruments iOld = new Instruments();
                                iOld.Id = ent.ObjectId;
                                iOld.Layer = ent.Layer;
                                iOld.LayerId = ent.LayerId;
                                LinhasOld.Add(iOld);
                                Invoke((MethodInvoker)delegate
                                {
                                    StringCollection keyTag = new StringCollection { "Tag", "Pipe Run To", "Pipe Run From", "Layer" };
                                    StringCollection valTag = dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), keyTag, true);
                                    ListViewItem item = new ListViewItem(valTag[0]);
                                    item.SubItems.Add(valTag[1]);
                                    listView.Items.Add(item);
                                });
                                trLine.Commit();
                                break;
                            }
                            else
                            {
                                MessageBox.Show(dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), iKeys, true)[1] + " não é uma linha!!");
                            }
                            trLine.Commit();
                        }

                    }
                }
            }
            if (!(checkBoxEquipmentOtherDWG.Checked) & Linhas.Count > 0)
            {
                PromptEntityResult equipment = ed.GetEntity("\nSelecione uma ou mais Linhas: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countRTE > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe uma ou mais linhas com From/to já preenchido, deseja substituir a propriedade?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (DocumentLock doclock = docLines.LockDocument())
                        {
                            using (var trEquipment = docLines.TransactionManager.StartTransaction())
                            {
                                Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                                //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                                if (ent.Id.ObjectClass.DxfName == "SLINE")
                                {
                                    int equipmentRowId = dlmLines.FindAcPpRowId(equipment.ObjectId);
                                    StringCollection eKeys = new StringCollection { "Tag" };
                                    StringCollection eVals = dlmLines.GetProperties(equipmentRowId, eKeys, true);
                                    foreach (ObjectId intrumentId in Linhas)
                                    {
                                        int instrumentRowId = dlmLines.FindAcPpRowId(intrumentId);
                                        StringCollection iKeys = new StringCollection
                                    {
                                        "Tag",
                                        "Pipe Run To",
                                        "Pipe Run From"
                                    };
                                        StringCollection iVals = dlmLines.GetProperties(instrumentRowId, iKeys, true);
                                        if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {
                                                iVals[1] = eVals[0];
                                                dbLines.StartTransaction();
                                                dlmLines.SetProperties(intrumentId, iKeys, iVals);
                                                Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                                ReplacePropertys(entEdited, LinhasOld);
                                                dbLines.CommitTransaction();
                                            }
                                        }
                                        else
                                        {
                                            iVals[1] = eVals[0];
                                            dbLines.StartTransaction();
                                            dlmLines.SetProperties(intrumentId, iKeys, iVals);
                                            Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                            ReplacePropertys(entEdited, LinhasOld);
                                            dbLines.CommitTransaction();
                                        }
                                    }
                                    MessageBox.Show("From To executado com sucesso!!");
                                    foreach (ListViewItem item in listView.Items)
                                        this.listView.Items.Remove(item);
                                    this.listView.Items.Clear();
                                    Linhas.Clear();
                                    LinhasOld.Clear();
                                    trEquipment.Commit();
                                    countRTE = 0;
                                    break;
                                }
                                else
                                {
                                    MessageBox.Show("Selecione uma linha!!");
                                }
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
        private bool HavePipeRunFromTo(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("Pipe Run To"))
                    return true;
            }
            return false;
        }
        private void ReplacePropertys(Entity entityEdited, List<Instruments> instruments)
        {
            foreach (Instruments i in instruments)
            {
                if (entityEdited.ObjectId == i.Id)
                {
                    entityEdited.Layer = i.Layer;
                    entityEdited.LayerId = i.LayerId;
                    break;
                }
            }

        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void buttonClearClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.Items)
                this.listView.Items.Remove(item);
            this.listView.Items.Clear();
            Linhas.Clear();
            LinhasOld.Clear();
            countRTE = 0;
        }
        public class Lines : IEnumerable
        {
            public Lines() { }
            public Lines(ObjectId Id, String Layer, ObjectId LayerId)
            {
                this.Id = Id;
                this.Layer = Layer;
                this.LayerId = LayerId;
            }
            public Autodesk.AutoCAD.DatabaseServices.ObjectId Id { get; set; }
            public String Layer { get; set; }
            public Autodesk.AutoCAD.DatabaseServices.ObjectId LayerId { get; set; }

            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

    }
}

