using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Forms.OAuth.Controls;

namespace Xamarin.Forms.OAuth.Controls
{
    public partial class HybridWebViewRenderer
    {

        private const string Format = "^(file|http|https)://(local|LOCAL)/Action(=|%3D)(?<Action>[\\w]+)/";
        private const string FuncFormat = "^(file|http|https)://(local|LOCAL)/Func(=|%3D)(?<CallbackIdx>[\\d]+)(&|%26)(?<FuncName>[\\w]+)/";
        private static readonly Regex Expression = new Regex(Format);
        private static readonly Regex FuncExpression = new Regex(FuncFormat);

#if __ANDROID__
        private void InjectNativeFunctionScript()
        {
            var builder = new StringBuilder();
            builder.Append("function Native(action, data){ ");
            builder.Append("Xamarin.call(action,  (typeof data == \"object\") ? JSON.stringify(data) : data);");
            builder.Append("}");
            this.Inject(builder.ToString());
        }
#else
        private void InjectNativeFunctionScript()
        {
            var builder = new StringBuilder();
            builder.Append("function Native(action, data){ ");
#if WINDOWS_PHONE
            builder.Append("window.external.notify(");
#else
            builder.Append("window.location = \"//LOCAL/Action=\" + ");
#endif
            builder.Append("action + \"/\"");
            builder.Append(" + ((typeof data == \"object\") ? JSON.stringify(data) : data)");
#if WINDOWS_PHONE
            builder.Append(")");
#endif
            builder.Append(" ;}");

            builder.Append("NativeFuncs = [];");
            builder.Append("function NativeFunc(action, data, callback){ ");

            builder.Append("  var callbackIdx = NativeFuncs.push(callback) - 1;");

#if WINDOWS_PHONE
            builder.Append("window.external.notify(");
#else
            builder.Append("window.location = '//LOCAL/Func=' + ");
#endif
            builder.Append("callbackIdx + '&' + ");
            builder.Append("action + '/'");
            builder.Append(" + ((typeof data == 'object') ? JSON.stringify(data) : data)");
#if WINDOWS_PHONE
            builder.Append(")");
#endif
            builder.Append(" ;}");
            builder.Append(" if (typeof(window.NativeFuncsReady) !== 'undefined') { ");
            builder.Append("   window.NativeFuncsReady(); ");
            builder.Append(" } ");

            this.Inject(builder.ToString());
        }
#endif

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Uri")
            {
                this.Load(this.Element.Uri);
            }
        }

        private void Bind()
        {
            this.Element.PropertyChanged += this.Model_PropertyChanged;
            if (this.Element.Uri != null)
            {
                this.Load (this.Element.Uri);
            }
            else if (this.Element.Source is HtmlWebViewSource)
            {
                var htmlSource = this.Element.Source as HtmlWebViewSource;
                this.LoadContent(null, htmlSource.Html);
            }
            else if (this.Element.Source is UrlWebViewSource)
            {
                var webViewSource = this.Element.Source as UrlWebViewSource;
                this.Load(new Uri(webViewSource.Url));
            }

            this.Element.PropertyChanged += this.Model_PropertyChanged;
            this.Element.JavaScriptLoadRequested += OnInjectRequest;
            this.Element.LoadFromContentRequested += LoadFromContent;
            this.Element.LoadContentRequested += LoadContent;
            this.Element.ClearCookieRequested += ClearCookie;
        }

        private void Unbind(HybridWebView oldElement)
        {
            if (oldElement != null)
            {
                oldElement.PropertyChanged -= this.Model_PropertyChanged;
                oldElement.JavaScriptLoadRequested -= OnInjectRequest;
                oldElement.LoadFromContentRequested -= LoadFromContent;
                oldElement.LoadContentRequested -= LoadContent;
                this.Element.ClearCookieRequested -= ClearCookie;
            }
        }

        private void OnInjectRequest(object sender, string script)
        {
            this.Inject(script);
        }

        partial void Inject(string script);

        partial void Load(Uri uri);

        partial void LoadFromContent(object sender, string contentFullName);

        partial void LoadContent(object sender, string contentFullName);

        partial void ClearCookie(object sender, string url);

        private bool CheckRequest(string request)
        {
            var m = Expression.Match(request);

            if (m.Success)
            {
                Action<string> action;
                var name = m.Groups["Action"].Value;

                if (this.Element.TryGetAction (name, out action))
                {
                    var data = Uri.UnescapeDataString (request.Remove (m.Index, m.Length));
                    action.Invoke (data);
                } 
                else
                {
                    System.Diagnostics.Debug.WriteLine ("Unhandled callback {0} was called from JavaScript", name);
                }
            }

            var mFunc = FuncExpression.Match(request);

            if (mFunc.Success)
            {
                Func<string, object[]> func;
                var name = mFunc.Groups["FuncName"].Value;
                var callBackIdx = mFunc.Groups["CallbackIdx"].Value;

                if (this.Element.TryGetFunc (name, out func))
                {
                    var data = Uri.UnescapeDataString (request.Remove (mFunc.Index, mFunc.Length));
                    ThreadPool.QueueUserWorkItem(o =>
                        {
                            var result = func.Invoke (data);
                            Element.CallJsFunction(string.Format("NativeFuncs[{0}]", callBackIdx), result);                            
                        });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine ("Unhandled callback {0} was called from JavaScript", name);
                }
            }

            return m.Success || mFunc.Success;
        }

        private void TryInvoke(string function, string data)
        {
            Action<string> action;

            if (this.Element.TryGetAction(function, out action))
            {
                action.Invoke(data);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Unhandled callback {0} was called from JavaScript", function);
            }
        }
    }
}
