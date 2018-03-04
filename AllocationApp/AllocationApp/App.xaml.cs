using System.Collections.Generic;
using System.Linq;
using AllocationApp.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Realms;
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
        //这个是保存所有的数据，用来展示所有数据的
        public static IList<AllocationData> Allocations = new List<AllocationData>();
        //这个是保存盘点过的数据（包括存在的和不存在的）,这个是用来同步到服务器
        public static Stack<AllocationData> CheckedAllocations = new Stack<AllocationData>();
        private Realm _realm;
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

            var config = new RealmConfiguration { SchemaVersion = 2 };
            _realm = Realm.GetInstance(config);
            var notUpdatedItems = _realm.All<AllocationData2>().ToList();
            CheckedAllocations.Clear();
            if (notUpdatedItems != null && notUpdatedItems.Count != 0)
            {
                foreach (var item in notUpdatedItems)
                {
                    CheckedAllocations.Push(new AllocationData
                    {
                        Amount = item.Amount,
                        IsChecked = (StateKind)item.IsChecked,
                        MasterAwb = item.MasterAwb,
                        SubAwb = item.SubAwb
                    });
                }
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
