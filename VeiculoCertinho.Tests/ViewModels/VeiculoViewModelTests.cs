using Microsoft.VisualStudio.TestTools.UnitTesting;
using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace VeiculoCertinho.Tests.ViewModels
{
    [TestClass]
    public class VeiculoViewModelTests
    {
        private VeiculoViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _viewModel = new VeiculoViewModel();
        }

        [TestMethod]
        public void AdicionarVeiculo_AdicionaVeiculoNaColecao()
        {
            var veiculo = new Veiculo { Id = 1, Modelo = "Modelo1" };
            _viewModel.AdicionarVeiculo(veiculo);

            Assert.AreEqual(1, _viewModel.Veiculos.Count);
            Assert.AreEqual(veiculo, _viewModel.Veiculos[0]);
        }

        [TestMethod]
        public void AdicionarDetalhes_AdicionaDetalhesNaColecao()
        {
            var detalhes = new VeiculoDetalhes { Id = 1, Cor = "Vermelho" };
            _viewModel.AdicionarDetalhes(detalhes);

            Assert.AreEqual(1, _viewModel.VeiculosDetalhes.Count);
            Assert.AreEqual(detalhes, _viewModel.VeiculosDetalhes[0]);
        }

        [TestMethod]
        public void VeiculoSelecionado_AlteraValorEDisparaEvento()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.VeiculoSelecionado))
                    eventRaised = true;
            };

            var veiculo = new Veiculo { Id = 1 };
            _viewModel.VeiculoSelecionado = veiculo;

            Assert.IsTrue(eventRaised);
            Assert.AreEqual(veiculo, _viewModel.VeiculoSelecionado);
        }

        [TestMethod]
        public void DetalhesSelecionados_AlteraValorEDisparaEvento()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.DetalhesSelecionados))
                    eventRaised = true;
            };

            var detalhes = new VeiculoDetalhes { Id = 1 };
            _viewModel.DetalhesSelecionados = detalhes;

            Assert.IsTrue(eventRaised);
            Assert.AreEqual(detalhes, _viewModel.DetalhesSelecionados);
        }

        [TestMethod]
        public void AtualizarVeiculo_DisparaEventoPropertyChanged()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.Veiculos))
                    eventRaised = true;
            };

            _viewModel.AtualizarVeiculo(new Veiculo { Id = 1 });

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void AtualizarDetalhes_DisparaEventoPropertyChanged()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.VeiculosDetalhes))
                    eventRaised = true;
            };

            _viewModel.AtualizarDetalhes(new VeiculoDetalhes { Id = 1 });

            Assert.IsTrue(eventRaised);
        }
    }
}
