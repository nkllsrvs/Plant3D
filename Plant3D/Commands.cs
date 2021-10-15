using System;
using System.Collections.Generic;
using System.Windows;

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
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using System.Reflection;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using System.Collections.Specialized;
using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Diagnostics;

[assembly: CommandClass(typeof(Plant3D.Commands))]
[assembly: CommandClass(typeof(Plant3D.FFRibbon))]
namespace Plant3D
{
    public class FFRibbon
    {
        [CommandMethod("vale", CommandFlags.Transparent)]
        public void TestRibbonTab()
        {
            RibbonControl control = ComponentManager.Ribbon;
            if (control != null)
            {
                RibbonTab tab = control.FindTab("VALE");
                if (tab != null)
                {
                    control.Tabs.Remove(tab);
                }
                tab = new RibbonTab
                {
                    Title = "VALE",
                    Id = "VALE Ribbon",
                    IsContextualTab = false
                };
                //Add the Tab
                control.Tabs.Add(tab);
                control.ActiveTab = tab;
                tab.Panels.Add(AddOnePanel());
            }
        }

        static RibbonPanel AddOnePanel()
        {
            //Create a Command Item that the Dialog Launcher can use,
            // for this test it is just a place holder.
            RibbonButton commandItem = new RibbonButton
            {
                Name = "VALE"
            };
            RibbonPanelSource panelSource = new RibbonPanelSource
            {
                Title = "VALE",
                DialogLauncher = commandItem
            };
            RibbonPanel panel = new RibbonPanel
            {
                Source = panelSource
            };
            //assign the Command Item to the DialgLauncher which auto-enables
            // the little button at the lower right of a Panel

            RibbonButton button1 = new RibbonButton
            {
                Text = "Related To",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\navigate_plus.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "1",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._RLTT "
            };
            RibbonButton button2 = new RibbonButton
            {
                Text = "Substitute",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\satellite32.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "2",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._SBTTT "
            };
            RibbonButton button3 = new RibbonButton
            {
                Text = "TEST1",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\navigate_plus.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "3",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._TEST1 "
            };
            RibbonButton button4 = new RibbonButton
            {
                Text = "ReloadLineType",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\navigate_plus.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "4",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._RLT "
            };

            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2, button3, button4 };
            foreach (RibbonButton rb in ribbonButtons)
            {
                panelSource.Items.Add(rb);
            }
            return panel;
        }
        public class FFRibbonButtonCommandHandler : System.Windows.Input.ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true; //return true means the button always enabled
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                RibbonCommandItem cmd = (RibbonCommandItem)parameter;
                Document dwg = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                dwg.SendStringToExecute(cmd.CommandParameter.ToString(), true, false, true);
            }
        }
    }
    public class Commands
    {
        [CommandMethod("_RLTT")]
        public void RelatedTo()
        {
            FormVALE formVALE = new FormVALE();
            formVALE.Show();
        }
        [CommandMethod("_SBTTT")]
        public void Substitute()
        {


        }

        //[CommandMethod("TEST1")]
        //public static void Test1()
        //{
        //    var doc = Application.DocumentManager.MdiActiveDocument;
        //    var db = doc.Database;
        //    var ed = doc.Editor;
        //    var allObjects = GetAllObjects(db);
        //    foreach (var item in allObjects)
        //    {
        //        ed.WriteMessage($"\n{item.Key.Handle,-6} {item.Value}");
        //    }
        //}

        [CommandMethod("TEST2")]
        public static void Test2()
        {
            //var doc = Application.DocumentManager.MdiActiveDocument;
            //var db = doc.Database;
            //var ed = doc.Editor;
            ////foreach (var item in GetAllEntities(db))
            ////{
            ////    ed.WriteMessage($"\n{item.Key.Handle,-6} {item.Value}");
            ////}
            //foreach (var item in GetAllLines(db))
            //{
            //    ed.WriteMessage($"\n{item.Id,-6} {item.Linetype} {item.PlotStyleName} {item.VisualStyleId}  ");
            //}
            string s = "";
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                PlantProject proj = PlantApplication.CurrentProject;
                ProjectPartCollection projParts = proj.ProjectParts;
                PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
                DataLinksManager dlm = pnidProj.DataLinksManager;
                PromptEntityResult entityRes = ed.GetEntity("pick the object: ");
                // // // added kwb
                if (entityRes.Status != PromptStatus.OK)
                    return;
                Entity en1 = (Entity)trans.GetObject(entityRes.ObjectId, OpenMode.ForWrite);
                string layerName = en1.Layer;
                StringCollection eKeys = new StringCollection
                {
                    "Description",
                    "Tag",
                    "From",
                    "To",
                    "ClassName",
                    "Linetype",
                    "Color"
                };
                StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(entityRes.ObjectId), eKeys, true);

                s += "\n PromptEntityResult PROPERTIES: ";
                s += "\n objectID : " + en1.ObjectId;
                s += "\n GetType : " + en1.GetType().ToString();
                s += "\n PickedPoint.(X, Y, Z) : " +
                entityRes.PickedPoint.X + " , " +
                entityRes.PickedPoint.Y + " , " + // // revised added + kwb
                entityRes.PickedPoint.Z;

                try
                {
                    Line l1 = (Line)(Object)en1;
                    s += "\n Line PROPERTIES: ";
                    s += "\n StartPoint: " + l1.StartPoint;
                    s += "\n EndPoint: " + l1.EndPoint;
                    s += "\n Status : " + entityRes.Status;
                }
                catch (System.Exception Ex)
                {
                    s += "\n\n Error in Line Convertion : " +
                    Ex.Message.ToString();
                }
                finally
                {
                }
                MessageBox.Show(s);

                trans.Commit();
                // // //
            }
        }

        [CommandMethod("SLT", CommandFlags.UsePickSet)]
        public void SetLineType()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var ed = doc.Editor;
            // Get the pickfirst selection set or ask the user to
            // select some entities
            var psr = ed.GetSelection();
            if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                return;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Get the IDs of the selected objects
                var ids = psr.Value.GetObjectIds();
                // Loop through in read-only mode, checking whether the
                // selected entities have the same linetype
                // (if so, it'll be set in ltId, otherwise different will
                // be true)
                var ltId = ObjectId.Null;
                bool different = false;
                foreach (ObjectId id in ids)
                {
                    // Get the entity for read
                    var ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    // On the first iteration we store the linetype Id
                    if (ltId == ObjectId.Null)
                        ltId = ent.LinetypeId;
                    else
                    {
                        // On subsequent iterations we check against the
                        // first one and set different to be true if they're
                        // not the same
                        if (ltId != ent.LinetypeId)
                        {
                            different = true;
                            break;
                        }
                    }
                }
                // Now we can display our linetype dialog with the common
                // linetype selected (if they have the same one)
                var ltd = new LinetypeDialog();
                if (!different)
                    ltd.Linetype = ltId;
                var dr = ltd.ShowDialog();
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return; // We might also commit before returning
                // Assuming we have a different linetype selected
                // (or the entities in the selected have different
                // linetypes to start with) then we'll loop through
                // to set the new linetype
                if (different || ltId != ltd.Linetype)
                {
                    foreach (ObjectId id in ids)
                    {
                        // This time we need write access
                        var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                        // Set the linetype if it's not the same
                        if (ent.LinetypeId != ltd.Linetype)
                            ent.LinetypeId = ltd.Linetype;
                    }
                }
                // Finally we commit the transaction
                tr.Commit();
            }
        }

        [CommandMethod("RLT", CommandFlags.Session)]
        public void relaodLinetype()
        {
            DocumentCollection docManager = Application.DocumentManager;

            Database db = docManager.MdiActiveDocument.Database;
            Transaction trans = db.TransactionManager.StartTransaction();
            bool bReload = false;
            using (trans)
            {
                LinetypeTable table = trans.GetObject(db.LinetypeTableId,OpenMode.ForRead) as LinetypeTable;
                if (table.Has("CENTER"))
                    bReload = true;
            }
            System.Int16 fileDia = (System.Int16)Application.GetSystemVariable("FILEDIA");
            Application.SetSystemVariable("FILEDIA", 0);

            //reload using linetype command...
            Object acadObject = Application.AcadApplication;

            object ActiveDocument = acadObject.GetType().InvokeMember("ActiveDocument",System.Reflection.BindingFlags.GetProperty,null,acadObject,null);
            object[] dataArry = new object[1];
            if (bReload)
                dataArry[0] = "-linetype Load CENTER\nacad.lin\nYes\n ";
            else
                dataArry[0] = "-linetype Load CENTER\nacad.lin\n ";
            ActiveDocument.GetType().InvokeMember("SendCommand",System.Reflection.BindingFlags.InvokeMethod,null,ActiveDocument,dataArry);
            Application.SetSystemVariable("FILEDIA", fileDia);
        }


        //static Dictionary<ObjectId, string> GetAllObjects(Database db)
        //{
        //    var dict = new Dictionary<ObjectId, string>();
        //    for (long i = 0; i < db.Handseed.Value; i++)
        //    {
        //        if (db.TryGetObjectId(new Handle(i), out ObjectId id))
        //            dict.Add(id, id.ObjectClass.Name);
        //    }
        //    return dict;
        //}

        //static Dictionary<ObjectId, string> GetAllEntities(Database db)
        //{
        //    var dict = new Dictionary<ObjectId, string>();
        //    using (var tr = db.TransactionManager.StartOpenCloseTransaction())
        //    {
        //        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        //        foreach (var btrId in bt)
        //        {
        //            var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
        //            if (btr.IsLayout)
        //            {
        //                foreach (var id in btr)
        //                {
        //                    dict.Add(id, id.ObjectClass.Name);
        //                }
        //            }
        //        }
        //        tr.Commit();
        //    }
        //    return dict;

        //}

        //public static IEnumerable<Line> GetAllLines(Database db)
        //{
        //    using (var docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
        //    {
        //        using (var tr = db.TransactionManager.StartTransaction())
        //        {
        //            var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //            var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
        //            foreach (var obj in btr)
        //            {
        //                var line = tr.GetObject(obj, OpenMode.ForRead) as Line;
        //                if (line != null)
        //                    yield return line;
        //            }
        //        }
        //    }
        //}

        //public IList<ObjectId> GetIdsByType()
        //{
        //    Func<Type, RXClass> getClass = RXObject.GetClass;

        //    // You can set this anywhere
        //    var acceptableTypes = new HashSet<RXClass>
        //    {
        //        getClass(typeof(Line)),
        //    };

        //    var doc = Application.DocumentManager.MdiActiveDocument;
        //    using (var trans = doc.TransactionManager.StartOpenCloseTransaction())
        //    {
        //        var modelspace = (BlockTableRecord)
        //        trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), OpenMode.ForRead);

        //        var lineIds = (from id in modelspace.Cast<ObjectId>()
        //                           where acceptableTypes.Contains(id.ObjectClass)
        //                           select id).ToList();

        //        trans.Commit();
        //        return lineIds;
        //    }
        //}

        void MeasureTime(Document doc, Func<int> func)
        {
            // Get the name of the running command(s)
            // (might also have queried the CommandMethod attribute
            // via reflection, but that would be a lot more work)
            var cmd = (string)Application.GetSystemVariable("CMDNAMES");
            // Start a Stopwatch to time the execution
            var sw = new Stopwatch();
            sw.Start();
            // Run the function, getting back the count of the results
            var cnt = func();
            // Stop the Stopwatch and print the results to the command-line
            sw.Stop();
            doc.Editor.WriteMessage("\n{0}: found {1} lines in {2}.", cmd, cnt, sw.Elapsed);
        }

        [CommandMethod("LL1")]
        public void ListLines1()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            MeasureTime(doc,() =>
              {
                  var ids = GetAllLines(doc.Database);
                  return ids.Count<Line>();
              }
            );
        }

        [CommandMethod("LL2")]
        public void ListLines2()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            MeasureTime(doc,() =>
              {
                  var rXClass = RXObject.GetClass(typeof(Line));
                  var ids = ObjectsOfType1(doc.Database, rXClass);
                  return ids.Count<ObjectId>();
              }
            );
        }

        [CommandMethod("LL3")]
        public void ListLines3()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            MeasureTime(doc,() =>
              {
                  var ids = ObjectsOfType2(doc.Database, RXObject.GetClass(typeof(Line)));
                  return ids.Count<ObjectId>();
              }
            );
        }

        public static IEnumerable<Line> GetAllLines(Database db)
        {
            using (var docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    foreach (var obj in btr)
                    {
                        var line = tr.GetObject(obj, OpenMode.ForRead) as Line;
                        if (line != null)
                        {
                            yield return line;
                        }
                    }
                }
            }
        }

        public static IEnumerable<ObjectId> ObjectsOfType1(Database db, RXClass cls)
        {
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                foreach (ObjectId id in btr)
                {
                    if (id.ObjectClass.IsDerivedFrom(cls))
                    {
                        yield return id;
                    }
                }
                tr.Commit();
            }
        }
        public static IEnumerable<ObjectId> ObjectsOfType2(Database db, RXClass cls)
        {
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var lineIds = from ObjectId id in btr where id.ObjectClass.IsDerivedFrom(cls) select id;
                tr.Commit();
                return lineIds;
            }
        }
    }
}
