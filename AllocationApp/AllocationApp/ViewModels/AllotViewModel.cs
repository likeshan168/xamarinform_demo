using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Annotations;
using AllocationApp.Models;
using Xamarin.Forms;

namespace AllocationApp.ViewModels
{
    public class AllotViewModel: INotifyPropertyChanged
    {
        private List<AllocationData> allots;
        private bool isRunning = false;

        public AllotViewModel()
        {
            LoadDataCommand = new Command(async () => await GetDataAsync(), () => !IsRunning);
        }

        private async Task GetDataAsync()
        {
            IsRunning = true;
            allots =  App.ServiceManager.GetListAsync().Result;
            await Application.Current.MainPage.DisplayAlert("提示", "获取数据成功", "确定");
        }

        public List<AllocationData> Allots
        {
            get => allots;
            set
            {
                if (allots != value)
                {
                    allots = value;
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
                }
            }
        }

        public ICommand LoadDataCommand { protected set; get; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
