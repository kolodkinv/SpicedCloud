using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SpicedCloud
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MetaData
    {
        [JsonProperty(PropertyName = "size")]
        public string size { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string hash { get; set; }

        [JsonProperty(PropertyName = "rev")]
        public string rev { get; set; }

        [JsonProperty(PropertyName = "thumb_exists")]
        public bool thumb_exists { get; set; }

        [JsonProperty(PropertyName = "bytes")]
        public long bytes { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public string modified { get; set; }

        [JsonProperty(PropertyName = "client_mtime")]
        public string client_mtime { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string path { get; set; }

        [JsonProperty(PropertyName = "is_dir")]
        public bool is_dir { get; set; }

        [JsonProperty(PropertyName = "is_deleted")]
        public bool is_deleted { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string icon { get; set; }

        [JsonProperty(PropertyName = "root")]
        public string root { get; set; }

        [JsonProperty(PropertyName = "mime_type")]
        public string mime_type { get; set; }

        [JsonProperty(PropertyName = "revision")]
        public int revision { get; set; }

        [JsonProperty(PropertyName = "photo_info")]
        public PhotoInfo photo_info { get; set; }

        [JsonProperty(PropertyName = "video_info")]
        public VideoInfo video_info { get; set; }

        [JsonProperty(PropertyName = "contents")]
        public List<MetaData> contents { get; set; }

        [JsonProperty(PropertyName = "shared_folder")]
        public SharedFolder shared_folder { get; set; }

        [JsonProperty(PropertyName = "read_only")]
        public bool read_only { get; set; }

        [JsonProperty(PropertyName = "parent_shared_folder_id")]
        public string parent_shared_folder_id { get; set; }

        [JsonProperty(PropertyName = "modifier")]
        public User modifier { get; set; }


        [JsonProperty(PropertyName = "Name")]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                {
                    return string.Empty;
                }

                if (path.LastIndexOf("/") == -1)
                {
                    return string.Empty;
                }

                return string.IsNullOrEmpty(path) ? "root" : path.Substring(path.LastIndexOf("/") + 1);
            }
        }

        [JsonProperty(PropertyName = "Extension")]
        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                {
                    return string.Empty;
                }

                if (path.LastIndexOf(".") == -1)
                {
                    return string.Empty;
                }

                return is_dir ? string.Empty : path.Substring(path.LastIndexOf("."));
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Revision
    {
        [JsonProperty(PropertyName = "size")]
        public string size { get; set; }
        
        [JsonProperty(PropertyName = "rev")]
        public string rev { get; set; }

        [JsonProperty(PropertyName = "thumb_exists")]
        public bool thumb_exists { get; set; }

        [JsonProperty(PropertyName = "bytes")]
        public long bytes { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public string modified { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string path { get; set; }

        [JsonProperty(PropertyName = "is_dir")]
        public bool is_dir { get; set; }

        [JsonProperty(PropertyName = "is_deleted")]
        public bool is_deleted { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string icon { get; set; }

        [JsonProperty(PropertyName = "root")]
        public string root { get; set; }

        [JsonProperty(PropertyName = "mime_type")]
        public string mime_type { get; set; }

        [JsonProperty(PropertyName = "revision")]
        public int revision { get; set; }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PhotoInfo
    {
        [JsonProperty(PropertyName = "time_taken")]
        public string time_taken { get; set; }

        [JsonProperty(PropertyName = "lat_long")]
        public double[] lat_long { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class VideoInfo
    {
        [JsonProperty(PropertyName = "time_taken")]
        public string time_taken { get; set; }

        [JsonProperty(PropertyName = "lat_long")]
        public double[] lat_long { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public long duration { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SharedFolder
    {
        [JsonProperty(PropertyName = "shared_folder_id")]
        public string shared_folder_id { get; set; }

        [JsonProperty(PropertyName = "shared_folder_name")]
        public string shared_folder_name { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string path { get; set; }

        [JsonProperty(PropertyName = "access_type")]
        public string access_type { get; set; }

        [JsonProperty(PropertyName = "shared_link_policy")]
        public string shared_link_policy { get; set; }

        [JsonProperty(PropertyName = "membership")]
        public Membership[] membership { get; set; }

        [JsonProperty(PropertyName = "owner")]
        public Owner owner { get; set; }

        [JsonProperty(PropertyName = "groups")]
        public string[] groups { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Membership
    {
        [JsonProperty(PropertyName = "user")]
        public User user { get; set; }

        [JsonProperty(PropertyName = "access_type")]
        public string access_type { get; set; }

        [JsonProperty(PropertyName = "active")]
        public bool active { get; set; }
       
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Owner
    {
        [JsonProperty(PropertyName = "display_name")]
        public string display_name { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public int uid { get; set; }

        [JsonProperty(PropertyName = "member_id")]
        public int member_id { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class User
    {
        [JsonProperty(PropertyName = "display_name")]
        public string display_name { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public int uid { get; set; }

        [JsonProperty(PropertyName = "member_id")]
        public int member_id { get; set; }

        [JsonProperty(PropertyName = "same_team")]
        public bool same_team { get; set; }
    }
   
}
