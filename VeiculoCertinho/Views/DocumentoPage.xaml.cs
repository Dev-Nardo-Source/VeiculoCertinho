using System;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.ViewModels;
using System.Threading.Tasks;

namespace VeiculoCertinho.Views
{
    public partial class DocumentoPage : ContentPage
    {
        private DocumentoViewModel? _viewModel;

        public DocumentoPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as DocumentoViewModel;
        }
    }
}
