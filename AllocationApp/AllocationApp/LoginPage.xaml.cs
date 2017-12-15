using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AllocationApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();

            BindingContext = new LoginViewModel { Navigation = Navigation, LPage = this };
        }

        private void PasswordEntry_Completed(object sender, EventArgs e)
        {
            if (Login.Command.CanExecute(null))
            {
                Login.Command.Execute(null);
            }
        }

        //private async void BtnLogin_Clicked(object sender, EventArgs e)
        //{
        //    Loading.IsRunning = true;
        //    Loading.IsVisible = true;
        //    var user = new User
        //    {
        //        UserName = UserName.Text,
        //        Password = Password.Text
        //    };
        //    if (AreCredentialsCorrect(user))
        //    {
        //        App.IsUserLoggedIn = true;
        //        //以下两行代码就是导航到主页
        //        Navigation.InsertPageBefore(new AllotPage(), this);
        //        await Navigation.PopAsync();
        //    }
        //    else
        //    {
        //        Message.Text = "登录失败";
        //        Password.Text = string.Empty;
        //    }

        //    Loading.IsRunning = false;
        //    Loading.IsVisible = false;

        //}

        //   bool AreCredentialsCorrect(User user)
        //{
        //    if (string.IsNullOrWhiteSpace(user.UserName)||string.IsNullOrWhiteSpace(user.Password))
        //    {
        //        return false;
        //    }
        //       //TODO: call web serivce to verify the user login
        //    return true;
        //}
    }
}