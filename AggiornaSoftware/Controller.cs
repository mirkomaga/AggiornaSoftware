using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AggiornaSoftware
{
    class Controller
    {
        public static string pathJson = @"C:\Users\config.json";
        //public static string pathLocal = @"C:\Users\edgesuser\AppData\Roaming\Autodesk\Revit\Addins\2020\FocchiStartup\";
        public static string AutocadPathLocal = "";
        public static string RevitPathLocal = "";
        public static string pathServer = @"X:\mirko\DatiAggiornati\";
        //public static string[] whiteListExtension = new string[] { ".dll", ".exe", ".pdb" };
        public static string[] whiteListFile = new string[] { "focchiCoreSupport.pdb", 
            "focchiCoreSupport.dll", 
            "focchiProjectEditLibrary.pdb", 
            "focchiProjectEditLibrary.dll",
            "RevitCoreSupport.pdb",
            "RevitCoreSupport.dll",
            "focchiProjectEdit.pdb",
            "focchiProjectEdit.exe",
            "FocchiStartup.pdb",
            "FocchiStartup.dll",
            "focchiBatch.dll",
            "focchiDesign.dll","System.Memory.dll","System.Numerics.Vectors.dll",
            "focchiDraw.dll"

        };
        public static DateTime lastUpdateLocal;
        public static DateTime lastUpdateServer;
        public static List<InfoFile> filesToRefresh = new List<InfoFile>();

        public static void refresh_OLDDDDDD()
        {
            try
            {
                LoadJson();
            }
            catch
            {
                writeJson("", null);
            }


            JObject pathLocalTmp = LoadJson();


            if (!string.IsNullOrEmpty((string)pathLocalTmp["localPath"]))
            {
                pathLocalTmp = LoadJson();

                // TODO Refresho
                //pathLocal = (string)pathLocalTmp["localPath"];

                //string[] filesLocal = getListFolder(pathLocal);

                //string[] filesServer = getListFolder(pathServer);

                //filesToRefresh = eseguoControlloDate(filesLocal, filesServer);
            }
        }

        public static void refresh()
        {
            List<Plugin> dataServer = analizzoServer();
            filesToRefresh = new List<InfoFile>();

            try
            {
                LoadJson();
            }
            catch
            {
                writeJson("", null);
            }


            JObject json = LoadJson();

            if (!string.IsNullOrEmpty((string)json["RevitPathLocal"]))
            {
                RevitPathLocal = (string)json["RevitPathLocal"];
                aggiornoListaFile("Revit", dataServer, RevitPathLocal);
            }
            else
            {
                Console.WriteLine("Non esiste path revit");
            }

            if (!string.IsNullOrEmpty((string)json["AutocadPathLocal"]))
            {
                AutocadPathLocal = (string)json["AutocadPathLocal"];
                aggiornoListaFile("Autocad", dataServer, AutocadPathLocal);
            }
            else
            {
                Console.WriteLine("Non esiste path autocad");
            }

        }

        public static void aggiornoListaFile(string programmaName, List<Plugin> dataServer, string pathLocal)
        {
            List<Plugin> programmaPlugins = dataServer.FindAll(p => p.nomeProgramma == programmaName);

            foreach (Plugin p in programmaPlugins)
            {
                string[] filesServer = getListFolder(p.pathRiferimentoServer);
                string[] filesLocal = getListFolder(pathLocal+"\\"+p.nomePlugin);
                filesToRefresh.AddRange(eseguoControlloDate(filesLocal, filesServer));
            }
        }

        public static List<Plugin> analizzoServer()
        {
            List <Plugin> pluginsList = new List<Plugin>();

            if (!string.IsNullOrEmpty(pathServer))
            {
                string[] dictsPrograms = Directory.GetDirectories(pathServer);

                foreach (string pathProgr in dictsPrograms)
                {
                    string nomeProgramma = new DirectoryInfo(pathProgr).Name;

                    string[] progrPlugins = Directory.GetDirectories(pathProgr);

                    foreach (string pathPlugins in progrPlugins)
                    {
                        string nomePlugin = new DirectoryInfo(pathPlugins).Name;

                        pluginsList.Add(new Plugin(nomeProgramma, nomePlugin, pathPlugins, ""));
                    }
                }
            }
            return pluginsList;
        }

        public static void chooseFolder(string type)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Autodesk\Revit\Addins\2020\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                writeJson(dialog.FileName, type);
            }
        }

        public static void writeJson(string folderPath, string? type)
        {
            try
            {
                string json = File.ReadAllText(pathJson);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj[type] = folderPath;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathJson, output);
            }
            catch
            {
                IDictionary<string, string> jsonObj = new Dictionary<string, string>();
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathJson, output);
            }
        }

        public static void replaceData(ListView form, ToolStripProgressBar pb)
        {

            ListView.CheckedListViewItemCollection checkedItems = form.CheckedItems;

            List<Controller.InfoFile> filesList = new List<Controller.InfoFile>();

            pb.Value = 0;
            pb.Maximum = checkedItems.Count;
            foreach (ListViewItem i in checkedItems)
            {
                Controller.InfoFile fi = Controller.filesToRefresh[Int32.Parse(i.Tag.ToString())];

                try
                {
                    //File.Copy(Path.Combine(Path.GetDirectoryName(fi.path), fi.nomeFile), Path.Combine(Path.GetDirectoryName(fi.pathLocale), fi.nomeFile), true);
                    Console.WriteLine("Sostituisco " + fi.nomeFile);
                    i.SubItems[3].Text = "Completed";
                    //i.Remove();
                }
                catch (System.IO.IOException e)
                {
                    MessageBox.Show("Chiudere is programmi");
                    i.SubItems[3].Text = "Failed";
                    break;
                }

                pb.Value = pb.Value + 1;
            }
            form.Refresh();
        }

        public static JObject LoadJson()
        {

            JObject o1 = JObject.Parse(File.ReadAllText(pathJson));
            return o1;
        }

        public static string[] getListFolder(string path)
        {
            string[] files = null;
            try
            {
                files = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories);
            }catch(System.IO.DirectoryNotFoundException e)
            {
                MessageBox.Show("Cartella inesistente: "+ path, "Errore");
            }

            return files;
        }

        public static List<InfoFile> eseguoControlloDate(string[] filesLocal, string[] filesServer)
        {
            List<InfoFile> fileDaAggiornare = new List<InfoFile>();

            List<InfoFile> filesLocalInfo = analyzeFiles(filesLocal);
            List<InfoFile> filesServerInfo = analyzeFiles(filesServer);

            if (filesLocalInfo.Count > 0)
            { 
                IOrderedEnumerable<InfoFile> fileLocalMostRecent = filesLocalInfo.OrderByDescending(f => f.dataLastEdit);
                lastUpdateLocal = fileLocalMostRecent.First().dataLastEdit;
            }


            if (filesServerInfo.Count > 0)
            { 
                IOrderedEnumerable<InfoFile> fileServerMostRecent = filesServerInfo.OrderByDescending(f => f.dataLastEdit);
                lastUpdateServer = fileServerMostRecent.First().dataLastEdit;
            }

            if (filesLocal.Length > 0 && filesServer.Length > 0)
            {
                foreach (InfoFile fs in filesServerInfo)
                {
                    InfoFile fileLocal = filesLocalInfo.Find(fl => fl.nomeFile == fs.nomeFile);

                    if (fs.dataLastEdit > fileLocal.dataLastEdit)
                    {
                        InfoFile toReplace = new InfoFile(fs.dataLastEdit, fs.extension, fs.nomeFile, fs.path, fileLocal.path);
                        fileDaAggiornare.Add(toReplace);
                    }
                }
            }

            return fileDaAggiornare;
        }

        public static List<InfoFile> analyzeFiles(string[] listFiles)
        {
            List<InfoFile> results = new List<InfoFile>();
            foreach (string file in listFiles)
            {
                string name = Path.GetFileName(file);

                if (whiteListFile.Contains(name))
                {
                    DateTime dt = File.GetLastWriteTime(file);
                    
                    string ext = Path.GetExtension(file);

                    results.Add(new InfoFile(dt, ext, name, file, null));
                }
            }
            return results;
        }

        public struct InfoFile
        {
            public InfoFile(DateTime dataLastEdit_, string extension_, string nomeFile_, string path_, string? pathLocale_)
            {
                dataLastEdit = dataLastEdit_;
                extension = extension_;
                nomeFile = nomeFile_;
                path = path_;
                pathLocale = pathLocale_;
            }

            public DateTime dataLastEdit { get; set; }
            public string extension { get; set; }
            public string nomeFile { get; set; }
            public string path { get; set; }
            public string pathLocale { get; set; }
        }

        public struct Plugin
        {
            public Plugin(string nomeProgramma_, string nomePlugin_, string pathRiferimentoServer_, string pathRiferimentoLocale_)
            {
                nomeProgramma = nomeProgramma_;
                nomePlugin = nomePlugin_;
                pathRiferimentoServer = pathRiferimentoServer_;
                pathRiferimentoLocale = pathRiferimentoLocale_;
            }

            public string nomeProgramma { get; set; }
            public string nomePlugin { get; set; }
            public string pathRiferimentoServer { get; set; }
            public string pathRiferimentoLocale { get; set; }
        }
    }
}
