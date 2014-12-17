﻿
using Newtonsoft.Json;
using System;

namespace Dropbox.Api
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Account
    {
        [JsonProperty(PropertyName = "uid")]
        public int Id { get; internal set; }

        [JsonProperty(PropertyName = "referral_link")]
        public string ReferralLink { get; internal set; }

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName { get; internal set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; internal set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; internal set; }

        [JsonProperty(PropertyName = "quota_info")]
        public Quota Quota { get; internal set; }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class Quota
    {
        [JsonProperty(PropertyName = "quota")]
        public long Total { get; internal set; }

        [JsonProperty(PropertyName = "shared")]
        public long Shared { get; internal set; }

        [JsonProperty(PropertyName = "normal")]
        public long Normal { get; internal set; }
    }
}