using Microsoft.Maui.Controls;

namespace VeiculoCertinho;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Registrar rotas para navegação
		Routing.RegisterRoute("//GaragemPage", typeof(Views.GaragemPage));
		Routing.RegisterRoute("VeiculoPage", typeof(Views.VeiculoPage));
		// Removido registro da rota ExcluirVeiculoPage
		// Routing.RegisterRoute("ExcluirVeiculoPage", typeof(Views.UserDeletePage));
	}
}
