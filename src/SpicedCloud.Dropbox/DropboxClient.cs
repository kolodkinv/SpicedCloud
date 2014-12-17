using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpicedCloud;
using Dropbox.Api;
using System.Threading;
using System.Web;
using System.IO;
using System.Diagnostics;

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

        public DropboxApi connectDropbox()
        {
            //var accessToken = new OAuthToken("6qa3xfuo1eg0xja1", "ji8ufxlb13s92ib");

            var accessToken = GetAccessToken();
            var api = new DropboxApi(ConsumerKey, ConsumerSecret, accessToken);

            return api;
        }

        public Account getInfoAccount(DropboxApi api)
        {
            return api.GetAccountInfo();
        }

        public FileSystemInfo createFolder(DropboxApi api, string name)
        {
            return api.CreateFolder("dropbox", "/" + name);
        }

        public FileSystemInfo delete(DropboxApi api, string name)
        {
            return api.Delete("dropbox", "/" + name);
        }

        public FileSystemInfo addFile(DropboxApi api, string name, string path)
        {
            return api.UploadFile("dropbox", name, path);
        }

        public FileSystemInfo downloadFile(DropboxApi api, string name)
        {
            return api.DownloadFile("dropbox", name);
        }

        public File viewFiles(DropboxApi api, string name)
        {
            return api.GetFiles("dropbox", name);
        }



    }
}
