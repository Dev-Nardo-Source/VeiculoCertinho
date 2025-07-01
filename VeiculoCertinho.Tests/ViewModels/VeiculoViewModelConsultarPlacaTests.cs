using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Tests.ViewModels
{
    [TestClass]
    public class VeiculoViewModelConsultarPlacaTests
    {
        private Mock<IVeiculoConsultaService> _mockConsultaService;
        private VeiculoViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _mockConsultaService = new Mock<IVeiculoConsultaService>();
            _viewModel = new VeiculoViewModel(_mockConsultaService.Object, null);
        }

        [TestMethod]
        public async Task ConsultarPlacaAsync_DeveAtualizarVeiculoComDadosConsultados()
        {
            // Arrange
            var placa = "ABC1234";
            var veiculoConsultado = new Veiculo
            {
                Uf = "SP",
                Municipio = "São Paulo",
                Marca = "MarcaTeste",
                Modelo = "ModeloTeste",
                Ano = 2020,
                AnoModelo = 2021,
                Cor = "Preto",
                Combustivel = "Gasolina",
                Placa = placa,
                Cilindrada = "2000",
                Potencia = "150",
                Passageiros = 5,
                Segmento = "Passeio",
                EspecieVeiculo = "Automóvel",
                Tipo = "TipoTeste",
                Chassi = "123456789",
                Motor = "MotorTeste",
                Observacoes = "Observações teste"
            };

            _mockConsultaService
                .Setup(s => s.ConsultarVeiculoPorPlacaAsync(placa, It.IsAny<CancellationToken>()))
                .ReturnsAsync(veiculoConsultado);

            // Act
            var resultado = await _viewModel.ConsultarPlacaAsync(placa);

            // Assert
            Assert.IsTrue(resultado);
            Assert.AreEqual(veiculoConsultado.Uf, _viewModel.Veiculo.Uf);
            Assert.AreEqual(veiculoConsultado.Municipio, _viewModel.Veiculo.Municipio);
            Assert.AreEqual(veiculoConsultado.Marca, _viewModel.Veiculo.Marca);
            Assert.AreEqual(veiculoConsultado.Modelo, _viewModel.Veiculo.Modelo);
            Assert.AreEqual(veiculoConsultado.Ano, _viewModel.Veiculo.Ano);
            Assert.AreEqual(veiculoConsultado.AnoModelo, _viewModel.Veiculo.AnoModelo);
            Assert.AreEqual(veiculoConsultado.Cor, _viewModel.Veiculo.Cor);
            Assert.AreEqual(veiculoConsultado.Combustivel, _viewModel.Veiculo.Combustivel);
            Assert.AreEqual(veiculoConsultado.Placa, _viewModel.Veiculo.Placa);
            Assert.AreEqual(veiculoConsultado.Cilindrada, _viewModel.Veiculo.Cilindrada);
            Assert.AreEqual(veiculoConsultado.Potencia, _viewModel.Veiculo.Potencia);
            Assert.AreEqual(veiculoConsultado.Passageiros, _viewModel.Veiculo.Passageiros);
            Assert.AreEqual(veiculoConsultado.Segmento, _viewModel.Veiculo.Segmento);
            Assert.AreEqual(veiculoConsultado.EspecieVeiculo, _viewModel.Veiculo.EspecieVeiculo);
            Assert.AreEqual(veiculoConsultado.Tipo, _viewModel.Veiculo.Tipo);
            Assert.AreEqual(veiculoConsultado.Chassi, _viewModel.Veiculo.Chassi);
            Assert.AreEqual(veiculoConsultado.Motor, _viewModel.Veiculo.Motor);
            Assert.AreEqual(veiculoConsultado.Observacoes, _viewModel.Veiculo.Observacoes);
        }

        [TestMethod]
        public async Task ConsultarPlacaAsync_VeiculoNaoEncontrado_DeveRetornarFalse()
        {
            // Arrange
            var placa = "XYZ9999";
            _mockConsultaService
                .Setup(s => s.ConsultarVeiculoPorPlacaAsync(placa, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Veiculo)null);

            // Act
            var resultado = await _viewModel.ConsultarPlacaAsync(placa);

            // Assert
            Assert.IsFalse(resultado);
        }
    }
}
