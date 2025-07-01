using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um relatório gerado para um veículo.
    /// </summary>
    public class Relatorio
    {
        /// <summary>
        /// Identificador único do relatório.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required]
        public int VeiculoId { get; set; }

        /// <summary>
        /// Tipo do relatório.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TipoRelatorio { get; set; } = string.Empty;

        /// <summary>
        /// Data de geração do relatório.
        /// </summary>
        [Required]
        public DateTime DataGeracao { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Caminho do arquivo do relatório.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string CaminhoArquivo { get; set; } = string.Empty;

        /// <summary>
        /// Observações adicionais sobre o relatório.
        /// </summary>
        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;
    }
}
