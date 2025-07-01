using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VeiculoCertinho.Services;

namespace VeiculoCertinho.Tests.Services
{
    [TestClass]
    public class VeiculoEspecificoServiceTests
    {
        private VeiculoEspecificoService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new VeiculoEspecificoService();
        }

        [TestMethod]
        public async Task MonitorarBateriaAsync_ReturnsStatus()
        {
            int veiculoId = 1;
            var result = await _service.MonitorarBateriaAsync(veiculoId);
            Assert.IsTrue(result.Contains("Bateria em"));
            Assert.IsTrue(result.Contains(veiculoId.ToString()));
        }

        [TestMethod]
        public async Task MonitorarTacografoAsync_ReturnsStatus()
        {
            int veiculoId = 2;
            var result = await _service.MonitorarTacografoAsync(veiculoId);
            Assert.IsTrue(result.Contains("Tacógrafo funcionando"));
            Assert.IsTrue(result.Contains(veiculoId.ToString()));
        }

        [TestMethod]
        public async Task MonitorarCorrenteTransmissaoAsync_ReturnsStatus()
        {
            int veiculoId = 3;
            var result = await _service.MonitorarCorrenteTransmissaoAsync(veiculoId);
            Assert.IsTrue(result.Contains("Corrente de transmissão"));
            Assert.IsTrue(result.Contains(veiculoId.ToString()));
        }
    }
}
