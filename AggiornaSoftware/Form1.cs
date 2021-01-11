using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AggiornaSoftware
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Controller.refresh();

            refreshData();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Controller.chooseFolder("RevitPathLocal");

            Controller.refresh();

            refreshData();
        }

        private void refreshData()
        {
            if (string.IsNullOrEmpty(Controller.RevitPathLocal))
            {
                label1.Text = "Nessuna cartella selezionata";
            }
            else
            {
                label1.Text = Controller.RevitPathLocal.ToString();
            }

            if (string.IsNullOrEmpty(Controller.AutocadPathLocal))
            {
                label8.Text = "Nessuna cartella selezionata";
            }
            else
            {
                label8.Text = Controller.AutocadPathLocal.ToString();
            }


            //if (Controller.getListFolder(Controller.pathServer).Length == 0)
            //{
            //    label2.Text = "Nessun file trovato";
            //}
            //else
            //{
            //    label2.Text = Controller.lastUpdateServer.ToString();
            //}

            //if (Controller.getListFolder(Controller.pathLocal).Length == 0)
            //{
            //    label3.Text = "Nessun file trovato";
            //}
            //else
            //{
            //    label3.Text = Controller.lastUpdateLocal.ToString();
            //}

            if(Controller.filesToRefresh.Count == 0)
            {
                button3.Enabled = false;
            }
            else
            {
                button3.Enabled = true;
            }

            int i = 0;
            reportFiles.Items.Clear();
            foreach (Controller.InfoFile fi in Controller.filesToRefresh)
            {
                ListViewItem itm = reportFiles.Items.Add(fi.nomeFile);
                itm.SubItems.Add(fi.path);
                itm.SubItems.Add(fi.dataLastEdit.ToString());
                itm.SubItems.Add("pending");
                itm.Checked = true;
                itm.Tag = i;
                i++;
            }

            reportFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            Controller.refresh();
            refreshData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Controller.replaceData(reportFiles, toolStripProgressBar1);
            //Controller.refresh();
            //refreshData();
            button3.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Controller.chooseFolder("AutocadPathLocal");

            Controller.refresh();

            refreshData();
        }
    }
}
