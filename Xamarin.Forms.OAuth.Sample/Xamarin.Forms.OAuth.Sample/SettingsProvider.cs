using Xamarin.Forms.OAuth.Interfaces;

namespace Xamarin.Forms.OAuth.Sample
{
    class SettingsProvider : ISettingsProvider
    {
        public SettingsProvider(string authority, string resource, string clientId, string redirectUrl)
        {
            RedirectUrl = redirectUrl;
            ClientId = clientId;
            Resource = resource;
            Authority = authority;
        }

        public string Authority { get; private set; }
        public string Resource { get; private set; }
        public string ClientId { get; private set; }
        public string RedirectUrl { get; private set; }
    }
}
