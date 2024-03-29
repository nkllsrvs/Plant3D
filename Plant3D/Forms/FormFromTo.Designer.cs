﻿using Autodesk.AutoCAD.ApplicationServices;
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
    partial class FormFromTo 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFromTo));
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonEquipment = new System.Windows.Forms.Button();
            this.buttonLines = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxOtherDWG = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.buttonClearSelection = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(9, 237);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(289, 212);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Lines";
            this.columnHeader1.Width = 77;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "From";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "To";
            this.columnHeader3.Width = 100;
            // 
            // buttonEquipment
            // 
            this.buttonEquipment.BackColor = System.Drawing.Color.Silver;
            this.buttonEquipment.Enabled = false;
            this.buttonEquipment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonEquipment.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonEquipment.Location = new System.Drawing.Point(43, 534);
            this.buttonEquipment.Name = "buttonEquipment";
            this.buttonEquipment.Size = new System.Drawing.Size(224, 42);
            this.buttonEquipment.TabIndex = 6;
            this.buttonEquipment.Text = "Object To";
            this.buttonEquipment.UseVisualStyleBackColor = false;
            this.buttonEquipment.Click += new System.EventHandler(this.buttonFromTo_Click);
            // 
            // buttonLines
            // 
            this.buttonLines.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonLines.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLines.Location = new System.Drawing.Point(43, 483);
            this.buttonLines.Name = "buttonLines";
            this.buttonLines.Size = new System.Drawing.Size(224, 45);
            this.buttonLines.TabIndex = 8;
            this.buttonLines.Text = "Line";
            this.buttonLines.UseVisualStyleBackColor = false;
            this.buttonLines.Click += new System.EventHandler(this.buttonLine_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Instruções:";
            // 
            // checkBoxOtherDWG
            // 
            this.checkBoxOtherDWG.AutoSize = true;
            this.checkBoxOtherDWG.Location = new System.Drawing.Point(9, 460);
            this.checkBoxOtherDWG.Name = "checkBoxOtherDWG";
            this.checkBoxOtherDWG.Size = new System.Drawing.Size(88, 17);
            this.checkBoxOtherDWG.TabIndex = 10;
            this.checkBoxOtherDWG.Text = "Element \"To\" in another DWG?";
            this.checkBoxOtherDWG.UseVisualStyleBackColor = true;
            this.checkBoxOtherDWG.Click += new System.EventHandler(this.checkBoxEquipmentOtherDWG_CheckedChanged);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(9, 20);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox1.Size = new System.Drawing.Size(289, 211);
            this.richTextBox1.TabIndex = 11;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // buttonClearSelection
            // 
            this.buttonClearSelection.BackColor = System.Drawing.Color.LightCoral;
            this.buttonClearSelection.Location = new System.Drawing.Point(209, 456);
            this.buttonClearSelection.Name = "buttonClearSelection";
            this.buttonClearSelection.Size = new System.Drawing.Size(89, 23);
            this.buttonClearSelection.TabIndex = 12;
            this.buttonClearSelection.Text = "Clear Select";
            this.buttonClearSelection.UseVisualStyleBackColor = false;
            this.buttonClearSelection.Click += new System.EventHandler(this.buttonClearClick);
            // 
            // FormFromTo
            // 
            this.ClientSize = new System.Drawing.Size(310, 590);
            this.Controls.Add(this.buttonClearSelection);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.checkBoxOtherDWG);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonLines);
            this.Controls.Add(this.buttonEquipment);
            this.Controls.Add(this.listView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(10, 160);
            this.Name = "FormFromTo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "VALE -  From To";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public ListView listView;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        public Button buttonEquipment;
        public Button buttonLines;
        private Label label1;
        public CheckBox checkBoxOtherDWG;
        private RichTextBox richTextBox1;
        public Button buttonClearSelection;
        private ColumnHeader columnHeader3;
    }
}