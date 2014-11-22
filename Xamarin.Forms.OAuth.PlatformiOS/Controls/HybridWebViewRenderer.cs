using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Forms;
using Xamarin.Forms.OAuth.Controls;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]

namespace Xamarin.Forms.OAuth.Controls
{
    /// <summary>
    /// The hybrid web view renderer.
    /// </summary>
    public partial class HybridWebViewRenderer : ViewRenderer<HybridWebView, UIWebView>
    {
        private UIWebView webView;

        /// <summary>
        /// The on element changed callback.
        /// </summary>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (this.webView == null)
            {
                this.webView = new UIWebView();
                this.webView.LoadFinished += LoadFinished;
                this.webView.ShouldStartLoad += this.HandleStartLoad;
                this.InjectNativeFunctionScript();
                this.SetNativeControl(this.webView);
            }

            Element.SizeChanged += HandleSizeChanged;

            this.Unbind(e.OldElement);
            this.Bind();
        }

        void HandleSizeChanged (object sender, EventArgs e)
        {
            LayoutViews ();
        }

        void LoadFinished(object sender, EventArgs e)
        {
            this.Element.OnLoadFinished(sender, e);
            this.Element.OnNavigating(this.Element.Uri.ToString());
            InjectNativeFunctionScript();
        }

        private bool HandleStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            this.Element.OnNavigating(request.Url.AbsoluteUrl.AbsoluteString);
            return !this.CheckRequest(request.Url.RelativeString);
        }

        partial void Inject(string script)
        {
            InvokeOnMainThread(() => {
                this.webView.EvaluateJavascript(script);
            });
        }

        /* 
         * This is a hack to because the base wasn't working 
         * when within a stacklayout
         */
        public override void LayoutSubviews()
        {
            LayoutViews ();
        }

        void LayoutViews()
        {
            if (this.Control != null) {
                var control = this.Control;
                var element = base.Element;
                control.Frame = new RectangleF ((float)element.X, (float)element.Y, (float)element.Width, (float)element.Height);
                Frame = new RectangleF ((float)element.X, (float)element.Y, (float)element.Width, (float)element.Height);
                Bounds = new RectangleF ((float)element.X, (float)element.Y, (float)element.Width, (float)element.Height);
            }
        }

        partial void Load(Uri uri)
        {
            if (uri != null)
            {
                this.webView.LoadRequest(new NSUrlRequest(new NSUrl(uri.AbsoluteUri)));
            }
        }

        partial void LoadFromContent(object sender, string contentFullName)
        {
            this.Element.Uri = new Uri(NSBundle.MainBundle.BundlePath + "/" + contentFullName);
            //string homePageUrl = NSBundle.MainBundle.BundlePath + "/" + contentFullName;
            //this.webView.LoadRequest(new NSUrlRequest(new NSUrl(homePageUrl, false)));
        }

        partial void LoadContent(object sender, string contentFullName)
        {
            this.webView.LoadHtmlString(contentFullName, new NSUrl(NSBundle.MainBundle.BundlePath, true));
        }
    }
}