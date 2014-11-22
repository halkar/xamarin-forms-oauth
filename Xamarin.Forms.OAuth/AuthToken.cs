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

        public override string ToString()
        {
            return string.Format("ExpiresOn: {0}, AccessToken: {1}, RefreshToken: {2}, UserId: {3}, Username: {4}, Email: {5}, FamilyName: {6}, GivenName: {7}", ExpiresOn, AccessToken, RefreshToken, UserId, Username, Email, FamilyName, GivenName);
        }
    }
}
