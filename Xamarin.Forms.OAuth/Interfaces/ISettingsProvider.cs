namespace Xamarin.Forms.OAuth.Interfaces
{
    public interface ISettingsProvider
    {
        string Authority { get; }
        string Resource { get; }
        string ClientId { get; } 
        string RedirectUrl { get; }
    }
}
