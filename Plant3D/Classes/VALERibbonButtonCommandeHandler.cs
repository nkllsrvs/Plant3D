﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant3D.Classes
{
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
