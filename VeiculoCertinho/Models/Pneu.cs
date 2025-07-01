using System;

namespace VeiculoCertinho.Models
{
    public class Pneu
    {
        public int Id { get; set; }
        public string Posicao { get; set; } = string.Empty;
        public DateTime DataInstalacao { get; set; }
        public double QuilometragemInstalacao { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public int VeiculoId { get; set; }
    }
} 