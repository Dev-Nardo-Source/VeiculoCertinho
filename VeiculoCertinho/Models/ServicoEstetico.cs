using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um serviço estético realizado em um veículo.
    /// </summary>
    public class ServicoEstetico
    {
        /// <summary>
        /// Identificador único do serviço estético.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required]
        public int VeiculoId { get; set; }

        /// <summary>
        /// Tipo do serviço estético.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TipoServico { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do serviço.
        /// </summary>
        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Valor do serviço.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Valor { get; set; }

        /// <summary>
        /// Data em que o serviço foi realizado.
        /// </summary>
        [Required]
        public DateTime DataServico { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Custo do serviço estético.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Custo { get; set; }

        /// <summary>
        /// Observações adicionais sobre o serviço.
        /// </summary>
        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;
    }
}
