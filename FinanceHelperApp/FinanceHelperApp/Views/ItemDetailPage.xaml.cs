using FinanceHelperApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace FinanceHelperApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}