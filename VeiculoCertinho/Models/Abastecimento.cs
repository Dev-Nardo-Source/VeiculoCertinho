using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um abastecimento de veículo.
    /// </summary>
    public class Abastecimento : BaseModel
    {
        private int _veiculoId;
        private DateTime _data = DateTime.Now;
        private string _tipoCombustivel = string.Empty;
        private decimal _precoPorUnidade;
        private decimal _quantidade;
        private int _quilometragemAtual;
        private string _posto = string.Empty;
        private decimal _valorTotal;

        /// <summary>
        /// ID do veículo abastecido.
        /// </summary>
        [Required(ErrorMessage = "Veículo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Veículo deve ser selecionado")]
        public int VeiculoId
        {
            get => _veiculoId;
            set => SetProperty(ref _veiculoId, value);
        }

        /// <summary>
        /// Data do abastecimento.
        /// </summary>
        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        /// <summary>
        /// Tipo de combustível (gasolina, etanol, diesel, GNV, elétrico, AdBlue, hidrogênio).
        /// </summary>
        [Required(ErrorMessage = "Tipo de combustível é obrigatório")]
        [StringLength(50, ErrorMessage = "Tipo de combustível deve ter no máximo 50 caracteres")]
        public string TipoCombustivel
        {
            get => _tipoCombustivel;
            set => SetStringProperty(ref _tipoCombustivel, value);
        }

        /// <summary>
        /// Preço por unidade (litro, m³, kWh, kg).
        /// </summary>
        [Required(ErrorMessage = "Preço por unidade é obrigatório")]
        [Range(0.01, 999.99, ErrorMessage = "Preço deve estar entre R$ 0,01 e R$ 999,99")]
        public decimal PrecoPorUnidade
        {
            get => _precoPorUnidade;
            set
            {
                if (SetProperty(ref _precoPorUnidade, value))
                {
                    // Recalcular valor total automaticamente
                    ValorTotal = _precoPorUnidade * _quantidade;
                }
            }
        }

        /// <summary>
        /// Quantidade abastecida.
        /// </summary>
        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.1, 9999.9, ErrorMessage = "Quantidade deve estar entre 0,1 e 9999,9")]
        public decimal Quantidade
        {
            get => _quantidade;
            set
            {
                if (SetProperty(ref _quantidade, value))
                {
                    // Recalcular valor total automaticamente
                    ValorTotal = _precoPorUnidade * _quantidade;
                }
            }
        }

        /// <summary>
        /// Quilometragem atual do veículo.
        /// </summary>
        [Required(ErrorMessage = "Quilometragem é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "Quilometragem deve ser positiva")]
        public int QuilometragemAtual
        {
            get => _quilometragemAtual;
            set => SetProperty(ref _quilometragemAtual, value);
        }

        /// <summary>
        /// Nome do posto de combustível.
        /// </summary>
        [Required(ErrorMessage = "Posto é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do posto deve ter no máximo 100 caracteres")]
        public string Posto
        {
            get => _posto;
            set => SetStringProperty(ref _posto, value);
        }

        /// <summary>
        /// Valor total do abastecimento (calculado automaticamente).
        /// </summary>
        public decimal ValorTotal
        {
            get => _valorTotal;
            private set => SetProperty(ref _valorTotal, value);
        }

        /// <summary>
        /// Valor total formatado para exibição.
        /// </summary>
        public string ValorTotalFormatado => ValorTotal.ToString("C2");

        /// <summary>
        /// Representação string do abastecimento.
        /// </summary>
        public override string ToString()
        {
            return $"{TipoCombustivel} - {Quantidade:F2}L - {ValorTotalFormatado} ({Data:dd/MM/yyyy})";
        }
    }
}
