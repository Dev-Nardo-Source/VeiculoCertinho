using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa uma manutenção corretiva realizada em um veículo.
    /// </summary>
    public class ManutencaoCorretiva
    {
        /// <summary>
        /// Identificador único da manutenção.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required]
        public int VeiculoId { get; set; }

        /// <summary>
        /// Descrição da falha que motivou a manutenção.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string DescricaoFalha { get; set; } = string.Empty;

        /// <summary>
        /// Data do registro da manutenção.
        /// </summary>
        [Required]
        public DateTime DataRegistro { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Data da resolução da manutenção, se já realizada.
        /// </summary>
        public DateTime? DataResolucao { get; set; }

        /// <summary>
        /// Solução aplicada para resolver a falha.
        /// </summary>
        [StringLength(500)]
        public string SolucaoAplicada { get; set; } = string.Empty;

        /// <summary>
        /// Observações adicionais sobre a manutenção.
        /// </summary>
        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        /// <summary>
        /// Caminhos ou URLs das fotos comprobatórias anexadas.
        /// </summary>
        [StringLength(1000)]
        public string FotosComprovantes { get; set; } = string.Empty;
    }
}
