using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Annotations;
using AllocationApp.Models;
using Xamarin.Forms;

namespace AllocationApp
{
    public class LoginViewModel : INotifyPropertyChanged
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

        public ICommand LoginCommand { protected set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                    //var mainPage = Application.Current.MainPage;
                    //mainPage.Navigation.InsertPageBefore(new AllotPage(), mainPage);
                    //await mainPage.Navigation.PopAsync();
                    await Application.Current.MainPage.Navigation.PushAsync(new AllotPage());
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

            return response.IsSuccess;
        }
    }
}