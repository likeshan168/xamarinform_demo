using System.Collections.Generic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace AllocationApp
{
    public partial class App
    {
        public static IList<string> PhoneNumbers { get; set; }
        public App()
        {
            InitializeComponent();
            PhoneNumbers = new List<string>();
            //MainPage = new NavigationPage(new MainPage());
            MainPage = new TabbedPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("android=70d8deae-b1cb-4e7e-a30a-2890540858b7;",
                typeof(Analytics), typeof(Crashes));
            //AppCenter.Start("android=70d8deae-b1cb-4e7e-a30a-2890540858b7;" + "uwp={Your UWP App secret here};" +
            //                "ios={Your iOS App secret here}",
            //    typeof(Analytics), typeof(Crashes));
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
