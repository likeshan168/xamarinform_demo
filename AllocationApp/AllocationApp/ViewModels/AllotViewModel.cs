using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using AllocationApp.Helpers;
using AllocationApp.Models;
using Plugin.MediaManager;
using Plugin.SimpleAudioPlayer.Abstractions;
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
        private IList<string> masterAwbs = new List<string>();
        private string selectedMasterAwb = string.Empty;
        private string subNo = string.Empty;

        private string summary = "已到货：{0}，未到货：{1}，溢装到货：{2}";
        private Realm _realm;
        ISimpleAudioPlayer okPlayer;
        ISimpleAudioPlayer yizhuangPlayer;
        Stream ok;
        Stream yizhuang;
        public AllotViewModel()
        {
            LoadDataCommand = new Command(async () => await GetDataAsync(), () => !IsRunning);
            SubNoKeyEnterCommand = new Command(async () => await SearchDataAsync());
            ShowAllData = new Command(() => ShowAllDataAsync());
            UpdateData = new Command(async () => await UpdateDataAsync());
            ResetData = new Command(async () => await ResetDataAsync());
            Logout = new Command(async () => await LogoutAsync());
            var config = new RealmConfiguration { SchemaVersion = 1 };
            _realm = Realm.GetInstance(config);
            ScanedCount = App.CheckedAllocations.Count;
            okPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            yizhuangPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            ok = GetStreamFromFile("ok.mp3");
            yizhuang = GetStreamFromFile("yizhuang.mp3");
            okPlayer.Load(ok);
            yizhuangPlayer.Load(yizhuang);
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
                        if (item.IsChecked == StateKind.Checked)
                        {
                            App.Allocations.First(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb)
                                .IsChecked = StateKind.NoChecked;
                            Allots.First(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb)
                                .IsChecked = StateKind.NoChecked;

                            //更新本地数据库中的数据
                            _realm.Write(() =>
                            {
                                var existedItem = _realm.All<AllocationData2>().FirstOrDefault(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb);
                                if (existedItem != null)
                                {
                                    existedItem.IsChecked = 1;
                                }
                            });
                        }
                        else if (item.IsChecked == StateKind.OverChecked)
                        {
                            App.Allocations.Remove(item);
                            Allots.Remove(item);
                            //删除本地数据当中的数据
                            _realm.Write(() =>
                            {
                                var existedItem = _realm.All<AllocationData2>().FirstOrDefault(p => p.MasterAwb == item.MasterAwb && p.SubAwb == item.SubAwb);
                                if (existedItem != null)
                                {
                                    _realm.Remove(existedItem);
                                }
                            });
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
                App.CheckedAllocations.Clear();
                SelectedMasterAwb = string.Empty;
                //清空本地数据库
                _realm.Write(() =>
                {
                    _realm.RemoveAll<AllocationData2>();
                });
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

            try
            {
                if (string.IsNullOrWhiteSpace(SubNo))
                    return;
                if (string.IsNullOrWhiteSpace(SelectedMasterAwb))
                {
                    await Application.Current.MainPage.DisplayAlert("提示", "选择主单号", "确定");
                    return;
                }
                var rst = App.Allocations.Where(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo);
                var enumerable = rst as AllocationData[] ?? rst.ToArray();
                if (enumerable.Any())
                {
                    var firstItem = enumerable.First();
                    // 如果已经盘点过就不要再进行盘点
                    if (firstItem.IsChecked != StateKind.Checked)
                    {
                        //清理
                        Allots.Clear();
                        firstItem.IsChecked = StateKind.Checked;

                        App.Allocations.First(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo).IsChecked =
                            StateKind.Checked;
                        //更新缓存中的数据，因为需要同步到服务器上
                        //App.CheckedAllocations.First(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo).IsChecked =
                        //    StateKind.Checked;
                        if (App.CheckedAllocations.Contains(firstItem))
                        {
                            foreach (var data in App.CheckedAllocations)
                            {
                                if (data.MasterAwb == SelectedMasterAwb && data.SubAwb == SubNo)
                                {
                                    data.IsChecked = StateKind.Checked;
                                }
                            }
                        }
                        else
                        {
                            App.CheckedAllocations.Push(firstItem);
                        }
                        //这个是持久化保存，就是防止手持设备突然退出时候，数据能够备份
                        _realm.Write(() =>
                        {
                            var first = _realm.All<AllocationData2>().FirstOrDefault(p => p.MasterAwb == SelectedMasterAwb && p.SubAwb == SubNo);
                            if (first != null)
                            {
                                first.IsChecked = 2;
                            }
                            else
                            {
                                _realm.Add(new AllocationData2
                                {
                                    Amount = firstItem.Amount,
                                    IsChecked = (int)firstItem.IsChecked,
                                    MasterAwb = firstItem.MasterAwb,
                                    SubAwb = firstItem.SubAwb,
                                });
                            }
                        });

                        okPlayer.Play();

                        Allots.Add(firstItem);
                        ScanedCount = ScanedCount + 1;
                        Count = Allots.Count;
                    }
                }
                else
                {
                    //新增
                    yizhuangPlayer.Play();
                    //http://101.201.28.235:91/version/nodata.wav
                    await CrossMediaManager.Current.Play("http://101.201.28.235:91/version/nodata.wav");
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
                        Allots.Add(newItem);
                        Count = allots.Count;
                        App.Allocations.Add(newItem);
                        App.CheckedAllocations.Push(newItem);
                        ScanedCount = ScanedCount + 1;
                        //添加到本地数据库
                        _realm.Write(() =>
                        {
                            _realm.Add(new AllocationData2
                            {
                                Amount = 1,
                                IsChecked = 3,
                                MasterAwb = SelectedMasterAwb,
                                SubAwb = SubNo,
                            });

                        });
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

        Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;

            var stream = assembly.GetManifestResourceStream("AllocationApp." + filename);

            return stream;
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
            var response = await GetListAsync();
            if (!response.IsSuccess)
            {
                IsRunning = false;
                await Application.Current.MainPage.DisplayAlert("提示", response.Message, "确定");
                return;
            }
            App.Allocations = response.Entities.ToList();
            allots.Clear();
            foreach (var item in App.Allocations)
            {
                //重新加载数据，需要把listview中的数据清除掉
                allots.Add(item);
            }
            IsRunning = false;
            Count = allots.Count;
            ScanedCount = 0;
            SelectedMasterAwb = string.Empty;
            MasterAwbs = allots.Distinct(p => p.MasterAwb).Select(p => p.MasterAwb).ToList();
            OnPropertyChanged(nameof(Summary));

            //清理掉缓存和本地数据库库中的数据
            App.CheckedAllocations.Clear();
            _realm.Write(() => _realm.RemoveAll<AllocationData2>());

            await Application.Current.MainPage.DisplayAlert("提示", "获取数据成功", "确定");
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
                    var rst = App.Allocations.Where(p => p.MasterAwb == value);
                    var enumerable = rst as AllocationData[] ?? rst.ToArray();
                    if (enumerable.Any())
                    {
                        Allots.Clear();
                        foreach (var item in enumerable)
                        {
                            Allots.Add(item);
                        }
                        Count = Allots.Count;
                    }

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

        public string Summary => string.Format(summary,
          string.IsNullOrWhiteSpace(SelectedMasterAwb) ? App.Allocations.Count(p => p.IsChecked == StateKind.Checked) : App.Allocations.Count(p => p.IsChecked == StateKind.Checked && p.MasterAwb == SelectedMasterAwb),
            string.IsNullOrWhiteSpace(SelectedMasterAwb) ? App.Allocations.Count(p => p.IsChecked == StateKind.NoChecked) : App.Allocations.Count(p => p.IsChecked == StateKind.NoChecked && p.MasterAwb == SelectedMasterAwb),
            string.IsNullOrWhiteSpace(SelectedMasterAwb) ? App.Allocations.Count(p => p.IsChecked == StateKind.OverChecked) : App.Allocations.Count(p => p.IsChecked == StateKind.OverChecked && p.MasterAwb == SelectedMasterAwb));

        public ICommand LoadDataCommand { protected set; get; }
        public ICommand SubNoKeyEnterCommand { protected set; get; }
        public ICommand ShowAllData { protected set; get; }
        public ICommand UpdateData { protected set; get; }
        public ICommand ResetData { protected set; get; }
        public ICommand Logout { protected set; get; }
    }
}
