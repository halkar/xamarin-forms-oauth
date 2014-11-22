using System;
using Acr.XamForms.UserDialogs.Droid;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.OAuth.Sample.Droid
{
    [Activity(Label = "Xamarin.Forms.OAuth.Sample", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : AndroidActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);

            var mainPage = new MainPage();
            mainPage.UserDialogService = new UserDialogService();

            SetPage(mainPage);
        }
    }
}

