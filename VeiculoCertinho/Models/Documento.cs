using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um documento relacionado a um veículo.
    /// </summary>
    public class Documento : BaseModel
    {
        private int _veiculoId;
        private string _tipoDocumento = string.Empty;
        private DateTime _dataVencimento = DateTime.MinValue;
        private bool _notificado;
        private string _observacoes = string.Empty;

        /// <summary>
        /// Identificador único do documento.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required(ErrorMessage = "Veículo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Veículo deve ser selecionado")]
        public int VeiculoId
        {
            get => _veiculoId;
            set => SetProperty(ref _veiculoId, value);
        }

        /// <summary>
        /// Tipo do documento (ex: IPVA, seguro, licenciamento, multas).
        /// </summary>
        [Required(ErrorMessage = "Tipo de documento é obrigatório")]
        [StringLength(100, ErrorMessage = "Tipo de documento deve ter no máximo 100 caracteres")]
        public string TipoDocumento
        {
            get => _tipoDocumento;
            set => SetStringProperty(ref _tipoDocumento, value);
        }

        /// <summary>
        /// Data de vencimento do documento.
        /// </summary>
        [Required(ErrorMessage = "Data de vencimento é obrigatória")]
        public DateTime DataVencimento
        {
            get => _dataVencimento;
            set => SetProperty(ref _dataVencimento, value);
        }

        /// <summary>
        /// Data de registro do documento (herda de BaseModel.CreatedAt).
        /// </summary>
        public DateTime DataRegistro => CreatedAt;

        /// <summary>
        /// Indica se o usuário foi notificado sobre o documento.
        /// </summary>
        public bool Notificado
        {
            get => _notificado;
            set => SetProperty(ref _notificado, value);
        }

        /// <summary>
        /// Observações adicionais sobre o documento.
        /// </summary>
        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string Observacoes
        {
            get => _observacoes;
            set => SetStringProperty(ref _observacoes, value);
        }

        /// <summary>
        /// Verifica se o documento está vencido.
        /// </summary>
        public bool EstaVencido => DataVencimento < DateTime.Today;

        /// <summary>
        /// Verifica se o documento está próximo do vencimento (30 dias).
        /// </summary>
        public bool ProximoDoVencimento => !EstaVencido && DataVencimento <= DateTime.Today.AddDays(30);

        /// <summary>
        /// Dias até o vencimento (negativo se vencido).
        /// </summary>
        public int DiasParaVencimento => (DataVencimento.Date - DateTime.Today).Days;

        /// <summary>
        /// Status do documento para exibição.
        /// </summary>
        public string StatusVencimento
        {
            get
            {
                if (EstaVencido)
                    return $"Vencido há {Math.Abs(DiasParaVencimento)} dias";
                if (ProximoDoVencimento)
                    return $"Vence em {DiasParaVencimento} dias";
                return "Em dia";
            }
        }

        /// <summary>
        /// Cor do status para UI (verde, amarelo, vermelho).
        /// </summary>
        public string CorStatus
        {
            get
            {
                if (EstaVencido) return "Red";
                if (ProximoDoVencimento) return "Orange";
                return "Green";
            }
        }

        /// <summary>
        /// Representação string do documento.
        /// </summary>
        public override string ToString()
        {
            return $"{TipoDocumento} - {StatusVencimento}";
        }

        /// <summary>
        /// Marca o documento como notificado.
        /// </summary>
        public void MarcarComoNotificado()
        {
            Notificado = true;
        }
    }
}
