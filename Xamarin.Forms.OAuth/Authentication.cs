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
        private AdalPage _adalPage;
        private IProgressDialog _progressDialog;
        private readonly ProgressConfig _progressConfig;
        private readonly IRootPageProvider _rootPageProvider;
        protected readonly ISettingsProvider _settingsProvider;
        private readonly TaskCompletionSource<AuthToken> _tcs = new TaskCompletionSource<AuthToken>();
        private readonly IUserDialogService _userDialogService;

        public Authentication(IRootPageProvider rootPageProvider, IUserDialogService userDialogService,
            ISettingsProvider settingsProvider)
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
            _adalPage = new AdalPage();
            _adalPage.HybridWebView.Navigating += Navigating;
            _adalPage.HybridWebView.Navigated += Navigated;
            await _rootPageProvider.Page.Navigation.PushModalAsync(_adalPage);

            _progressDialog = _userDialogService.Progress(_progressConfig);

            _adalPage.HybridWebView.Uri = new Uri(GetAuthUrl());

            await _tcs.Task;
            return _tcs.Task.Result;
        }

        public async Task<AuthToken> RefreshToken(string refreshToken)
        {
            var requestStartTime = DateTime.Now;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, GetRequestUri(_settingsProvider))
            {
                Content = new StringContent(
                    GetRefreshTokenRequest(refreshToken, _settingsProvider),
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

        private void Navigated(object sender, string e)
        {
            if (_progressDialog == null)
                return;
            _adalPage.HybridWebView.Navigated -= Navigated;
            _progressDialog.Hide();
            _progressDialog = null;
        }

        protected virtual string GetAuthUrl()
        {
            var url = string.Format(
                "{0}/oauth2/{1}?response_type=code&client_id={2}&redirect_uri={3}",
                _settingsProvider.Authority,
                _settingsProvider.Path,
                _settingsProvider.ClientId,
                _settingsProvider.RedirectUrl);
            if (_settingsProvider.AdditionalParameters == null)
                return url;
            var sb = new StringBuilder(url);
            foreach (var parameter in _settingsProvider.AdditionalParameters)
            {
                sb.Append("&")
                    .Append(parameter.Key)
                    .Append("=")
                    .Append(parameter.Value);
            }
            url = sb.ToString();
            return url;
        }

        private async void Navigating(object sender, string returnUrl)
        {
            if (!returnUrl.StartsWith(_settingsProvider.RedirectUrl)) return;

            _adalPage.HybridWebView.IsVisible = false;

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
                    GetTokenRequest(code, _settingsProvider),
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded")
            };

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent == null) return null;

            return DeserializeAuthToken(responseContent, requestStartTime);
        }

        protected virtual AuthToken DeserializeAuthToken(string responseContent, DateTime requestStartTime)
        {
            var token = JsonConvert.DeserializeObject<OAuthToken>(responseContent);

            return new AuthToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresOn = requestStartTime.AddSeconds(token.ExpiresIn)
            };
        }

        protected virtual string GetTokenRequest(string code, ISettingsProvider settingsProvider)
        {
            return string.Format(
                "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}&client_secret={3}",
                code,
                settingsProvider.ClientId,
                Uri.EscapeUriString(settingsProvider.RedirectUrl),
                settingsProvider.ClientSecret);
        }

        protected virtual string GetRequestUri(ISettingsProvider settingsProvider)
        {
            return string.Format("{0}/oauth2/token", settingsProvider.Authority);
        }

        protected virtual string GetRefreshTokenRequest(string refreshToken, ISettingsProvider settingsProvider)
        {
            return string.Format(
                "grant_type=refresh_token&client_id={0}&redirect_uri={1}&refresh_token={2}&client_secret={3}",
                settingsProvider.ClientId,
                Uri.EscapeUriString(settingsProvider.RedirectUrl),
                refreshToken,
                settingsProvider.ClientSecret);
        }

        public void ClearCookies()
        {
            _adalPage.HybridWebView.ClearCookies(null);
        }
    }
}