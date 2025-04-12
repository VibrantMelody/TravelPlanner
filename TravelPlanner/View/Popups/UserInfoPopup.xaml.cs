using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TravelPlanner.View.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UserInfoPopup : PopupPage
	{
		public UserInfoPopup (TripUsers user)
		{
			InitializeComponent ();
			System.Diagnostics.Debug.WriteLine(user);
			BindingContext = user;
		}
	}
}