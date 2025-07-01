using System;
using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class AbastecimentoPage : ContentPage
    {
        private readonly AbastecimentoViewModel _viewModel;

        public AbastecimentoPage()
        {
            InitializeComponent();
            _viewModel = (AbastecimentoViewModel)BindingContext;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarAsync();
        }
    }
}
