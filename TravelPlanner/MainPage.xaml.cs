using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.View;
using Xamarin.Forms;

namespace TravelPlanner
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await SplashLogo.FadeTo(1, 500, Easing.SinIn);
            await Task.Delay(3000);
            await this.FadeTo(0, 500, Easing.SinOut);

            var LoginPage = new View.LoginPage();
            LoginPage.Opacity = 0;
            Application.Current.MainPage = LoginPage;
            await LoginPage.FadeTo(1, 500, Easing.SinIn);
        }
    }
}
