﻿using System;
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
using Plant3D.Forms;
using System.IO;

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
        private const string equipamentoNaoEncontrado = "EQUIPAMENTO NÃO ENCONTRADO";

        public static FormRelatedTo formRelateTo = new FormRelatedTo();
        public static FormFromTo formFromTo = new FormFromTo();
        public static FormInconsistence formInconsistence;
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
                                    StringCollection keyTag = new StringCollection { "Tag", "RelatedToEquip", "Layer", "RowIdRelatedTo", "DWGNameRelatedTo" };
                                    StringCollection valTag = dlmInstrumentsRT.GetProperties(dlmInstrumentsRT.FindAcPpRowId(instrument.ObjectId), keyTag, true);

                                    Entity ent = (Entity)trInstruments.GetObject(instrument.ObjectId, OpenMode.ForRead);
                                    InstrumentsRT.Add(instrument.ObjectId);
                                    Instruments iOld = new Instruments();
                                    iOld.Id = ent.ObjectId;
                                    iOld.Layer = ent.Layer;
                                    iOld.LayerId = ent.LayerId;
                                    iOld.Tag = valTag[0];
                                    iOld.UsedRelatedTo = true;
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
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais elementos com RelatedToEquip já preenchidos, deseja substituir o valor atual do atributo?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                                        UsedRelatedTo = true,
                                        OtherDWG = false,
                                        Equipment = true

                                    };
                                    try
                                    {
                                        documentInfos[IndexOfDocuments(documentInfos, docInstrumentsRT.Name)].UsedDocumentObjects.Add(documentObjectE);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        ex.ToString();
                                    }

                                    foreach (ObjectId intrumentId in InstrumentsRT)
                                    {
                                        int instrumentRowId = dlmInstrumentsRT.FindAcPpRowId(intrumentId);

                                        StringCollection iKeys = new StringCollection { "Tag", "RelatedToEquip", "OtherDWG", "OtherDWGName", "UsedFromTo", "UsedRelatedTo",
                                            "RowIdRelatedTo", "DWGNameRelatedTo" };
                                        StringCollection iVals = dlmInstrumentsRT.GetProperties(instrumentRowId, iKeys, true);

                                        DocumentObject documentObjectI = new()
                                        {
                                            Id = intrumentId,
                                            Tag = iVals[0],
                                            UsedRelatedTo = true,
                                            OtherDWG = false,
                                            RelatedId = ent.ObjectId,
                                            Instrument = true

                                        };
                                        try
                                        {
                                            documentInfos[IndexOfDocuments(documentInfos, docInstrumentsRT.Name)].UsedDocumentObjects.Add(documentObjectI);
                                        }
                                        catch (System.Exception ex)
                                        {
                                            ex.ToString();
                                        }
                                        if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                        {
                                            if (String.IsNullOrEmpty(iVals[1]))
                                            {
                                                iVals[1] = eVals[0];
                                                iVals[2] = "false";
                                                iVals[3] = docInstrumentsRT.Name;
                                                iVals[5] = "true";
                                                iVals[6] = equipmentRowId.ToString();
                                                iVals[7] = docInstrumentsRT.Name;
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
                                            iVals[2] = "false";
                                            iVals[3] = docInstrumentsRT.Name;
                                            iVals[5] = "true";
                                            iVals[6] = equipmentRowId.ToString();
                                            iVals[7] = docInstrumentsRT.Name;
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
                        messageReplaceRelatedToEquip = MessageBox.Show("Existe um ou mais elementos com RelatedToEquip já preenchidos, deseja substituir o valor atual do atributo?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                                    UsedRelatedTo = true,
                                    OtherDWG = true,
                                    OtherDWGName = doc.Name,
                                    RowIdRelated = equipmentRowId,
                                };
                                try
                                {
                                    documentInfos[IndexOfDocuments(documentInfos, doc.Name)].UsedDocumentObjects.Add(documentObject);
                                }
                                catch (System.Exception ex)
                                {
                                    ex.ToString();
                                }

                                foreach (ObjectId intrumentId in InstrumentsRT)
                                {
                                    int instrumentRowId = dlmInstrumentsRT.FindAcPpRowId(intrumentId);
                                    StringCollection iKeys = new StringCollection { "Tag", "RelatedToEquip", "OtherDWG", "OtherDWGName", "UsedFromTo", "UsedRelatedTo",
                                        "RowIdRelatedTo", "DWGNameRelatedTo",};
                                    StringCollection iVals = dlmInstrumentsRT.GetProperties(instrumentRowId, iKeys, true);
                                    if (countRTE > 0 & messageReplaceRelatedToEquip == DialogResult.No)
                                    {
                                        if (String.IsNullOrEmpty(iVals[1]))
                                        {
                                            iVals[1] = eVals[0];
                                            iVals[2] = "true";
                                            iVals[3] = doc.Name;
                                            iVals[5] = "true";
                                            iVals[6] = equipmentRowId.ToString();
                                            iVals[7] = doc.Name;

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
                                        iVals[2] = "true";
                                        iVals[3] = doc.Name;
                                        iVals[5] = "true";
                                        iVals[6] = equipmentRowId.ToString();
                                        iVals[7] = doc.Name;

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
                            docObj.UsedRelatedTo = true;
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
            string caminhoDocumentoAtual = acDoc.Name;
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
            PromptSelectionResult prompt = acDoc.Editor.SelectAll();
            StringCollection objectKeys = new StringCollection
            {
                "Tag",
                "OtherDWG",
                "OtherDWGName",
                "UsedFromTo",
                "UsedRelatedTo",
                "PipeRunFrom",
                "PipeRunTo",
                "RelatedToEquip",

                "RowIdRelatedTo",//8
                "DWGNameRelatedTo",//9

                "RowIdFromToOrigin", //10
                "DWGNameFromToOrigin",//11

                "RowIdFromToDestiny",//12
                "DWGNameFromToDestiny",//13
            };
            acDoc.LockDocument();
            List<Inconsistence> Erros = new List<Inconsistence>();
            List<Element> elements = new List<Element>();
            using (Transaction tr = acDoc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (ObjectId obj in prompt.Value.GetObjectIds())
                {
                    Entity ent = (Entity)tr.GetObject(obj, OpenMode.ForRead);
                    LayerTableRecord layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                    try
                    {
                        if (!layer.IsFrozen)
                        {
                            StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(ent.ObjectId), objectKeys, true);
                            if (!String.IsNullOrEmpty(objectValues[0]))
                            {
                                //if (!String.IsNullOrEmpty(objectValues[3]) || !String.IsNullOrEmpty(objectValues[4]))
                                //{
                                // countAlteredLines++;
                                Element element = new Element();
                                element.id = ent.ObjectId;
                                element.TAG = objectValues[0];
                                element.RowIdRelatedTo = !String.IsNullOrEmpty(objectValues[8]) ? Convert.ToInt32(objectValues[8]) : 0;
                                element.RowIdFromToOrigin = !String.IsNullOrEmpty(objectValues[10]) ? Convert.ToInt32(objectValues[10]) : 0;
                                element.RowIdFromToDestiny = !String.IsNullOrEmpty(objectValues[12]) ? Convert.ToInt32(objectValues[12]) : 0;
                                var teste = ent.GetType().FullName;
                                try
                                {
                                    element.ClassName = ((Autodesk.ProcessPower.PnIDObjects.Asset)ent).ClassName;
                                }
                                catch
                                {

                                }

                                switch (ent.Id.ObjectClass.DxfName)
                                {
                                    case "ACPPASSET":
                                        if (HaveRelatedToEquip(dlm.GetAllProperties(ent.ObjectId, true)))
                                        {
                                            element.Type = "Instrumento";
                                            element.RelatedTo = objectValues[7]/* == "EQUIPAMENTO NÃO ENCONTRADO" ? "" : objectValues[7]*/;
                                            element.OtherDWGRT = objectValues[9] != caminhoDocumentoAtual;
                                            element.DWGNameRelatedTo = objectValues[9];
                                            element.HaveRT = Validate(element.RowIdRelated, element.DWGNameRelatedTo, element.RelatedTo);
                                        }
                                        else
                                        {
                                            element.Type = "Equipamento";
                                        }
                                        elements.Add(element);

                                        break;
                                    case "SLINE":

                                        element.Type = "Linha";

                                        element.PipeRunFrom = objectValues[5] /*== "EQUIPAMENTO NÃO ENCONTRADO" ? "" : objectValues[5]*/;
                                        element.OtherDWGFTFrom = objectValues[11] != caminhoDocumentoAtual;
                                        element.DWGNameFromToOrigin = objectValues[11];
                                        element.HaveFTFrom = Validate(element.RowIdRelated, element.DWGNameFromToOrigin, element.PipeRunFrom);

                                        element.PipeRunTo = objectValues[6] /*== "EQUIPAMENTO NÃO ENCONTRADO" ? "" : objectValues[6]*/;
                                        element.OtherDWGFTTo = objectValues[13] != caminhoDocumentoAtual;
                                        element.DWGNameFromToDestiny = objectValues[13];
                                        element.HaveFTTo = Validate(element.RowIdRelated, element.DWGNameFromToDestiny, element.PipeRunTo);

                                        elements.Add(element);

                                        break;
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

            using (Transaction tr = acDoc.Database.TransactionManager.StartTransaction())
            {

                foreach (Element element in elements.Where(w => w.Type == "Linha" &
                                                            (!String.IsNullOrEmpty(w.PipeRunFrom) |
                                                             !String.IsNullOrEmpty(w.PipeRunTo))))
                {
                    //Linha editada com o PipeRunFrom em branco
                    if (String.IsNullOrEmpty(element.PipeRunFrom))
                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run From do elemento {element.TAG} está vazio!"));
                    //Linha editada com o PipeRunTo em branco
                    if (String.IsNullOrEmpty(element.PipeRunTo))
                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run To do elemento {element.TAG} está vazio"));

                    //Linha com PipeRunFrom relacionado a um equipamento inexistente
                    if (!String.IsNullOrEmpty(element.PipeRunFrom) && element.PipeRunFrom != equipamentoNaoEncontrado && !element.OtherDWGFTFrom && !elements.Any(w => w.TAG == element.PipeRunFrom))
                    {
                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[5] = "ELEMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(element.id, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run From do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                    }

                    //Linha com PipeRunTo relacionado a um equipamento inexistente
                    if (!String.IsNullOrEmpty(element.PipeRunTo) && element.PipeRunTo != equipamentoNaoEncontrado && !element.OtherDWGFTTo && !elements.Any(w => w.TAG == element.PipeRunTo))
                    {
                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[6] = "ELEMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(rowId, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run To do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                    }

                    //Linha com PipeRunTo relacionado a um equipamento de outro documento, porém inexistente                                
                    if (!String.IsNullOrEmpty(element.PipeRunTo) && element.PipeRunTo != equipamentoNaoEncontrado && element.OtherDWGFTTo && !element.HaveFTTo)
                    {
                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[6] = "ELEMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(rowId, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run To do elemento {element.TAG} não foi encontrado no documento {element.DWGNameFromToDestiny}"));
                    }
                    
                    if (!String.IsNullOrEmpty(element.PipeRunFrom) && element.PipeRunFrom != equipamentoNaoEncontrado && element.OtherDWGFTFrom && !element.HaveFTFrom)
                    {
                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[6] = "ELEMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(rowId, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run From do elemento {element.TAG} não foi encontrado no documento {element.DWGNameFromToOrigin}"));
                    }
                }

                foreach (Element element in elements.Where(w => w.Type == "Instrumento" & !String.IsNullOrEmpty(w.RelatedTo)))
                {
                    //Instrumento vinculado a um equipamento que não foi encontrado  
                    if (!element.OtherDWGRT && element.RelatedTo != equipamentoNaoEncontrado && !elements.Any(w => w.Type == "Equipamento" & w.TAG == element.RelatedTo))
                    {
                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[7] = "ELEMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(rowId, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Related To Equip do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                    }

                    //Instrumento vinculado a um equipamento de outro documento que não foi encontrado          
                    if (element.OtherDWGRT && element.RelatedTo != equipamentoNaoEncontrado && !element.HaveRT)
                    {

                        int rowId = dlm.FindAcPpRowId(element.id);
                        StringCollection objectValues = dlm.GetProperties(dlm.FindAcPpRowId(element.id), objectKeys, true);
                        objectValues[7] = "EQUIPAMENTO NÃO ENCONTRADO";
                        db.StartTransaction();
                        dlm.SetProperties(rowId, objectKeys, objectValues);
                        db.CommitTransaction();

                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Related To Equip do elemento {element.TAG} não foi encontrado no documento {element.DWGNameRelatedTo}"));

                    }
                }

                foreach (Element element in elements.Where(w => w.PipeRunFrom == equipamentoNaoEncontrado | w.PipeRunTo == equipamentoNaoEncontrado | w.RelatedTo == equipamentoNaoEncontrado))
                {
                    if (element.PipeRunFrom == equipamentoNaoEncontrado)
                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run From do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                    if (element.PipeRunTo == equipamentoNaoEncontrado)
                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Pipe Run To do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                    if (element.RelatedTo == equipamentoNaoEncontrado)
                        Erros.Add(new Inconsistence(element.TAG, element.Type, $"O atributo Related To do elemento {element.TAG} está com o valor \"ELEMENTO NÃO ENCONTRADO\"!"));
                }
                tr.Commit();
            }
            string strDWGName = acDoc.Name;
            acDoc.Database.SaveAs(strDWGName, true, DwgVersion.Current, acDoc.Database.SecurityParameters);

            if (Erros.Any())
            {
                try
                {
                    if (formInconsistence == null || formInconsistence.IsDisposed)
                        formInconsistence = new FormInconsistence();
                    formInconsistence.Equipments = elements.Where(w => w.Type == "Equipamento").ToList();
                    formInconsistence.InconsistenceList = Erros;

                    formInconsistence.Show();
                    formInconsistence.Focus();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                { }
            }
        }

        public bool Validate(int rowId, string dwgPath, string tag)
        {
            PlantProject mainPrj = PlantApplication.CurrentProject;
            Project prj = mainPrj.ProjectParts["PnId"];
            DataLinksManager dlm = prj.DataLinksManager;

            PpObjectIdArray ids = dlm.FindAcPpObjectIds(rowId);
            foreach (PpObjectId ppid in ids)
            {
                ObjectId oid = dlm.MakeAcDbObjectId(ppid);
                try
                {
                    try
                    {
                        var allProperties = dlm.GetAllProperties(rowId, false);
                        if (allProperties != null && allProperties.Any(a => a.Value == tag))
                        {
                            return true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        return false;
                    }

                }
                catch (System.Exception ex)
                {

                }
            }
            return false;
        }

        public bool HaveRelatedToEquip(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("RelatedToEquip"))
                    return true;
            }
            return false;
        }

        public void DocBeginDocClose2(object senderObj, DocumentBeginCloseEventArgs docBegClsEvtArgs)
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
                        foreach (DocumentObject obj in doc.DocumentObjects.Where(w => w.Instrument == true))
                        {
                            DocumentObject itemAlterado = itensAlterados.FirstOrDefault(a => a.Id.Equals(obj.RelatedId));
                            if (itemAlterado != null)
                            {
                                List<DocumentObject> itensAdd = docCompare.DocumentObjects.Except(doc.DocumentObjects).ToList();
                                List<DocumentObject> itensRemoved = doc.DocumentObjects.Except(docCompare.DocumentObjects).ToList();
                                string modfies = string.Join("\n", itensRemoved.Select(s => s.Tag));
                                modfies += " => " + string.Join("\n", itensAdd.Select(s => s.Tag));
                                string message = $"Houve mudanças no documento!!\n{modfies}\nDeseja refatorar as referências?";
                                if (System.Windows.Forms.MessageBox.Show(message, "Trigger", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                    acDoc.Database.SaveAs(acDoc.Name, true, DwgVersion.Current, acDoc.Database.SecurityParameters);
                                using (DocumentLock doclock = acDoc.LockDocument())
                                {
                                    using (var tr = acDoc.TransactionManager.StartTransaction())
                                    {
                                        int rowId = dlm.FindAcPpRowId(obj.Id);
                                        if (obj.UsedRelatedTo == true)
                                        {
                                            StringCollection oKeys = new StringCollection { "Tag", "RelatedToEquip" };
                                            StringCollection oVals = dlm.GetProperties(rowId, oKeys, true);
                                            oVals[1] = itemAlterado.Tag;
                                            db.StartTransaction();
                                            dlm.SetProperties(rowId, oKeys, oVals);
                                            db.CommitTransaction();
                                        }
                                        if (obj.UsedFromTo == true)
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
        //[CommandMethod("tt")]
        //public void test()
        //{
        //    //Document doc = Application.DocumentManager.MdiActiveDocument;
        //    //PlantProject proj = PlantApplication.CurrentProject;
        //    //ProjectPartCollection projParts = proj.ProjectParts;
        //    //PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
        //    //DataLinksManager dlm = pnidProj.DataLinksManager;
        //    //PnPDatabase db = dlm.GetPnPDatabase();
        //    //PromptSelectionResult selection = doc.Editor.SelectAll();
        //    //List<DocumentObject> objects = new List<DocumentObject>();
        //    //int countAlteredLines = 0;

        //    // Prepare to the work: Let's get some entity's ObjectId

        //    Database db = Application.DocumentManager.MdiActiveDocument.Database;
        //    DataLinksManager dlm = DataLinksManager.GetManager(db);
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    ObjectId entId = ed.GetEntity("Pick a P&ID item: ").ObjectId;
        //    StringCollection objectKeys = new StringCollection { "Tag", "OtherDWG", "OtherDWGName", "UsedFromTo", "UsedRelatedTo", "PipeRunFrom", "PipeRunTo", "RelatedToEquip", "RowId" };
        //    StringCollection objectValuesOD = dlm.GetProperties(entId, objectKeys, true);

        //    // Now get the PnPID (i.e. PpObjectId)
        //    //   from the selected entity's ObjectId
        //    PpObjectId pnpId = dlm.MakeAcPpObjectId(entId);
        //    // Now let's do an opposite action - find ObjectId(s) of the entity
        //    int rowId1 = dlm.FindAcPpRowId(entId); // You can use ObjectId
        //    int rowId2 = dlm.FindAcPpRowId(pnpId); //          or PpObjectId

        //    // rowId1 and rowId2 are always equal
        //    PpObjectIdArray ids = dlm.FindAcPpObjectIds(rowId1);

        //    // NOTE: It returns a COLLECTION of AcPpObjectId!
        //    //     I.e., multiple AcDbObjectIds may be linked to a single RowID

        //    // Now find the ObjectID for each PpObjectId
        //    foreach (PpObjectId ppid in ids)
        //    {
        //        ObjectId oid = dlm.MakeAcDbObjectId(ppid);
        //        try
        //        {
        //            string dwgFlpath = objectValuesOD[2];
        //            using (Database dbt = new Database(false, true))
        //            {
        //                dbt.ReadDwgFile("C:\\Users\\nikol\\Documents\\FF-VALE\\MDR_Autodesk_v0_WIP_25_11_2021 11_06_26\\PID DWG\\API02.dwg", FileOpenMode.OpenForReadAndAllShare, false, null);

        //                DataLinksManager dlm2 = DataLinksManager.GetManager(dbt);

        //                try
        //                {
        //                    var testRowIdDeOutroDWG = dlm2.GetAllProperties(13, false);
        //                }
        //                catch (System.Exception ex)
        //                {

        //                }
        //                try
        //                {
        //                    var testRowIdDWGAtual = dlm2.GetAllProperties(rowId1, true);
        //                }
        //                catch (System.Exception ex)
        //                {

        //                }

        //                dbt.SaveAs(dwgFlpath, DwgVersion.Current);

        //            }
        //            Application.ShowAlertDialog("All files processed");
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
        //        }
        //        try
        //        {
        //            string dwgFlpath = objectValuesOD[2];
        //            using (Database dbt = new Database(false, true))
        //            {
        //                dbt.ReadDwgFile("C:\\Users\\nikol\\Documents\\FF-VALE\\MDR_Autodesk_v0_WIP_25_11_2021 11_06_26\\PID DWG\\API01.dwg", FileOpenMode.OpenForReadAndAllShare, false, null);

        //                DataLinksManager dlm2 = DataLinksManager.GetManager(dbt);

        //                try
        //                {
        //                    var testRowIdDeOutroDWG = dlm2.GetAllProperties(13, false);
        //                }
        //                catch (System.Exception ex)
        //                {

        //                }
        //                try
        //                {
        //                    var testRowIdDWGAtual = dlm2.GetAllProperties(rowId1, true);
        //                }
        //                catch (System.Exception ex)
        //                {

        //                }

        //                dbt.SaveAs(dwgFlpath, DwgVersion.Current);

        //            }
        //            Application.ShowAlertDialog("All files processed");
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
        //        }
        //    }


        //}
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
