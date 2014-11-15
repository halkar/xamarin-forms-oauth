using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Acr.XamForms.UserDialogs;
using Newtonsoft.Json;
using Xamarin.Forms.OAuth.Interfaces;

namespace Xamarin.Forms.OAuth
{
    public class Authentication : IAuthentication
    {
        private readonly IRootPageProvider _rootPageProvider;
        private readonly IUserDialogService _userDialogService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly TaskCompletionSource<AuthToken> _tcs = new TaskCompletionSource<AuthToken>();
        private readonly ProgressConfig _progressConfig;
       
        private IProgressDialog _progressDialog;

        public Authentication(IRootPageProvider rootPageProvider, IUserDialogService userDialogService, ISettingsProvider settingsProvider)
        {
            _rootPageProvider = rootPageProvider;
            _userDialogService = userDialogService;
            _settingsProvider = settingsProvider;
            _progressConfig = new ProgressConfig
            {
                AutoShow = true,
                Title = "Loading..."
            };
        }

        public async Task<AuthToken> Authenticate()
        {
            var adalPage = new AdalPage();
            adalPage.HybridWebView.Navigating += Navigating;
            adalPage.HybridWebView.LoadFinished += LoadFinished;
            await _rootPageProvider.Page.Navigation.PushModalAsync(adalPage);

            _progressDialog = _userDialogService.Progress(_progressConfig);

            adalPage.HybridWebView.Uri = new Uri(GetAuthUrl(_settingsProvider));

            await _tcs.Task;
            return _tcs.Task.Result;
        }

        private void LoadFinished(object sender, EventArgs e)
        {
            if (_progressDialog == null) 
                return;
            _progressDialog.Hide();
            _progressDialog = null;
        }

        protected virtual string GetAuthUrl(ISettingsProvider settingsProvider)
        {
            return string.Format(
                "{0}/oauth2/authorize?response_type=code&resource={1}&client_id={2}&redirect_uri={3}",
                settingsProvider.Authority,
                settingsProvider.Resource,
                settingsProvider.ClientId,
                settingsProvider.RedirectUrl);
        }

        private async void Navigating(object sender, string returnUrl)
        {
            if (!returnUrl.StartsWith(_settingsProvider.RedirectUrl)) return;

            var uri = new Uri(returnUrl);

            var code = uri.Query.Remove(0, 6);

            _progressDialog = _userDialogService.Progress(_progressConfig);

            var authToken = await RequestToken(code);

            await _rootPageProvider.Page.Navigation.PopModalAsync();

            _progressDialog.Hide();

            _tcs.SetResult(authToken);
        }

        private async Task<AuthToken> RequestToken(string code)
        {
            var requestStartTime = DateTime.Now;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, GetRequestUri(_settingsProvider))
            {
                Content = new StringContent(
                    GetRefreshTokenRequest(code, _settingsProvider),
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded")
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

        protected virtual string GetRefreshTokenRequest(string code, ISettingsProvider settingsProvider)
        {
            return string.Format(
                "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}",
                code,
                settingsProvider.ClientId,
                Uri.EscapeUriString(settingsProvider.RedirectUrl));
        }

        protected virtual string GetRequestUri(ISettingsProvider settingsProvider)
        {
            return string.Format("{0}/oauth2/token", settingsProvider.Authority);
        }

        public async Task<AuthToken> RefreshToken(string refreshToken)
        {
            var requestStartTime = DateTime.Now;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, GetRequestUri(_settingsProvider))
            {
                Content = new StringContent(
                    GetTokenRequest(refreshToken, _settingsProvider),
                    Encoding.UTF8,
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

        protected virtual string GetTokenRequest(string refreshToken, ISettingsProvider settingsProvider)
        {
            return string.Format(
                "grant_type=refresh_token&client_id={0}&redirect_uri={1}&refresh_token={2}",
                settingsProvider.ClientId,
                Uri.EscapeUriString(settingsProvider.RedirectUrl),
                refreshToken);
        }
    }
}

