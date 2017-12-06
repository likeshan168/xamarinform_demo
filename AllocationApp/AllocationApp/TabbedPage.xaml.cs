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
    public partial class TabbedPage
    {
        public TabbedPage()
        {
            InitializeComponent();

            Children.Add(new MainPage());
            Children.Add(new CallHistoryPage());
            Children.Add(new ListViewPage());
        }
    }
}