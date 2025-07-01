using VeiculoCertinho.Models;

namespace VeiculoCertinho.Services
{
    public interface IVeiculoConsultaServiceSelenium
    {
        Task<Veiculo?> ConsultarVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
    }
} 