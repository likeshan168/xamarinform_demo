using System.Collections.Generic;
using AllocationApp.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace AllocationApp
{
    public partial class App
    {
        public static IList<string> PhoneNumbers { get; set; }

        public static bool IsUserLoggedIn { get; set; }

        public static RestServiceManager ServiceManager { get; private set; }

        public static IList<AllocationData> Allocations = new List<AllocationData>();
        public App()
        {
            InitializeComponent();
            PhoneNumbers = new List<string>();
            ServiceManager = new RestServiceManager(new RestService());
            if (IsUserLoggedIn)
            {
                MainPage = new NavigationPage(new AllotPage());
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            //MainPage = new TabbedPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("android=70d8deae-b1cb-4e7e-a30a-2890540858b7;",
                typeof(Analytics), typeof(Crashes));
            //AppCenter.Start("android=70d8deae-b1cb-4e7e-a30a-2890540858b7;" + "uwp={Your UWP App secret here};" +
            //                "ios={Your iOS App secret here}",
            //    typeof(Analytics), typeof(Crashes));
            AppCenter.Start("android=70d8deae-b1cb-4e7e-a30a-2890540858b7", typeof(Distribute));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
