using System;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.ViewModels;
using System.Threading.Tasks;

namespace VeiculoCertinho.Views
{
    public partial class ControlePneusPage : ContentPage
    {
        private ControlePneusViewModel? _viewModel;

        public ControlePneusPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as ControlePneusViewModel;
        }
    }
}
