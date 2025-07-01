using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class UsuarioService : BaseService
    {
        private readonly UsuarioRepositorio _repositorio;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(UsuarioRepositorio repositorio, ILogger<UsuarioService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os usuários de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos Usuario.</returns>
        public async Task<List<Usuario>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuários.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo usuário de forma assíncrona.
        /// </summary>
        /// <param name="usuario">Objeto Usuario a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                await _repositorio.AdicionarAsync(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar usuário.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um usuário existente de forma assíncrona.
        /// </summary>
        /// <param name="usuario">Objeto Usuario a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                await _repositorio.AtualizarAsync(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário.");
                throw;
            }
        }

        /// <summary>
        /// Remove um usuário pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do usuário a ser removido.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task RemoverAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "ID deve ser maior que zero.");

            try
            {
                await _repositorio.RemoverAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover usuário.");
                throw;
            }
        }
    }
}
