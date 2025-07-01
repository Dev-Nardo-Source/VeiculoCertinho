using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace VeiculoCertinho.Views
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            NavigateToLoginPageAsync();
        }

        private async void NavigateToLoginPageAsync()
        {
            await Task.Delay(3000); // 3 seconds delay for splash screen
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
