using System;
using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage()
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel();
            BindingContext = _viewModel;

            // Aqui pode-se carregar os dados iniciais, por exemplo:
            // _viewModel.CarregarIndicadores();
        }
    }
}
