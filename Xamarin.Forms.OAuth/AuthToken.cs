using System;

namespace Xamarin.Forms.OAuth
{
    public class AuthToken
    {
        public DateTime ExpiresOn { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
    }
}
