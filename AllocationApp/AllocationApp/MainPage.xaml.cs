using System;
using Xamarin.Forms;

namespace AllocationApp
{
    public partial class MainPage
    {
        string _translatedNumber;
        public MainPage()
        {
            InitializeComponent();
        }

        void OnTranslate(object sender, EventArgs e)
        {
            _translatedNumber = PhonewordTranslator.ToNumber(PhoneNumberText.Text);
            if (!string.IsNullOrWhiteSpace(_translatedNumber))
            {
                CallButton.IsEnabled = true;
                CallButton.Text = "Call " + _translatedNumber;
            }
            else
            {
                CallButton.IsEnabled = false;
                CallButton.Text = "Call";
            }
        }

        async void OnCall(object sender, EventArgs e)
        {
            if (await DisplayAlert(
                "Dial a Number",
                "Would you like to call " + _translatedNumber + "?",
                "Yes",
                "No"))
            {
                var dialer = DependencyService.Get<IDialer>();
                if (dialer != null)
                {
                    App.PhoneNumbers.Add(_translatedNumber);
                    CallHistoryButton.IsEnabled = true;
                    dialer.Dial(_translatedNumber);
                }
            }
        }

        async void OnCallHistory(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CallHistoryPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误信息", ex.Message, "yes");
            }
        }

        private async void NavigateToSecondPage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SecondPage(PhoneNumberText.Text));
        }
    }
}
