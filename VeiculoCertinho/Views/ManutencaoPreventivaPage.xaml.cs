using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class ManutencaoPreventivaPage : ContentPage
    {
        private readonly ManutencaoPreventivaViewModel _viewModel;

        public ManutencaoPreventivaPage()
        {
            InitializeComponent();
            _viewModel = new ManutencaoPreventivaViewModel();
            BindingContext = _viewModel;
        }
    }
}
