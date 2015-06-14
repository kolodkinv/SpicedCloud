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
        FileSystemWatcher watcher;

        private string curs;
        private string selectedFolder;
        List<string> selectedFolders;
        string lastFile;

        public MainWindow()
        {
            InitializeComponent();
            selectedFolder = "";
            lastFile = "";
            selectedFolders = new List<string>();
            curs = Properties.Settings.Default.cursor;
            client = new DropboxClient();
            
            string folderUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);


            if (System.IO.Directory.Exists(folderUser + "\\" + "Синхронизируемая папка") == false)
            {
                System.IO.Directory.CreateDirectory(folderUser + "\\" + "Синхронизируемая папка");         
            }
          
            if(Properties.Settings.Default.selectedFolders==null)
            {
                System.Collections.Specialized.StringCollection folders = new System.Collections.Specialized.StringCollection();
                folders.Add(folderUser + "\\" + "Синхронизируемая папка");
                Properties.Settings.Default.selectedFolders = folders;
                Properties.Settings.Default.Save();
            }

            Properties.Settings.Default.selecFolder = folderUser + "\\" + "Синхронизируемая папка";       
            Properties.Settings.Default.Save();
            selectedFolder = folderUser + "\\" + "Синхронизируемая папка";
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

            //moveFileInFolderAsync(api, "/Синхронизируемая папка");

            WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIconName.Visible = true;

            /*if (Properties.Settings.Default.cursorsFolders != null)
            {
                for (int i = 0; i < Properties.Settings.Default.cursorsFolders.Count; i++)
                {
                    folders.Add(Properties.Settings.Default.cursorsFolders[i]);
                }
            }
            else
            {
                folders.Add(Properties.Settings.Default.cursor);
            }*/

            Properties.Settings.Default.cursorsFolders = null;
            if (Properties.Settings.Default.cursorsFolders == null)
            {
                System.Collections.Specialized.StringCollection cF = new System.Collections.Specialized.StringCollection();
                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    cF.Add("");
                }
                Properties.Settings.Default.cursorsFolders = cF;
                Properties.Settings.Default.Save();
            }



            if (System.IO.Directory.Exists(Properties.Settings.Default.selecFolder))
            {
                if (Properties.Settings.Default.fl == null)
                {
                    Properties.Settings.Default.fl = "access";
                    Properties.Settings.Default.Save();
                    updateState(false);
                    getAllFiles(api, selectedFolder, "/" + selectedFolder.Split('\\').Last());
                    updateState(true);
                }
                else
                {

                    createNeedFolders(api, Properties.Settings.Default.selectedFolders);

                    for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                    {
                        selectedFolders.Add(Properties.Settings.Default.selectedFolders[i]);
                        getStartedSynchronizationAsync(api, selectedFolders[i], i);
                        synchronizationAsync(api, selectedFolders[i], i);
                        createWatcher(selectedFolders[i]);
                    }
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(folderUser + "\\" + "Синхронизируемая папка");
                updateState(false);
                getAllFiles(api, selectedFolder, "/" + selectedFolder.Split('\\').Last());
                updateState(true);
            }

        }



        #region watcher
        private void createWatcher(string observedFolder)
        {
            watcher = new FileSystemWatcher(observedFolder);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watcher.NotifyFilter =  /*NotifyFilters.LastAccess |*/ NotifyFilters.LastWrite |
                                    NotifyFilters.FileName | NotifyFilters.DirectoryName;


            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
            watcher.Deleted += new FileSystemEventHandler(wathcer_Deleted);
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);

        }

        async void deletedAsync(FileSystemEventArgs e)
        {
            try
            {

                string selectFolder = "";

                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    if (e.FullPath.Contains(Properties.Settings.Default.selectedFolders[i]))
                    {
                        selectFolder = Properties.Settings.Default.selectedFolders[i];
                    }
                }

                string name = e.FullPath.Substring(selectFolder.Length + 1);
                string[] Directories = selectFolder.Split('\\');
                string nameFolder = Directories.Last();

                var obj = await client.deleteAsync(api, nameFolder + '/' + name);
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
                string selectFolder = "";

                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    if (e.FullPath.Contains(Properties.Settings.Default.selectedFolders[i]))
                    {
                        selectFolder = Properties.Settings.Default.selectedFolders[i];
                    }
                }

                string name = e.FullPath.Substring(selectFolder.Length + 1);

                string[] Directories = selectFolder.Split('\\');
                string nameFolder = Directories.Last();

                if (isFolder(name))
                {
                    if (System.IO.Directory.Exists(e.FullPath))
                    {

                        var folder = await client.createFolderAsync(api, nameFolder + '/' + name);
                    }
                }
                else
                {
                    if (System.IO.File.Exists(e.FullPath))
                    {
                        var folder = await client.addFileAsync(api, nameFolder + '/' + name, e.FullPath);
                    }
                }

            }
            catch (Exception ex) { }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (lastFile != e.Name + DateTime.Now.ToString())
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

                string selectFolder = "";

                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    if (e.FullPath.Contains(Properties.Settings.Default.selectedFolders[i]))
                    {
                        selectFolder = Properties.Settings.Default.selectedFolders[i];
                    }
                }

                string name = e.FullPath.Substring(selectFolder.Length + 1);

                string[] Directories = selectFolder.Split('\\');
                string nameFolder = Directories.Last();



                if (isFolder(name))
                {
                    var folder = await client.createFolderAsync(api, nameFolder + '/' + name);
                }
                else
                {

                    var file = await client.addFileAsync(api, nameFolder + '/' + name, e.FullPath);
                }
            }
            catch (Exception ex) { }
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
                string selectFolder = "";

                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    if (e.FullPath.Contains(Properties.Settings.Default.selectedFolders[i]))
                    {
                        selectFolder = Properties.Settings.Default.selectedFolders[i];
                    }
                }

                string name = e.FullPath.Substring(selectFolder.Length + 1);
                string[] Directories = selectFolder.Split('\\');
                string nameFolder = Directories.Last();

                var folder = await client.moveAsync(api, nameFolder + '/' + e.OldFullPath.Substring(selectFolder.Length + 1), nameFolder + '/' + name);
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

        private async void createNeedFolders(DropboxApi api,System.Collections.Specialized.StringCollection buf)
        {
            var files = await client.viewFilesAsync(api, "");
            bool flag = false;

            for (int i = 0; i < buf.Count; i++)           
            {
                string tmp = "/" + buf[i].Split('\\').Last();

                foreach (var file in files.Contents)
                {

                    
                    if (file.Path == tmp)
                    {
                        flag = true;
                    }             
                }

                if (flag == false)
                {
                    var fol = await client.createFolderAsync(api, tmp/*"/" + buf[i].Split('\\').Last()*/ /*nameFolder + "/" + name*/);
                }

                flag = false;
            }
        }

        private async void moveFileInFolderAsync(DropboxApi api, string folder)
        {
            var files = await client.viewFilesAsync(api, "");

            foreach (var file in files.Contents)
            {
                if (file.Path != "/Синхронизируемая папка")
                {
                    var obj = await client.moveAsync(api, file.Path, folder + file.Path);
                }
            }
        }

        private async void getStartedSynchronizationAsync(DropboxApi api, string syncFolder, int indexCursor)
        {
            updateState(false);
            string nameSyncFolder = "/" + syncFolder.Split('\\').Last();

            string cur = Properties.Settings.Default.cursorsFolders[indexCursor]/*Properties.Settings.Default.cursor*/;
            Delta delta;
            if (cur != "")
            {
                delta = await api.GetDeltaAsync(cur, nameSyncFolder);
            }
            else
            {
                delta = await api.GetDeltaAsync(nameSyncFolder);
            }
            List<string> newFiles = new List<string>();

            //updateExistingFiles(selectedFolder, newFiles ,delta);

            if (delta.entries.Count() > 0)
            {
                delta = await api.GetDeltaAsync(cur, nameSyncFolder /*"/Синхронизируемая папка"*/);
                cur = delta.cursor;
                //Properties.Settings.Default.cursor = cur;
                Properties.Settings.Default.cursorsFolders[indexCursor] = cur;
                Properties.Settings.Default.Save();
                string[] Directories = syncFolder.Split('\\');

                foreach (var file in delta.entries)
                {

                    if (file.Item2 != null)
                    {
                        if (file.Item2.is_dir == false)
                        {
                            string nameFile = Directories.Last();
                            nameFile = file.Item2.path.Substring(nameFile.Length + 1).Replace('/', '\\');
                            var folder = await client.downloadFileAsync(api, file.Item2.path.TrimStart('/'));
                            watcher.EnableRaisingEvents = false;
                            folder.Save(syncFolder + nameFile);
                            watcher.EnableRaisingEvents = true;
                            newFiles.Add(syncFolder + nameFile);
                        }
                        else
                        {
                            string nameFolder = Directories.Last();
                            nameFolder = file.Item2.path.Substring(nameFolder.Length + 1).Replace('/', '\\');
                            Directory.CreateDirectory(syncFolder + nameFolder);
                        }
                    }
                    else
                    {
                        string nameFile = Directories.Last();
                        nameFile = file.Item1.Substring(nameFile.Length + 1).Replace('/', '\\');

                        if (!isFolder(file.Item1.ToString()))
                        {
                            if (System.IO.File.Exists(syncFolder + nameFile))
                            {
                                watcher.EnableRaisingEvents = false;
                                try
                                {
                                    System.IO.File.Delete(syncFolder + nameFile);
                                }
                                catch (Exception e) { }
                                watcher.EnableRaisingEvents = true;
                            }
                        }
                        else
                        {
                            if (System.IO.Directory.Exists(syncFolder + nameFile))
                            {
                                watcher.EnableRaisingEvents = false;
                                try
                                {
                                    System.IO.Directory.Delete(syncFolder + nameFile);
                                }
                                catch (Exception e) { }
                                watcher.EnableRaisingEvents = true;
                            }
                        }
                    }
                }
            }

            uploadNewFiles(syncFolder, newFiles, indexCursor);
        }

        /// <summary>
        /// Синхронизация с сервера на клиент
        /// </summary>
        /// <param name="api">DropboxApi api</param>
        /// <param name="syncFolder">Синхронизируемая папка. Полный путь</param>
        private async void synchronizationAsync(DropboxApi api, string syncFolder, int indexCursor)
        {
            string cur = Properties.Settings.Default.cursorsFolders[indexCursor]/*Properties.Settings.Default.cursor*/;
            string nameSyncFolder = "/" + syncFolder.Split('\\').Last();

            Delta delta = await api.GetDeltaAsync(cur, nameSyncFolder);
            cur = delta.cursor;
            while (true)
            {
                LongPollDelta lpDelta = await api.GetLongPollDeltaAsync(cur);

                if (lpDelta.changes)
                {
                    updateState(false);
                    delta = await api.GetDeltaAsync(cur, nameSyncFolder);
                    cur = delta.cursor;
                    //Properties.Settings.Default.cursor = cur;
                    Properties.Settings.Default.cursorsFolders[indexCursor] = cur;
                    Properties.Settings.Default.Save();

                    foreach (var file in delta.entries)
                    {
                        {
                            string[] Directories = syncFolder.Split('\\');
                            if (file.Item2 != null)
                            {
                                if (file.Item2.is_dir == false)
                                {
                                    string nameFile = Directories.Last();
                                    nameFile = file.Item2.path.Substring(nameFile.Length + 1).Replace('/', '\\');
                                    var folder = await client.downloadFileAsync(api, file.Item2.path.TrimStart('/'));
                                    watcher.EnableRaisingEvents = false;
                                    folder.Save(syncFolder + nameFile);
                                    watcher.EnableRaisingEvents = true;
                                }
                                else
                                {
                                    string nameFolder = Directories.Last();
                                    nameFolder = file.Item2.path.Substring(nameFolder.Length + 1).Replace('/', '\\');
                                    Directory.CreateDirectory(syncFolder + nameFolder);
                                }
                            }
                            else
                            {
                                string nameFile = Directories.Last();
                                nameFile = file.Item1.Substring(nameFile.Length + 1).Replace('/', '\\');

                                if (!isFolder(file.Item1.ToString()))
                                {
                                    if (System.IO.File.Exists(syncFolder + nameFile))
                                    {
                                        watcher.EnableRaisingEvents = false;
                                        try
                                        {
                                            System.IO.File.Delete(syncFolder + nameFile);
                                        }
                                        catch (Exception e) { }
                                        watcher.EnableRaisingEvents = true;
                                    }
                                }
                                else
                                {
                                    if (System.IO.Directory.Exists(syncFolder + nameFile))
                                    {
                                        watcher.EnableRaisingEvents = false;
                                        try
                                        {
                                            System.IO.Directory.Delete(syncFolder + nameFile);
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
                    //Properties.Settings.Default.cursor = cur;
                    Properties.Settings.Default.cursorsFolders[indexCursor] = cur;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private async void getInfoUser(DropboxApi api)
        {
            var account = await client.getInfoAccountAsync(api);
            diskSpace.Series[0].Points.Clear();
            diskSpace.Series[0].Points.AddXY(0, account.Quota.Normal);
            diskSpace.Series[0].Points[0].LegendText = "Занято " + account.Quota.Normal / 1048576 + " Мб";
            diskSpace.Series[0].Points.AddXY(0, account.Quota.Total - account.Quota.Normal);
            diskSpace.Series[0].Points[1].LegendText = "Свободно " + (account.Quota.Total - account.Quota.Normal) / 1048576 + " Мб";
            label1.Text = account.DisplayName;
        }

        private async void getAllFiles(DropboxApi api, string syncFolder, string path)
        {

            List<string> tmp = new List<string>();
            var files = await client.viewFilesAsync(api, path);
            foreach (var file in files.Contents)
            {
                string syncFile = "";
                for (int i = 0; i < file.Path.Split('/').Count(); i++)
                {
                    if (i != 1 && file.Path.Split('/').ElementAt(i) != "")
                    {
                        syncFile += "\\" + file.Path.Split('/').ElementAt(i);
                    }
                }

                string filename = selectedFolder + syncFile;
                if (isFolder(filename) == false)
                {
                    if (System.IO.File.Exists(filename) == false)
                    {
                        var folder = await client.downloadFileAsync(api, file.Path.ToString());
                        watcher.EnableRaisingEvents = false;
                        string nameNewFile = syncFolder + file.Path.ToString().Replace('/', '\\').Substring(path.Length);
                        folder.Save(filename);
                        watcher.EnableRaisingEvents = true;
                    }
                }
                else
                {
                    if (Directory.Exists(filename) == false)
                    {
                        Directory.CreateDirectory(filename);
                    }
                    string root = filename.Substring(syncFolder.Length + 1);
                    getAllFiles(api, syncFolder, path.Replace('\\', '/') + "/" + root.Replace('\\', '/').Split('/').Last());
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

        private void chooseFolder(object sender, EventArgs e)
        {

            FolderWindow form = new FolderWindow();
            form.ShowDialog();

            createNeedFolders(api, Properties.Settings.Default.selectedFolders);

            for (int i = 0; i < FolderWindow.newFolders.Count; i++)
            {
                int index = Properties.Settings.Default.selectedFolders.IndexOf(FolderWindow.newFolders[i]);

                selectedFolders.Add(Properties.Settings.Default.selectedFolders[index]);

                getStartedSynchronizationAsync(api, Properties.Settings.Default.selectedFolders[index], index);
                synchronizationAsync(api, Properties.Settings.Default.selectedFolders[index], index);
                createWatcher(Properties.Settings.Default.selectedFolders[index]);
            }
        }

        private async void deleteOldFiles(string folder, List<string> newFiles, int indexCursor)
        {
            var dir = new DirectoryInfo(folder);
            string root = folder.Substring(selectedFolders[indexCursor].Length).Replace('\\', '/');
            string nameFolder = folder.Split('\\').Last();
            var fold = await client.viewFilesAsync(api, nameFolder + root);
            bool flagExistence;

            foreach (var obj in fold.Contents)
            {
                flagExistence = false;
                try
                {
                    foreach (System.IO.FileSystemInfo file in dir.GetFileSystemInfos())
                    {
                        string fullNameFile = "/" + nameFolder + file.FullName.Substring(selectedFolders[indexCursor].Length).Replace('\\', '/');
                        if (obj.Path == fullNameFile)
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
                if (string.Compare(file.Item1, nameFile, true) == 0)
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

                        if (fileContains(delta, "/" + name))
                        {
                            try
                            {
                                var fil = await client.addFileAsync(api, selectedFolder.Split('\\').Last() + "/Конфликтная версия " + name, Directories.First());
                            }
                            catch (Exception ex) { }
                        }
                        else
                        {
                            try
                            {
                                var fil = await client.addFileAsync(api, selectedFolder.Split('\\').Last() + "/" + name, Directories.First());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
            }
        }

        private async void uploadNewFiles(string folder, List<string> newFiles, int indexCursor)
        {
            var dir = new DirectoryInfo(folder);
            string root = folder.Substring(selectedFolders[indexCursor].Length).Replace('\\', '/');
            string nameFolder = folder.Split('\\').Last();
            var fold = await client.viewFilesAsync(api, nameFolder /*+ root*/);
            bool flagExistence;

            foreach (System.IO.FileSystemInfo file in dir.GetFileSystemInfos())
            {
                string name = file.FullName.Substring(selectedFolders[indexCursor].Length).Replace('\\', '/');
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
                            var fol = await client.createFolderAsync(api, nameFolder + "/" + name);
                            uploadNewFiles(file.FullName, newFiles, indexCursor);
                        }
                        catch (Exception ex) { }
                    }
                    else
                    {
                        try
                        {
                            var fil = await client.addFileAsync(api, nameFolder + "/" + name, file.FullName);
                        }
                        catch (Exception ex) { }
                    }
                }
                else
                {
                    if (isFolder(name))
                    {
                        uploadNewFiles(file.FullName, newFiles, indexCursor);
                    }
                }
            }
            deleteOldFiles(selectedFolders[indexCursor], newFiles, indexCursor);
        }


        private void saveTimeModifieldFile(string nameFile, string folder)
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
            Delta delta1 = await api.GetDeltaAsync(curs, "/Синхронизируемая папка");
            curs = delta1.cursor;
            /*Properties.Settings.Default.cursor = curs;
            Properties.Settings.Default.Save();
            saveTimeModifieldFile("spiceCloud.spcd", selectedFolder); */
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
            //if (this.GetAccesToken())
                //chooseFolder();
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

        private void button1_Click(object sender, EventArgs e)
        {
            FilesWindow form = new FilesWindow();
            form.ShowDialog();
        } 
    }
}
