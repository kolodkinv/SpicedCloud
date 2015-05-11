using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpicedCloud;
using Dropbox.Api;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Web;
using Nemiro.OAuth;
using Nemiro.OAuth.LoginForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Dropbox.Api;
using Nemiro.OAuth;
using Nemiro.OAuth.LoginForms;
using System.Web;

namespace Dropbox.Api
{
    public class DropboxClient
    {
        private const string ConsumerKey = "z4my98dfq3znbui";
        private const string ConsumerSecret = "nxuqhcs1pg746u1";
              

        private static OAuthToken GetAccessToken()
        {
            var oauth = new OAuth();

            var requestToken = oauth.GetRequestToken(new Uri(DropboxInfoApi.BaseUri), ConsumerKey, ConsumerSecret);

            var authorizeUri = oauth.GetAuthorizeUri(new Uri(DropboxInfoApi.AuthorizeBaseUri), requestToken);
            Process.Start(authorizeUri.AbsoluteUri);
            Thread.Sleep(10000); //Ждем 10 секунд для разрешения подключения


            return oauth.GetAccessToken(new Uri(DropboxInfoApi.BaseUri), ConsumerKey, ConsumerSecret, requestToken);
        }

        public DropboxApi connectDropbox(string accesToken)
        {          
            var api = new DropboxApi(ConsumerKey, ConsumerSecret, accesToken);

            return api;
        }

        #region Синхронные функции
        public Account getInfoAccount(DropboxApi api)
        {
            return api.GetAccountInfo();
        }

        public FileSystemInfo createFolder(DropboxApi api, string name)
        {
            return api.CreateFolder("dropbox", "/" + name.Replace('\\', '/'));
        }

        public FileSystemInfo delete(DropboxApi api, string name)
        {
            return api.Delete("dropbox", "/" + name.Replace('\\', '/'));
        }

        public FileSystemInfo addFile(DropboxApi api, string name, string path)
        {
            string pathServer = "dropbox";
            if (name.LastIndexOf('\\') != -1)
            {
                pathServer += "/" + name.Substring(0, name.LastIndexOf('\\'));
                name = path.Split('\\').Last();
            }

            return api.UploadFile(pathServer, name, path);
        }

        public FileSystemInfo downloadFile(DropboxApi api, string name)
        {
            return api.DownloadFile("dropbox", name);
        }

        public File viewFiles(DropboxApi api, string name)
        {
            return api.GetFiles("dropbox", name);
        }
        #endregion

        #region Асинхронные функции
        public async Task<Account> getInfoAccountAsync(DropboxApi api)
        {
            Account account = await api.GetAccountInfoAsync();
            return account;
        }
       
        public async Task<FileSystemInfo> createFolderAsync(DropboxApi api, string name)
        {
            FileSystemInfo folder = await api.CreateFolderAsync("dropbox", "/" + name.Replace('\\', '/'));
            return folder;
        }

        public async Task<FileSystemInfo> moveAsync(DropboxApi api, string nameOld, string nameNew)
        {

            FileSystemInfo folder = await api.MoveAsync("dropbox", "/" + nameOld.Replace('\\', '/'), "/" + nameNew.Replace('\\', '/'));
            //FileSystemInfo folder = await api.CreateFolderAsync("dropbox", "/" + name.Replace('\\', '/'));
            return folder;
        }

        public async Task<FileSystemInfo> deleteAsync(DropboxApi api, string name)
        {
            FileSystemInfo file = await api.DeleteAsync("dropbox", "/" + name.Replace('\\', '/'));
            return file;
        }
       
        public async Task<FileSystemInfo> addFileAsync(DropboxApi api, string name, string path)
        {
            string pathServer = "auto";
            if (name.LastIndexOf('\\') != -1)
            {
                pathServer += "/" + name.Substring(0, name.LastIndexOf('\\'));
                name = path.Split('\\').Last();
            }

            FileSystemInfo file= await api.UploadFileAsync(pathServer, name, path);
            return file;
        }
        
        public async Task<FileSystemInfo> downloadFileAsync(DropboxApi api, string name)
        {
            FileSystemInfo file = await api.DownloadFileAsync("dropbox", name);
            return file;
        }
      
        public async  Task<File> viewFilesAsync(DropboxApi api, string name)
        {
            File file = await api.GetFilesAsync("auto", name);
            return file;
        }
        #endregion
    }
}
