using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpicedCloud;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dropbox.Api
{
    public class DropboxApi
    {
        private readonly string _accessToken;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        public DropboxApi(string consumerKey, string consumerSecret, string accessToken)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
        }

        #region Синхронные функции
        private string GetResponse(Uri uri)
        {           
            var oauth = new OAuth();
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        private string GetResponse(string action, string parameters)
        {
            var oauth = new OAuth();
            var Uri = new Uri(new Uri(DropboxInfoApi.BaseUri), action);
            var requestUri = oauth.SignRequestOAuth2(Uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        public Account GetAccountInfo()
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "account/info");
            var json = GetResponse(uri);
            return ParseJson<Account>(json);
        }

        public File GetFiles(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), String.Format("metadata/{0}/{1}", root, path));
            var json = GetResponse(uri);
            return ParseJson<File>(json);
        }

        public FileSystemInfo CreateFolder(string root, string path)
        {
            var json = GetResponse("fileops/create_folder", "root=" + root + "&path=" + UpperCaseUrlEncode(path));
            return ParseJson<FileSystemInfo>(json);
        }

        public FileSystemInfo Move(string root, string fromPath, string toPath)
        {
            var json = GetResponse("fileops/move",
                "root=" + root +
                "&from_path=" + UpperCaseUrlEncode(fromPath) +
                "&to_path=" + UpperCaseUrlEncode(toPath));
            return ParseJson<FileSystemInfo>(json);
        }

        public FileSystemInfo Delete(string root, string path)
        {
            var json = GetResponse("fileops/delete", "root=" + root + "&path=" + UpperCaseUrlEncode(path));

            return ParseJson<FileSystemInfo>(json);
        }

        public LongPollDelta GetLongPollDelta(string cursor)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiNotifyServer), "longpoll_delta");
            var oauth = new OAuth();

            string parameters = "cursor=" + cursor;
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = true;
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return ParseJson<LongPollDelta>(reader.ReadToEnd());
        }

        public Delta GetDelta(string cursor)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "delta");
            var oauth = new OAuth();

            string parameters = "cursor=" + cursor;
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());

            return ParseJson<Delta>(reader.ReadToEnd());
        }

        public FileSystemInfo UploadFile(string root, string path, string file)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiContentServer),
                String.Format("files_put/{0}/{1}",
                root, UpperCaseUrlEncode(path)));

            var oauth = new OAuth();

            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Put;
            request.KeepAlive = true;

            byte[] buffer;
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                int length = (int)fileStream.Length;
                buffer = new byte[length];
                fileStream.Read(buffer, 0, length);
            }

            request.ContentLength = buffer.Length;
            using (var requestStream = request.GetRequestStream())
            {

                requestStream.Write(buffer, 0, buffer.Length);
            }

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();
            return ParseJson<FileSystemInfo>(json);
        }

        public FileSystemInfo DownloadFile(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiContentServer),
                String.Format("files/auto/{0}", UpperCaseUrlEncode(path)));

            var oauth = new OAuth();

            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var response = request.GetResponse();

            var metadata = response.Headers["x-dropbox-metadata"];
            var file = ParseJson<FileSystemInfo>(metadata);

            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);
                } while (bytesRead > 0);

                file.Data = memoryStream.ToArray();
            }

            return file;
        }
        #endregion

        #region Асинхронные функции
        private async Task<string> GetResponseAsync(Uri uri)
        {
            var oauth = new OAuth();
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;

            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return responseStream.ReadToEnd();
        }

        private async Task<string> GetResponseAsyncPost(Uri uri)
        {
            var oauth = new OAuth();
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Post;

            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return responseStream.ReadToEnd();
        }

        private async Task<string> GetResponseAsync(string action, string parameters)
        {

            var oauth = new OAuth();
            var Uri = new Uri(new Uri(DropboxInfoApi.BaseUri), action);
            var requestUri = oauth.SignRequestOAuth2(Uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            
            
            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            
            return responseStream.ReadToEnd();
        }
      
        public async Task<Account> GetAccountInfoAsync()
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "account/info");
            var json = await GetResponseAsync(uri);
            return ParseJson<Account>(json);
        }
        
        public async Task<File> GetFilesAsync(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), String.Format("metadata/{0}/{1}", root, path));
            var json = await GetResponseAsync(uri);
            return ParseJson<File>(json);
        }
             
        public async Task<FileSystemInfo> CreateFolderAsync(string root, string path)
        {
            var json = await GetResponseAsync("fileops/create_folder", "root=" + root + "&path=" + UpperCaseUrlEncode(path));
            return ParseJson<FileSystemInfo>(json);
        }

        public async Task<List<Revision>> GetRevisionAsync(string path)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), String.Format("revisions/auto/{0}", path));
            
            var json = await GetResponseAsync(uri);
            List<MetaData> tmp = new List<MetaData>();
            var jobject = JArray.Parse(json);
            return JsonConvert.DeserializeObject<List<Revision>>(jobject.ToString());
            
        }
        
        
        public async Task<FileSystemInfo> MoveAsync(string root, string fromPath, string toPath)
        {
            var json = await GetResponseAsync("fileops/move",
                "root=" + root +
                "&from_path=" + UpperCaseUrlEncode(fromPath) +
                "&to_path=" + UpperCaseUrlEncode(toPath));
            return ParseJson<FileSystemInfo>(json);
        }
       
        public async Task<FileSystemInfo> DeleteAsync(string root, string path)
        {
            var json = await GetResponseAsync("fileops/delete", "root=" + root + "&path=" + UpperCaseUrlEncode(path));

            return ParseJson<FileSystemInfo>(json);
        }

        public async Task<LongPollDelta> GetLongPollDeltaAsync(string cursor)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiNotifyServer), "longpoll_delta");
            var oauth = new OAuth();

            string parameters = "cursor=" + cursor;
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = true;

            var task = request.GetResponseAsync();         
            var res = (HttpWebResponse)await task;

            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return ParseJson<LongPollDelta>(responseStream.ReadToEnd());           
        }

        

        public async Task<CursorDelta> GetLatestDeltaAsync()
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "delta/latest_cursor");
            var json = await GetResponseAsyncPost(uri);
            return ParseJson<CursorDelta>(json);
        }
       
        public async Task<Delta> GetDeltaAsync(string cursor, string folder)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "delta");
            var oauth = new OAuth();

            string parameters = "cursor=" + cursor + "&path_prefix=" + folder;
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;

            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return ParseJson<Delta>(responseStream.ReadToEnd());
        }

        public async Task<Delta> GetDeltaAsync( string folder)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.BaseUri), "delta");
            var oauth = new OAuth();

            string parameters = "path_prefix=" + folder;
            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken, parameters);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;

            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return ParseJson<Delta>(responseStream.ReadToEnd());
        }

        public async Task<FileSystemInfo> UploadFileAsync(string root, string path, string file)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiContentServer),
                String.Format("files_put/{0}/{1}",
                root, UpperCaseUrlEncode(path)));

            
            
            var oauth = new OAuth();

            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Put;
            request.KeepAlive = true;

            byte[] buffer;
            //FileStream fileStream = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            while (true)
            {
                try
                {
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        int length = (int)fileStream.Length;
                        buffer = new byte[length];
                        fileStream.Read(buffer, 0, length);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
            

            request.ContentLength = buffer.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }

            var task = request.GetResponseAsync();
            var res = (HttpWebResponse)await task;
            StreamReader responseStream = new StreamReader(res.GetResponseStream());
            return ParseJson<FileSystemInfo>(responseStream.ReadToEnd());

        }
        
        public async Task<FileSystemInfo> DownloadFileAsync(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxInfoApi.ApiContentServer),
                String.Format("files/auto/{0}", UpperCaseUrlEncode(path)));

            var oauth = new OAuth();

            var requestUri = oauth.SignRequestOAuth2(uri, _accessToken);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var task = request.GetResponseAsync();
            var response = (HttpWebResponse)await task;

            var metadata = response.Headers["x-dropbox-metadata"];
            var file = ParseJson<FileSystemInfo>(metadata);

            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);
                } while (bytesRead > 0);

                file.Data = memoryStream.ToArray();
            }

            return file;
        }
#endregion

        #region Служебные функции
        private static T ParseJson<T>(string json) where T : class, new()
        {
            var jobject = JObject.Parse(json);
            return JsonConvert.DeserializeObject<T>(jobject.ToString());
        }
        
        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }

            var values = new Dictionary<string, string>()
            {
                { "+", "%20" },
                { "(", "%28" },
                { ")", "%29" }
            };

            var data = new StringBuilder(new string(temp));
            foreach (string character in values.Keys)
            {
                data.Replace(character, values[character]);
            }
            return data.ToString();
        }
#endregion
    }
}
