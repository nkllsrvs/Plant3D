﻿using Autodesk.AutoCAD.ApplicationServices;
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
        private readonly List<Instruments> InstrumentsOld = new List<Instruments>();
        private readonly List<ObjectId> Instruments = new List<ObjectId>();
        private Document docInstruments;
        DataLinksManager dlmInstruments;
        PnPDatabase dbInstruments;
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
            if (Instruments.Count > 0)
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
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais instrumentos com RelatedToEquip já preenchido, deseja substituir a propriedade?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (var trEquipment = doc.TransactionManager.StartTransaction())
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
                                        "RelatedToEquip"
                                    };
                                    StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
                                    if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                    {
                                        if (String.IsNullOrEmpty(iVals[1]))
                                        {
                                            iVals[1] = eVals[0];
                                            dbInstruments.StartTransaction();
                                            dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                            Transaction trInstrumenst = docInstruments.TransactionManager.StartTransaction();
                                            Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                            ReplacePropertys(entEdited, InstrumentsOld);
                                            trInstrumenst.Commit();
                                            dbInstruments.CommitTransaction();
                                        }
                                    }
                                    else
                                    {
                                        iVals[1] = eVals[0];
                                        dbInstruments.StartTransaction();
                                        dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                        Transaction trInstrumenst = docInstruments.TransactionManager.StartTransaction();
                                        Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                        ReplacePropertys(entEdited, InstrumentsOld);
                                        trInstrumenst.Commit();
                                        dbInstruments.CommitTransaction();
                                    }
                                }
                                
                                MessageBox.Show("Related To executado com sucesso!!");
                                foreach (ListViewItem item in listView.Items)
                                    this.listView.Items.Remove(item);
                                this.listView.Items.Clear();
                                Instruments.Clear();
                                InstrumentsOld.Clear();
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
            dlmInstruments = pnidProj.DataLinksManager;
            dbInstruments = dlmInstruments.GetPnPDatabase();
            docInstruments = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = docInstruments.Editor;
            PromptEntityResult line;
            docInstruments.LockDocument();

            while (true)
            {
                line = ed.GetEntity("\nSelecione uma linha: ");
                StringCollection iKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "RelatedToEquip",
                    "ClassName"
                };
                StringCollection iVals = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(line.ObjectId), iKeys, true);
                if (line.Status == PromptStatus.OK)
                {
                    using (DocumentLock doclock = docInstruments.LockDocument())
                    {
                        using (var trInstruments = docInstruments.TransactionManager.StartTransaction())
                        {
                            Entity ent1 = (Entity)trInstruments.GetObject(line.ObjectId, OpenMode.ForRead);

                            if (ent1.Id.ObjectClass.DxfName == "SLINE")
                            {
                                if (!String.IsNullOrEmpty(iVals[2]))
                                    countRTE++;
                                if (Instruments.Contains(line.ObjectId))
                                    MessageBox.Show("A linha já foi selecionada!!");
                                else
                                {
                                    Entity ent = (Entity)trInstruments.GetObject(line.ObjectId, OpenMode.ForRead);
                                    Instruments.Add(line.ObjectId);
                                    Instruments iOld = new Instruments();
                                    iOld.Id = ent.ObjectId;
                                    iOld.Layer = ent.Layer;
                                    iOld.LayerId = ent.LayerId;
                                    InstrumentsOld.Add(iOld);
                                    Invoke((MethodInvoker)delegate
                                    {
                                        StringCollection keyTag = new StringCollection { "Tag", "RelatedToEquip", "Layer" };
                                        StringCollection valTag = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(line.ObjectId), keyTag, true);
                                        ListViewItem item = new ListViewItem(valTag[0]);
                                        item.SubItems.Add(valTag[1]);
                                        listView.Items.Add(item);
                                    });
                                }
                            }
                            else
                            {
                                MessageBox.Show(dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(line.ObjectId), iKeys, true)[1] + " não é uma linha!!");
                            }
                            trInstruments.Commit();
                        }

                    }
                }
                DialogResult messageBox = MessageBox.Show("Deseja selecionar outra linha?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (messageBox == DialogResult.No)
                    break;
            }
            if (!(checkBoxEquipmentOtherDWG.Checked) & Instruments.Count > 0)
            {
                PromptEntityResult equipment = ed.GetEntity("\nSelecione uma ou mais Linhas: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countRTE > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe uma ou mais linhas com From já preenchido, deseja substituir a propriedade?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (DocumentLock doclock = docInstruments.LockDocument())
                        {
                            using (var trEquipment = docInstruments.TransactionManager.StartTransaction())
                            {
                                Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                                //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                                if (ent.Id.ObjectClass.DxfName == "SLINE")
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
                                        "RelatedToEquip"
                                    };
                                        StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
                                        if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {
                                                iVals[1] = eVals[0];
                                                dbInstruments.StartTransaction();
                                                dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                                Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                                ReplacePropertys(entEdited, InstrumentsOld);
                                                dbInstruments.CommitTransaction();
                                            }
                                        }
                                        else
                                        {
                                            iVals[1] = eVals[0];
                                            dbInstruments.StartTransaction();
                                            dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
                                            Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                            ReplacePropertys(entEdited, InstrumentsOld);
                                            dbInstruments.CommitTransaction();
                                        }
                                    }
                                    MessageBox.Show("From To executado com sucesso!!");
                                    foreach (ListViewItem item in listView.Items)
                                        this.listView.Items.Remove(item);
                                    this.listView.Items.Clear();
                                    Instruments.Clear();
                                    InstrumentsOld.Clear();
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
        private bool HaveRelatedToEquip(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("RelatedToEquip"))
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
            Instruments.Clear();
            InstrumentsOld.Clear();
            countRTE = 0;
        }
    }
}

