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
            RibbonButton button4 = new RibbonButton
            {
                Text = "CHPS",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\map_location64.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "CHPS",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "_CHPS "
            };
            RibbonButton button5 = new RibbonButton
            {
                Text = "CHPM",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\map_location64.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "CHPM",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "_CHPM "
            };
            RibbonButton button6 = new RibbonButton
            {
                Text = "CHPP",
                LargeImage = new BitmapImage(new Uri(@"C:\PlantImg\img\map_location64.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "CHPP",
                CommandHandler = new FFRibbonButtonCommandHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "_CHPP "
            };
            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2, button3, button4, button5, button6 };
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
        //public class PropertyChangerCmds
        //{
        [CommandMethod("_CHPS", CommandFlags.Modal | CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public void ChangePropertyOnSelectedEntities()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            try
            {
                PromptSelectionResult psr = ed.GetSelection();
                if (psr.Status == PromptStatus.OK)
                {
                    System.Type objType;
                    string propName;
                    object newPropValue;
                    bool recurse;
                    if (SelectClassPropertyAndValue(out objType, out propName, out newPropValue, out recurse))
                    {
                        int count = ChangePropertyOfEntitiesOfType(psr.Value.GetObjectIds(), objType, propName, newPropValue, recurse);
                        // Update the display, and print the count
                        ed.Regen();
                        ed.WriteMessage("\nChanged " + count + " object" + (count == 1 ? "" : "s") + " of type " + objType.Name + " to have a " + propName + " of " + newPropValue + ".");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("Exception: " + ex);
            }
        }

        [CommandMethod("_CHPM")]
        public void ChangePropertyOnModelSpaceContents()
        {
            ChangePropertyOnSpaceContents(BlockTableRecord.ModelSpace);
        }

        [CommandMethod("_CHPP")]
        public void ChangePropertyOnPaperSpaceContents()
        {
            ChangePropertyOnSpaceContents(BlockTableRecord.PaperSpace);
        }

        private void ChangePropertyOnSpaceContents(string spaceName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            try
            {
                System.Type objType;
                string propName;
                object newPropValue;
                bool recurse;

                if (SelectClassPropertyAndValue(out objType, out propName, out newPropValue, out recurse))
                {
                    ObjectId spaceId;
                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                        spaceId = bt[spaceName];
                        // Not needed, but quicker than aborting
                        tr.Commit();
                    }
                    // Call our recursive function to set the new
                    // value in our nested objects
                    int count = ChangePropertyOfEntitiesOfType(spaceId, objType, propName, newPropValue, recurse);
                    // Update the display, and print the count
                    ed.Regen();
                    ed.WriteMessage("\nChanged " + count + " object" + (count == 1 ? "" : "s") + " of type " + objType.Name + " to have a " + propName + " of " + newPropValue + ".");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("Exception: " + ex);
            }
        }

        private bool SelectClassPropertyAndValue(out System.Type objType, out string propName, out object newPropValue, out bool recurse)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            objType = null;
            propName = "";
            newPropValue = null;
            recurse = true;
            // Let's first get the class to query for
            PromptResult ps = ed.GetString("\nEnter type of objects to look for: ");
            if (ps.Status == PromptStatus.OK)
            {
                string typeName = ps.StringResult;
                // Use reflection to get the type from the string
                objType = System.Type.GetType(typeName, false, true);
                // If we didn't find it, try prefixing with
                // "Autodesk.AutoCAD.DatabaseServices."
                if (objType == null)
                {
                    objType = System.Type.GetType("Autodesk.AutoCAD.DatabaseServices." + typeName + ", acdbmgd", false, true);
                }
                if (objType == null)
                {
                    ed.WriteMessage("\nType " + typeName + " not found.");
                }
                else
                {
                    // If we have a valid type then let's
                    // first list its writable properties
                    ListProperties(objType);
                    // Prompt for a property
                    ps = ed.GetString("\nEnter property to modify: ");
                    if (ps.Status == PromptStatus.OK)
                    {
                        propName = ps.StringResult;
                        // Make sure the property exists...
                        System.Reflection.PropertyInfo propInfo = objType.GetProperty(propName);
                        if (propInfo == null)
                        {
                            ed.WriteMessage("\nProperty " + propName + " for type " + typeName + " not found.");
                        }
                        else
                        {
                            if (!propInfo.CanWrite)
                            {
                                ed.WriteMessage("\nProperty " + propName + " of type " + typeName + " is not writable.");
                            }
                            else
                            {
                                // If the property is writable...
                                // ask for the new value
                                System.Type propType = propInfo.PropertyType;
                                string prompt = "\nEnter new value of " + propName + " property for all objects of type " + typeName + ": ";
                                // Only certain property types are currently
                                // supported: Int32, Double, String, Boolean
                                switch (propType.ToString())
                                {
                                    case "System.Int32":
                                        PromptIntegerResult pir = ed.GetInteger(prompt);
                                        if (pir.Status == PromptStatus.OK)
                                            newPropValue = pir.Value;
                                        break;
                                    case "System.Double":
                                        PromptDoubleResult pdr = ed.GetDouble(prompt);
                                        if (pdr.Status == PromptStatus.OK)
                                            newPropValue = pdr.Value;
                                        break;
                                    case "System.String":
                                        PromptResult psr = ed.GetString(prompt);
                                        if (psr.Status == PromptStatus.OK)
                                            newPropValue = psr.StringResult;
                                        break;
                                    case "System.Boolean":
                                        PromptKeywordOptions pko = new PromptKeywordOptions(prompt);
                                        pko.Keywords.Add("True");
                                        pko.Keywords.Add("False");
                                        PromptResult pkr = ed.GetKeywords(pko);
                                        if (pkr.Status == PromptStatus.OK)
                                        {
                                            if (pkr.StringResult == "True")
                                                newPropValue = true;
                                            else
                                                newPropValue = false;
                                        }
                                        break;
                                    default:
                                        ed.WriteMessage("\nProperties of type " + propType.ToString() + " are not currently supported.");
                                        break;
                                }
                                if (newPropValue != null)
                                {
                                    PromptKeywordOptions pko = new PromptKeywordOptions("\nChange properties in nested blocks: ");

                                    pko.AllowNone = true;
                                    pko.Keywords.Add("Yes");
                                    pko.Keywords.Add("No");
                                    pko.Keywords.Default = "Yes";
                                    PromptResult pkr = ed.GetKeywords(pko);
                                    if (pkr.Status == PromptStatus.None | pkr.Status == PromptStatus.OK)
                                    {
                                        if (pkr.Status == PromptStatus.None | pkr.StringResult == "Yes")
                                            recurse = true;
                                        else
                                            recurse = false;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }


        private void ListProperties(System.Type objType)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ed.WriteMessage("\nWritable properties for " + objType.Name + ": ");
            PropertyInfo[] propInfos = objType.GetProperties();
            foreach (PropertyInfo propInfo in propInfos)
            {
                if (propInfo.CanWrite)
                {
                    ed.WriteMessage("\n  " + propInfo.Name + " : " + propInfo.PropertyType);
                }
            }
            ed.WriteMessage("\n");
        }

        // Version of the function that takes a container ID

        private int ChangePropertyOfEntitiesOfType(ObjectId btrId, System.Type objType, string propName, object newValue, bool recurse)
        {

            // We simply open the container, extract the IDs

            // and pass them to another version of the function...

            // If efficiency is an issue, then this could be

            // streamlined (i.e. duplicated, rather than factored)


            ObjectIdCollection btrContents = new ObjectIdCollection();

            Document doc = Application.DocumentManager.MdiActiveDocument;

            Transaction tr = doc.TransactionManager.StartTransaction();

            using (tr)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                foreach (ObjectId entId in btr)
                {
                    btrContents.Add(entId);
                }
                tr.Commit();
            }
            ObjectId[] ids = new ObjectId[btrContents.Count];
            btrContents.CopyTo(ids, 0);

            // Call the other version of this function

            return ChangePropertyOfEntitiesOfType(ids, objType, propName, newValue, recurse);
        }

        // Version of the function that takes a list of ents

        private int ChangePropertyOfEntitiesOfType(ObjectId[] objIds, System.Type objType, string propName, object newValue, bool recurse)
        {
            int changedCount = 0;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Transaction tr = doc.TransactionManager.StartTransaction();
            using (tr)
            {
                foreach (ObjectId entId in objIds)
                {
                    Entity ent = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                    // Change each entity, one by one

                    if (ent != null)
                    {
                        changedCount += ChangeSingleProperty(ent, objType, propName, newValue);
                    }

                    // If we are to recurse and it's a blockref...

                    if (recurse)
                    {
                        BlockReference br = ent as BlockReference;
                        if (br != null)
                        {
                            // ...then recurse
                            changedCount += ChangePropertyOfEntitiesOfType(br.BlockTableRecord, objType, propName, newValue, recurse);
                        }
                    }
                }
                tr.Commit();
            }
            return changedCount;
        }

        // Function to change an individual entity's property

        private int ChangeSingleProperty(Entity ent, System.Type objType, string propName, object newValue)
        {
            int changedCount = 0;

            // Are we dealing with an entity we care about?

            if (objType.IsInstanceOfType(ent))
            {
                // Check the existing value
                object res = objType.InvokeMember(propName, BindingFlags.GetProperty, null, ent, new object[0]);
                // If it is not the same then change it
                if (!res.Equals(newValue))
                {
                    // Entity is only open for read
                    ent.UpgradeOpen();
                    object[] args = new object[1];
                    args[0] = newValue;
                    res = objType.InvokeMember(propName, BindingFlags.SetProperty, null, ent, args);
                    changedCount++;
                    ent.DowngradeOpen();
                }
            }
            return changedCount;
        }
        //}



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
            List<PromptEntityResult> resultList = new List<PromptEntityResult>();
            resultList.Add(ed.GetEntity("\nSelecione uma  entidade: "));

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
            FormVALE formVALE = new FormVALE();
            //PlantProject proj = PlantApplication.CurrentProject;
            //ProjectPartCollection projParts = proj.ProjectParts;
            //PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            //DataLinksManager dlm = pnidProj.DataLinksManager;
            //PnPDatabase db = dlm.GetPnPDatabase();
            //Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //Editor ed = doc.Editor;
            formVALE.Show();

            //List<PromptEntityResult> Instruments = new List<PromptEntityResult>();
            //PromptEntityResult result;

            //bool loop = true;
            //while (loop)
            //{
            //    result = ed.GetEntity("\nSelecione um  Instrumento: ");

            //    if (result.Status == PromptStatus.OK)
            //        Instruments.Add(result);
            //    DialogResult dr = MessageBox.Show("Deseja continuar a selecionar Instrumentos?", "RelatedTo", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //    if (dr == DialogResult.No)
            //        break;
            //}

            //PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento: ");
            //if (equipment.Status == PromptStatus.OK)
            //{
            //    int equipmentRowId = dlm.FindAcPpRowId(equipment.ObjectId);
            //    StringCollection eKeys = new StringCollection
            //    {
            //        "Description",
            //        "Tag",
            //        "RelatedTo"
            //    };
            //    StringCollection eVals = dlm.GetProperties(equipmentRowId, eKeys, true);
            //    foreach (PromptEntityResult entityResult in Instruments)
            //    {
            //        int instrumentRowId = dlm.FindAcPpRowId(entityResult.ObjectId);
            //        StringCollection iKeys = new StringCollection
            //        {
            //            "Description",
            //            "Tag",
            //            "RelatedTo"
            //        };
            //        StringCollection iVals = dlm.GetProperties(instrumentRowId, iKeys, true);

            //        iVals[2] = eVals[1];

            //        db.StartTransaction();
            //        dlm.SetProperties(entityResult.ObjectId, iKeys, iVals);
            //        db.CommitTransaction();
            //    }
            //}
        }

        private void PrintDump(PromptEntityResult id, Editor ed)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            //Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Database db = id.ObjectId.Database;
            DataLinksManager dlm = DataLinksManager.GetManager(db);
            PnPRowIdArray aPid = dlm.SelectAcPpRowIds(db);
            var tr = id.ObjectId.Database.TransactionManager.StartTransaction();

            // get line segment
            var lineSegment = tr.GetObject(id.ObjectId, OpenMode.ForRead);
            if (lineSegment == null) ;
            // get line segment row id
            var lineSegmentRowId = dlm.FindAcPpRowId(id.ObjectId);


            foreach (int rid in aPid)
            {
                if (rid == lineSegmentRowId)
                {
                    StringCollection sKeys = new StringCollection();
                    sKeys.Add("Description");
                    sKeys.Add("Tag");
                    sKeys.Add("RelatedTo");
                    StringCollection sVals = dlm.GetProperties(rid, sKeys, true);
                    MessageBox.Show("\nDescription - " + sVals[0] + "\nTag - " + sVals[1] + "\nRelatedTo - " + sVals[2] + "");
                }
            }

            using (tr)
            {
                var dbObj = tr.GetObject(id.ObjectId, OpenMode.ForWrite);
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
                    if (t.Name == "Entity")
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
