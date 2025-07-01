using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Views
{
    public partial class ListaUsuariosPage : ContentPage
    {
        public ListaUsuariosPage(IConfiguration configuration, UsuarioViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
