using System.Threading.Tasks;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace VeiculoCertinho.Tests.Services
{
    [TestClass]
    public class AbastecimentoServiceTests
    {
        private Mock<AbastecimentoRepositorio> _mockRepositorio;
        private Mock<Microsoft.Extensions.Logging.ILogger<AbastecimentoService>> _mockLogger;
        private AbastecimentoService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepositorio = new Mock<AbastecimentoRepositorio>();
            _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<AbastecimentoService>>();
            _service = new AbastecimentoService(_mockRepositorio.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ObterTodosAsync_ReturnsList()
        {
            var lista = new System.Collections.Generic.List<Abastecimento>
            {
                new Abastecimento { Id = 1, Quantidade = 10, TipoCombustivel = "Gasolina", QuilometragemAtual = 1000 },
                new Abastecimento { Id = 2, Quantidade = 20, TipoCombustivel = "Etanol", QuilometragemAtual = 2000 }
            };
            _mockRepositorio.Setup(r => r.ObterTodosAsync()).ReturnsAsync(lista);

            var result = await _service.ObterTodosAsync();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void CalcularConsumoKmPorUnidade_ReturnsCorrectValue()
        {
            var abastecimentos = new System.Collections.Generic.List<Abastecimento>
            {
                new Abastecimento { QuilometragemAtual = 1000, Quantidade = 10, TipoCombustivel = "Gasolina" },
                new Abastecimento { QuilometragemAtual = 2000, Quantidade = 20, TipoCombustivel = "Gasolina" }
            };

            var consumo = _service.CalcularConsumoKmPorUnidade(abastecimentos, "Gasolina");

            Assert.AreEqual(50, consumo);
        }

        [TestMethod]
        public async Task AdicionarAsync_AddsAbastecimento()
        {
            var abastecimento = new Abastecimento { Id = 1, Quantidade = 10, TipoCombustivel = "Gasolina", QuilometragemAtual = 1000 };
            await _service.AdicionarAsync(abastecimento);
            _mockRepositorio.Verify(r => r.AdicionarAsync(It.Is<Abastecimento>(a => Object.ReferenceEquals(a, abastecimento))), Times.Once);
        

            await _service.AtualizarAsync(abastecimento);
            _mockRepositorio.Verify(r => r.AtualizarAsync(It.Is<Abastecimento>(a => a == abastecimento)), Times.Once);
        

            await _service.AtualizarAsync(abastecimento);
            _mockRepositorio.Verify(r => r.AtualizarAsync(It.Is<Abastecimento>(a => Object.ReferenceEquals(a, abastecimento))), Times.Once);
        

            await _service.RemoverAsync(1);
            _mockRepositorio.Verify(r => r.RemoverAsync(It.Is<int>(id => id == 1)), Times.Once);
        

            await _service.RemoverAsync(1);
            _mockRepositorio.Verify(r => r.RemoverAsync(It.Is<int>(id => id.Equals(1))), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AdicionarAsync_ThrowsArgumentNullException()
        {
            await _service.AdicionarAsync(null);
        }

        [TestMethod]
        public async Task AtualizarAsync_UpdatesAbastecimento()
        {
            var abastecimento = new Abastecimento { Id = 1, Quantidade = 10, TipoCombustivel = "Gasolina", QuilometragemAtual = 1000 };
            await _service.AtualizarAsync(abastecimento);
            _mockRepositorio.Verify(r => r.AtualizarAsync(It.Is<Abastecimento>(a => a == abastecimento)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AtualizarAsync_ThrowsArgumentNullException()
        {
            await _service.AtualizarAsync(null);
        }

        [TestMethod]
        public async Task RemoverAsync_RemovesAbastecimento()
        {
            await _service.RemoverAsync(1);
            _mockRepositorio.Verify(r => r.RemoverAsync(It.Is<int>(id => id == 1)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RemoverAsync_ThrowsArgumentOutOfRangeException()
        {
            await _service.RemoverAsync(0);
        }

        [TestMethod]
        public void CompararCombustiveis_ReturnsBestFuel()
        {
            var abastecimentos = new System.Collections.Generic.List<Abastecimento>
            {
                new Abastecimento { QuilometragemAtual = 1000, Quantidade = 10, TipoCombustivel = "Gasolina" },
                new Abastecimento { QuilometragemAtual = 2000, Quantidade = 20, TipoCombustivel = "Etanol" }
            };
            var combustiveisAceitos = new System.Collections.Generic.List<string> { "Gasolina", "Etanol" };
            var precos = new System.Collections.Generic.Dictionary<string, decimal>
            {
                { "Gasolina", 5.0m },
                { "Etanol", 3.0m }
            };

            var melhor = _service.CompararCombustiveis(abastecimentos, combustiveisAceitos, precos);

            Assert.AreEqual("Etanol", melhor);
        }

        [TestMethod]
        public void CompararCombustiveis_ReturnsNullForNullInputs()
        {
            Assert.IsNull(_service.CompararCombustiveis(null, null, null));
        }
    }
}
