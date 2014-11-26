using System;
using System.Collections.Generic;
using Acr.XamForms.UserDialogs;
using Xamarin.Forms.OAuth.Interfaces;

namespace Xamarin.Forms.OAuth.Sample
{
    public partial class MainPage
    {
        private AuthToken _token;
        private Authentication _oauth;
        public MainPage()
        {
            InitializeComponent();
        }

        public IUserDialogService UserDialogService { get; set; }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            if (_oauth == null) {
                _oauth = new Authentication (new RootPageProvider { Page = this }, UserDialogService,
                    new SettingsProvider (AuthorityEntry.Text, ClientIdEntry.Text, ClientSecretEntry.Text,
                        RedirectUrlEntry.Text, "auth", new Dictionary<string, string> { { "scope", "profile" } }));
            }
            if (_token == null) {
                _token = await _oauth.Authenticate ();
                UserDialogService.Alert (new AlertConfig{ Message = _token.ToString () });
                AccessTokenEntry.Text = _token.AccessToken;
                RefreshTokenEntry.Text = _token.RefreshToken;
            } else {
                var token = await _oauth.RefreshToken (_token.RefreshToken);
                _token.ExpiresOn = token.ExpiresOn;
                _token.AccessToken = token.AccessToken;
                AccessTokenEntry.Text = _token.AccessToken;
            }
        }
    }
}