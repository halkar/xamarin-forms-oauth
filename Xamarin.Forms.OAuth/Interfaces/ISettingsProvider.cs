using System.Collections.Generic;

namespace Xamarin.Forms.OAuth.Interfaces
{
    /// <summary>
    /// Configuration for <see cref="Authentication"/>.
    /// </summary>
    /// <remarks>
    /// Example of URL used in authentication process:
    /// "{Authority}/oauth2/{Path}?response_type=code&client_id={ClientId}&redirect_uri={RedirectUrl}"
    /// </remarks>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Host for all OAuth requests.
        /// </summary>
        string Authority { get; }
        /// <summary>
        /// Client ID for OAuth requests.
        /// </summary>
        string ClientId { get; }
        /// <summary>
        /// Client secret for OAuth requests.
        /// </summary>
        string ClientSecret { get; }
        /// <summary>
        /// Redirect URl that will be used after successfull authentication.
        /// </summary>
        string RedirectUrl { get; }
        /// <summary>
        /// Gets the path for authentication URL.
        /// </summary>
        /// <remarks>
        /// Usualy "auth" or "authentication".
        /// </remarks>
        string Path { get; }
        /// <summary>
        /// Additional parameters for authentication URL.
        /// </summary>
        Dictionary<string, string> AdditionalParameters { get; } 
    }
}
