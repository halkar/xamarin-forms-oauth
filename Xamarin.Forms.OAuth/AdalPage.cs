using Xamarin.Forms.OAuth.Controls;

namespace Xamarin.Forms.OAuth
{
    public class AdalPage : ContentPage
    {
        private readonly HybridWebView _hybridWebView = new HybridWebView();

        public AdalPage()
        {
            Content = _hybridWebView;
        }

        public HybridWebView HybridWebView
        {
            get { return _hybridWebView; }
        }
    }
}

