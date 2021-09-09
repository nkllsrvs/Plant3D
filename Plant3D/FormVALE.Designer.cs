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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVALE));
            this.buttonSelection = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRelatedTo = new System.Windows.Forms.Button();
            this.anotherDwg = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonSelection
            // 
            this.buttonSelection.BackColor = System.Drawing.Color.Gray;
            this.buttonSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSelection.Location = new System.Drawing.Point(544, 404);
            this.buttonSelection.Name = "buttonSelection";
            this.buttonSelection.Size = new System.Drawing.Size(103, 41);
            this.buttonSelection.TabIndex = 0;
            this.buttonSelection.Text = "Selection";
            this.buttonSelection.UseVisualStyleBackColor = false;
            this.buttonSelection.Click += new System.EventHandler(this.buttonSelection_Click);
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(544, 41);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(224, 327);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
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
            this.buttonRelatedTo.Location = new System.Drawing.Point(679, 404);
            this.buttonRelatedTo.Name = "buttonRelatedTo";
            this.buttonRelatedTo.Size = new System.Drawing.Size(89, 23);
            this.buttonRelatedTo.TabIndex = 2;
            this.buttonRelatedTo.Text = "RelatedTo";
            this.buttonRelatedTo.UseVisualStyleBackColor = true;
            this.buttonRelatedTo.Click += new System.EventHandler(this.buttonRelatedTo_Click);
            // 
            // anotherDwg
            // 
            this.anotherDwg.AutoSize = true;
            this.anotherDwg.Location = new System.Drawing.Point(37, 74);
            this.anotherDwg.Name = "anotherDwg";
            this.anotherDwg.Size = new System.Drawing.Size(194, 17);
            this.anotherDwg.TabIndex = 3;
            this.anotherDwg.Text = "O equipamento está em outro dwg?";
            this.anotherDwg.UseVisualStyleBackColor = true;
            this.anotherDwg.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 122);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(387, 104);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // FormVALE
            // 
            this.ClientSize = new System.Drawing.Size(838, 490);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.anotherDwg);
            this.Controls.Add(this.buttonRelatedTo);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.buttonSelection);
            this.Name = "FormVALE";
            this.Load += new System.EventHandler(this.FormVALE_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private CheckBox anotherDwg;
        private Label label1;
    }
}