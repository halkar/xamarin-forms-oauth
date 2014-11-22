using System.Collections.Generic;
using Xamarin.Forms.OAuth.Interfaces;

namespace Xamarin.Forms.OAuth.Sample
{
    class SettingsProvider : ISettingsProvider
    {
        public SettingsProvider(string authority, string clientId, string clientSecret, string redirectUrl, string path, Dictionary<string, string> additionalParameters = null)
        {
            ClientSecret = clientSecret;
            AdditionalParameters = additionalParameters;
            Path = path;
            RedirectUrl = redirectUrl;
            ClientId = clientId;
            Authority = authority;
        }

        public string Authority { get; private set; }

        public string ClientId { get; private set; }

        public string ClientSecret { get; private set; }

        public string RedirectUrl { get; private set; }

        public string Path { get; private set; }

        public Dictionary<string, string> AdditionalParameters { get; private set; }
    }
}
