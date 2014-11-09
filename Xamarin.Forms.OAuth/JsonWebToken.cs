using Newtonsoft.Json;

namespace Xamarin.Forms.OAuth
{
    public class JsonWebToken
    {
        [JsonProperty(PropertyName = "oid")]
        public string UserId { get; set; }
        [JsonProperty(PropertyName = "upn")]
        public string Username { get; set; }
        [JsonProperty(PropertyName = "unique_name")]
        public string Email { get; set; }
        [JsonProperty(PropertyName="given_name")]
        public string GivenName { get; set; }
        [JsonProperty(PropertyName = "family_name")]
        public string FamilyName { get; set; }
    }
}