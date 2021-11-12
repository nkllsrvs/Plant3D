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

[assembly: CommandClass(typeof(Plant3D.Commands))]
[assembly: CommandClass(typeof(Plant3D.VALERibbon))]
namespace Plant3D
{
    public class VALERibbon
    {

        [CommandMethod("vale", CommandFlags.Transparent)]
        public void TestRibbonTab()
        {
            RibbonControl ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl != null)
            {
                RibbonTab ribbonTab = ribbonControl.FindTab("VALE");
                if (ribbonTab != null)
                {
                    ribbonControl.Tabs.Remove(ribbonTab);
                }
                ribbonTab = new RibbonTab
                {
                    Title = "VALE",
                    Id = "VALE"
                };
                RibbonPanel panel = ribbonControl.FindPanel("VALE", true);
                if (panel != null)
                {
                    ribbonControl.Tabs.Remove(ribbonTab);
                }
                ribbonTab = new RibbonTab
                {
                    Title = "VALE",
                    Id = "VALE"
                };
                //Add the Tab
                ribbonControl.Tabs.Add(ribbonTab);
                ribbonControl.ActiveTab = ribbonTab;
                ribbonTab.Panels.Add(AddOnePanel());
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
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Plant3DValeAddin\img\relatedto.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "1",
                CommandHandler = new VALERibbonButtonCommandeHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._RLTT "
            };
            RibbonButton button2 = new RibbonButton
            {
                Text = "Update \nLinetype by Status",
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Plant3DValeAddin\img\substitute.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "2",
                CommandHandler = new VALERibbonButtonCommandeHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._ULTBS "
            };
            RibbonButton button3 = new RibbonButton
            {
                Text = "Update \nLinetype by Status",
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Plant3DValeAddin\img\relatedto.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "2",
                CommandHandler = new VALERibbonButtonCommandeHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._RLTT "
            };

            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2 };
            foreach (RibbonButton rb in ribbonButtons)
            {
                panelSource.Items.Add(rb);
            }
            return panel;
        }
        public class VALERibbonButtonCommandeHandler : System.Windows.Input.ICommand
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
        FormVALE relatedToVALE = new FormVALE();
        FormVALE fromToVALE = new FormVALE();
        static StringCollection linetypesSubstitute = new StringCollection
        {
            "Continuous",
            "DASHDOT",
            "HIDDEN",
            "HIDDEN2"
        };

        [CommandMethod("_RLTT")]
        public void RelatedTo()
        {
            try
            {
                if (relatedToVALE == null || relatedToVALE.IsDisposed)
                    relatedToVALE = new FormVALE();
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
                                        if(ent.LinetypeId != lti)
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
                    if(countAlteredLines > 0)
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
        static public void LoadLineTypes(Database db)
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
        public class Linetype : IEnumerable
        {
            public Linetype() { }
            public Linetype(ObjectId Id, String Name)
            {
                this.Id = Id;
                this.Name = Name;
            }
            public Autodesk.AutoCAD.DatabaseServices.ObjectId Id { get; set; }
            public String Name { get; set; }

            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [CommandMethod("_FRMT")]
        public void FromTo()
        {
            try
            {
                if (relatedToVALE == null || relatedToVALE.IsDisposed)
                    relatedToVALE = new FormVALE();
                relatedToVALE.Show();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            { }
        }
    }
}

