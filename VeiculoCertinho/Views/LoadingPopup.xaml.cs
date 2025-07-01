using Mopups.Pages;

namespace VeiculoCertinho.Views
{
    public partial class LoadingPopup : PopupPage
    {
        public LoadingPopup(string message = "Aguarde...!!!")
        {
            InitializeComponent();
            MessageLabel.Text = "Aguarde...!!!"; // Sempre usar esta mensagem independente do parâmetro
        }
    }
} 