using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Security.AccessControl;
using System.Net.NetworkInformation;

namespace AggiornaSoftware
{
    class Controller
    {
        
        private static string pathAutodesk = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\Autodesk";
        private static string pathProgetti = @"C:\_Progetti\";
        private static string pathRilascio = @"\bin\Release";
        private static List<string> nomeProgetti = new List<string>()
        {
            "FocchiStartup",
            "ProjectEdit",
            "focchiBatch",
            "focchiDesign",
            "focchiDraw"
        };

        public static string pathJson = @"C:\Users\config.json";
        public static string AutocadPathLocal = "";
        public static string RevitPathLocal = "";
        public static string pathServer = @"X:\mirko\DatiAggiornati\";
        //public static string[] whiteListExtension = new string[] { ".dll", ".exe", ".pdb" };
        public static string[] whiteListFile = new string[] {
            "focchiProjectEdit.exe",
            "FocchiStartup.dll",
            "focchiBatch.dll",
            "focchiDesign.dll",
            "focchiDraw.dll",
            "EdgeAutocadPlugins.dll"
        };

        internal static Dictionary<FileInfo, FileInfo> cercoSoluzioniRilasciate()
        {
            List<string> filesCompilati = new List<string>();

            foreach (string s in nomeProgetti)
            {
                string pathComposta = pathProgetti + s + pathRilascio;

                try
                {
                    string[] files = Directory.GetFileSystemEntries(pathComposta, "*");

                    foreach (string f in files)
                    {

                        string nomeFile = Path.GetFileName(f);

                        if (whiteListFile.Contains(nomeFile))
                        {
                            filesCompilati.Add(f);
                        }
                    }
                }
                catch { }

            }

            string[] filesServer = getListFolder(pathServer);

            Dictionary<FileInfo, FileInfo> data = new Dictionary<FileInfo, FileInfo>();

            foreach (string fiCompilato in filesCompilati.ToArray()) 
            {
                FileInfo fiC = new FileInfo(fiCompilato);

                FileInfo fiS = new FileInfo(filesServer.Where(fi => fi.Contains(fiC.Name)).FirstOrDefault());

                if(fiS != null)
                {
                    if(fiC.LastWriteTime > fiS.LastWriteTime)
                    {
                        data.Add(fiC, fiS);
                    }
                }
            }

            return data;
        }

        // Restituisce fonte/server, destinazione/locale
        internal static Dictionary<FileInfo, FileInfo> CheckPluginStatus(string nomePlugin, string percorsoLocale)
        {
            Dictionary<FileInfo, FileInfo> assoc = new Dictionary<FileInfo, FileInfo>();

            if(destinazioneInstallazionePlugins.ContainsKey(nomePlugin) && fonteInstallazionePlugins.ContainsKey(nomePlugin))
            {
                List<string> destPath = Directory.GetFiles(percorsoLocale, "*", SearchOption.AllDirectories).ToList();

                string pathFonte = fonteInstallazionePlugins[nomePlugin];

                List<string> fontePath = Directory.GetFiles(pathFonte, "*", SearchOption.AllDirectories).ToList();

                foreach (string file in fontePath)
                {
                    string fonteFname = Path.GetFileName(file);

                    if (whiteListFile.Contains(fonteFname))
                    {
                        FileInfo fiFonte = new FileInfo(file);

                        FileInfo fiDest = new FileInfo(destPath.Where(f => f.Contains(fiFonte.Name)).FirstOrDefault());

                        Console.WriteLine(fiFonte.Name);

                        if(fiFonte.LastWriteTime > fiDest.LastWriteTime)
                        {
                            assoc.Add(fiFonte, fiDest);
                        }
                    }
                }
            }

            return assoc;
        }

        public static string[] namePlugins = new string[]
        {
            "FocchiStartup",
            "focchiBatch.bundle",
            "focchiDesign.bundle",
            "focchiDraw.bundle",
            "EdgePlugins.bundle"
        };

        public static Dictionary<string,string> destinazioneInstallazionePlugins = new Dictionary<string, string>()
        {
            { "FocchiStartup", @"\Autodesk\Revit\Addins\2020"},
            { "focchiBatch.bundle", @"\Autodesk\ApplicationPlugins"},
            { "focchiDesign.bundle", @"\Autodesk\ApplicationPlugins"},
            { "focchiDraw.bundle", @"\Autodesk\ApplicationPlugins"},
            { "EdgePlugins.bundle", @"\Autodesk\ApplicationPlugins"}
        };

        public static Dictionary<string, string> fonteInstallazionePlugins = new Dictionary<string, string>()
        {
            { "FocchiStartup", @"X:\mirko\DatiAggiornati\Revit\FocchiStartup"},
            { "focchiBatch.bundle", @"X:\mirko\DatiAggiornati\Autocad\focchiBatch.bundle"},
            { "focchiDesign.bundle", @"X:\mirko\DatiAggiornati\Autocad\focchiDesign.bundle"},
            { "focchiDraw.bundle", @"X:\mirko\DatiAggiornati\Autocad\focchiDraw.bundle"},
            { "EdgePlugins.bundle", @"X:\mirko\DatiAggiornati\Autocad\EdgePlugins.bundle"}
        };

        internal static bool sostituiscoFile(FileInfo fiServer, FileInfo fiLocal)
        {
            try
            {
                File.Copy(Path.Combine(Path.GetDirectoryName(fiServer.FullName), fiServer.Name), Path.Combine(Path.GetDirectoryName(fiLocal.FullName), fiLocal.Name), true);
                return true;
            }
            catch (System.IO.IOException e)
            {
                return false;
            }
        }

        public static DateTime lastUpdateLocal;
        public static DateTime lastUpdateServer;
        public static List<InfoFile> filesToRefresh = new List<InfoFile>();


        public static List<Plugin> findPath(List<Plugin> dataServer)
        {
            string pathAutodesk = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\Autodesk";

            List<Plugin> result = new List<Plugin>();

            if (Directory.Exists(pathAutodesk))
            {
                foreach(string namePlugin in namePlugins)
                {
                    IEnumerable<string> dirs = Directory.GetDirectories(pathAutodesk, namePlugin, SearchOption.AllDirectories);
                    
                    Plugin pl;
                    string path;

                    switch (namePlugin)
                    {
                        case "FocchiStartup":
                            pl = dataServer.Find(f => f.nomePlugin == namePlugin);

                            path = dirs.Where(d => d.Contains("2020")).FirstOrDefault();

                            pl.pathRiferimentoLocale = path;

                            result.Add(pl);
                            break;
                        case "ProjectEdit":
                            pl = dataServer.Find(f => f.nomePlugin == namePlugin);

                            dirs = Directory.GetDirectories(pathAutodesk, "FocchiStartup", SearchOption.AllDirectories);
                            path = dirs.Where(d => d.Contains("2020")).FirstOrDefault();

                            pl.pathRiferimentoLocale = path;

                            result.Add(pl);
                            break;
                        case "focchiBatch.bundle":case "focchiDraw.bundle":case "focchiDesign.bundle":
                            pl = dataServer.Find(f => f.nomePlugin == namePlugin);

                            path = dirs.FirstOrDefault();

                            pl.pathRiferimentoLocale = path;

                            result.Add(pl);
                            break;
                    }
                }
            }

            return result;
        }

        
        internal static void DisinstallaPlugins(Dictionary<string, string> dictionaries)
        {
            foreach (KeyValuePair<string, string> v in dictionaries)
            {
                string nomePlugin = v.Key;
                string pathLocale = v.Value;

                if (!string.IsNullOrEmpty(pathLocale))
                {
                    DirectoryInfo di = new DirectoryInfo(pathLocale);

                    foreach (FileInfo file in di.EnumerateFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.EnumerateDirectories())
                    {
                        dir.Delete(true);
                    }
                    Directory.Delete(pathLocale);
                }
            }
        }

        internal static void InstallaPlugins(Dictionary<string, string> dictionaries)
        {
            foreach (KeyValuePair<string, string> v in dictionaries)
            {
                string nomePlugin = v.Key;
                string pathLocale = v.Value;

                if (string.IsNullOrEmpty(pathLocale))
                {
                    if (destinazioneInstallazionePlugins.ContainsKey(nomePlugin))
                    {
                        string pathInstallazione = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + destinazioneInstallazionePlugins[nomePlugin] + "\\" +nomePlugin;

                        if (fonteInstallazionePlugins.ContainsKey(nomePlugin))
                        {
                            string pathFonte = fonteInstallazionePlugins[nomePlugin];

                            Directory.CreateDirectory(pathInstallazione);

                            //Now Create all of the directories
                            foreach (string dirPath in Directory.GetDirectories(pathFonte, "*",
                                SearchOption.AllDirectories))
                                Directory.CreateDirectory(dirPath.Replace(pathFonte, pathInstallazione));

                            //Copy all the files & Replaces any files with the same name
                            foreach (string newPath in Directory.GetFiles(pathFonte, "*.*",
                                SearchOption.AllDirectories))
                                File.Copy(newPath, newPath.Replace(pathFonte, pathInstallazione), true);
                        }                            
                    }
                }
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

        //public static void replaceData(ListView form, ToolStripProgressBar pb)
        //{
        //    ListView.CheckedListViewItemCollection checkedItems = form.CheckedItems;
        //    List<Controller.InfoFile> filesList = new List<Controller.InfoFile>();
        //    pb.Value = 0;
        //    pb.Maximum = checkedItems.Count;
        //    foreach (ListViewItem i in checkedItems)
        //    {
        //        Controller.InfoFile fi = Controller.filesToRefresh[Int32.Parse(i.Tag.ToString())];
        //        try
        //        {
        //            File.Copy(Path.Combine(Path.GetDirectoryName(fi.path), fi.nomeFile), Path.Combine(Path.GetDirectoryName(fi.pathLocale), fi.nomeFile), true);
        //            Console.WriteLine("Sostituisco " + fi.nomeFile);
        //            i.SubItems[3].Text = "Completed";
        //            //i.Remove();
        //        }
        //        catch (System.IO.IOException e)
        //        {
        //            MessageBox.Show("Chiudere is programmi");
        //            i.SubItems[3].Text = "Failed";
        //            break;
        //        }
        //        pb.Value = pb.Value + 1;
        //    }
        //    form.Refresh();
        //}

        //public static void replaceData(ListView form, ToolStripProgressBar pb, List<InfoFile> listIF)
        //{
        //    ListView.CheckedListViewItemCollection checkedItems = form.CheckedItems;
        //    List<Controller.InfoFile> filesList = new List<Controller.InfoFile>();
        //    pb.Value = 0;
        //    pb.Maximum = checkedItems.Count;
        //    foreach (ListViewItem i in checkedItems)
        //    {
        //        Controller.InfoFile fi = listIF[Int32.Parse(i.Tag.ToString())];
        //        try
        //        {
        //            File.Copy(Path.Combine(Path.GetDirectoryName(fi.path), fi.nomeFile), Path.Combine(Path.GetDirectoryName(fi.pathLocale), fi.nomeFile), true);
        //            Console.WriteLine("Sostituisco " + fi.nomeFile);
        //            i.SubItems[3].Text = "Completed";
        //            //i.Remove();
        //        }
        //        catch (System.IO.IOException e)
        //        {
        //            MessageBox.Show("Chiudere is programmi");
        //            i.SubItems[3].Text = "Failed";
        //            break;
        //        }
        //        pb.Value = pb.Value + 1;
        //    }
        //    form.Refresh();
        //}

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
        
        
        
        
        
        // Controllo se i plugin sono installati o meno nella path di autocad
        public static Dictionary<string, string> controlloStatoSoftware()
        {
            List<string> paths = Directory.GetDirectories(pathAutodesk, "*", SearchOption.AllDirectories).ToList();

            Dictionary<string, string> pluginsLocale = new Dictionary<string, string>();

            foreach (string pn in namePlugins)
            {
                if (paths.Where(f => f.Contains(pn)).Count() != 0) {
                    if (pn == "FocchiStartup") 
                    {
                        string path = paths.Where(f => f.Contains(pn)).Where(f => f.Contains("2020")).FirstOrDefault();
                        pluginsLocale.Add(pn, path);

                    }
                    else
                    {
                        pluginsLocale.Add(pn, paths.Where(f => f.Contains(pn)).FirstOrDefault()); 
                    }

                }
                else { pluginsLocale.Add(pn, null); }
            }

            return pluginsLocale;
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
