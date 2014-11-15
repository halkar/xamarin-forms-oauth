using System;
using Android.Webkit;
using Xamarin.Forms.OAuth.Controls;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]

namespace Xamarin.Forms.OAuth.Controls
{
    public partial class HybridWebViewRenderer : ViewRenderer<HybridWebView, Android.Webkit.WebView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged (e);

            if (this.Control == null)
            {
                var webView = new Android.Webkit.WebView(this.Context);

                webView.Settings.JavaScriptEnabled = true;

                webView.SetWebViewClient(new Client(this));
                webView.SetWebChromeClient(new ChromeClient());

                webView.AddJavascriptInterface(new Xamarin(this), "Xamarin");

                this.SetNativeControl(webView);
            }

            this.Unbind(e.OldElement);

            this.Bind();
        }
            
        partial void Inject(string script)
        {
            this.Control.LoadUrl(string.Format("javascript: {0}", script));
        }

        partial void Load(Uri uri)
        {
            if (uri != null)
            {
                this.Control.LoadUrl(uri.AbsoluteUri);
                this.InjectNativeFunctionScript();
            }
        }

        partial void LoadFromContent(object sender, string contentFullName)
        {
            this.Element.Uri = new Uri("file:///android_asset/" + contentFullName);
        }

        partial void LoadContent(object sender, string contentFullName)
        {
            this.Control.LoadDataWithBaseURL("file:///android_asset/", contentFullName, "text/html", "UTF-8", null);
            // we can't really set the URI and fire up native function injection so the workaround is to do it here
            this.InjectNativeFunctionScript();
        }

        private class Client : WebViewClient
        {
            private readonly WeakReference<HybridWebViewRenderer> webHybrid;

            public Client(HybridWebViewRenderer webHybrid)
            {
                this.webHybrid = new WeakReference<HybridWebViewRenderer>(webHybrid);
            }

            public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
            {
                HybridWebViewRenderer hybrid;

                if (!this.webHybrid.TryGetTarget(out hybrid) || !hybrid.CheckRequest(url))
                {
                    return base.ShouldOverrideUrlLoading(view, url);
                }

                return true;
            }

            public override void OnPageFinished(Android.Webkit.WebView view, string url)
            {
                base.OnPageFinished(view, url);
            }

            public override void OnPageStarted(Android.Webkit.WebView view, string url, Android.Graphics.Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);

                HybridWebViewRenderer hybrid;
                if (!this.webHybrid.TryGetTarget(out hybrid))
                    return;
                hybrid.Element.OnNavigating(url);
            }
        }

        /// <summary>
        /// Java callback class for JavaScript.
        /// </summary>
        public class Xamarin : Java.Lang.Object
        {
            private readonly WeakReference<HybridWebViewRenderer> webHybrid;

            public Xamarin(HybridWebViewRenderer webHybrid)
            {
                this.webHybrid = new WeakReference<HybridWebViewRenderer>(webHybrid);
            }

            [JavascriptInterface]
            [Java.Interop.Export("call")]
            public void Call(string function, string data)
            {
                HybridWebViewRenderer hybrid;

                if (this.webHybrid.TryGetTarget(out hybrid))
                {
                    hybrid.TryInvoke(function, data);
                }
            }
        }

        private class ChromeClient : WebChromeClient 
        {
            public override bool OnJsAlert(Android.Webkit.WebView view, string url, string message, JsResult result)
            {
                // the built-in alert is pretty ugly, you could do something different here if you wanted to
                return base.OnJsAlert(view, url, message, result);
            }
        }
    }
}

