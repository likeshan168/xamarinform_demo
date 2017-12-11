using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Helpers;
using AllocationApp.Models;
using Xamarin.Forms;

namespace AllocationApp.ViewModels
{
    public class AllotViewModel : ViewModelBase
    {
        private ObservableCollection<AllocationData> allots = new ObservableCollection<AllocationData>();
        private bool isRunning;
        private bool isUpdating;
        private int count;
        private int scanedCount;
        private IList<string> masterAwbs = new List<string>();
        private string selectedMasterAwb = string.Empty;
        private string subNo = string.Empty;

        public AllotViewModel()
        {
            LoadDataCommand = new Command(async () => await GetDataAsync(), () => !IsRunning);
            SubNoKeyEnterCommand = new Command(async () => await SearchDataAsync());
            ShowAllData = new Command(() => ShowAllDataAsync());
            UpdateData = new Command(async () => await UpdateDataAsync());
            ResetData = new Command(async () => await GetDataAsync());
            Logout = new Command(async () => await LogoutAsync());
        }

        private async Task LogoutAsync()
        {
            if (ScanedCount > 0)
            {
                if (await Application.Current.MainPage.DisplayAlert("提示", "还有数据未保存,确定退出吗", "确定", "取消"))
                {
                    App.IsUserLoggedIn = false;
                    var currentPage = Application.Current.MainPage;
                    currentPage.Navigation.InsertPageBefore(new LoginPage(), currentPage);
                    await currentPage.Navigation.PopAsync();
                }
            }
        }

        private async Task UpdateDataAsync()
        {
            if (ScanedCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("提示", "没有数据需要更新", "确定");
                return;
            }
            IsUpdating = true;
            var response = await UpdateDataToServerAsync();
            if (response.IsSuccess)
            {
                //保存成功之后清零
                ScanedCount = 0;
            }
            IsUpdating = false;
            await Application.Current.MainPage.DisplayAlert("提示", response.Result, "确定");
        }

        public async Task<ServiceResponse> UpdateDataToServerAsync()
        {
            return await App.ServiceManager.UpdateDataAsync(App.Allocations);
        }


        private void ShowAllDataAsync()
        {
            foreach (var item in App.Allocations)
            {
                if (Allots.FirstOrDefault(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb) != null)
                    continue;
                Allots.Add(item);
            }

            MasterAwbs = allots.Distinct(p => p.MasterAwb).Select(p => p.MasterAwb).ToList();
            Count = allots.Count;
        }

        private async Task SearchDataAsync()
        {

            if (string.IsNullOrWhiteSpace(SubNo))
                return;
            if (string.IsNullOrWhiteSpace(SelectedMasterAwb))
                await Application.Current.MainPage.DisplayAlert("提示", "选择主单号", "确定");

            var rst = App.Allocations.Where(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo);
            var enumerable = rst as AllocationData[] ?? rst.ToArray();
            if (enumerable.Any())
            {
                Allots.Clear();

                foreach (var item in enumerable)
                {
                    item.IsChecked = StateKind.Checked;
                    //更新缓存中的数据，因为需要同步到服务器上
                    App.Allocations.First(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo).IsChecked =
                        StateKind.Checked;
                    Allots.Add(item);
                    ScanedCount = ScanedCount + 1;
                }
            }
            else
            {
                //新增
                if (await Application.Current.MainPage.DisplayAlert("提示", "是否标记为溢装到货", "确定", "取消"))
                {
                    Allots.Clear();
                    var newItem = new AllocationData
                    {
                        IsChecked = StateKind.OverChecked,
                        MasterAwb = SelectedMasterAwb,
                        SubAwb = SubNo,
                        Amount = 1
                    };
                    allots.Add(newItem);
                    Count = allots.Count;
                    App.Allocations.Add(newItem);
                    ScanedCount = ScanedCount + 1;
                }
            }
            //TODO: 需要找到扫码完成之后选中文本
            SubNo = string.Empty;
        }

        private async Task GetDataAsync()
        {
            if (ScanedCount > 0)
            {
                if (!await Application.Current.MainPage.DisplayAlert("提示", "还有数据未保存是否重置", "确定", "取消"))
                {
                    IsRunning = false;
                    return;
                }
            }
            IsRunning = true;
            App.Allocations = await GetListAsync();
            allots.Clear();
            foreach (var item in App.Allocations)
            {
                //重新加载数据，需要把listview中的数据清除掉
                allots.Add(item);
            }
            IsRunning = false;
            Count = allots.Count;
            ScanedCount = 0;
            MasterAwbs = allots.Distinct(p => p.MasterAwb).Select(p => p.MasterAwb).ToList();
            await Application.Current.MainPage.DisplayAlert("提示", "获取数据成功", "确定");
        }

        private async Task<List<AllocationData>> GetListAsync()
        {
            return await App.ServiceManager.GetListAsync();
        }

        public ObservableCollection<AllocationData> Allots
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

        public bool IsUpdating
        {
            get => isUpdating;
            set
            {
                if (isUpdating != value)
                {
                    isUpdating = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Count
        {
            get => count;
            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ScanedCount
        {
            get => scanedCount;
            set
            {
                if (scanedCount != value)
                {
                    scanedCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public IList<string> MasterAwbs
        {
            get => masterAwbs;
            set
            {
                if (!Equals(masterAwbs, value))
                {
                    masterAwbs = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedMasterAwb
        {
            get => selectedMasterAwb;
            set
            {
                if (selectedMasterAwb != null && value != null && selectedMasterAwb != value)
                {
                    selectedMasterAwb = value;
                    var rst = App.Allocations.Where(p => p.MasterAwb == selectedMasterAwb);
                    var enumerable = rst as AllocationData[] ?? rst.ToArray();
                    if (enumerable.Any())
                    {
                        Allots.Clear();
                    }
                    foreach (var item in enumerable)
                    {
                        Allots.Add(item);
                    }
                    Count = Allots.Count;
                    OnPropertyChanged();
                }
            }
        }

        public string SubNo
        {
            get => subNo;
            set
            {
                if (subNo != null && value != null && subNo != value)
                {
                    subNo = value;
                    //var rst = App.Allocations.Where(p => p.MasterAwb == selectedMasterAwb && p.SubAwb == subNo);
                    //var enumerable = rst as AllocationData[] ?? rst.ToArray();
                    //if (enumerable.Any())
                    //{
                    //    Allots.Clear();
                    //}
                    //foreach (var item in enumerable)
                    //{
                    //    Allots.Add(item);
                    //}

                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadDataCommand { protected set; get; }
        public ICommand SubNoKeyEnterCommand { protected set; get; }
        public ICommand ShowAllData { protected set; get; }
        public ICommand UpdateData { protected set; get; }
        public ICommand ResetData { protected set; get; }
        public ICommand Logout { protected set; get; }
    }
}
