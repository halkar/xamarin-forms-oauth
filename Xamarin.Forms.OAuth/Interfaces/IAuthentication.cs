using System.Threading.Tasks;

namespace Xamarin.Forms.OAuth.Interfaces
{
    public interface IAuthentication
    {
        Task<AuthToken> Authenticate();
        Task<AuthToken> RefreshToken(string refreshToken);
        Task Logout(string token);
    }
}