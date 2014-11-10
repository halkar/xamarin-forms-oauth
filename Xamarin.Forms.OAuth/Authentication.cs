using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xamarin.Forms.OAuth
{
    public class Authentication
    {
        private readonly Page _rootPage;
        private readonly string _authority;
        private readonly string _resource;
        private readonly string _clientId;
        private readonly string _redirectUrl;

        private readonly TaskCompletionSource<AuthToken> _tcs = new TaskCompletionSource<AuthToken>();

        public Authentication(Page rootPage, string authority, string resource, string clientId, string redirectUrl)
        {
            _rootPage = rootPage;
            _authority = authority;
            _resource = resource;
            _clientId = clientId;
            _redirectUrl = redirectUrl;
        }

        public async Task<AuthToken> Authenticate()
        {
            var adalPage = new AdalPage();
            adalPage.HybridWebView.Navigating += Navigating;
            await _rootPage.Navigation.PushModalAsync(adalPage);

            adalPage.HybridWebView.Uri = new Uri(GetAuthUrl());

            await _tcs.Task;
            return _tcs.Task.Result;
        }

        protected virtual string GetAuthUrl()
        {
            return string.Format(
                "{0}/oauth2/authorize?response_type=code&resource={1}&client_id={2}&redirect_uri={3}",
                _authority,
                _resource,
                _clientId,
                _redirectUrl);
        }

        private async void Navigating(object sender, string returnUrl)
        {
            if (!returnUrl.StartsWith(_redirectUrl)) return;

            var uri = new Uri(returnUrl);

            var code = uri.Query.Remove(0, 6);

            var authToken = await RequestToken(code);

            await _rootPage.Navigation.PopModalAsync();

            _tcs.SetResult(authToken);
        }

        private async Task<AuthToken> RequestToken(string code)
        {
            var requestStartTime = DateTime.Now;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, GetRequestUri())
            {
                Content = new StringContent(GetTokenRequest(code), Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent == null) return null;

            var token = JsonConvert.DeserializeObject<OAuthToken>(responseContent);
            var jwt = JsonWebTokenConvert.Decode(token.IdToken, (byte[])null, false);
            var identity = JsonConvert.DeserializeObject<JsonWebToken>(jwt);

            return new AuthToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                Email = identity.Email,
                ExpiresOn = requestStartTime.AddSeconds(token.ExpiresIn),
                FamilyName = identity.FamilyName,
                GivenName = identity.GivenName,
                UserId = identity.UserId,
                Username = identity.Username
            };
        }

        protected virtual string GetTokenRequest(string code)
        {
            return string.Format(
                "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}",
                code,
                _clientId,
                Uri.EscapeUriString(_redirectUrl));
        }

        protected virtual string GetRequestUri()
        {
            return string.Format("{0}/oauth2/token", _authority);
        }

        public async Task<AuthToken> RefreshToken(string refreshToken)
        {
            var requestStartTime = DateTime.Now;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, GetRequestUri())
            {
                Content =
                    new StringContent(GetTokenRefreshRequest(refreshToken), Encoding.UTF8,
                        "application/x-www-form-urlencoded")
            };

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent == null) return null;

            var token = JsonConvert.DeserializeObject<OAuthToken>(responseContent);

            return new AuthToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresOn = requestStartTime.AddSeconds(token.ExpiresIn)
            };
        }

        protected virtual string GetTokenRefreshRequest(string refreshToken)
        {
            return string.Format(
                "grant_type=refresh_token&client_id={0}&redirect_uri={1}&refresh_token={2}",
                _clientId,
                Uri.EscapeUriString(_redirectUrl),
                refreshToken);
        }
    }
}

