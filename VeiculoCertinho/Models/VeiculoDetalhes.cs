using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa detalhes adicionais de um veículo.
    /// </summary>
    public class VeiculoDetalhes
    {
        /// <summary>
        /// Identificador único dos detalhes.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do veículo associado.
        /// </summary>
        [Required]
        public int VeiculoId { get; set; }

        /// <summary>
        /// Cor do veículo.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Cor { get; set; } = string.Empty;

        /// <summary>
        /// Placa do veículo.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Placa { get; set; } = string.Empty;

        /// <summary>
        /// Foto do veículo (caminho ou URL).
        /// </summary>
        [StringLength(500)]
        public string Foto { get; set; } = string.Empty;

        /// <summary>
        /// Personalização do veículo.
        /// </summary>
        [StringLength(500)]
        public string Personalizacao { get; set; } = string.Empty;

        /// <summary>
        /// Observações adicionais.
        /// </summary>
        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        /// <summary>
        /// Quilometragem atual do veículo.
        /// </summary>
        public decimal Quilometragem { get; set; }

        /// <summary>
        /// Data da última atualização dos detalhes.
        /// </summary>
        public DateTime DataAtualizacao { get; set; }

        public VeiculoDetalhes()
        {
            Id = 0;
            VeiculoId = 0;
            DataAtualizacao = DateTime.Now;
        }
    }
}
