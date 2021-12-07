using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Plant3D.Commands;

namespace Plant3D.Classes
{
    public class VALERibbon : Commands
    {
        [CommandMethod("vale", CommandFlags.Transparent)]
        public void TestRibbonTab()
        {
            this.AddDocEvent();
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
                Text = "FromTo",
                LargeImage = new BitmapImage(new Uri(@"C:\Program Files\Autodesk\AutoCAD 2022\Plant3DValeAddin\img\relatedto.png")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Id = "3",
                CommandHandler = new VALERibbonButtonCommandeHandler(),
                //actual AutoCAD command passed to ICommand.Execute().
                CommandParameter = "._FRMT "
            };

            List<RibbonButton> ribbonButtons = new List<RibbonButton> { button1, button2, button3 };
            foreach (RibbonButton rb in ribbonButtons)
            {
                panelSource.Items.Add(rb);
            }

            return panel;
        }
    }
}
