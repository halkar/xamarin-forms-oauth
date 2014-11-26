using System;

namespace Xamarin.Forms.OAuth
{
    public class AuthToken
    {
        public DateTime ExpiresOn { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public override string ToString()
        {
            return string.Format("ExpiresOn: {0}, AccessToken: {1}, RefreshToken: {2}", ExpiresOn, AccessToken, RefreshToken);
        }
    }
}
