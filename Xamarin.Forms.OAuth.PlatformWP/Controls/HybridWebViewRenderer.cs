using System;
using System.Linq;
using Microsoft.Phone.Controls;
using Xamarin.Forms;
using Xamarin.Forms.OAuth.Controls;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace Xamarin.Forms.OAuth.Controls
{
    /// <summary>
    /// The hybrid web view renderer.
    /// </summary>
    public partial class HybridWebViewRenderer : ViewRenderer<HybridWebView, WebBrowser>
    {
        /// <summary>
        /// The web view.
        /// </summary>
        protected WebBrowser WebView;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (this.WebView == null)
            {
                this.WebView = new WebBrowser { IsScriptEnabled = true };

                this.WebView.Navigating += webView_Navigating;
                this.WebView.LoadCompleted += webView_LoadCompleted;
                this.WebView.ScriptNotify += WebViewOnScriptNotify;

                this.SetNativeControl(this.WebView);
            }

            this.Unbind(e.OldElement);
            this.Bind();
        }

        private void WebViewOnScriptNotify(object sender, NotifyEventArgs notifyEventArgs)
        {
            Action<string> action;
            var values = notifyEventArgs.Value.Split('/');
            var name = values.FirstOrDefault();

            if (name != null && this.Element.TryGetAction(name, out action))
            {
                var data = Uri.UnescapeDataString(values.ElementAt(1));
                action.Invoke(data);
            }
        }

        private void webView_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.InjectNativeFunctionScript();
            this.Element.OnLoadFinished(sender, EventArgs.Empty);
        }

        partial void LoadContent(object sender, string contentFullName)
        {
            LoadFromContent(sender, contentFullName);
        }

        private void webView_Navigating(object sender, NavigatingEventArgs e)
        {
            this.Element.OnNavigating(e.Uri.ToString());
            if (e.Uri.IsAbsoluteUri && this.CheckRequest(e.Uri.AbsoluteUri))
            {
                System.Diagnostics.Debug.WriteLine(e.Uri);
            }
        }

        partial void Inject(string script)
        {
            try
            {
                this.WebView.InvokeScript("eval", script);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        partial void Load(Uri uri)
        {
            if (uri != null)
            {
                this.WebView.Source = uri;
            }
        }

        partial void LoadFromContent(object sender, string contentFullName)
        {
            this.Element.Uri = new Uri(contentFullName, UriKind.Relative);
        }
    }
}