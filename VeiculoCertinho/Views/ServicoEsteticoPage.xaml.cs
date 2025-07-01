using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class ServicoEsteticoPage : ContentPage
    {
        private readonly ServicoEsteticoViewModel _viewModel;

        public ServicoEsteticoPage()
        {
            InitializeComponent();
            _viewModel = new ServicoEsteticoViewModel();
            BindingContext = _viewModel;
        }
    }
}
