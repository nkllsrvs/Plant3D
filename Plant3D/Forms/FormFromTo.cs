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
using Plant3D.Classes;

namespace Plant3D
{
    public partial class FormFromTo : Commands
    {
        private readonly List<Instruments> LinhasOld = new List<Instruments>();
        private readonly List<ObjectId> Linhas = new List<ObjectId>();
        private Document docLines;
        DataLinksManager dlmLines;
        PnPDatabase dbLines;
        int countFT = 0;

        public FormFromTo()
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
        private void buttonFromTo_Click(object sender, EventArgs e)
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
                    if (countFT > 0)
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
                                    if (countFT > 0 & messageReplaceRelatedToEquip == DialogResult.No)
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
                                countFT = 0;
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
            List<DocumentObject> pipeLineGroup = new List<DocumentObject>();

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
                            Entity entityIf = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);

                            if (entityIf.Id.ObjectClass.DxfName == "SLINE")
                            {
                                Entity ent = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);
                                StringCollection keyTag = new StringCollection { "Tag", "Pipe Run To", "Pipe Run From", "Layer" };
                                StringCollection valTag = dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), keyTag, true);
                                DocumentObject selectedLine = new()
                                {
                                    Layer = ent.Layer,
                                    LayerId = ent.LayerId,
                                    Tag = valTag[0],
                                    Id = ent.ObjectId,
                                    BelongingDocument = docLines.Name
                                };
                                PromptSelectionResult selection = docLines.Editor.SelectAll();


                                using (Transaction tr = docLines.Database.TransactionManager.StartOpenCloseTransaction())
                                {
                                    foreach (ObjectId id in selection.Value.GetObjectIds())
                                    {
                                        Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);
                                        LayerTableRecord layer = (LayerTableRecord)tr.GetObject(entity.LayerId, OpenMode.ForRead);
                                        if (!layer.IsFrozen)
                                        {
                                            StringCollection entKeys = new StringCollection { "Tag", "Pipe Run To", "Pipe Run From", "Layer" };
                                            //existe um objeto onde eu n posso pegar o rowId para fazer um get 
                                            //no database dos valores respectivos as chaves
                                            try
                                            {
                                                if (HaveTag(dlmLines.GetAllProperties(id, true)))
                                                {
                                                    StringCollection entVal = dlmLines.GetProperties(dlmLines.FindAcPpRowId(entity.ObjectId), entKeys, true);
                                                    DocumentObject docObj = new()
                                                    {
                                                        Layer = entity.Layer,
                                                        LayerId = entity.LayerId,
                                                        Tag = entVal[0],
                                                        Id = entity.ObjectId,
                                                        BelongingDocument = docLines.Name
                                                    };
                                                    if (SamePipeLineGroup(TagPipeLineGroup(selectedLine.Tag), TagPipeLineGroup(docObj.Tag)))
                                                    {
                                                        pipeLineGroup.Add(docObj);
                                                        Invoke((MethodInvoker)delegate
                                                        {
                                                            ListViewItem item = new ListViewItem(entVal[0]);
                                                            item.SubItems.Add(entVal[1]);
                                                            formFromTo.listView.Items.Add(item);
                                                        });
                                                    }

                                                }
                                            }
                                            catch (DLException)
                                            {

                                            }
                                        }
                                    }
                                    tr.Commit();
                                    trLine.Commit();
                                    break;
                                }
                            }
                            else
                            {
                                MessageBox.Show(dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), iKeys, true)[1] + " não é uma linha!!");
                            }
                        }
                    }
                }
            }
            if (!(checkBoxOtherDWG.Checked) & pipeLineGroup.Count > 0)
            {
                //mudar equipment para object
                PromptEntityResult objectTo = ed.GetEntity("\nSelecione o objeto To: ");
                if (objectTo.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countFT > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe uma ou mais linhas com Pipe Run To já preenchido, deseja substituir a propriedade?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (DocumentLock doclock = docLines.LockDocument())
                        {
                            using (var trEquipment = docLines.TransactionManager.StartTransaction())
                            {
                                Entity ent = (Entity)trEquipment.GetObject(objectTo.ObjectId, OpenMode.ForRead);
                                //ao invés de usar uma classe use o metodo de verificar se tem TAG ou usar classe de Equipment(Geral)
                                if (ent.Id.ObjectClass.DxfName == "ACPPASSET")
                                {
                                    int equipmentRowId = dlmLines.FindAcPpRowId(objectTo.ObjectId);
                                    StringCollection eKeys = new StringCollection { "Tag" };
                                    StringCollection eVals = dlmLines.GetProperties(equipmentRowId, eKeys, true);
                                    foreach (DocumentObject lineObj in pipeLineGroup)
                                    {
                                        int lineRowId = dlmLines.FindAcPpRowId(lineObj.Id);
                                        StringCollection lKeys = new StringCollection
                                            {
                                                "Tag",
                                                "Pipe Run To",
                                                "Pipe Run From"
                                            };
                                        StringCollection lVals = dlmLines.GetProperties(lineRowId, lKeys, true);
                                        if (countFT > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(lVals[1]))
                                            {
                                                lVals[1] = eVals[0];
                                                dbLines.StartTransaction();
                                                dlmLines.SetProperties(lineObj.Id, lKeys, lVals);
                                                Entity entEdited = (Entity)trEquipment.GetObject(lineObj.Id, OpenMode.ForWrite);
                                                ReplacePropertys(entEdited, LinhasOld);
                                                dbLines.CommitTransaction();
                                            }
                                        }
                                        else
                                        {
                                            lVals[1] = eVals[0];
                                            dbLines.StartTransaction();
                                            dlmLines.SetProperties(lineObj.Id, lKeys, lVals);
                                            Entity entEdited = (Entity)trEquipment.GetObject(lineObj.Id, OpenMode.ForWrite);
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
                                    countFT = 0;
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
                if (checkBoxOtherDWG.Checked == true)
                {
                    buttonEquipment.Enabled = true;
                    buttonEquipment.BackColor = Color.LightGreen;
                }
                if (checkBoxOtherDWG.Checked == false)
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
            countFT = 0;
        }

    }
}

