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

[assembly: CommandClass(typeof(Plant3D.Commands))]
[assembly: CommandClass(typeof(Plant3D.Classes.VALERibbon))]
[assembly: CommandClass(typeof(Plant3D.Classes.DocumentObject))]
[assembly: CommandClass(typeof(Plant3D.Classes.Instruments))]
[assembly: CommandClass(typeof(Plant3D.Classes.Linetype))]
[assembly: CommandClass(typeof(Plant3D.Classes.VALERibbonButtonCommandeHandler))]
namespace Plant3D
{
    public class Commands
    {
        FormRelatedTo relatedToVALE = new FormRelatedTo();
        FromTo fromToVALE = new FromTo();
        static readonly StringCollection linetypesSubstitute = new StringCollection { "Continuous", "DASHDOT", "HIDDEN", "HIDDEN2" };
        static List<DocumentInfo> documentInfos = new List<DocumentInfo>();

        [CommandMethod("_RLTT")]
        public void RelatedTo()
        {
            try
            {
                if (relatedToVALE == null || relatedToVALE.IsDisposed)
                    relatedToVALE = new FormRelatedTo();
                relatedToVALE.Show();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            { }
        }

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

        [CommandMethod("_FRMT")]
        public void FromTo()
        {
            try
            {
                if (fromToVALE == null || fromToVALE.IsDisposed)
                    fromToVALE = new FromTo();
                fromToVALE.Show();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            { }
        }

        [CommandMethod("AddDocEvent")]
        static public void AddDocEvent()
        {
            // Get the current document
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            if (acDoc.IsNamedDrawing == true)
            {
                acDoc.BeginDocumentClose += new DocumentBeginCloseEventHandler(docBeginDocClose);
                documentInfos.Add(ReturnDocumentInfo(acDoc));
            }
        }

        [CommandMethod("RemoveDocEvent")]
        public static void RemoveDocEvent()
        {
            // Get the current document
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            acDoc.BeginDocumentClose -= new DocumentBeginCloseEventHandler(docBeginDocClose);
        }

        public static void docBeginDocClose(object senderObj, DocumentBeginCloseEventArgs docBegClsEvtArgs)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            DocumentInfo docCompare = new DocumentInfo();
            docCompare = ReturnDocumentInfo(acDoc);
            foreach (DocumentInfo doc in documentInfos)
            {
                if (doc.Document == docCompare.Document)
                {
                    var test = doc.DocumentObjects.Except(docCompare.DocumentObjects).ToList();
                    if (doc.DocumentObjects == docCompare.DocumentObjects) MessageBox.Show("São iguais!!!");
                }
            }

            // Display a message box prompting to continue closing the document
            if (System.Windows.Forms.MessageBox.Show(
                                 "Houve mudanças no documento!!" +
                                 "\nDeseja executar rotina?",
                                 "Trigger",
                                 System.Windows.Forms.MessageBoxButtons.YesNo) ==
                                 System.Windows.Forms.DialogResult.No)
            {
                docBegClsEvtArgs.Veto();
            }
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

            using (Transaction tr = acDoc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                DocumentObject docObj = new DocumentObject();
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
                document.DocumentObjects.Sort();
                return document;
            }
        }


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
            pso.SetRejectMessage("\nObject must be of type Line!");
            pso.AddAllowedClass(typeof(Line), false);
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

        public static bool HaveTag(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("Tag"))
                    return true;
            }
            return false;
        }

    }
}

