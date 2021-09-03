using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace Plant3D
{
    partial class FormVALE
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonSelection = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRelatedTo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonSelection
            // 
            this.buttonSelection.Location = new System.Drawing.Point(242, 333);
            this.buttonSelection.Name = "buttonSelection";
            this.buttonSelection.Size = new System.Drawing.Size(98, 23);
            this.buttonSelection.TabIndex = 0;
            this.buttonSelection.Text = "Selection";
            this.buttonSelection.UseVisualStyleBackColor = true;
            this.buttonSelection.Click += new System.EventHandler(this.buttonSelection_Click);
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(175, 51);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(387, 232);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "RowID";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "InstrumentName";
            // 
            // buttonRelatedTo
            // 
            this.buttonRelatedTo.Location = new System.Drawing.Point(395, 333);
            this.buttonRelatedTo.Name = "buttonRelatedTo";
            this.buttonRelatedTo.Size = new System.Drawing.Size(89, 23);
            this.buttonRelatedTo.TabIndex = 2;
            this.buttonRelatedTo.Text = "RelatedTo";
            this.buttonRelatedTo.UseVisualStyleBackColor = true;
            this.buttonRelatedTo.Click += new System.EventHandler(this.buttonRelatedTo_Click);
            // 
            // FormVALE
            // 
            this.ClientSize = new System.Drawing.Size(596, 395);
            this.Controls.Add(this.buttonRelatedTo);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.buttonSelection);
            this.Name = "FormVALE";
            this.ResumeLayout(false);

        }
        private void RelatedTo()
        {
            PlantProject proj = PlantApplication.CurrentProject;
            ProjectPartCollection projParts = proj.ProjectParts;
            PnIdProject pnidProj = (PnIdProject)projParts["PnId"];
            DataLinksManager dlm = pnidProj.DataLinksManager;
            PnPDatabase db = dlm.GetPnPDatabase();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            List<PromptEntityResult> Instruments = new List<PromptEntityResult>();
            PromptEntityResult result;

            bool loop = true;
            while (loop)
            {
                result = ed.GetEntity("\nSelecione um  Instrumento: ");
                StringCollection eKeys = new StringCollection
                {

                    "Description",
                    "Tag",
                    "RelatedTo",
                    "ClassName",
                    "Class"
                };
                StringCollection eVals = dlm.GetProperties(dlm.FindAcPpRowId(result.ObjectId), eKeys, true);
                if (result.Status == PromptStatus.OK){
                    Instruments.Add(result); 
                }
                DialogResult dr = MessageBox.Show("Deseja continuar a selecionar Instrumentos?", "RelatedTo", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.No)
                    break;
            }

            PromptEntityResult equipment = ed.GetEntity("\nSelecione um Equipamento: ");
            if (equipment.Status == PromptStatus.OK)
            {
                int equipmentRowId = dlm.FindAcPpRowId(equipment.ObjectId);
                StringCollection eKeys = new StringCollection
                {   
                    
                    "Description",
                    "Tag",
                    "RelatedTo",
                    "ClassName",
                    "Class"
                };
                StringCollection eVals = dlm.GetProperties(equipmentRowId, eKeys, true);
                foreach (PromptEntityResult entityResult in Instruments)
                {
                    int instrumentRowId = dlm.FindAcPpRowId(entityResult.ObjectId);
                    StringCollection iKeys = new StringCollection
                    {
                        "Description",
                        "Tag",
                        "RelatedTo",
                        "ClassName",
                        "Class"
                    };
                    StringCollection iVals = dlm.GetProperties(instrumentRowId, iKeys, true);

                    iVals[2] = eVals[1];

                    db.StartTransaction();
                    dlm.SetProperties(entityResult.ObjectId, iKeys, iVals);
                    db.CommitTransaction();
                }
            }
        }
        #endregion
        private System.Windows.Forms.ProgressBar progressBar1;
        private Button buttonSelection;
        private ListView listView;
        private Button buttonRelatedTo;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
    }
}