using System;
using System.Collections.Generic;

//Autocad namespaces
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

//Plant3D nasmespaces
using Autodesk.ProcessPower.ProjectManager;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.Windows;
using System.Windows.Media.Imaging;
using Orientation = System.Windows.Controls.Orientation;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.P3dProjectParts;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Forms;
using static Plant3D.Commands;

//Classes
using Plant3D.Classes;
using System.Runtime.InteropServices;
using System.Linq;
using Autodesk.ProcessPower.DataObjects;

[assembly: CommandClass(typeof(Plant3D.Commands))]
[assembly: CommandClass(typeof(Plant3D.Classes.VALERibbon))]
[assembly: CommandClass(typeof(Plant3D.Classes.DocumentObject))]
[assembly: CommandClass(typeof(Plant3D.Classes.Instruments))]
[assembly: CommandClass(typeof(Plant3D.Classes.Linetype))]
[assembly: CommandClass(typeof(Plant3D.Classes.VALERibbonButtonCommandeHandler))]
namespace Plant3D
{
    public class Commands : Form
    {
        public static FormRelatedTo formRelateTo = new FormRelatedTo();
        public static FormFromTo formFromTo = new FormFromTo();
        static readonly StringCollection linetypesSubstitute = new StringCollection { "Continuous", "DASHDOT", "HIDDEN", "HIDDEN2" };
        public static List<DocumentInfo> documentInfos = new List<DocumentInfo>();
        public readonly List<Instruments> InstrumentsRTOld = new List<Instruments>();
        public readonly List<ObjectId> InstrumentsRT = new List<ObjectId>();
        public Document docInstrumentsRT;
        public DataLinksManager dlmInstrumentsRT;
        public PnPDatabase dbInstrumentsRT;
        int countRTE = 0;

        #region RelatedTo
        [CommandMethod("_RLTT")]
        public void RelatedTo()
        {
            try
            {
                if (formRelateTo == null || formRelateTo.IsDisposed)
                    formRelateTo = new FormRelatedTo();
                formRelateTo.Show();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            { }
        }
        public void buttonInstrumentsRT_Click(object sender, EventArgs e)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            dlmInstrumentsRT = pnidProj.DataLinksManager;
            dbInstrumentsRT = dlmInstrumentsRT.GetPnPDatabase();
            docInstrumentsRT = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = docInstrumentsRT.Editor;
            PromptEntityResult instrument;
            docInstrumentsRT.LockDocument();
            while (true)
            {
                instrument = ed.GetEntity("\nSelecione um  Instrumento: ");
                StringCollection iKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "RelatedToEquip",
                    "ClassName"
                };
                StringCollection iVals = dlmInstrumentsRT.GetProperties(dlmInstrumentsRT.FindAcPpRowId(instrument.ObjectId), iKeys, true);
                if (instrument.Status == PromptStatus.OK)
                {
                    using (DocumentLock doclock = docInstrumentsRT.LockDocument())
                    {
                        using (var trInstruments = docInstrumentsRT.TransactionManager.StartTransaction())
                        {
                            if (formRelateTo.HaveRelatedToEquip(dlmInstrumentsRT.GetAllProperties(instrument.ObjectId, true)))
                            {
                                if (!String.IsNullOrEmpty(iVals[2]))
                                    countRTE++;
                                if (InstrumentsRT.Contains(instrument.ObjectId))
                                    MessageBox.Show("O instrumento já foi selecionado!!");
                                else
                                {
                                    StringCollection keyTag = new StringCollection { "Tag", "RelatedToEquip", "Layer" };
                                    StringCollection valTag = dlmInstrumentsRT.GetProperties(dlmInstrumentsRT.FindAcPpRowId(instrument.ObjectId), keyTag, true);

                                    Entity ent = (Entity)trInstruments.GetObject(instrument.ObjectId, OpenMode.ForRead);
                                    InstrumentsRT.Add(instrument.ObjectId);
                                    Instruments iOld = new Instruments();
                                    iOld.Id = ent.ObjectId;
                                    iOld.Layer = ent.Layer;
                                    iOld.LayerId = ent.LayerId;
                                    iOld.Tag = valTag[0];
                                    iOld.RelatedTo = true;
                                    InstrumentsRTOld.Add(iOld);

                                    Invoke((MethodInvoker)delegate
                                    {
                                        ListViewItem item = new ListViewItem(valTag[0]);
                                        item.SubItems.Add(valTag[1]);
                                        formRelateTo.listView.Items.Add(item);
                                    });
                                }
                            }
                            else
                            {
                                MessageBox.Show(dlmInstrumentsRT.GetProperties(dlmInstrumentsRT.FindAcPpRowId(instrument.ObjectId), iKeys, true)[1] + " não é um instrumento!!");
                            }
                            trInstruments.Commit();
                        }

                    }
                }
                DialogResult messageBox = MessageBox.Show("Deseja selecionar outro Instrumento?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (messageBox == DialogResult.No)
                    break;
            }
            if (!(formRelateTo.checkBoxEquipmentOtherDWG.Checked) & InstrumentsRT.Count > 0)
            {
                PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento ou Linhas: ");
                if (equipment.Status == PromptStatus.OK)
                {
                    DialogResult messageReplaceRelatedToEquip = new DialogResult();
                    if (countRTE > 0)
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais instrumentos com RelatedToEquip já preenchido, deseja substituir a propriedade?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    while (true)
                    {
                        using (DocumentLock doclock = docInstrumentsRT.LockDocument())
                        {
                            using (var trEquipment = docInstrumentsRT.TransactionManager.StartTransaction())
                            {
                                Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
                                //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
                                if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE") //ACPPDYNAMICASSET
                                {
                                    int equipmentRowId = dlmInstrumentsRT.FindAcPpRowId(equipment.ObjectId);
                                    StringCollection eKeys = new StringCollection { "Tag" };
                                    StringCollection eVals = dlmInstrumentsRT.GetProperties(equipmentRowId, eKeys, true);

                                    DocumentObject documentObjectE = new()
                                    {
                                        Id = ent.ObjectId,
                                        Tag = eVals[0],
                                        RelatedTo = true,
                                        FromOtherDWG = false,
                                        Equipment = true

                                    };
                                    documentInfos[IndexOfDocuments(documentInfos, docInstrumentsRT.Name)].UsedDocumentObjects.Add(documentObjectE);

                                    foreach (ObjectId intrumentId in InstrumentsRT)
                                    {
                                        int instrumentRowId = dlmInstrumentsRT.FindAcPpRowId(intrumentId);
                                        StringCollection iKeys = new StringCollection
                                        {
                                            "Tag",
                                            "RelatedToEquip"
                                        };
                                        StringCollection iVals = dlmInstrumentsRT.GetProperties(instrumentRowId, iKeys, true);

                                        DocumentObject documentObjectI = new()
                                        {
                                            Id = intrumentId,
                                            Tag = iVals[0],
                                            RelatedTo = true,
                                            FromOtherDWG = false,
                                            RelatedId = ent.ObjectId,
                                            Instrument = true

                                        };
                                        documentInfos[IndexOfDocuments(documentInfos, docInstrumentsRT.Name)].UsedDocumentObjects.Add(documentObjectI);
                                        if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {

                                                iVals[1] = eVals[0];
                                                dbInstrumentsRT.StartTransaction();
                                                dlmInstrumentsRT.SetProperties(intrumentId, iKeys, iVals);
                                                Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                                formRelateTo.ReplacePropertys(entEdited, InstrumentsRTOld);
                                                dbInstrumentsRT.CommitTransaction();
                                            }
                                        }
                                        else
                                        {

                                            iVals[1] = eVals[0];
                                            dbInstrumentsRT.StartTransaction();
                                            dlmInstrumentsRT.SetProperties(intrumentId, iKeys, iVals);
                                            Entity entEdited = (Entity)trEquipment.GetObject(intrumentId, OpenMode.ForWrite);
                                            formRelateTo.ReplacePropertys(entEdited, InstrumentsRTOld);
                                            dbInstrumentsRT.CommitTransaction();
                                        }
                                    }
                                    MessageBox.Show("Related To executado com sucesso!!");
                                    foreach (ListViewItem item in formRelateTo.listView.Items)
                                        formRelateTo.listView.Items.Remove(item);
                                    formRelateTo.listView.Items.Clear();
                                    InstrumentsRT.Clear();
                                    InstrumentsRTOld.Clear();
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
            }
        }
        public void buttonEquipment_Click(object sender, EventArgs e)
        {
            if (InstrumentsRT.Count > 0)
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
                                int equipmentRowId = dlmInstrumentsRT.FindAcPpRowId(equipment.ObjectId);
                                StringCollection eKeys = new StringCollection { "Tag" };
                                StringCollection eVals = dlmInstrumentsRT.GetProperties(equipmentRowId, eKeys, true);
                                DocumentObject documentObject = new()
                                {
                                    Id = ent.ObjectId,
                                    Tag = eVals[0],
                                    RelatedTo = true,
                                    FromOtherDWG = true,
                                    OtherDWGDocument = doc.Name
                                };
                                documentInfos[IndexOfDocuments(documentInfos, doc.Name)].UsedDocumentObjects.Add(documentObject);

                                foreach (ObjectId intrumentId in InstrumentsRT)
                                {
                                    int instrumentRowId = dlmInstrumentsRT.FindAcPpRowId(intrumentId);
                                    StringCollection iKeys = new StringCollection
                                    {
                                        "Tag",
                                        "RelatedToEquip"
                                    };
                                    StringCollection iVals = dlmInstrumentsRT.GetProperties(instrumentRowId, iKeys, true);
                                    if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                    {
                                        if (String.IsNullOrEmpty(iVals[1]))
                                        {
                                            iVals[1] = eVals[0];
                                            dbInstrumentsRT.StartTransaction();
                                            dlmInstrumentsRT.SetProperties(intrumentId, iKeys, iVals);
                                            Transaction trInstrumenst = docInstrumentsRT.TransactionManager.StartTransaction();
                                            Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                            formRelateTo.ReplacePropertys(entEdited, InstrumentsRTOld);
                                            trInstrumenst.Commit();
                                            dbInstrumentsRT.CommitTransaction();
                                        }
                                    }
                                    else
                                    {
                                        iVals[1] = eVals[0];
                                        dbInstrumentsRT.StartTransaction();
                                        dlmInstrumentsRT.SetProperties(intrumentId, iKeys, iVals);
                                        Transaction trInstrumenst = docInstrumentsRT.TransactionManager.StartTransaction();
                                        Entity entEdited = (Entity)trInstrumenst.GetObject(intrumentId, OpenMode.ForWrite);
                                        formRelateTo.ReplacePropertys(entEdited, InstrumentsRTOld);
                                        trInstrumenst.Commit();
                                        dbInstrumentsRT.CommitTransaction();
                                    }
                                }

                                MessageBox.Show("Related To executado com sucesso!!");
                                foreach (ListViewItem item in formRelateTo.listView.Items)
                                    formRelateTo.listView.Items.Remove(item);
                                formRelateTo.listView.Items.Clear();
                                InstrumentsRT.Clear();
                                InstrumentsRTOld.Clear();
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

        public void RelatedToTrue(DocumentObject documentObject, List<DocumentInfo> documentInfos)
        {
            foreach (DocumentInfo di in documentInfos)
            {
                if (documentObject.BelongingDocument == di.Document)
                {
                    foreach (DocumentObject docObj in di.DocumentObjects)
                    {
                        if (documentObject.Id == docObj.Id)
                        {
                            docObj.RelatedTo = true;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region UpdateLinetypeByStatus
        [CommandMethod("_ULTBS")]
        public void UpdateLinetypeByStatus()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            LoadLineTypes(doc.Database);
            PromptSelectionResult selection = ed.SelectAll();
            bool success = false;
            int countAlteredLines = 0;

            if (selection.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (ObjectId id in selection.Value.GetObjectIds())
                    {
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        LayerTableRecord layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                        if (!layer.IsFrozen & ent.Id.ObjectClass.DxfName == "SLINE")
                        {
                            StringCollection eKeys = new StringCollection { "Status" };
                            //existe um objeto onde eu n posso pegar o rowId para fazer um get 
                            //no database dos valores respectivos as chaves
                            try
                            {
                                StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(ent.ObjectId), eKeys, true);
                                if (!String.IsNullOrEmpty(eVals[0]))
                                {
                                    if (eVals[0] != "Future Flow Line" & eVals[0] != "Alternative / Intermittent Flow Line")
                                    {
                                        ObjectId lti = RetornaLinetypeId(eVals[0], tr, doc.Database);
                                        if (ent.LinetypeId != lti)
                                        {
                                            ent.UpgradeOpen();
                                            ent.LinetypeId = lti;
                                            ent.DowngradeOpen();
                                            countAlteredLines++;
                                        }
                                    }
                                }
                            }
                            catch (DLException e)
                            {
                                _ = e;// runtime dando erro por nao haver link com a entidade 
                            }
                        }
                    }
                    tr.Commit();
                    if (countAlteredLines > 0)
                        MessageBox.Show(countAlteredLines + " linhas alteradas com sucesso!!", "Update Linetype By Status", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    else
                        MessageBox.Show("Não houve nenhuma alteração!!", "Update Linetype By Status", MessageBoxButtons.OK, MessageBoxIcon.Question);
                } // using
            } // if

        }
        private ObjectId RetornaLinetypeId(string descricao, Transaction tr, Database db)
        {
            LinetypeTable tbl = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForWrite);
            List<Linetype> lines = new List<Linetype>();
            ObjectId linetype = new ObjectId();
            foreach (var linetypeId in tbl)
            {
                LinetypeTableRecord obj = (LinetypeTableRecord)tr.GetObject(linetypeId, OpenMode.ForRead);
                lines.Add(new Linetype { Id = obj.ObjectId, Name = obj.Name });
            }
            switch (descricao)
            {
                case "New":
                    foreach (Linetype ltr in lines)
                    {
                        if (ltr.Name == "Continuous")
                            linetype = ltr.Id;
                    }
                    return linetype;
                case "Future":
                    foreach (Linetype ltr in lines)
                    {
                        if (ltr.Name == "DASHDOT")
                            linetype = ltr.Id;
                    }
                    return linetype;
                case "Existing":
                    foreach (Linetype ltr in lines)
                    {
                        if (ltr.Name == "HIDDEN2")
                            linetype = ltr.Id;
                    }
                    return linetype;
                case "Alternative / Intermittent":
                    foreach (Linetype ltr in lines)
                    {
                        if (ltr.Name == "HIDDEN")
                            linetype = ltr.Id;
                    }
                    return linetype;
            }
            foreach (Linetype ltr in lines)
            {
                if (ltr.Name == "Continuous")
                    linetype = ltr.Id;
            }
            return linetype;
        }
        public static void LoadLineTypes(Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LinetypeTable tbl = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForWrite);
                foreach (string linetype in linetypesSubstitute)
                {
                    if (!tbl.Has(linetype))
                    {
                        db.LoadLineTypeFile(linetype, "acad.lin");
                    }
                }
                tr.Commit();
            }
        }
        #endregion

        #region FromTo
        [CommandMethod("_FRMT")]
        public void FromTo()
        {
            try
            {
                if (formFromTo == null || formFromTo.IsDisposed)
                    formFromTo = new FormFromTo();
                formFromTo.Show();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            { }
        }
        #endregion

        # region Trigger
        [CommandMethod("AddDocEvent")]
        public void AddDocEvent()
        {
            // Get the current document
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            if (acDoc.IsNamedDrawing == true & !acDoc.Name.Contains("projSymbolStyle"))
            {
                //só vai adicionar o trigger de fechar o documento
                //e buscar os objetos no dwg se for um documento salvo
                //e Name do arquivo não conter projSymbolStyle
                acDoc.BeginDocumentClose += new DocumentBeginCloseEventHandler(DocBeginDocClose);
                documentInfos.Add(ReturnDocumentInfo(acDoc));
            }
        }
        [CommandMethod("RemoveDocEvent")]
        public void RemoveDocEvent()
        {
            // Get the current document
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            acDoc.BeginDocumentClose -= new DocumentBeginCloseEventHandler(DocBeginDocClose);
        }
        public void DocBeginDocClose(object senderObj, DocumentBeginCloseEventArgs docBegClsEvtArgs)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
            DocumentInfo docCompare = new DocumentInfo();
            docCompare = ReturnDocumentInfo(acDoc);
            foreach (DocumentInfo doc in documentInfos)
            {
                if (doc.Document == docCompare.Document)
                {
                    //var excpt1 = doc.DocumentObjects.Except(docCompare.DocumentObjects).ToList();
                    //var excpt2 = docCompare.DocumentObjects.Except(doc.DocumentObjects).ToList();
                    //var test2 = doc.DocumentObjects.Distinct().ToList();
                    //var test3 = docCompare.DocumentObjects.Distinct().ToList();
                    //var a = doc.DocumentObjects.All(docCompare.DocumentObjects.Contains) && doc.DocumentObjects.Count == docCompare.DocumentObjects.Count;
                    bool contemTodos = doc.DocumentObjects.All(docCompare.DocumentObjects.Contains);
                    bool itensRemovidos = doc.DocumentObjects.Count != docCompare.DocumentObjects.Count;
                    List<DocumentObject> itensAlterados = ReturnTagChanges(doc.UsedDocumentObjects, acDoc);
                    if (!contemTodos | itensRemovidos)
                    {
                        foreach (DocumentObject obj in doc.UsedDocumentObjects.Where(w => w.Instrument == true))
                        {
                            DocumentObject itemAlterado = itensAlterados.FirstOrDefault(a => a.Id.Equals(obj.RelatedId));
                            if (itemAlterado != null)
                            {
                                using (DocumentLock doclock = acDoc.LockDocument())
                                {
                                    using (var tr = acDoc.TransactionManager.StartTransaction())
                                    {
                                        int rowId = dlm.FindAcPpRowId(obj.Id);
                                        if (obj.RelatedTo == true)
                                        {
                                            StringCollection oKeys = new StringCollection { "Tag", "RelatedToEquip" };
                                            StringCollection oVals = dlm.GetProperties(rowId, oKeys, true);
                                            oVals[1] = itemAlterado.Tag;
                                            db.StartTransaction();
                                            dlm.SetProperties(rowId, oKeys, oVals);
                                            db.CommitTransaction();
                                        }
                                        if (obj.FromTo == true)
                                        {
                                            StringCollection oKeys = new StringCollection { "Tag", "Pipe Run To", "Pipe Run From" };
                                            StringCollection oVals = dlm.GetProperties(rowId, oKeys, true);
                                            oVals[1] = itemAlterado.Tag;
                                            db.StartTransaction();
                                            dlm.SetProperties(rowId, oKeys, oVals);
                                            db.CommitTransaction();
                                        }
                                        tr.Commit();
                                    }
                                }
                            }
                        }
                    }
                    List<DocumentObject> itensAdd = docCompare.DocumentObjects.Except(doc.DocumentObjects).ToList();
                    List<DocumentObject> itensRemoved = doc.DocumentObjects.Except(docCompare.DocumentObjects).ToList();
                    string modfies =  string.Join("\n", itensRemoved.Select(s => s.Tag));
                    modfies += " => " + string.Join("\n", itensAdd.Select(s => s.Tag));
                    string message = $"Houve mudanças no documento!!\n{modfies}\nDeseja refatorar as referências?";
                    if (System.Windows.Forms.MessageBox.Show(message, "Trigger", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        acDoc.Database.SaveAs(acDoc.Name, true, DwgVersion.Current, acDoc.Database.SecurityParameters);

                    documentInfos.Remove(docCompare);
                }
            }
        }
        private List<DocumentObject> ReturnTagChanges(List<DocumentObject> usedDocumentObjects, Document acDoc)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;

            List<DocumentObject> tagChanges = new List<DocumentObject>();
            using (Transaction tr = acDoc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (DocumentObject dO in usedDocumentObjects)
                {
                    try
                    {
                        StringCollection tagKey = new StringCollection { "Tag" };
                        StringCollection tagVal = dlm.GetProperties(dlm.FindAcPpRowId(dO.Id), tagKey, true);

                        if (tagVal[0] != dO.Tag)
                        {
                            dO.Tag = tagVal[0];
                            tagChanges.Add(dO);
                        }

                    }
                    catch (DLException)
                    {
                        DocumentObject exception = new()
                        {
                            Id = dO.Id,
                            Tag = dO.Tag + "deletado"

                        };
                        tagChanges.Add(exception);
                    }
                }
                tr.Commit();
            }

            return tagChanges;
        }
        public static DocumentInfo ReturnDocumentInfo(Document acDoc)
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PromptSelectionResult selection = acDoc.Editor.SelectAll();

            DocumentInfo document = new DocumentInfo();
            document.Document = acDoc.Name;
            document.DocumentObjects = new List<DocumentObject>();
            document.UsedDocumentObjects = new List<DocumentObject>();

            using (Transaction tr = acDoc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (ObjectId id in selection.Value.GetObjectIds())
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    LayerTableRecord layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                    if (!layer.IsFrozen)
                    {
                        StringCollection entKeys = new StringCollection { "Tag" };
                        //existe um objeto onde eu n posso pegar o rowId para fazer um get 
                        //no database dos valores respectivos as chaves

                        try
                        {
                            if (HaveTag(dlm.GetAllProperties(id, true)))
                            {
                                DocumentObject docObj = new DocumentObject();
                                StringCollection entVal = dlm.GetProperties(dlm.FindAcPpRowId(ent.ObjectId), entKeys, true);
                                docObj.Id = ent.ObjectId;
                                docObj.Tag = entVal[0];
                                docObj.BelongingDocument = acDoc.Name;
                                document.DocumentObjects.Add(docObj);
                            }
                        }
                        catch (DLException e)
                        {
                            _ = e;// runtime dando erro por nao haver link com a entidade 
                        }


                    }
                }
                tr.Commit();
                return document;
            }
        }
        public static bool HaveTag(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("Tag"))
                    return true;
            }
            return false;
        }

        #endregion

        #region Testes
        //ads_queueexpr
        [DllImport("accore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ads_queueexpr")]
        extern static private int ads_queueexpr(byte[] command);
        [CommandMethod("tJL", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void TestConnectedLines()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            PromptEntityOptions pso = new PromptEntityOptions("\nPick a single line to join: ");
            PromptEntityResult res = ed.GetEntity(pso);
            if (res.Status != PromptStatus.OK) return;
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("PICKFIRST", 1);
                    Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("PEDITACCEPT", 0);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTableRecord;
                    List<ObjectId> ids = JoinLines(btr, res.ObjectId);
                    ObjectId[] lines = ids.ToArray();
                    ed.SetImpliedSelection(lines);
                    PromptSelectionResult chres = ed.SelectImplied();

                    if (chres.Status != PromptStatus.OK)
                    {
                        ed.WriteMessage("\nNothing added in the chain!");
                        return;
                    }
                    else
                    {
                        ed.WriteMessage(chres.Value.Count.ToString());
                    }
                    SelectionSet newset = SelectionSet.FromObjectIds(lines);
                    ed.SetImpliedSelection(newset);
                    string handles = "";
                    foreach (SelectedObject selobj in newset)
                    {
                        Entity subent = tr.GetObject(selobj.ObjectId, OpenMode.ForWrite) as Entity;
                        string hdl = string.Format("(handent \"{0}\")", subent.Handle.ToString());
                        handles = handles + hdl + " ";
                    }
                    System.Text.UnicodeEncoding uEncode = new System.Text.UnicodeEncoding();
                    // if PEDITACCEPT is set to 1 enshort the command avoiding "_Y" argument:
                    ads_queueexpr(uEncode.GetBytes("(COMMAND \"_.PEDIT\" \"_M\"" + handles + "\"\"" + "\"_Y\" \"_J\" \"\" \"\")"));
                    tr.Commit();
                }
            }
        }
        private void SelectConnectedLines(BlockTableRecord btr, List<ObjectId> ids, ObjectId id)
        {
            Entity en = id.GetObject(OpenMode.ForRead, false) as Entity;
            Line ln = en as Line;
            if (ln != null)
                foreach (ObjectId idx in btr)
                {
                    Entity ex = idx.GetObject(OpenMode.ForRead, false) as Entity;
                    Line lx = ex as Line;
                    if (((ln.StartPoint == lx.StartPoint) || (ln.StartPoint == lx.EndPoint)) ||
                        ((ln.EndPoint == lx.StartPoint) || (ln.EndPoint == lx.EndPoint)))
                        if (!ids.Contains(idx))
                        {
                            ids.Add(idx);
                            SelectConnectedLines(btr, ids, idx);
                        }
                }
        }
        public List<ObjectId> JoinLines(BlockTableRecord btr, ObjectId id)
        {
            List<ObjectId> ids = new List<ObjectId>();
            SelectConnectedLines(btr, ids, id);
            return ids;
        }
        [CommandMethod("TS")]
        public void Group()
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityResult line;
            doc.LockDocument();

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
                StringCollection iVals = dlm.GetProperties(dlm.FindAcPpRowId(line.ObjectId), iKeys, true);
                if (line.Status == PromptStatus.OK)
                {
                    using (DocumentLock doclock = doc.LockDocument())
                    {
                        using (var trLine = doc.TransactionManager.StartTransaction())
                        {
                            Entity entityIf = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);

                            if (entityIf.Id.ObjectClass.DxfName == "SLINE")
                            {
                                Entity ent = (Entity)trLine.GetObject(line.ObjectId, OpenMode.ForRead);
                                StringCollection keyTag = new StringCollection { "Tag", "Pipe Run To", "Pipe Run From", "Layer" };
                                StringCollection valTag = dlm.GetProperties(dlm.FindAcPpRowId(line.ObjectId), keyTag, true);
                                DocumentObject selectedLine = new()
                                {
                                    Layer = ent.Layer,
                                    LayerId = ent.LayerId,
                                    Tag = valTag[0],
                                    Id = ent.ObjectId,
                                    BelongingDocument = doc.Name
                                };
                                PromptSelectionResult selection = doc.Editor.SelectAll();

                                List<DocumentObject> pipeLineGroup = new List<DocumentObject>();

                                using (Transaction tr = doc.Database.TransactionManager.StartOpenCloseTransaction())
                                {
                                    foreach (ObjectId id in selection.Value.GetObjectIds())
                                    {
                                        Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);
                                        LayerTableRecord layer = (LayerTableRecord)tr.GetObject(entity.LayerId, OpenMode.ForRead);
                                        if (!layer.IsFrozen)
                                        {
                                            StringCollection entKeys = new StringCollection { "Tag" };
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
                                                        BelongingDocument = doc.Name
                                                    };
                                                    if (SamePipeLineGroup(TagPipeLineGroup(selectedLine.Tag), TagPipeLineGroup(docObj.Tag)))
                                                        pipeLineGroup.Add(docObj);
                                                }
                                            }
                                            catch (DLException e)
                                            {
                                                _ = e;// runtime dando erro por nao haver link com a entidade 
                                            }
                                        }
                                    }


                                    tr.Commit();
                                    trLine.Commit();
                                }
                                break;
                            }
                            else
                            {
                                MessageBox.Show(dlm.GetProperties(dlm.FindAcPpRowId(line.ObjectId), iKeys, true)[1] + " não é uma linha!!");
                            }
                            trLine.Commit();
                        }

                    }
                }
            }

        }
        public void ListarBloques()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database objs = doc.Database;
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;

            using (Transaction tr = objs.TransactionManager.StartTransaction())
            {
                BlockTable blckTbl;
                blckTbl = tr.GetObject(objs.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord blckTblRcrd;
                blckTblRcrd = tr.GetObject(blckTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                var blockBeginId = dlm.GetAllProperties(blckTblRcrd.BlockBeginId, true);
                var blockEndId = dlm.GetAllProperties(blckTblRcrd.BlockEndId, true);
                foreach (ObjectId id in blckTblRcrd)
                {

                    var dbObj = tr.GetObject(id, OpenMode.ForRead);
                    if (dbObj is BlockReference)
                    {
                        var blckRef = (BlockReference)dbObj;
                        doc.Editor.WriteMessage("" + blckRef.Name);
                    }
                }

                tr.Commit();
            }
        }

        #endregion

        public int IndexOfDocuments(List<DocumentInfo> documentInfos, string doc)
        {
            for (int i = 0; i < documentInfos.Count; i++)
            {
                if (documentInfos[i].Document == doc)
                    return i;
            }
            return -1;
        }
        public string[] TagPipeLineGroup(string tag)
        {
            return tag.Split('-');
        }
        public bool SamePipeLineGroup(string[] tag1, string[] tag2)
        {
            if (tag1.Length > 4 & tag2.Length > 4)
            {
                if ((tag1[1] != "?" & tag1[4] != "?") & (tag2[1] != "?" & tag2[4] != "?") & (tag1[1] + tag1[4] == tag2[1] + tag2[4]))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

    }
}
