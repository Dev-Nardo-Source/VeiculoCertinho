using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class ManutencaoCorretivaPage : ContentPage
    {
        private ManutencaoCorretivaViewModel? _viewModel;

        public ManutencaoCorretivaPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as ManutencaoCorretivaViewModel;
        }
    }
}
