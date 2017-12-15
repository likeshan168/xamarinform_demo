using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Annotations;
using AllocationApp.Models;
using AllocationApp.ViewModels;
using Realms;
using Xamarin.Forms;

namespace AllocationApp
{
    public class LoginViewModel : ViewModelBase
    {
        private string message = string.Empty;
        private bool isRunning;
        private string userName = string.Empty;
        private string password = string.Empty;
        private bool rememberMe;
        private Realm _realm;
        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await ValidateLoginAsync(), () => !IsRunning);
            var config = new RealmConfiguration { SchemaVersion = 1 };
            _realm = Realm.GetInstance(config);

            var user = _realm.All<User>().FirstOrDefault();
            if (user != null)
            {
                UserName = user.UserName;
                Password = user.Password;
                RememberMe = true;
            }
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

        public bool RememberMe
        {
            get => rememberMe;
            set
            {
                rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
            }
        }

        public INavigation Navigation { get; set; }

        public ContentPage LPage { get; set; }

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
                    //记录用户名和密码
                    if (RememberMe)
                    {
                        _realm.Write(() =>
                        {
                            _realm.RemoveAll<User>();
                            _realm.Add(new User {UserName = UserName, Password = Password});
                        });
                    }
                    else
                    {
                        _realm.Write(() =>
                        {
                            _realm.RemoveAll<User>();
                        });
                    }

                    App.IsUserLoggedIn = true;
                    //以下两行代码就是导航到主页
                    Navigation.InsertPageBefore(new AllotPage(), LPage);
                    await Navigation.PopAsync();
                    //await Navigation.PushAsync(new AllotPage(), true);
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