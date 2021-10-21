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
using Orientation = System.Windows.Controls.Orientation;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.ProcessPower.DataLinks;
using System.Collections.Specialized;
using Autodesk.AutoCAD.Windows;

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
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Support\en-us\img\navigate_plus.png")),
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
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Support\en-us\img\satellite32.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "2",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._SBTTT "
            };

            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2 };
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
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PromptSelectionResult selection = ed.SelectAll();
            if (selection.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (ObjectId id in selection.Value.GetObjectIds())
                    {
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        LayerTableRecord layer = (LayerTableRecord)tr.GetObject(ent.LayerId, OpenMode.ForRead);
                        if (!layer.IsFrozen & ent.ToString() == "Autodesk.AutoCAD.DatabaseServices.ImpCurve")
                        {
                            StringCollection eKeys = new StringCollection { "Status" };
                            StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(ent.ObjectId), eKeys, true);
                            if (eVals[0] != null)
                            {
                                ent.UpgradeOpen();
                                var ltd = new LinetypeDialog();
                                Handle handle = RetornaLinetypeHandle(eVals[0]);
                                if(handle.Value != 0)
                                {
                                    ltd.Linetype = doc.Database.GetObjectId(false, handle, 0);
                                    if (ent.LinetypeId != ltd.Linetype)
                                    ent.LinetypeId = ltd.Linetype;
                                }
                                ent.DowngradeOpen();
                            }
                            //Object LinetypeId = tr.GetObject(RetornaLinetype(eVals[0]).);
                            //ent.LinetypeId = RetornaLinetype(eVals[0]);
                            //ed.WriteMessage("\nTipo:{0} + + Status{1}", ent.ToString(), eVals[0]);

                        }
                    }
                    tr.Commit();
                } // using
            } // if

        }

        [CommandMethod("SLT", CommandFlags.UsePickSet)]
        public void SetLineType()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var ed = doc.Editor;
            var psr = ed.GetSelection();
            if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                return;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                var ids = psr.Value.GetObjectIds();
                var ltId = ObjectId.Null;
                bool different = false;
                foreach (ObjectId id in ids)
                {
                    var ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    if (ltId == ObjectId.Null)
                        ltId = ent.LinetypeId;
                    else
                    {
                        if (ltId != ent.LinetypeId)
                        {
                            different = true;
                            break;
                        }
                    }
                }
                var ltd = new LinetypeDialog();
                if (!different)
                    ltd.Linetype = ltId;
                var dr = ltd.ShowDialog();
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return; // We might also commit before returning

                if (different || ltId != ltd.Linetype)
                {
                    foreach (ObjectId id in ids)
                    {
                        // This time we need write access
                        var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                        if (ent.LinetypeId != ltd.Linetype)
                            ent.LinetypeId = ltd.Linetype;
                    }
                }
                tr.Commit();
            }
        }
        private Handle RetornaLinetypeHandle(string descricao)
        {
            switch (descricao)
            {
                //CONTINUOUS
                case "Novo":
                    return new Handle(22);
                //DASHDOT
                case "Futuro":
                    return new Handle(13122);
                //HIDDEN2
                case "Existente":
                    return new Handle(167);
                //HIDDEN
                case "Alternativo/Intermitente":
                    return new Handle(4934);

                //HIDDEN2
                case "Existing":
                    return new Handle(167);
                //PNEUMATIC
                case "Demolition":
                    return new Handle(4554);
                //CONTINUOUS
                case "New":
                    return new Handle(22);

                //Defalt
                default:
                    return new Handle(0);
            }
        }
    }
}
