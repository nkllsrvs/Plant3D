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
                Title = "VALE",
                DialogLauncher = commandItem
            };
            RibbonPanel panel = new RibbonPanel
            {
                Source = panelSource
            };
            //assign the Command Item to the DialgLauncher which auto-enables
            // the little button at the lower right of a Panel

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
            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button3 };
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
        [CommandMethod("_DUMP")]
        public void Dump()
        {
            FormVALE formVALE = new FormVALE();
            formVALE.Show();
        }

    }
}
