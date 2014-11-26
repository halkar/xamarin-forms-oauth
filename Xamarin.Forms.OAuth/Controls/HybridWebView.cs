using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Xamarin.Forms.OAuth.Controls
{
    /// <summary>
    /// The hybrid web view.
    /// </summary>
    public class HybridWebView : WebView
    {
        /// <summary>
        /// The inject lock.
        /// </summary>
        private readonly object injectLock = new object();

        /// <summary>
        /// The JSON serializer.
        /// </summary>
        private readonly JsonSerializer jsonSerializer;

        /// <summary>
        /// The registered actions.
        /// </summary>
        private readonly Dictionary<string, Action<string>> registeredActions;

        /// <summary>
        /// The registered actions.
        /// </summary>
        private readonly Dictionary<string, Func<string, object[]>> registeredFunctions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridWebView"/> class.
        /// </summary>
        /// <remarks>HybridWebView will use either <see cref="IJsonSerializer"/> configured
        /// with IoC or if missing it will use <see cref="SystemJsonSerializer"/> by default.</remarks>
        public HybridWebView()
        {
            this.jsonSerializer = new JsonSerializer();
            this.registeredActions = new Dictionary<string, Action<string>>();
            registeredFunctions = new Dictionary<string, Func<string, object[]>>();
        }

        /// <summary>
        /// The uri property.
        /// </summary>
        public static readonly BindableProperty UriProperty = BindableProperty.Create<HybridWebView, Uri>(p => p.Uri, default(Uri));

        /// <summary>
        /// Gets or sets the uri.
        /// </summary>
        public Uri Uri
        {
            get { return (Uri)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        /// <summary>
        /// Registers a native callback.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public void RegisterCallback(string name, Action<string> action)
        {
            this.registeredActions.Add(name, action);
        }

        /// <summary>
        /// Removes a native callback.
        /// </summary>
        /// <param name="name">
        /// The name of the callback.
        /// </param>
        public bool RemoveCallback(string name)
        {
            return this.registeredActions.Remove(name);
        }

        /// <summary>
        /// Registers a native callback and returns data to closure.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public void RegisterNativeFunction(string name, Func<string, object[]> func)
        {
            this.registeredFunctions.Add(name, func);
        }

        /// <summary>
        /// Removes a native callback function.
        /// </summary>
        /// <param name="name">
        /// The name of the callback.
        /// </param>
        public bool RegisterNativeFunction(string name)
        {
            return this.registeredFunctions.Remove(name);
        }

        public void LoadFromContent(string contentFullName)
        {
            var handler = this.LoadFromContentRequested;
            if (handler != null)
            {
                handler(this, contentFullName);
            }
        }

        public void LoadContent(string content)
        {
            var handler = this.LoadContentRequested;
            if (handler != null)
            {
                handler(this, content);
            }
        }

        public void OnNavigating(string url)
        {
            var handler = this.Navigating;
            if (handler != null)
            {
                handler(this, url);
            }
        }

        public void OnNavigated(string url)
        {
            var handler = this.Navigated;
            if (handler != null)
            {
                handler(this, url);
            }
        }

        public void InjectJavaScript(string script)
        {
            lock (this.injectLock)
            {
                var handler = this.JavaScriptLoadRequested;
                if (handler != null)
                {
                    handler(this, script);
                }
            }
        }

        public void ClearCookies(string url)
        {
           var handler = this.ClearCookieRequested;
           if (handler != null)
           {
               handler(this, url);
           }
        }

        public void CallJsFunction(string funcName, params object[] parameters)
        {
            var builder = new StringBuilder();

            builder.Append(funcName);
            builder.Append("(");

            for (var n = 0; n < parameters.Length; n++)
            {
                using(var writer = new StringWriter()){
                    this.jsonSerializer.Serialize(writer, parameters[n]);
                    builder.Append(writer.ToString());
                }
                if (n < parameters.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(");");

            this.InjectJavaScript(builder.ToString());
        }

        public EventHandler LoadFinished;

        public bool TryGetAction(string name, out Action<string> action)
        {
            return this.registeredActions.TryGetValue(name, out action);
        }

        public bool TryGetFunc(string name, out Func<string, object[]> func)
        {
            return this.registeredFunctions.TryGetValue(name, out func);
        }

        public void OnLoadFinished(object sender, EventArgs e)
        {
            var handler = this.LoadFinished;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public EventHandler<string> JavaScriptLoadRequested;
        public EventHandler<string> LoadFromContentRequested;
        public EventHandler<string> ClearCookieRequested;
        public EventHandler<string> LoadContentRequested;
        public EventHandler<string> Navigating;
        public EventHandler<string> Navigated;
    }
}
