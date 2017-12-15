using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.ViewModels;
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
            BindingContext = new AllotViewModel { Navigation = Navigation, APage = this };
        }
    }
}