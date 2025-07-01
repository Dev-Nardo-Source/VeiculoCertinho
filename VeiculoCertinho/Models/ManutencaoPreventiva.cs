using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa uma manutenção preventiva programada para um veículo.
    /// </summary>
    public class ManutencaoPreventiva
    {
        /// <summary>
        /// Identificador único da manutenção preventiva.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required]
        public int VeiculoId { get; set; }

        /// <summary>
        /// Tipo da manutenção preventiva.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TipoManutencao { get; set; } = string.Empty;

        /// <summary>
        /// Data agendada para a manutenção.
        /// </summary>
        [Required]
        public DateTime DataAgendada { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Quilometragem agendada para a manutenção.
        /// </summary>
        [Required]
        public int QuilometragemAgendada { get; set; }

        /// <summary>
        /// Data em que a manutenção foi realizada, se aplicável.
        /// </summary>
        public DateTime? DataRealizada { get; set; }

        /// <summary>
        /// Quilometragem em que a manutenção foi realizada, se aplicável.
        /// </summary>
        public int? QuilometragemRealizada { get; set; }

        /// <summary>
        /// Checklist de itens verificados na manutenção.
        /// </summary>
        [StringLength(1000)]
        public string Checklist { get; set; } = string.Empty;

        /// <summary>
        /// Observações adicionais sobre a manutenção preventiva.
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
