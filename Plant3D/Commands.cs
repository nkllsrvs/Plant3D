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
                Title = "FF - VALE",
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
                Text = "RelatedTo",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\navigate_plus.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "RelatedTo_1",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._PBRT "
            };

            RibbonButton button2 = new RibbonButton
            {
                Text = "SelectAll",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\satellite32.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "SelectAll_2",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._SANF "
            };

            RibbonButton button3 = new RibbonButton
            {
                Text = "Dump",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\map_location64.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "Dump_3",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._DUMP "
            };

            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2, button3 };
            foreach(RibbonButton rb in ribbonButtons)
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
        //sempre que chamar pelo plant a exec pushButton ele executa o metodo descrito abaixo
        [CommandMethod("_PBRT")]
        public void RelatedTo()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            MessageBox.Show(ed.SelectAll().ToString());
            // pega o projeto atual
            PlantProject currentProject = PlantApplication.CurrentProject;

            // pega todas as partes do projeto atual
            ProjectPartCollection currentProjectPartsColl = currentProject.ProjectParts;
            foreach (Project project in currentProjectPartsColl)
            {
                if (project.ProjectFileItemType == "FE")
                {
                    MessageBox.Show("FE - FE - FE - FE - FE!! :)");
                }
                // pegar o nome das partes
                string projectPartName = currentProject.ProjectPartName(project);

                //escrever a informação
                ed.WriteMessage("\nProject {0} - Part {1} de {2}", project.ProjectName, projectPartName, project.ProjectDwgDirectory);

                // pegar todas os arquivos
                List<PnPProjectDrawing> dwgList = project.GetPnPDrawingFiles();
                foreach (PnPProjectDrawing dwg in dwgList)
                {
                    ed.WriteMessage("\n\t{0}", dwg.AbsoluteFileName);
                }
            }
        }

        [CommandMethod("_SANF")]
        public static void SelectAllExceptFrozenLayers()
        {
            //Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //Editor ed = doc.Editor;
            //PromptSelectionResult selection = ed.SelectAll();
            //if (selection.Status == PromptStatus.OK)
            //    using (Transaction tr =
            //                doc.Database.TransactionManager.StartTransaction())
            //    {
            //        foreach (ObjectId id in selection.Value.GetObjectIds())
            //        {
            //            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
            //            LayerTableRecord layer = (LayerTableRecord)tr.GetObject(
            //                                        ent.LayerId, OpenMode.ForRead);
            //            if (!layer.IsFrozen)
            //            if (MessageBox.Show("\n{BlockName: " + ent.BlockName +
            //                "\n{ClassID: " + ent.ClassID +
            //                "\n{Color: " + ent.Color +
            //                "\n{BlockName: " + ent.Database.Filename +
            //                "} \n\nDeseja continuar varrendo todas as entidades?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //            {
            //                // user clicked yes
            //            }
            //            else
            //            {
            //                // user clicked no
            //                MessageBox.Show("A varredura foi parada!! :(");
            //                break;
            //            }
            //        }
            //        tr.Commit();
            //    } // using
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            FormVALE formVALE = new FormVALE();
            formVALE.Show();
            bool loop = true;
            List<PromptEntityResult> resultList = new List<PromptEntityResult>();
            while (loop)
            {
                resultList.Add(ed.GetEntity("\nSelecione uma  entidade: "));

            }
            foreach (PromptEntityResult result in resultList)
            {
                if (result.Status == PromptStatus.OK)
                {
                    var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                    using (var tr = result.ObjectId.Database.TransactionManager.StartTransaction())
                    {
                        var dbObj = tr.GetObject(result.ObjectId, OpenMode.ForRead);
                        var types = new List<Type>();
                        types.Add(dbObj.GetType());
                        while (true)
                        {
                            var type = types[0].BaseType; //para o campo de layer só é necessário o Entity
                            types.Insert(0, type);
                            if (type == typeof(RXObject))
                                break;
                        }
                        foreach (Type t in types)
                        {
                            if (t.Name == "Entity")
                            {
                                //ed.WriteMessage($"\n\n - {t.Name} -");
                                foreach (var prop in t.GetProperties(flags))
                                {
                                    if (prop.Name == "Layer")
                                    {
                                        //ed.WriteMessage("\n{0,-40}: ", prop.Name);
                                        try
                                        {
                                            formVALE.textBox1_Test(($"\n\n{prop.Name} - {prop.GetValue(dbObj, null)}"));
                                            //ed.WriteMessage("{0}", prop.GetValue(dbObj, null));
                                        }
                                        catch (System.Exception e)
                                        {
                                            ed.WriteMessage(e.Message);
                                        }
                                    }

                                }
                            }
                        }
                        tr.Commit();
                    }
                }

            }
            Autodesk.AutoCAD.ApplicationServices.Application.DisplayTextScreen = true;
        } // if
        [CommandMethod("_DUMP")]
        public void Dump()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var result = ed.GetEntity("\nSelecione uma  entidade: ");
            if (result.Status == PromptStatus.OK)
                PrintDump(result.ObjectId, ed);
            Autodesk.AutoCAD.ApplicationServices.Application.DisplayTextScreen = true;

        }

        private void PrintDump(ObjectId id, Editor ed)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var dbObj = tr.GetObject(id, OpenMode.ForRead);                
                var types = new List<Type>();
                types.Add(dbObj.GetType());
                while (true)
                {
                    var type = types[0].BaseType; //para o campo de layer só é necessário o Entity
                    types.Insert(0, type);
                    if (type == typeof(Entity))
                        break;
                }


                foreach (Type t in types)
                {
                    if(t.Name == "Entity")
                    {
                        ed.WriteMessage($"\n\n - {t.Name} -");
                        foreach (var prop in t.GetProperties(flags))
                        {
                            if (prop.Name == "Layer")
                            {
                                ed.WriteMessage("\n{0,-40}: ", prop.Name);
                                try
                                {
                                    ed.WriteMessage("{0}", prop.GetValue(dbObj, null));
                                }
                                catch (System.Exception e)
                                {
                                    ed.WriteMessage(e.Message);
                                }
                            }
                            
                        }

                    }
                }
                tr.Commit();
            }
        }
    }
}
