using System;

namespace VeiculoCertinho.Models
{
    public class ControlePneus
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public string Posicao { get; set; } = string.Empty; // Ex: dianteiro esquerdo, traseiro direito, etc.
        public DateTime DataInstalacao { get; set; }
        public int QuilometragemInstalacao { get; set; }
        public DateTime? DataRemocao { get; set; }
        public int? QuilometragemRemocao { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }
}
