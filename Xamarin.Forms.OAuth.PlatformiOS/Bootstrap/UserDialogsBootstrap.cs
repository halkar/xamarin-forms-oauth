// HACK: this is to deal with the linker nuking the assembly
using Acr.XamForms.UserDialogs.iOS;

namespace Xamarin.Forms.OAuth.Bootstrap
{
    public class UserDialogsBootstrap 
    {
        public UserDialogsBootstrap() 
        {
            new UserDialogService();
        }
    }
}