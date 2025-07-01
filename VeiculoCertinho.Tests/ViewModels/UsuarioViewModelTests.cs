using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.ViewModels;
using System.Threading.Tasks;
using System.ComponentModel;
using System;

namespace VeiculoCertinho.Tests.ViewModels
{
    [TestClass]
    public class UsuarioViewModelTests
    {
        private UsuarioViewModel _viewModel;
        private Mock<UserRepository> _mockRepository;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<UserRepository>();
            _viewModel = new UsuarioViewModel();
            // Inject mock repository if possible, else mock repository methods via other means
        }

        [TestMethod]
        public void ValidateNome_EmptyName_SetsError()
        {
            _viewModel.Nome = "";
            Assert.IsTrue(_viewModel.NomeErroVisivel);
            Assert.AreEqual("Nome é obrigatório.", _viewModel.NomeErro);
        }

        [TestMethod]
        public void ValidateEmail_InvalidEmail_SetsError()
        {
            _viewModel.Email = "invalidemail";
            Assert.IsTrue(_viewModel.EmailErroVisivel);
            Assert.AreEqual("Email inválido.", _viewModel.EmailErro);
        }

        [TestMethod]
        public void ValidateSenha_ShortPassword_SetsError()
        {
            _viewModel.Senha = "123";
            Assert.IsTrue(_viewModel.SenhaErroVisivel);
            Assert.AreEqual("Senha deve ter pelo menos 6 caracteres.", _viewModel.SenhaErro);
        }

        [TestMethod]
        public void LoadUsuarios_PopulatesUsuariosCollection()
        {
            var usuarios = new System.Collections.Generic.List<Usuario>
            {
                new Usuario { Id = 1, Nome = "User1", Email = "user1@example.com" },
                new Usuario { Id = 2, Nome = "User2", Email = "user2@example.com" }
            };
            _mockRepository.Setup(r => r.GetAll()).Returns(usuarios);
            _viewModel.LoadUsuarios();
            Assert.AreEqual(2, _viewModel.Usuarios.Count);
        }

        [TestMethod]
        public void UsuarioLogado_Property_SetGet()
        {
            var usuario = new Usuario { Id = 1, Nome = "User1", Email = "user1@example.com" };
            _viewModel.UsuarioLogado = usuario;
            Assert.AreEqual(usuario, _viewModel.UsuarioLogado);
        }

        // Additional tests for commands OnRegistrar, OnEditar, OnExcluir would require more setup and mocking of Application.Current.MainPage and Shell navigation, which is complex in unit tests.
    }
}
