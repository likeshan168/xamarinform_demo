using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Annotations;
using AllocationApp.Models;
using AllocationApp.ViewModels;
using Xamarin.Forms;

namespace AllocationApp
{
    public class LoginViewModel : ViewModelBase
    {
        private string message = string.Empty;
        private bool isRunning = false;
        private string userName = string.Empty;
        private string password = string.Empty;
        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await ValidateLoginAsync(), () => !IsRunning);
        }

        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged();
                }

            }
        }

        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged();
                }

            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    OnPropertyChanged();

                    ((Command)LoginCommand).ChangeCanExecute();
                }

            }
        }

        public INavigation Navigation { get; set; }

        public ICommand LoginCommand { protected set; get; }

        //async Task LoginAsync()
        //{
        //    await Task.Run(() => ValidateLoginAsync());
        //}

        private async Task ValidateLoginAsync()
        {
            try
            {
                IsRunning = true;
                //await Task.Delay(4000);
                if (await AreCredentialsCorrectAsync())
                {
                    //await Application.Current.MainPage.DisplayAlert("Login", "Login Successfully", "OK");
                    App.IsUserLoggedIn = true;
                    //以下两行代码就是导航到主页
                    //Navigation.InsertPageBefore(new AllotPage(), Application.Current.MainPage);
                    //await Navigation.PopAsync();
                    await Navigation.PushAsync(new AllotPage());
                }
                else
                {
                    Password = string.Empty;
                }

                IsRunning = false;
            }
            catch (Exception ex)
            {
                IsRunning = false;
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        async Task<bool> AreCredentialsCorrectAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "用户名和密码不能为空";
                return false;
            }
            //TODO: call web serivce to verify the user login

            LoginResponse response =
                await App.ServiceManager.LoginAsync(new User { UserName = UserName, Password = Password });

            Message = response.Result;
            App.TenantId = response.TenantId;
            App.IsAdmin = response.IsAdmin;

            return response.IsSuccess;
        }
    }
}