using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Dropbox.Api;
using Nemiro.OAuth;
using Nemiro.OAuth.LoginForms;
using System.Web;
using System.Threading;

namespace SpicedCloud.GUI
{
    public partial class MainWindow : Form
    {
        DropboxClient client;
        DropboxApi api;        
        private string curs;
        private string selectedFolder;
        FileSystemWatcher watcher;
        bool flagCreation;
        string lastFile;

        public MainWindow()
        {
            InitializeComponent();
            selectedFolder = "";
            lastFile = "";
            curs = Properties.Settings.Default.cursor;
            client = new DropboxClient();
            flagCreation = false;     

            if (String.IsNullOrEmpty(Properties.Settings.Default.AccessToken))
            {
                while (this.GetAccesToken() == false)
                {
                    Properties.Settings.Default.selecFolder = "";
                    Properties.Settings.Default.Save();
                }
            }

            api = client.connectDropbox(Properties.Settings.Default.AccessToken);          
            getInfoUser(api);

            WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIconName.Visible = true;

            if (String.IsNullOrEmpty(Properties.Settings.Default.selecFolder))
            {
                chooseFolder();
            }
            else
            {
                selectedFolder = Properties.Settings.Default.selecFolder;
                getStartedSynchronizationAsync(api);
                synchronizationAsync(api);
                createWatcher(selectedFolder);
            }

        }

        #region watcher
        private void createWatcher(string observedFolder)
        {
            watcher = new FileSystemWatcher(observedFolder);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

           watcher.NotifyFilter =  /*NotifyFilters.LastAccess |*/ NotifyFilters.LastWrite |
                                   NotifyFilters.FileName | NotifyFilters.DirectoryName ;


            watcher.Created += new FileSystemEventHandler(watcher_Created/*watcher_Changed*/);
            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
            watcher.Deleted += new FileSystemEventHandler(wathcer_Deleted);
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
           
        }

        async void deletedAsync(FileSystemEventArgs e)
        {
            try
            {
                string name = e.FullPath.Substring(selectedFolder.Length + 1);
                var obj = await client.deleteAsync(api, name);
            }
            catch (Exception ex) { }
        }

        void wathcer_Deleted(object sender, FileSystemEventArgs e)
        {
            updateState(false);
            deletedAsync(e);
            updateState(true);
        }

        async void changedAsync(FileSystemEventArgs e)
        {
            try
            {
                string name = e.FullPath.Substring(selectedFolder.Length + 1);

                if (isFolder(name))
                {
                    if (System.IO.Directory.Exists(e.FullPath))
                    {
                        var folder = await client.createFolderAsync(api, name);
                    }
                }
                else
                {
                    if (System.IO.File.Exists(e.FullPath))
                    {
                        var folder = await client.addFileAsync(api, name, e.FullPath);                  
                    }
                }
              
            }
            catch (Exception ex) { }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(lastFile!= e.Name + DateTime.Now.ToString())
            {
                updateState(false);            
                changedAsync(e);
                updateState(true);
                lastFile = e.Name + DateTime.Now.ToString();
            }
        }

        async void createAsync(FileSystemEventArgs e)
        {
            try
            {
                string name = e.FullPath.Substring(selectedFolder.Length + 1);

                if (isFolder(name))
                {
                    var folder = await client.createFolderAsync(api, name);
                }
                else
                {

                    var file = await client.addFileAsync(api, name, e.FullPath);
                }
            }
            catch (Exception ex) { /*MessageBox.Show(ex.Message); */}
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            updateState(false);
            createAsync(e);
            updateState(true);
            lastFile = e.Name + DateTime.Now.ToString();
        }

        async void renamedAsync(RenamedEventArgs e)
        {
            try
            {
                string name = e.FullPath.Substring(selectedFolder.Length + 1);
                var folder = await client.moveAsync(api, e.OldFullPath.Substring(selectedFolder.Length + 1), name);
            }
            catch (Exception ex) { }
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            updateState(false);
            renamedAsync(e);
            updateState(true);
        }
        #endregion
        
        private async void getStartedSynchronizationAsync(DropboxApi api)
         {
             updateState(false);
             string cur = Properties.Settings.Default.cursor;
             Delta delta = await api.GetDeltaAsync(cur);
             List<string> newFiles = new List<string>();

             updateExistingFiles(selectedFolder, newFiles ,delta);

             if(delta.entries.Count()>0)
             {
                 delta = await api.GetDeltaAsync(cur);
                 cur = delta.cursor;
                 Properties.Settings.Default.cursor = cur;
                 Properties.Settings.Default.Save();

                 foreach (var file in delta.entries)
                 {
                     if (file.Item2 != null)
                     {
                         if (file.Item2.is_dir == false)
                         {
                             var folder = await client.downloadFileAsync(api, file.Item2.path.TrimStart('/'));
                             watcher.EnableRaisingEvents = false;
                             folder.Save(selectedFolder + file.Item2.path.ToString().Replace('/', '\\'));
                             watcher.EnableRaisingEvents = true;
                             newFiles.Add(selectedFolder + file.Item2.path.ToString().Replace('/', '\\'));
                         }
                         else
                         {
                             Directory.CreateDirectory(selectedFolder + file.Item2.path);
                         }
                     }
                     else
                     {
                         if (!isFolder(file.Item1.ToString()))
                         {
                             if (System.IO.File.Exists(selectedFolder + file.Item1.ToString().Replace('/', '\\')))
                             {
                                 watcher.EnableRaisingEvents = false;
                                 try
                                 {
                                     System.IO.File.Delete(selectedFolder + file.Item1.ToString().Replace('/', '\\').TrimStart('/'));
                                 }
                                 catch (Exception e) { }
                                 watcher.EnableRaisingEvents = true;
                                 newFiles.Add(selectedFolder + file.Item1.ToString().Replace('/', '\\'));
                             }
                         }
                         else
                         {
                             if (System.IO.Directory.Exists(selectedFolder + file.Item1.ToString().Replace('/', '\\')))
                             {
                                 watcher.EnableRaisingEvents = false;
                                 try
                                 {
                                     System.IO.Directory.Delete(selectedFolder + file.Item1.ToString().Replace('/', '\\'));
                                 }
                                 catch (Exception e) { }
                                 watcher.EnableRaisingEvents = true;
                             }
                         }
                     }
                 }                     
             }
             uploadNewFiles(selectedFolder, newFiles);
         }

        private async void synchronizationAsync(DropboxApi api)
        {                    
            string cur = Properties.Settings.Default.cursor;
            Delta delta = await api.GetDeltaAsync(cur);
           
            while (true)
            {
                LongPollDelta lpDelta = await api.GetLongPollDeltaAsync(cur);

                if (lpDelta.changes)
                {
                    updateState(false);
                    delta = await api.GetDeltaAsync(cur);
                    cur = delta.cursor;
                    Properties.Settings.Default.cursor = cur;
                    Properties.Settings.Default.Save();

                    foreach (var file in delta.entries)
                    {
                       // if (lastFile != file.Item1.TrimStart('/'))
                        {
                            if (file.Item2 != null)
                            {
                                if (file.Item2.is_dir == false)
                                {
                                    var folder = await client.downloadFileAsync(api, file.Item2.path.TrimStart('/'));
                                    watcher.EnableRaisingEvents = false;
                                    folder.Save(selectedFolder + file.Item2.path.ToString().Replace('/', '\\'));
                                    watcher.EnableRaisingEvents = true;
                                }
                                else
                                {
                                    Directory.CreateDirectory(selectedFolder + file.Item2.path);
                                }
                            }
                            else
                            {
                                if (!isFolder(file.Item1.ToString()))
                                {
                                    if (System.IO.File.Exists(selectedFolder + file.Item1.ToString().Replace('/', '\\')))
                                    {
                                        watcher.EnableRaisingEvents = false;
                                        try
                                        {
                                            System.IO.File.Delete(selectedFolder + file.Item1.ToString().Replace('/', '\\'));
                                        }
                                        catch (Exception e) { }
                                        watcher.EnableRaisingEvents = true;
                                    }
                                }
                                else
                                {
                                    if (System.IO.Directory.Exists(selectedFolder + file.Item1.ToString().Replace('/', '\\')))
                                    {
                                        watcher.EnableRaisingEvents = false;
                                        try
                                        {
                                            System.IO.Directory.Delete(selectedFolder + file.Item1.ToString().Replace('/', '\\'));
                                        }
                                        catch (Exception e) { }
                                        watcher.EnableRaisingEvents = true;
                                    }
                                }
                            }
                        }
                    }

                    updateState(true);
                }
                else
                {
                    Properties.Settings.Default.cursor = cur;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private async void getInfoUser(DropboxApi api)
        {
            var account = await client.getInfoAccountAsync(api);
            diskSpace.Series[0].Points.Clear();
            diskSpace.Series[0].Points.AddXY(0, account.Quota.Normal);
            diskSpace.Series[0].Points[0].LegendText = "Занято " +  account.Quota.Normal/1048576 + " Мб";
            diskSpace.Series[0].Points.AddXY(0, account.Quota.Total - account.Quota.Normal);
            diskSpace.Series[0].Points[1].LegendText = "Свободно " + (account.Quota.Total - account.Quota.Normal) / 1048576 + " Мб"; ;
        }

       
        private async void getAllFiles(DropboxApi api, string selectedFolder, string path)
        {
            List<string> tmp = new List<string>();
            var files = await client.viewFilesAsync(api, path.TrimStart('/'));
            foreach (var file in files.Contents)
            {
                string filename = selectedFolder + file.Path.ToString().Replace('/', '\\');

                if (isFolder(filename) == false)
                {
                    if (System.IO.File.Exists(filename) == false)
                    {
                        var folder = await client.downloadFileAsync(api, file.Path.ToString());
                        watcher.EnableRaisingEvents = false;
                        folder.Save(selectedFolder + file.Path.ToString().Replace('/', '\\'));
                        watcher.EnableRaisingEvents = true;
                    }
                }
                else
                {
                    if (Directory.Exists(filename) == false)
                    {
                        Directory.CreateDirectory(filename);
                    }

                    string root = filename.Substring(selectedFolder.Length + 1);
                    getAllFiles(api, selectedFolder, root);
                }

            }
           
        }

        private bool GetAccesToken()
        {

            var login = new DropboxLogin("isttjzta1kup2jm", "bjb5z1h9nxikgps");
            login.Owner = this;
            login.ShowDialog();

            if (login.IsSuccessfully)
            {
                Properties.Settings.Default.AccessToken = login.AccessToken.Value;
                Properties.Settings.Default.Save();
                return true;
            }

            Properties.Settings.Default.AccessToken = "";
            Properties.Settings.Default.selecFolder = "";
            Properties.Settings.Default.cursor = "";
            Properties.Settings.Default.Save();
            return false;
            

        }

        private async void chooseFolder()
        {

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description= "Выберите синхронизируемую папку";
            dialog.ShowNewFolderButton = true;
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                selectedFolder = dialog.SelectedPath;

                Properties.Settings.Default.selecFolder = selectedFolder;
                Properties.Settings.Default.Save();

                if (System.IO.Directory.GetDirectories(selectedFolder).Length + System.IO.Directory.GetFiles(selectedFolder).Length == 0)
                {
                    updateState(false);
                    getAllFiles(api, selectedFolder, "");
                    updateState(true);
                }
                else
                {
                    updateState(false);
                    getStartedSynchronizationAsync(api);
                    updateState(true);
                }

                synchronizationAsync(api);
                createWatcher(selectedFolder);
            }
        }

        private async void chooseFolder(object sender, EventArgs e)
        {
            chooseFolder();
        }
       
       
        

        private async void deleteOldFiles(string folder, List<string> newFiles)
        {

            var dir = new DirectoryInfo(folder);
            var fold = await client.viewFilesAsync(api, "");
            bool flagExistence;

            foreach (var obj in fold.Contents)
            {
                flagExistence = false;
                try
                {
                    foreach (System.IO.FileSystemInfo file in dir.GetFileSystemInfos())
                    {
                        if (obj.Path == file.FullName.Substring(selectedFolder.Length).Replace('\\', '/'))
                        {
                            flagExistence = true;
                        }
                    }
                }
                catch (Exception ex) { }

                if (flagExistence == false)
                {
                    try
                    {
                        if (newFiles.Contains(obj.Path) == false)
                        {
                            var fil = await client.deleteAsync(api, obj.Path);
                        }
                    }
                    catch (Exception ex) { }
                }

            }
            updateState(true);
        }

        private bool fileContains(Delta delta, string nameFile)
        {
            foreach (var file in delta.entries)
            {
                if (string.Compare(file.Item1,nameFile,true)==0)
                {
                    return true;
                }

            }

            return false;
        }

        private async void updateExistingFiles(string folder, List<string> newFiles, Delta delta)
        {
            List<string> names = new List<string>();
            loadTimeModifieldFile("spiceCloud.spcd", ref names);

            for (int i = 0; i < names.Count; i++)
            {
                string[] Directories = names[i].Split('|');
                string name = Directories.First(); 

                if (System.IO.File.Exists(name))
                {
                    if (Directories.Last() != System.IO.File.GetLastWriteTime(name).ToString())
                    {
                        name = Directories.First().Substring(selectedFolder.Length + 1); 

                        if(fileContains(delta,"/"+ name))
                        {                            
                            try
                            {
                                var fil = await client.addFileAsync(api, "Конфликтная версия " + name, Directories.First());
                            }
                            catch (Exception ex) { }
                        }
                        else
                        {
                            try
                            {
                                 var fil = await client.addFileAsync(api, name, Directories.First());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
            }
        }

        private async void uploadNewFiles(string folder, List<string> newFiles)
        {
            var dir = new DirectoryInfo(folder);
            string root = folder.Substring(selectedFolder.Length).Replace('\\', '/');
            var fold = await client.viewFilesAsync(api, root.TrimStart('/'));            
            bool flagExistence;

            foreach (System.IO.FileSystemInfo file in dir.GetFileSystemInfos())
            {
                string name = file.FullName.Substring(selectedFolder.Length).Replace('\\','/');
                flagExistence = false;
                foreach (var obj in fold.Contents)
                {             
                    if (obj.Path == name)
                    {                    
                        flagExistence = true;
                    }
                }

                if (flagExistence == false)
                {
                    if (isFolder(name))
                    {
                        try
                        {
                            var fol = await client.createFolderAsync(api, name);
                            uploadNewFiles(file.FullName, newFiles);
                        }
                        catch (Exception ex) { }
                    }
                    else
                    {
                        try
                        {                            
                            var fil = await client.addFileAsync(api, name, file.FullName);
                        }
                        catch (Exception ex) { }
                    }

                }
                else
                {
                    if (isFolder(name))
                    {
                        uploadNewFiles(file.FullName, newFiles);
                    }
                }
                
           }

            deleteOldFiles(selectedFolder, newFiles);  
       }


        private void saveTimeModifieldFile(string nameFile,  string folder)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(nameFile))
            {

                writeTimeFile(file, folder);
            }                   
        }

        private void loadTimeModifieldFile(string nameFile, ref List<string> names)
        {
            string line;
            if (System.IO.File.Exists(nameFile))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(nameFile))
                {

                    while ((line = file.ReadLine()) != null)
                    {
                        names.Add(line);
                    }
                }
            }
        }

        private void writeTimeFile(System.IO.StreamWriter file, string folder)
        {
            var dir = new DirectoryInfo(folder);

            try
            {
                foreach (System.IO.FileSystemInfo obj in dir.GetFileSystemInfos())
                {
                    if (!isFolder(obj.FullName))
                    {
                        string nameTimeFile = obj.FullName + "|" + obj.LastWriteTime;
                        file.WriteLine(nameTimeFile);
                    }
                    else
                    {
                        writeTimeFile(file, obj.FullName);
                    }
                }
            }
            catch (Exception e) { }
        }

        private async void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Delta delta1 = api.GetDelta(curs);
            curs = delta1.cursor;
            Properties.Settings.Default.cursor = curs;
            Properties.Settings.Default.Save();
            saveTimeModifieldFile("spiceCloud.spcd", selectedFolder);           
        }

        //Получить имя файла без пути
        private string extractFileName(string path)
        {
            string[] Directories = path.Split('\\');
            Directories.Last();
            return Directories.Last();
        }

        private bool isFolder(string path)
        {
            if (path.Contains('.'))
                return false;
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Properties.Settings.Default.AccessToken = "";
            Properties.Settings.Default.cursor = "";
            Properties.Settings.Default.selecFolder = "";
            Properties.Settings.Default.Save();
            if(this.GetAccesToken())
                chooseFolder();
            
        }

        void updateState(bool flagState)
        {
            if (flagState)
            {
                pictureBoxState.Image = Properties.Resources.OK;
                labelState.Text = "Обновлено";
                notifyIconName.Icon = Properties.Resources.ok2;
                notifyIconName.Text = "Обновлено";

            }
            else
            {
                pictureBoxState.Image = Properties.Resources.sync;
                labelState.Text = "Идет синхронизация";
                notifyIconName.Icon = Properties.Resources.sync2;
                notifyIconName.Text = "Идет синхронизация";
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIconName.Visible = false;
                getInfoUser(api);
            }
        }

        

        private void MainWindow_Resize_1(object sender, EventArgs e)
        {
            ShowInTaskbar = !(this.WindowState == FormWindowState.Minimized);
            notifyIconName.Visible = this.WindowState == FormWindowState.Minimized;
        }          
    }
}
