using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using Xamarin.Forms;

namespace TravelPlanner.ViewModel
{
    class AdminPageViewModel
    {
        private readonly DatabaseHelper _helper;
        public AdminPageViewModel()
        {
            _helper = new DatabaseHelper();
        }
    }
}
