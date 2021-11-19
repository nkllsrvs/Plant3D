using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using Plant3D.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace Plant3D
{
    public partial class FormRelatedTo : Commands
    {
        int countRTE = 0;
        
        public FormRelatedTo()
        {
            InitializeComponent();

        }

        private void ButtonRelatedTo_Click(object sender, EventArgs e)
        {

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxEquipmentOtherDWG_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxEquipmentOtherDWG.Checked == true)
                {
                    buttonEquipment.Enabled = true;
                    buttonEquipment.BackColor = Color.LightGreen;
                }
                if (checkBoxEquipmentOtherDWG.Checked == false)
                {
                    buttonEquipment.Enabled = false;
                    buttonEquipment.BackColor = Color.Silver;
                }
            }
            catch { }
        }
        public bool HaveRelatedToEquip(List<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                if (kvp.Key.Equals("RelatedToEquip"))
                    return true;
            }
            return false;
        }
        public void ReplacePropertys(Entity entityEdited, List<Instruments> instruments)
        {
            foreach (Instruments i in instruments)
            {
                if (entityEdited.ObjectId == i.Id)
                {
                    entityEdited.Layer = i.Layer;
                    entityEdited.LayerId = i.LayerId;
                    break;
                }
            }

        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void buttonClearClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.Items)
                this.listView.Items.Remove(item);
            this.listView.Items.Clear();
            InstrumentsRT.Clear();
            InstrumentsRTOld.Clear();
            countRTE = 0;
        }
    }

}

