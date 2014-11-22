using System;
using System.Collections.Generic;
using Acr.XamForms.UserDialogs;
using Xamarin.Forms.OAuth.Interfaces;

namespace Xamarin.Forms.OAuth.Sample
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public IUserDialogService UserDialogService { get; set; }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            var oauth = new Authentication(new RootPageProvider {Page = this}, UserDialogService,
                new SettingsProvider(AuthorityEntry.Text, ClientIdEntry.Text, ClientSecretEntry.Text,
                    RedirectUrlEntry.Text, "auth", new Dictionary<string, string> {{"scope", "profile"}}));
            var authToken = await oauth.Authenticate();
            UserDialogService.Alert(new AlertConfig{Message = authToken.ToString()});
        }
    }
}