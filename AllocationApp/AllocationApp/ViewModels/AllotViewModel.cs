using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Helpers;
using AllocationApp.Models;
using Realms;
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
        private IList<AllocationData> masterAwbs = new List<AllocationData>();
        private AllocationData selectedMasterAwb = new AllocationData();
        private string subNo = string.Empty;

        private string summary = "已到货：{0}，未到货：{1}，溢装到货：{2}";

        private Realm _realm;

        public AllotViewModel()
        {
            LoadDataCommand = new Command(async () => await GetDataAsync(), () => !IsRunning);
            SubNoKeyEnterCommand = new Command(async () => await SearchDataAsync());
            ShowAllData = new Command(ShowAllDataAsync);
            UpdateData = new Command(async () => await UpdateDataAsync());
            ResetData = new Command(async () => await ResetDataAsync());
            Logout = new Command(async () => await LogoutAsync());

            ClearAllData = new Command(async () => await ClearAllDataAsync());
            var config = new RealmConfiguration { SchemaVersion = 1 };
            _realm = Realm.GetInstance(config);
            //管理员查看所有的
            if (App.IsAdmin)
            {
                Entries = _realm.All<AllocationData>().Where(p => p.IsShow);
                MasterAwbs = _realm.All<AllocationData>().AsEnumerable().Distinct(new AllocationDataComparer()).ToList();
            }
            else
            {
                Entries = _realm.All<AllocationData>().Where(p => p.IsShow && p.TenantId == App.TenantId);
                MasterAwbs = _realm.All<AllocationData>().Where(p => p.TenantId == App.TenantId).AsEnumerable().Distinct(new AllocationDataComparer()).ToList();
            }


            //MasterAwbs = _realm.All<AllocationData>().AsEnumerable().Distinct(p => p.MasterAwb).Select(p => p.MasterAwb).ToList();
        }

        private async Task ClearAllDataAsync()
        {
            if (!await Application.Current.MainPage.DisplayAlert("提示", "你确定要清空本地数据吗", "确定", "取消"))
                return;
            if (App.IsAdmin)
            {
                _realm.Write(() => _realm.RemoveAll<AllocationData>());
            }
            else
            {
                var toDeleteItems = _realm.All<AllocationData>().Where(p => p.TenantId == App.TenantId);
                _realm.Write(() => _realm.RemoveRange(toDeleteItems));
            }

            App.CheckedAllocations.Clear();
            MasterAwbs = default(IList<AllocationData>);
            ScanedCount = 0;
            Count = 0;
            OnPropertyChanged(nameof(Summary));
            await Application.Current.MainPage.DisplayAlert("提示", "数据已清空", "确定");
        }

        private async Task ResetDataAsync()
        {
            //恢复到上一步
            if (ScanedCount > 0)
            {
                if (await Application.Current.MainPage.DisplayAlert("提示", "确定要恢复至上一步吗", "确定", "取消"))
                {
                    ScanedCount = ScanedCount - 1;
                    //取最上面的一个进行恢复
                    if (App.CheckedAllocations.Count > 0)
                    {
                        var item = App.CheckedAllocations.Pop();
                        if (item.IsChecked == 2)
                        {
                            AllocationData matchedItem;
                            if (App.IsAdmin)
                            {
                                matchedItem = _realm
                                    .All<AllocationData>().FirstOrDefault(p =>
                                        p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb);
                            }
                            else
                            {
                                matchedItem = _realm
                                    .All<AllocationData>().FirstOrDefault(p =>
                                        p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb && p.TenantId == App.TenantId);
                            }

                            _realm.Write(() => matchedItem.IsChecked = 1);
                        }
                        else if (item.IsChecked == 3)
                        {
                            _realm.Write(() => _realm.Remove(item));
                        }
                        OnPropertyChanged(nameof(Summary));
                        await Application.Current.MainPage.DisplayAlert("提示", "已经恢复至上一步", "确定");
                    }
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("提示", "已经恢复至初始状态", "确定");
            }
        }

        private async Task LogoutAsync()
        {
            if (ScanedCount > 0)
            {
                if (await Application.Current.MainPage.DisplayAlert("提示", "还有数据未保存,确定退出吗", "确定", "取消"))
                {
                    App.IsUserLoggedIn = false;
                    Navigation.InsertPageBefore(new LoginPage(), APage);
                    await Navigation.PopAsync(true);
                }
            }
            else
            {
                App.IsUserLoggedIn = false;
                Navigation.InsertPageBefore(new LoginPage(), APage);
                await Navigation.PopAsync(true);
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
                App.CheckedAllocations.Clear();
                SelectedMasterAwb = new AllocationData();
            }
            IsUpdating = false;
            await Application.Current.MainPage.DisplayAlert("提示", response.Result, "确定");
        }

        public async Task<ServiceResponse> UpdateDataToServerAsync()
        {
            return await App.ServiceManager.UpdateDataAsync(App.CheckedAllocations.ToList());
        }


        private void ShowAllDataAsync()
        {
            IEnumerable<AllocationData> items;
            if (App.IsAdmin)
            {
                items = _realm.All<AllocationData>();
            }
            else
            {
                items = _realm.All<AllocationData>().Where(p => p.TenantId == App.TenantId);
            }
            foreach (var item in items)
            {
                _realm.Write(() => item.IsShow = true);
            }

            Count = Entries.Count();
            MasterAwbs = _realm.All<AllocationData>().Where(p => p.TenantId == App.TenantId).AsEnumerable().Distinct(new AllocationDataComparer()).ToList();
            OnPropertyChanged(nameof(Summary));
        }

        private async Task SearchDataAsync()
        {

            try
            {
                if (string.IsNullOrWhiteSpace(SubNo))
                    return;
                if (string.IsNullOrWhiteSpace(SelectedMasterAwb?.MasterAwb))
                {
                    await Application.Current.MainPage.DisplayAlert("提示", "选择主单号", "确定");
                    return;
                }

                AllocationData rst;
                if (App.IsAdmin)
                {
                    rst = _realm.All<AllocationData>().FirstOrDefault(p => p.MasterAwb == SelectedMasterAwb.MasterAwb && p.SubAwb == SubNo);
                }
                else
                {
                    rst = _realm.All<AllocationData>().FirstOrDefault(p => p.MasterAwb == SelectedMasterAwb.MasterAwb && p.SubAwb == SubNo && p.TenantId == App.TenantId);
                }
                //将其他的隐藏掉
                var others = _realm.All<AllocationData>()
                    .Where(p => p.MasterAwb == SelectedMasterAwb.MasterAwb && p.SubAwb != SubNo);
                _realm.Write(() =>
                {
                    foreach (var item in others)
                    {
                        item.IsShow = false;
                    }
                });
                if (rst != null)
                {
                    // 如果已经盘点过就不要再进行盘点
                    if (rst.IsChecked != 2)
                    {
                        _realm.Write(() => rst.IsChecked = 2);
                        if (App.CheckedAllocations.Contains(rst))
                        {
                            foreach (var data in App.CheckedAllocations)
                            {
                                if (data.MasterAwb == SelectedMasterAwb.MasterAwb && data.SubAwb == SubNo)
                                {
                                    data.IsChecked = 2;
                                }
                            }
                        }
                        else
                        {
                            App.CheckedAllocations.Push(rst);
                        }
                        ScanedCount = ScanedCount + 1;
                        Count = Entries.Count();
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
                            IsChecked = 3,
                            MasterAwb = SelectedMasterAwb.MasterAwb,
                            SubAwb = SubNo,
                            Amount = 1,
                            IsShow = true
                        };
                        if (!App.IsAdmin)
                        {
                            newItem.TenantId = App.TenantId;
                        }
                        _realm.Write(() => _realm.Add(newItem));
                        Count = Entries.Count();
                        App.CheckedAllocations.Push(newItem);
                        ScanedCount = ScanedCount + 1;
                    }
                }
                //TODO: 需要找到扫码完成之后选中文本
                SubNo = string.Empty;
                OnPropertyChanged(nameof(Summary));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("提示", $"盘点出错:{ex.Message}", "确定");
            }
        }

        private async Task GetDataAsync()
        {
            try
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
                var response = await GetListAsync();
                if (!response.IsSuccess)
                {
                    IsRunning = false;
                    await Application.Current.MainPage.DisplayAlert("提示", response.Message, "确定");
                    return;
                }
                //将本地所有的收货的都同步到服务器上
                IQueryable<AllocationData> localCheckedItems;
                if (App.IsAdmin)
                {
                    localCheckedItems = _realm.All<AllocationData>().Where(p => p.IsChecked == 2 || p.IsChecked == 3);
                }
                else
                {
                    localCheckedItems = _realm.All<AllocationData>().Where(p => (p.IsChecked == 2 || p.IsChecked == 3) && p.TenantId == App.TenantId);
                }
                //先同步到服务器，然后下载服务器的最新数据
                if (localCheckedItems.Any())
                {
                    if (localCheckedItems.Any())
                    {
                        var serviceResponse = await App.ServiceManager.UpdateDataAsync(localCheckedItems.ToList());
                        if (!serviceResponse.IsSuccess)
                        {
                            IsRunning = false;
                            await Application.Current.MainPage.DisplayAlert("提示", "同步数据失败", "确定");
                            return;
                        }
                    }
                }

                foreach (var item in response.Entities)
                {
                    AllocationData existedItem;
                    if (App.IsAdmin)
                    {
                        existedItem = _realm.All<AllocationData>()
                            .FirstOrDefault(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb);
                    }
                    else
                    {
                        existedItem = _realm.All<AllocationData>()
                            .FirstOrDefault(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb && p.TenantId == App.TenantId);
                    }

                    //主要是更新到货的状态，如果是在网页上进行的盘点操作，那就需要更新到app中的数据库中
                    if (existedItem != null)
                    {
                        _realm.Write(() => existedItem.IsShow = true);
                        if (item.IsChecked == 2 && existedItem.IsChecked != 2)
                            _realm.Write(() => existedItem.IsChecked = 2);
                    }

                    if (existedItem == null)
                    {
                        item.IsShow = true;
                        _realm.Write(() => _realm.Add(item));
                    }
                }
                //如果是服务器上的不存在，在app本地存在的，则需要删除掉，否则在同步的过程中又会同步到服务器上
                //TODO: 先不考虑这个删除的问题

                IsRunning = false;
                Count = Entries.Count();
                ScanedCount = 0;
                if (App.IsAdmin)
                {
                    MasterAwbs = _realm.All<AllocationData>().AsEnumerable()
                        .Distinct(new AllocationDataComparer()).ToList();
                }
                else
                {
                    MasterAwbs = _realm.All<AllocationData>().Where(p => p.TenantId == App.TenantId).AsEnumerable()
                        .Distinct(new AllocationDataComparer()).ToList();
                }
                SelectedMasterAwb = new AllocationData();
                OnPropertyChanged(nameof(Summary));
                await Application.Current.MainPage.DisplayAlert("提示", "同步数据成功", "确定");
            }
            catch (Exception ex)
            {
                IsRunning = false;
                await Application.Current.MainPage.DisplayAlert("提示", "同步数据异常:" + ex.Message, "确定");
            }
        }

        private async Task<GetListResponse<AllocationData>> GetListAsync()
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
        //public IList<string> MasterAwbs
        //{
        //    get => masterAwbs;
        //    set
        //    {
        //        if (!Equals(masterAwbs, value))
        //        {
        //            masterAwbs = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        public IList<AllocationData> MasterAwbs
        {
            get => masterAwbs;
            set
            {
                if (value != null && masterAwbs != null && !Equals(masterAwbs, value))
                {
                    masterAwbs = value;
                    OnPropertyChanged();
                }
            }
        }

        public AllocationData SelectedMasterAwb
        {
            get => selectedMasterAwb;
            set
            {
                if (selectedMasterAwb != null && value != null && value.MasterAwb != null && selectedMasterAwb != value)
                {
                    IEnumerable<AllocationData> hideItems;
                    IEnumerable<AllocationData> showItems;
                    if (App.IsAdmin)
                    {
                        hideItems = _realm.All<AllocationData>().Where(p => p.MasterAwb != value.MasterAwb);
                        showItems = _realm.All<AllocationData>().Where(p => p.MasterAwb == value.MasterAwb);
                    }
                    else
                    {
                        hideItems = _realm.All<AllocationData>().Where(p => p.MasterAwb != value.MasterAwb && p.TenantId == App.TenantId);
                        showItems = _realm.All<AllocationData>().Where(p => p.MasterAwb == value.MasterAwb && p.TenantId == App.TenantId);
                    }

                    foreach (var item in hideItems)
                    {
                        _realm.Write(() => item.IsShow = false);
                    }
                    foreach (var item in showItems)
                    {
                        _realm.Write(() => item.IsShow = true);
                    }
                    Count = Entries.Count();
                    selectedMasterAwb = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Summary));
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
                    OnPropertyChanged();
                }
            }
        }

        public string Summary => string.Format(summary,
           Entries.Count(p => p.IsChecked == 2),
            Entries.Count(p => p.IsChecked == 1),
        Entries.Count(p => p.IsChecked == 3));

        public IEnumerable<AllocationData> Entries { get; }
        public INavigation Navigation { get; set; }
        public ContentPage APage { get; set; }

        public ICommand LoadDataCommand { protected set; get; }
        public ICommand SubNoKeyEnterCommand { protected set; get; }
        public ICommand ShowAllData { protected set; get; }
        public ICommand UpdateData { protected set; get; }
        public ICommand ResetData { protected set; get; }
        public ICommand Logout { protected set; get; }
        public ICommand ClearAllData { protected set; get; }
    }
}
