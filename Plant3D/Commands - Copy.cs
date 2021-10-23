//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.ProcessPower.PlantInstance;
//using Autodesk.ProcessPower.ProjectManager;
//using System.Windows.Forms;
//using System;

//private void buttonInstruments_Click(object sender, EventArgs e)
//{
//    PlantProject proj = PlantApplication.CurrentProject;
//    ProjectPartCollection projParts = proj.ProjectParts;
//    PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
//    dlmInstruments = pnidProj.DataLinksManager;
//    dbInstruments = dlmInstruments.GetPnPDatabase();
//    docInstruments = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
//    Editor ed = docInstruments.Editor;
//    PromptEntityResult instrument;
//    while (true)
//    {
//        instrument = ed.GetEntity("\nSelecione um  Instrumento: ");
//        StringCollection eKeys = new StringCollection
//                {
//                    "Description",
//                    "Tag",
//                    "RelatedTo",
//                    "ClassName"
//                };
//        StringCollection eVals = dlmInstruments.GetProperties(dlmInstruments.FindAcPpRowId(instrument.ObjectId), eKeys, true);
//        if (instrument.Status == PromptStatus.OK)
//        {
//            using (var tr = docInstruments.TransactionManager.StartTransaction())
//            {
//                //Entity ent = (Entity)tr.GetObject(instrument.ObjectId, OpenMode.ForRead);
//                if (HaveRelatedToEquip(dlmInstruments.GetAllProperties(instrument.ObjectId, true)))
//                {
//                    if (Instruments.Contains(instrument.ObjectId))
//                        MessageBox.Show("O instrumento já foi selecionado!!");
//                    else
//                    {
//                        Instruments.Add(instrument.ObjectId);
//                        Invoke((MethodInvoker)delegate
//                        {
//                            StringCollection keyTag = new StringCollection { "Tag", "RelatedTo" };








//[CommandMethod("testmyRibbon", CommandFlags.Transparent)]
//public void Testme()
//{
//    RibbonControl ribbon = ComponentManager.Ribbon;
//    if (ribbon != null)
//    {
//        RibbonTab rtab = ribbon.FindTab("TESTME");
//        if (rtab != null)
//        {
//            ribbon.Tabs.Remove(rtab);
//        }
//        rtab = new RibbonTab();
//        rtab.Title = "TEST  ME";
//        rtab.Id = "Testing";
//        //Add the Tab
//        ribbon.Tabs.Add(rtab);
//        addContent(rtab);
//    }
//}

//static void addContent(RibbonTab rtab)
//{
//    rtab.Panels.Add(AddOnePanel());
//}

//static RibbonPanel AddOnePanel()
//{
//    RibbonButton rb;
//    RibbonPanelSource rps = new RibbonPanelSource();
//    rps.Title = "Test One";
//    RibbonPanel rp = new RibbonPanel();
//    rp.Source = rps;

//    //Create a Command Item that the Dialog Launcher can use,
//    // for this test it is just a place holder.
//    RibbonButton rci = new RibbonButton();
//    rci.Name = "TestCommand";

//    //assign the Command Item to the DialgLauncher which auto-enables
//    // the little button at the lower right of a Panel
//    rps.DialogLauncher = rci;

//    rb = new RibbonButton();
//    rb.Name = "Test Button";
//    rb.ShowText = true;
//    rb.Text = "Test Button";
//    //Add the Button to the Tab
//    rps.Items.Add(rb);
//    return rp;
//}










//        }
//        DialogResult messageBox = MessageBox.Show("Deseja selecionar outro Instrumento?", "Related To", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
//        if (messageBox == DialogResult.No)
//            break;
//    }
//    if (!(checkBoxEquipmentOtherDWG.Checked))
//    {
//        PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento: ");
//        if (equipment.Status == PromptStatus.OK)
//        {
//            while (true)
//            {
//                using (var trEquipment = docInstruments.TransactionManager.StartTransaction())
//                {
//                    Entity ent = (Entity)trEquipment.GetObject(equipment.ObjectId, OpenMode.ForRead);
//                    //Pegando o nome da classe que aparace no PLants como parametro de filtro entre linha e equipamento
//                    if (ent.Id.ObjectClass.DxfName == "ACPPASSET" | ent.Id.ObjectClass.DxfName == "SLINE")
//                    {
//                        int equipmentRowId = dlmInstruments.FindAcPpRowId(equipment.ObjectId);
//                        StringCollection eKeys = new StringCollection { "Tag" };
//                        StringCollection eVals = dlmInstruments.GetProperties(equipmentRowId, eKeys, true);
//                        foreach (ObjectId intrumentId in Instruments)
//                        {
//                            int instrumentRowId = dlmInstruments.FindAcPpRowId(intrumentId);
//                            StringCollection iKeys = new StringCollection
//                                    {
//                                        "Tag",
//                                        "RelatedTo"
//                                    };
//                            StringCollection iVals = dlmInstruments.GetProperties(instrumentRowId, iKeys, true);
//                            iVals[1] = eVals[0];
//                            dbInstruments.StartTransaction();
//                            dlmInstruments.SetProperties(intrumentId, iKeys, iVals);
//                            dbInstruments.CommitTransaction();
//                        }
//                        MessageBox.Show("Related To executado com sucesso!!");
//                        foreach (ListViewItem item in listView.Items)
//                            this.listView.Items.Remove(item);
//                        this.listView.Items.Clear();
//                        Instruments.Clear();
//                        trEquipment.Commit();
//                        break;
//                    }
//                    else
//                    {
//                        MessageBox.Show("Selecione um equipamento ou linha!!");
//                    }

//                }
//            }
//        }

//    }

//}
