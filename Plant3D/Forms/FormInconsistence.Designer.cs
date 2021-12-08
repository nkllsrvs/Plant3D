namespace Plant3D.Forms
{
    partial class FormInconsistence
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lvwInconsistence = new System.Windows.Forms.ListView();
            this.TAG = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvwEquipements = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 606);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1112, 50);
            this.panel1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1025, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lvwInconsistence);
            this.panel2.Controls.Add(this.lvwEquipements);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1112, 606);
            this.panel2.TabIndex = 2;
            // 
            // lvwInconsistence
            // 
            this.lvwInconsistence.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TAG,
            this.Type,
            this.Message});
            this.lvwInconsistence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwInconsistence.FullRowSelect = true;
            this.lvwInconsistence.GridLines = true;
            this.lvwInconsistence.HideSelection = false;
            this.lvwInconsistence.Location = new System.Drawing.Point(0, 0);
            this.lvwInconsistence.Name = "lvwInconsistence";
            this.lvwInconsistence.Size = new System.Drawing.Size(904, 606);
            this.lvwInconsistence.TabIndex = 1;
            this.lvwInconsistence.UseCompatibleStateImageBehavior = false;
            this.lvwInconsistence.View = System.Windows.Forms.View.Details;
            // 
            // TAG
            // 
            this.TAG.Text = "TAG";
            this.TAG.Width = 200;
            // 
            // Type
            // 
            this.Type.Text = "Type";
            this.Type.Width = 100;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 3000;
            // 
            // lvwEquipements
            // 
            this.lvwEquipements.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvwEquipements.Dock = System.Windows.Forms.DockStyle.Right;
            this.lvwEquipements.GridLines = true;
            this.lvwEquipements.HideSelection = false;
            this.lvwEquipements.Location = new System.Drawing.Point(904, 0);
            this.lvwEquipements.Name = "lvwEquipements";
            this.lvwEquipements.Size = new System.Drawing.Size(208, 606);
            this.lvwEquipements.TabIndex = 2;
            this.lvwEquipements.UseCompatibleStateImageBehavior = false;
            this.lvwEquipements.View = System.Windows.Forms.View.Details;
            this.lvwEquipements.Visible = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Equipments";
            this.columnHeader1.Width = 200;
            // 
            // FormInconsistence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 656);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "FormInconsistence";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inconsistências";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormInconsistence_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ListView lvwInconsistence;
        private System.Windows.Forms.ColumnHeader TAG;
        private System.Windows.Forms.ColumnHeader Type;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ListView lvwEquipements;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}