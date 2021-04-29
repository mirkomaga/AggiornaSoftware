using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AggiornaSoftware
{
    public partial class RefreshForm : Form
    {   
        // ? Database
        csDatabase m_db;

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        bool statusStrtup = false;

        private Thread workerThread = null;

        public RefreshForm()
        {
            // ? apre connessione
            m_db = new csDatabase("aggiornamentoPlugins");

            InitializeComponent();

            refreshButtonStartup();

            workerThread = new Thread(frmLoadingShowing);
            workerThread.Start();

            // ? Riferimenti ai plugin
            Dictionary<string, string> data = Controller.controlloStatoSoftware();


            // ? Operazione automatica allo startup
            Controller.DisinstallaPlugins(Controller.controlloStatoSoftware(), m_db);
            Controller.InstallaPlugins(Controller.controlloStatoSoftware(), m_db);

            workerThread.Abort();
        }

        private void refreshButtonStartup()
        {
            object a = rkApp.GetValue("MyApp");
            if (a == null)
            {
                strButton.Text = "Aggiungi a Startup";
                statusStrtup = true;
            }
            else
            {
                strButton.Text = "Rimuovi da Startup";
                statusStrtup = false;
            }
        }

        private void RefreshForm_Load(object sender, EventArgs e)
        {

            if (System.Environment.UserName == "edgesuser") {
                btnRefreshSoluzione.Enabled = true;
            }
            else
            {
                btnRefreshSoluzione.Enabled = false;
            }

            aggiorna();
        }

        private void aggiorna()
        {
            // ? Riferimenti ai plugin
            Dictionary<string, string> data = Controller.controlloStatoSoftware();

            // ? Aggiorno in grid lo stato dei plugin
            AggiornoStatoPlugins(data);

            // ? Aggiorno in grid stato plugin installati
            AggiornoStatoPluginsInstallati(data);
        }

        private void frmLoadingShowing()
        {
            frmLoading fl = new frmLoading();
            fl.ShowDialog();
        }

        private void AggiornoStatoPluginsInstallati(Dictionary<string, string> dictionaries)
        {
            reportFiles.Items.Clear();

            foreach (KeyValuePair<string, string> e in dictionaries)
            {
                string nomePlugin = e.Key;
                string percorso = e.Value;

                if (!string.IsNullOrEmpty(percorso))
                {
                    Dictionary<FileInfo, FileInfo>  filesToRefresh = Controller.CheckPluginStatus(nomePlugin, percorso);

                    if(filesToRefresh.Count() > 0)
                    {
                        int index = 0;
                        foreach(KeyValuePair<FileInfo, FileInfo> ele in filesToRefresh)
                        {
                            FileInfo fServer = ele.Key;
                            FileInfo fLocale = ele.Value;

                            ListViewItem itm = new ListViewItem(fLocale.Name);
                            itm.SubItems.Add(fLocale.FullName);
                            itm.SubItems.Add(fLocale.LastWriteTime.ToString());
                            itm.SubItems.Add("Aggiornare");
                            reportFiles.Items.Add(itm);

                            itm.Tag = ele;

                            itm.Checked = true;

                            index += 1;

                        }
                    }
                    else
                    {
                        //MessageBox.Show("Nessun file da aggiornare", "Attenzione");
                    }
                }
            }
        }

        private void AggiornoStatoPlugCompilati(Dictionary<FileInfo, FileInfo> dictionaries)
        {
            reportFiles.Items.Clear();

            foreach (KeyValuePair<FileInfo, FileInfo> ele in dictionaries)
            {
                FileInfo fServer = ele.Key;
                FileInfo fLocale = ele.Value;

                ListViewItem itm = new ListViewItem(fLocale.Name);
                itm.SubItems.Add(fLocale.FullName);
                itm.SubItems.Add(fLocale.LastWriteTime.ToString());
                itm.SubItems.Add("Aggiornare");
                reportFiles.Items.Add(itm);

                itm.Tag = ele;

                itm.Checked = true;
            }
        }

        // Coloro la form in base ai plugin installati
        private void AggiornoStatoPlugins(Dictionary<string, string> dictionaries)
        {
            lvSoftware.Items.Clear();

            foreach (KeyValuePair<string, string> e in dictionaries)
            {
                string nomePlugin = e.Key;
                string percorso = e.Value;

                if (!string.IsNullOrEmpty(percorso))
                {
                    // Installato
                    ListViewItem itm = new ListViewItem(nomePlugin);
                    itm.BackColor = Color.Green;
                    itm.SubItems.Add(percorso);
                    itm.SubItems.Add("Installato");
                    lvSoftware.Items.Add(itm);

                }
                else
                {
                    // Non installato

                    ListViewItem itm = new ListViewItem(nomePlugin);
                    itm.BackColor = Color.Red;
                    itm.SubItems.Add("");
                    itm.SubItems.Add("Non installato");

                    lvSoftware.Items.Add(itm);
                }
            }

            lvSoftware.Sorting = SortOrder.Ascending;
        }

        private void addToStartup()
        {
            rkApp.SetValue("MyApp", Application.ExecutablePath);
            refreshButtonStartup();
        }

        private void revomeToStartup()
        {
            rkApp.DeleteValue("MyApp", false);
            refreshButtonStartup();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            aggiorna();
            //    toolStripProgressBar1.Value = 0;
            //    Controller.refresh();
            //    refreshData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ListView.CheckedListViewItemCollection checkedItems = reportFiles.CheckedItems;
            ToolStripProgressBar pb = toolStripProgressBar1;

            pb.Value = 0;
            pb.Maximum = checkedItems.Count;

            foreach (ListViewItem i in checkedItems)
            {
                string name = i.SubItems[0].Text;

                KeyValuePair<FileInfo, FileInfo> filesInfo = (KeyValuePair<FileInfo, FileInfo>)i.Tag;

                FileInfo fiServer = filesInfo.Key;
                FileInfo fiLocal = filesInfo.Value;

                if(Controller.sostituiscoFile(fiServer, fiLocal))
                {
                    i.SubItems[3].Text = "Completato";
                }
                else
                {
                    i.SubItems[3].Text = "Fallito";
                }

                pb.Value += 1;
            }

            //aggiorna();

            button3.Enabled = false;
        }

        private void strButton_Click(object sender, EventArgs e)
        {
            if (statusStrtup)
            {
                addToStartup();
            }
            else
            {
                revomeToStartup();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Dictionary<FileInfo, FileInfo> data = Controller.cercoSoluzioniRilasciate();

            if (data.Count == 0)
                MessageBox.Show("Tutte le soluzioni sono aggiornate con il server", "Attenzione");
            else
            {
                AggiornoStatoPlugCompilati(data);
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Maximum = 2;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Value = 1;
            Controller.InstallaPlugins(Controller.controlloStatoSoftware(), m_db);
            toolStripProgressBar1.Value = 2;
            aggiorna();
        }

        private void btnDisinstall_Click(object sender, EventArgs e)
        {
            Controller.DisinstallaPlugins(Controller.controlloStatoSoftware(), m_db);
            aggiorna();
        }

        private void RefreshForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_db.Close();
        }
    }
}
