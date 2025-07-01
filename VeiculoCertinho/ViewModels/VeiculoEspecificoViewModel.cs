using System.Threading.Tasks;
using VeiculoCertinho.Services;

namespace VeiculoCertinho.ViewModels
{
    public class VeiculoEspecificoViewModel : BaseViewModel
    {
        private readonly VeiculoEspecificoService _service;

        public VeiculoEspecificoViewModel() : this(new VeiculoEspecificoService())
        {
        }

        public VeiculoEspecificoViewModel(VeiculoEspecificoService service)
        {
            _service = service;
        }

        public Task<string> MonitorarBateriaAsync(int veiculoId)
        {
            return _service.MonitorarBateriaAsync(veiculoId);
        }

        public Task<string> MonitorarTacografoAsync(int veiculoId)
        {
            return _service.MonitorarTacografoAsync(veiculoId);
        }

        public Task<string> MonitorarCorrenteTransmissaoAsync(int veiculoId)
        {
            return _service.MonitorarCorrenteTransmissaoAsync(veiculoId);
        }
    }
}
