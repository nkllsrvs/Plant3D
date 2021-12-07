using Plant3D.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plant3D.Forms
{
    public partial class FormInconsistence : Form
    {
        public List<Inconsistence> InconsistenceList { get; set; }
        public List<Element> Equipments { get; set; }
        public FormInconsistence()
        {
            InitializeComponent();
           // InconsistenceList = pInconsistenceList;
        }

        private void FormInconsistence_Load(object sender, EventArgs e)
        {
            lvwEquipements.Items.Clear();
            foreach (Element equipment in Equipments.Where(w => !w.ClassName.Contains("Nozzle")).Distinct().OrderBy(o => o.TAG))
            {
                ListViewItem item = new ListViewItem(equipment.TAG);

                lvwEquipements.Items.Add(item);
            }

            lvwInconsistence.Items.Clear();
            foreach (Inconsistence inconsistence in InconsistenceList)
            {
                ListViewItem item = new ListViewItem(inconsistence.TAG);
                item.SubItems.Add(inconsistence.Type);
                item.SubItems.Add(inconsistence.Message);

                lvwInconsistence.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
