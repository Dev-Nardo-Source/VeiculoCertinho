using System;

namespace VeiculoCertinho.Models
{
    public class DashboardIndicador
    {
        public int Id { get; set; }
        public string NomeIndicador { get; set; } = string.Empty;
        public double Valor { get; set; }
        public DateTime DataReferencia { get; set; } = DateTime.Now;
        public string Descricao { get; set; } = string.Empty;
    }
}
