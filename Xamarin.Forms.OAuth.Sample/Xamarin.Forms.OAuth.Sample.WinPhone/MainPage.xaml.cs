using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Acr.XamForms.UserDialogs.WindowsPhone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Xamarin.Forms;
using Xamarin.Forms.OAuth.Controls;


namespace Xamarin.Forms.OAuth.Sample.WinPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            Forms.Init();
            //HACK. Forcing HybridWebViewRenderer to load.
            var renderer = new HybridWebViewRenderer();
            var mainPage = new Xamarin.Forms.OAuth.Sample.MainPage();
            mainPage.UserDialogService = new UserDialogService();
            Content = mainPage.ConvertPageToUIElement(this);
        }
    }
}
