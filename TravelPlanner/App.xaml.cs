using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.StyleSheets;
using Xamarin.Forms.Xaml;

namespace TravelPlanner
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            //NavigateToLoginAfterDelay();
        }

        private async void NavigateToLoginAfterDelay()
        {
            await Task.Delay(5000);
            Application.Current.MainPage = new View.LoginPage(); 
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
