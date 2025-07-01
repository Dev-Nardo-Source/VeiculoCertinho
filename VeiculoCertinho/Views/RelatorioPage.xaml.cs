using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Services;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Configuration;

namespace VeiculoCertinho.Views
{
    public partial class RelatorioPage : ContentPage
    {
        private readonly RelatorioViewModel _viewModel;

        public RelatorioPage()
        {
            InitializeComponent();
            _viewModel = new RelatorioViewModel();
            BindingContext = _viewModel;
        }
    }
}
