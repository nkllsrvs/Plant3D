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
using Plant3D.Classes;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using System.IO;
using System.Reflection;

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
        List<DocumentObject> pipeLineGroup = new List<DocumentObject>();
        DocumentObject selectedLine;
        DocumentObject selectedFrom;
        DocumentObject selectedTo;
        TransactionManager tmLinesFirstDWG;
        TransactionManager tmLinesSecondDWG;

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
            tmLinesFirstDWG = docLines.TransactionManager;
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
                        using (tmLinesFirstDWG.StartTransaction())
                        {
                            Entity entityIf = (Entity)tmLinesFirstDWG.GetObject(line.ObjectId, OpenMode.ForRead);

                            if (entityIf.Id.ObjectClass.DxfName == "SLINE")
                            {
                                Entity ent = (Entity)tmLinesFirstDWG.GetObject(line.ObjectId, OpenMode.ForRead);
                                StringCollection keyTag = new StringCollection { "Tag", "PipeRunTo", "PipeRunFrom", "Layer" };
                                StringCollection valTag = dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), keyTag, true);
                                selectedLine = new()
                                {
                                    Layer = ent.Layer,
                                    LayerId = ent.LayerId,
                                    Tag = valTag[0],
                                    Id = ent.ObjectId,
                                    BelongingDocument = docLines.Name,
                                    FromOtherDWG = false
                                };
                                PromptSelectionResult selection = docLines.Editor.SelectAll();
                                using (tmLinesFirstDWG.StartTransaction())
                                {
                                    foreach (ObjectId id in selection.Value.GetObjectIds())
                                    {
                                        Entity entity = (Entity)tmLinesFirstDWG.GetObject(id, OpenMode.ForRead);
                                        LayerTableRecord layer = (LayerTableRecord)tmLinesFirstDWG.GetObject(entity.LayerId, OpenMode.ForRead);
                                        if (!layer.IsFrozen)
                                        {
                                            StringCollection entKeys = new StringCollection { "Tag", "PipeRunTo", "PipeRunFrom", "Layer" };
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
                                                        BelongingDocument = docLines.Name,
                                                        FromOtherDWG = false
                                                    };
                                                    if (SamePipeLineGroup(TagPipeLineGroup(selectedLine.Tag), TagPipeLineGroup(docObj.Tag)))
                                                    {
                                                        if (!String.IsNullOrEmpty(entVal[1]) | !String.IsNullOrEmpty(entVal[2]))
                                                            countFT++;
                                                        pipeLineGroup.Add(docObj);
                                                        Linhas.Add(id);
                                                        Invoke((MethodInvoker)delegate
                                                        {
                                                            ListViewItem item = new ListViewItem(entVal[0]);
                                                            item.SubItems.Add(entVal[2]);
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
                                    tmLinesFirstDWG.StartTransaction().Commit();
                                    break;
                                }
                            }
                            else
                            {
                                MessageBox.Show(dlmLines.GetProperties(dlmLines.FindAcPpRowId(line.ObjectId), iKeys, true)[1] + " não é uma linha!!");
                            }
                            tmLinesFirstDWG.StartTransaction().Commit();
                        }
                    }
                }
            }
            PromptEntityResult objectFrom = ed.GetEntity("\nSelecione o objeto From: ");
            if (objectFrom.Status == PromptStatus.OK)
            {
                while (true)
                {
                    using (DocumentLock doclock = docLines.LockDocument())
                    {
                        using (var trFrom = docLines.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)trFrom.GetObject(objectFrom.ObjectId, OpenMode.ForRead);
                            //ao invés de usar uma classe use o metodo de verificar se tem TAG ou usar classe de Equipment(Geral)
                            if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                            {
                                int fromRowId = dlmLines.FindAcPpRowId(objectFrom.ObjectId);
                                StringCollection fromKeys = new StringCollection { "Tag" };
                                StringCollection fromVals = dlmLines.GetProperties(fromRowId, fromKeys, true);
                                selectedFrom = new DocumentObject()
                                {
                                    Layer = ent.Layer,
                                    LayerId = ent.LayerId,
                                    Tag = fromVals[0],
                                    Id = ent.ObjectId,
                                    BelongingDocument = docLines.Name,
                                    FromOtherDWG = false
                                };
                                foreach (DocumentObject lineObj in pipeLineGroup)
                                {
                                    int lineRowId = dlmLines.FindAcPpRowId(lineObj.Id);
                                    StringCollection lKeys = new StringCollection
                                    {
                                        "Tag",
                                        "PipeRunFrom"
                                    };
                                    StringCollection lVals = dlmLines.GetProperties(lineRowId, lKeys, true);
                                    lVals[1] = fromVals[0];
                                    dbLines.StartTransaction();
                                    dlmLines.SetProperties(lineObj.Id, lKeys, lVals);
                                    dbLines.CommitTransaction();
                                }
                                trFrom.Commit();
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Selecione um objeto válido!!");
                            }
                        }
                    }
                }
            }
            if (!(checkBoxOtherDWG.Checked) & pipeLineGroup.Count > 0)
            {
                while (true)
                {

                    //mudar equipment para object
                    PromptEntityResult objectTo = ed.GetEntity("\nSelecione o objeto To: ");
                    if (objectTo.Status == PromptStatus.OK)
                    {
                        DialogResult messageReplaceFromTo = DialogResult.OK;
                        if (countFT > 0)
                            messageReplaceFromTo = MessageBox.Show("Existe um ou mais elementos com PipeRunFrom/PipeRunTo já preenchidos, deseja substituir o valor atual do atributo?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (messageReplaceFromTo == DialogResult.OK)
                        {
                            using (DocumentLock doclock = docLines.LockDocument())
                            {
                                using (var trTo = docLines.TransactionManager.StartTransaction())
                                {
                                    Entity ent = (Entity)trTo.GetObject(objectTo.ObjectId, OpenMode.ForRead);
                                    //ao invés de usar uma classe use o metodo de verificar se tem TAG ou usar classe de Equipment(Geral)
                                    if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                                    {
                                        int toRowId = dlmLines.FindAcPpRowId(objectTo.ObjectId);
                                        StringCollection toKeys = new StringCollection { "Tag" };
                                        StringCollection toVals = dlmLines.GetProperties(toRowId, toKeys, true);
                                        foreach (DocumentObject lineObj in pipeLineGroup)
                                        {
                                            int lineRowId = dlmLines.FindAcPpRowId(lineObj.Id);
                                            StringCollection lKeys = new StringCollection
                                        {
                                            "Tag",
                                            "PipeRunTo",
                                            "PipeRunFrom"
                                        };
                                            StringCollection lVals = dlmLines.GetProperties(lineRowId, lKeys, true);
                                            if (countFT > 0 & messageReplaceFromTo == DialogResult.No)
                                            {
                                                if (String.IsNullOrEmpty(lVals[1]))
                                                {
                                                    lVals[1] = toVals[0];
                                                    dbLines.StartTransaction();
                                                    dlmLines.SetProperties(lineObj.Id, lKeys, lVals);
                                                    Entity entEdited = (Entity)trTo.GetObject(lineObj.Id, OpenMode.ForWrite);
                                                    ReplacePropertys(entEdited, LinhasOld);
                                                    dbLines.CommitTransaction();
                                                }
                                            }
                                            else
                                            {
                                                lVals[1] = toVals[0];
                                                dbLines.StartTransaction();
                                                dlmLines.SetProperties(lineObj.Id, lKeys, lVals);
                                                Entity entEdited = (Entity)trTo.GetObject(lineObj.Id, OpenMode.ForWrite);
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
                                        trTo.Commit();
                                        countFT = 0;
                                        break;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Categoria de elemento inválida!");

                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("From/To não foi executado.", "From To", MessageBoxButtons.OK, MessageBoxIcon.Question);
                            foreach (ListViewItem item in listView.Items)
                                this.listView.Items.Remove(item);
                            this.listView.Items.Clear();
                            Linhas.Clear();
                            LinhasOld.Clear();
                            countFT = 0;
                            break;
                        }
                    }
                }
            }
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
                PnPDatabase db = dlm.GetPnPDatabase();
                tmLinesSecondDWG = doc.Database.TransactionManager;
                doc.LockDocument();
                PromptSelectionResult selection = doc.Editor.SelectAll();
                using (var tr1 = doc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in selection.Value.GetObjectIds())
                    {
                        Entity entity = (Entity)tr1.GetObject(id, OpenMode.ForRead);
                        LayerTableRecord layer = (LayerTableRecord)tr1.GetObject(entity.LayerId, OpenMode.ForRead);
                        if (!layer.IsFrozen)
                        {
                            StringCollection entKeys = new StringCollection { "Tag", "PipeRunTo", "PipeRunFrom", "Layer" };
                            //existe um objeto onde eu n posso pegar o rowId para fazer um get 
                            //no database dos valores respectivos as chaves
                            try
                            {
                                if (HaveTag(dlm.GetAllProperties(id, true)))
                                {
                                    StringCollection entVal = dlm.GetProperties(dlm.FindAcPpRowId(entity.ObjectId), entKeys, true);
                                    DocumentObject docObj = new()
                                    {
                                        Layer = entity.Layer,
                                        LayerId = entity.LayerId,
                                        Tag = entVal[0],
                                        Id = entity.ObjectId,
                                        BelongingDocument = doc.Name,
                                        FromOtherDWG = true,
                                    };
                                    if (SamePipeLineGroup(TagPipeLineGroup(docObj.Tag), TagPipeLineGroup(selectedLine.Tag)))
                                    {
                                        pipeLineGroup.Add(docObj);
                                        Linhas.Add(id);
                                        Invoke((MethodInvoker)delegate
                                        {
                                            ListViewItem item = new ListViewItem(entVal[0]);
                                            item.SubItems.Add(entVal[2]);
                                            item.SubItems.Add(entVal[1]);
                                            formFromTo.listView.Items.Add(item);
                                        });
                                        int lineRowId = dlm.FindAcPpRowId(docObj.Id);
                                        StringCollection lKeys = new StringCollection
                                        {
                                            "Tag",
                                            "PipeRunFrom"
                                        };
                                        StringCollection lVals = dlmLines.GetProperties(lineRowId, lKeys, true);
                                        lVals[1] = selectedFrom.Tag;
                                        dlm.GetPnPDatabase().StartTransaction();
                                        dlm.SetProperties(docObj.Id, lKeys, lVals);
                                        dlm.GetPnPDatabase().CommitTransaction();
                                    }
                                }
                            }
                            catch (DLException)
                            {

                            }
                        }
                    }
                    tr1.Commit();
                }
                PromptEntityResult equipment = ed.GetEntity("\nSelecione o objeto To: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countFT > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais instrumentos com PipeRunTo e From já preenchido, deseja substituir a propriedade?", "From To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (var tr2 = doc.TransactionManager.StartOpenCloseTransaction())
                        {
                            Entity ent = (Entity)tr2.GetObject(equipment.ObjectId, OpenMode.ForRead);
                            //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                            if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
                            {
                                int equipmentRowId = dlm.FindAcPpRowId(equipment.ObjectId);
                                StringCollection eKeys = new StringCollection { "Tag" };
                                StringCollection eVals = dlm.GetProperties(equipmentRowId, eKeys, true);
                                selectedTo = new DocumentObject()
                                {
                                    Layer = ent.Layer,
                                    LayerId = ent.LayerId,
                                    Tag = eVals[0],
                                    Id = ent.ObjectId,
                                    BelongingDocument = doc.Name,
                                    FromOtherDWG = true,
                                };
                                foreach (ObjectId lineID in Linhas)
                                {
                                    if (pipeLineGroup.Find(f => f.Id == lineID).FromOtherDWG == true)
                                    {
                                        int lineRowID = dlm.FindAcPpRowId(lineID);
                                        StringCollection iKeys = new StringCollection
                                        {
                                            "Tag",
                                            "PipeRunTo",
                                            "PipeRunFrom"
                                        };
                                        StringCollection iVals = dlm.GetProperties(lineRowID, iKeys, true);
                                        if (countFT > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {
                                                iVals[1] = selectedTo.Tag;
                                                db.StartTransaction();
                                                dlm.SetProperties(lineID, iKeys, iVals);
                                                //Entity entEdited = (Entity)tmLinesSecondDWG.GetObject(lineID, OpenMode.ForWrite);
                                                //ReplacePropertys(entEdited, LinhasOld);
                                                db.CommitTransaction();
                                            }
                                        }
                                        else
                                        {
                                            iVals[1] = selectedTo.Tag;
                                            db.StartTransaction();
                                            dlm.SetProperties(lineID, iKeys, iVals);
                                            //Entity entEdited = (Entity)tmLinesSecondDWG.GetObject(lineID, OpenMode.ForWrite);
                                            //ReplacePropertys(entEdited, LinhasOld);
                                            db.CommitTransaction();
                                            StringCollection test = dlmLines.GetProperties(lineRowID, iKeys, true);
                                            Log($"Tag = {test[0]} PipeRunFrom = {test[2]} PipeRunTo = {test[1]}", "PlantsLog");
                                        }
                                    }
                                    else
                                    {
                                        int lineRowID = dlmLines.FindAcPpRowId(lineID);
                                        StringCollection iKeys = new StringCollection
                                        {
                                            "Tag",
                                            "PipeRunTo",
                                            "PipeRunFrom"
                                        };
                                        StringCollection iVals = dlmLines.GetProperties(lineRowID, iKeys, true);
                                        Log($"countFT = {countFT} messageReplaceRelatedToEquip = {messageReplaceRelatedToEquip.ToString()} DialogResult = {DialogResult.No.ToString()}", "PlantsLog");
                                        if (countFT > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {
                                                iVals[1] = selectedTo.Tag;
                                                dbLines.StartTransaction();
                                                var val = docLines.TransactionManager.StartTransaction();
                                                dlmLines.SetProperties(lineID, iKeys, iVals);
                                                //Entity entEdited = (Entity)tmLinesFirstDWG.GetObject(lineID, OpenMode.ForWrite);
                                                //ReplacePropertys(entEdited, LinhasOld);
                                                val.Commit();
                                                dbLines.CommitTransaction();
                                            }
                                        }
                                        else
                                        {
                                            iVals[1] = selectedTo.Tag;
                                            dbLines.StartTransaction();
                                            var val = docLines.TransactionManager.StartTransaction();
                                            dlmLines.SetProperties(lineID, iKeys, iVals);
                                            //Entity entEdited = (Entity)tmLinesFirstDWG.GetObject(lineID, OpenMode.ForWrite);
                                            //ReplacePropertys(entEdited, LinhasOld);
                                            val.Commit();
                                            dbLines.CommitTransaction();
                                            StringCollection test = dlmLines.GetProperties(lineRowID, iKeys, true);
                                            Log($"Tag = {test[0]} PipeRunFrom = {test[2]} PipeRunTo = {test[1]}", "PlantsLog");
                                        }
                                    }

                                }

                                MessageBox.Show("From To executado com sucesso!!");
                                foreach (ListViewItem item in listView.Items)
                                    this.listView.Items.Remove(item);
                                this.listView.Items.Clear();
                                Linhas.Clear();
                                LinhasOld.Clear();
                                countFT = 0;
                                doc.TransactionManager.StartTransaction().Commit();
                                tr2.Commit();
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Selecione o objeto To!!");
                            }

                        }
                    }
                }
                else
                {
                    MessageBox.Show("Objeto selecionado inválido!");
                }
            }
            else
            {
                MessageBox.Show("Lista de linhas está vazia, selecione uma ou mais linhas!!");
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

        public static bool Log(string strMensagem, string strNomeArquivo = "ArquivoLog")
        {
            try
            {
                var caminhoExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string caminhoArquivo = Path.Combine(caminhoExe, strNomeArquivo);
                if (!File.Exists(caminhoArquivo))
                {
                    FileStream arquivo = File.Create(caminhoArquivo);
                    arquivo.Close();
                }
                using (StreamWriter w = File.AppendText(caminhoArquivo))
                {
                    AppendLog(strMensagem, w);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static void AppendLog(string logMensagem, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entrada : ");
                txtWriter.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine($"  :{logMensagem}");
                txtWriter.WriteLine("------------------------------------");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}


