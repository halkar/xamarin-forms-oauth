using System.Collections.Generic;

namespace Xamarin.Forms.OAuth.Interfaces
{
    public interface ISettingsProvider
    {
        string Authority { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string RedirectUrl { get; }
        string Path { get; }
        Dictionary<string, string> AdditionalParameters { get; } 
    }
}
