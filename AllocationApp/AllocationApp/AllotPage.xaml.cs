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
    public partial class AllotPage
    {
        public AllotPage()
        {
            InitializeComponent();
        }

        private async void SignOff_Clicked(object sender, EventArgs e)
        {
            App.IsUserLoggedIn = false;
            Navigation.InsertPageBefore(new LoginPage(), this);
            await Navigation.PopAsync();
        }
    }
}