//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

////Autocad namespaces
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.EditorInput;

////Plant3D nasmespaces
//using Autodesk.ProcessPower.ProjectManager;
//using Autodesk.ProcessPower.PlantInstance;
//using Autodesk.Windows;
//using System.Windows.Media.Imaging;
//using System.Windows.Controls;
//using System.Windows.Input;

//[assembly:  CommandClass(typeof(Plant3D.TestRibbon))]
//[assembly:  CommandClass(typeof(Plant3D.Commands))]
//namespace Plant3D
//{
//    public class TestRibbon
//    {
//        public void Testme()
//        {
//            RibbonControl ribbon = ComponentManager.Ribbon;
//            if (ribbon != null)
//            {
//                RibbonTab rtab = ribbon.FindTab("VALE");
//                if (rtab != null)
//                {
//                    ribbon.Tabs.Remove(rtab);
//                }
//                rtab = new RibbonTab
//                {
//                    Title = "VALE",
//                    Id = "VALE Ribbon"
//                };
//                //Add the Tab

//                ribbon.Tabs.Add(rtab);
//                addContent(rtab);
//            }
//        }

//        static void addContent(RibbonTab rtab)
//        {
//            rtab.Panels.Add(AddOnePanel());
//        }

//        static RibbonPanel AddOnePanel()
//        {
//            RibbonButton rb;
//            RibbonPanelSource rps = new RibbonPanelSource();
//            rps.Title = "Teste VALE";
//            RibbonPanel rp = new RibbonPanel();
//            rp.Source = rps;

//            //Create a Command Item that the Dialog Launcher can use,
//            // for this test it is just a place holder.
//            RibbonButton rci = new RibbonButton
//            {
//                Name = "TestCommand"
//            };

//            //assign the Command Item to the DialgLauncher which auto-enables
//            // the little button at the lower right of a Panel
//            rps.DialogLauncher = rci;

//            rb = new RibbonButton();
//            Uri uriImage = new Uri(@"C:\PlantImg\img\navigate_plus.png");
//            BitmapImage largeImage = new BitmapImage(uriImage);
//            rb.LargeImage = largeImage;
//            rb.Name = "Test Button";
//            rb.ShowText = true;
//            rb.Text = "Test Button";
//            rb.ShowImage = true;
//            rb.Size = RibbonItemSize.Large;
//            //Add the Button to the Tab

//            rb.CommandHandler = new MyRibbonButtonCommandHandler();

//            rb.CommandParameter = "._getProjectParts "; //actual AutoCAD command passed to ICommand.Execute().

//            //Add the new button to a ribbon panel in a ribbon tab here

//            rps.Items.Add(rb);
//            return rp;
//        }

//        public class MyRibbonButtonCommandHandler : System.Windows.Input.ICommand
//        {
//            public bool CanExecute(object parameter)
//            {
//                return true; //return true means the button always enabled
//            }
//            public event EventHandler CanExecuteChanged;
//            public void Execute(object parameter)

//            {
//                RibbonCommandItem cmd = parameter as RibbonCommandItem;

//                Document dwg = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

//                dwg.SendStringToExecute(cmd.CommandParameter.ToString(), true, false, true);
//            }
//        }
//    }

//    public class Commandsj
//    {
//        //sempre que chamar pelo plant a exec getProjectParts ele executa o metodo descrito abaixo
//        [CommandMethod("getProjectParts")]
//        public void CmdGetProjectParts()
//        {
//            MessageBox.Show("TESTE!!!");


//            //Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

//            //// pega o projeto atual
//            //PlantProject currentProject = PlantApplication.CurrentProject;

//            //// pega todas as partes do projeto atual
//            //ProjectPartCollection currentProjectPartsColl = currentProject.ProjectParts;
//            //foreach (Project project in currentProjectPartsColl)
//            //{
//            //    // pegar o nome das partes
//            //    string projectPartName = currentProject.ProjectPartName(project);

//            //    //escrever a informação
//            //    ed.WriteMessage("\nProject {0} - Part {1} de {2}", project.ProjectName, projectPartName, project.ProjectDwgDirectory);

//            //    // pegar todas os arquivos
//            //    List<PnPProjectDrawing> dwgList = project.GetPnPDrawingFiles();
//            //    foreach (PnPProjectDrawing dwg in dwgList)
//            //    {
//            //        ed.WriteMessage("\n\t{0}", dwg.AbsoluteFileName);
//            //    }
//            //}
//        }
//    }
//}