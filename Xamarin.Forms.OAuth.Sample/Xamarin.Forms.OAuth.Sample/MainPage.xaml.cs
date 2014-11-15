using System;
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
            var oauth = new CustomAuthentication(new RootPageProvider { Page = this }, UserDialogService,
                new SettingsProvider(AuthorityEntry.Text, ResourceEntry.Text, ClientIdEntry.Text, RedirectUrlEntry.Text));
            await oauth.Authenticate();
        }
    }

    public class CustomAuthentication : Authentication
    {
        public CustomAuthentication(IRootPageProvider rootPageProvider, IUserDialogService userDialogService, ISettingsProvider settingsProvider) 
            : base(rootPageProvider, userDialogService, settingsProvider)
        {
        }

        protected override string GetAuthUrl(ISettingsProvider settingsProvider)
        {
            return string.Format(
                "{0}/oauth2/authenticate?response_type=code&client_id={1}&redirect_uri={2}",
                settingsProvider.Authority,
                settingsProvider.ClientId,
                Uri.EscapeUriString(settingsProvider.RedirectUrl));
        }
    }
}
