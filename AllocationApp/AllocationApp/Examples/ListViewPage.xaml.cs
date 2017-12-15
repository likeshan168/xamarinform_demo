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
    public partial class ListViewPage : ContentPage
    {
        public ListViewPage()
        {
            InitializeComponent();

            PersonList.ItemsSource = new List<Person>
            {
                new Person{Name="Sherman",Age=34},
                new Person{Name="Jack",Age=24},
                new Person{Name="Likeshan",Age=20}
            };
        }
    }
}