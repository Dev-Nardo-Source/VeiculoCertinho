using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa o compartilhamento de um veículo entre usuários.
    /// </summary>
    public class Compartilhamento : BaseModel
    {
        private int _veiculoId;
        private int _usuarioId;
        private string _permissoes = "leitura";
        private bool _ativo = true;

        /// <summary>
        /// ID do veículo compartilhado.
        /// </summary>
        [Required(ErrorMessage = "Veículo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Veículo deve ser selecionado")]
        public int VeiculoId
        {
            get => _veiculoId;
            set => SetProperty(ref _veiculoId, value);
        }

        /// <summary>
        /// ID do usuário com quem o veículo é compartilhado.
        /// </summary>
        [Required(ErrorMessage = "Usuário é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Usuário deve ser selecionado")]
        public int UsuarioId
        {
            get => _usuarioId;
            set => SetProperty(ref _usuarioId, value);
        }

        /// <summary>
        /// Permissões do usuário (leitura, escrita, admin).
        /// </summary>
        [Required(ErrorMessage = "Permissões são obrigatórias")]
        [StringLength(50, ErrorMessage = "Permissões devem ter no máximo 50 caracteres")]
        public string Permissoes
        {
            get => _permissoes;
            set => SetStringProperty(ref _permissoes, value?.ToLowerInvariant());
        }

        /// <summary>
        /// Data do compartilhamento (herda de BaseModel.CreatedAt).
        /// </summary>
        public DateTime DataCompartilhamento => CreatedAt;

        /// <summary>
        /// Indica se o compartilhamento está ativo.
        /// </summary>
        public bool Ativo
        {
            get => _ativo;
            set => SetProperty(ref _ativo, value);
        }

        /// <summary>
        /// Verifica se o usuário tem permissão de leitura.
        /// </summary>
        public bool TemPermissaoLeitura => Ativo && (
            Permissoes.Contains("leitura") || 
            Permissoes.Contains("escrita") || 
            Permissoes.Contains("admin"));

        /// <summary>
        /// Verifica se o usuário tem permissão de escrita.
        /// </summary>
        public bool TemPermissaoEscrita => Ativo && (
            Permissoes.Contains("escrita") || 
            Permissoes.Contains("admin"));

        /// <summary>
        /// Verifica se o usuário tem permissão de administrador.
        /// </summary>
        public bool TemPermissaoAdmin => Ativo && Permissoes.Contains("admin");

        /// <summary>
        /// Nível de permissão para exibição.
        /// </summary>
        public string NivelPermissao
        {
            get
            {
                if (!Ativo) return "Inativo";
                if (TemPermissaoAdmin) return "Administrador";
                if (TemPermissaoEscrita) return "Edição";
                if (TemPermissaoLeitura) return "Visualização";
                return "Sem permissão";
            }
        }

        /// <summary>
        /// Cor do status para UI.
        /// </summary>
        public string CorStatus
        {
            get
            {
                if (!Ativo) return "Gray";
                if (TemPermissaoAdmin) return "Red";
                if (TemPermissaoEscrita) return "Orange";
                if (TemPermissaoLeitura) return "Blue";
                return "Gray";
            }
        }

        /// <summary>
        /// Email do usuário.
        /// </summary>
        public string EmailUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Representação string do compartilhamento.
        /// </summary>
        public override string ToString()
        {
            return $"Usuário {UsuarioId} - {NivelPermissao} - Veículo {VeiculoId}";
        }

        /// <summary>
        /// Ativa o compartilhamento.
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
        }

        /// <summary>
        /// Desativa o compartilhamento.
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
        }

        /// <summary>
        /// Atualiza as permissões do compartilhamento.
        /// </summary>
        public void AtualizarPermissoes(string novasPermissoes)
        {
            Permissoes = novasPermissoes;
        }
    }
}
