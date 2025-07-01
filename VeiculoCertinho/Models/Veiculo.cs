using System;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um veículo.
    /// </summary>
    public class Veiculo : BaseModel
    {
        // Campos privados
        private string _cor = string.Empty;
        private string _observacoes = string.Empty;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private int _anoFabricacao;
        private int _anoModelo;
        private string _placa = string.Empty;
        private bool _importado;
        private string _cilindrada = string.Empty;
        private string _potencia = string.Empty;
        private int _passageiros;
        private string _ufOrigem = string.Empty;
        private int _ufOrigemId;
        private string _municipioOrigem = string.Empty;
        private string _ufAtual = string.Empty;
        private int _ufAtualId;
        private string _municipioAtual = string.Empty;
        private string _segmento = string.Empty;
        private string _especieVeiculo = string.Empty;
        private string? _chassi;
        private string? _motor;
        private bool _usaGasolina;
        private bool _usaEtanol;
        private bool _usaGNV;
        private bool _usaRecargaEletrica;
        private bool _usaDiesel;
        private bool _usaHidrogenio;

        // Propriedades públicas
        [StringLength(50)]
        public string Cor
        {
            get => _cor;
            set => SetStringProperty(ref _cor, value);
        }

        [StringLength(500)]
        public string Observacoes
        {
            get => _observacoes;
            set => SetStringProperty(ref _observacoes, value);
        }

        [StringLength(100)]
        public string Marca
        {
            get => _marca;
            set => SetStringProperty(ref _marca, value);
        }

        [StringLength(100)]
        public string Modelo
        {
            get => _modelo;
            set => SetStringProperty(ref _modelo, value);
        }

        [Range(1900, 2100)]
        public int AnoFabricacao
        {
            get => _anoFabricacao;
            set => SetProperty(ref _anoFabricacao, value);
        }

        [Range(1900, 2100)]
        public int AnoModelo
        {
            get => _anoModelo;
            set => SetProperty(ref _anoModelo, value);
        }

        [StringLength(20)]
        public string Placa
        {
            get => _placa;
            set => SetStringProperty(ref _placa, value);
        }

        public bool Importado
        {
            get => _importado;
            set => SetProperty(ref _importado, value);
        }

        [StringLength(50)]
        public string Cilindrada
        {
            get => _cilindrada;
            set => SetStringProperty(ref _cilindrada, value);
        }

        [StringLength(50)]
        public string Potencia
        {
            get => _potencia;
            set => SetStringProperty(ref _potencia, value);
        }

        public int Passageiros
        {
            get => _passageiros;
            set => SetProperty(ref _passageiros, value);
        }

        [StringLength(50)]
        public string UfOrigem
        {
            get => _ufOrigem;
            set => SetStringProperty(ref _ufOrigem, value);
        }

        public int UfOrigemId
        {
            get => _ufOrigemId;
            set => SetProperty(ref _ufOrigemId, value);
        }

        [StringLength(100)]
        public string MunicipioOrigem
        {
            get => _municipioOrigem;
            set => SetStringProperty(ref _municipioOrigem, value);
        }

        [StringLength(50)]
        public string UfAtual
        {
            get => _ufAtual;
            set => SetStringProperty(ref _ufAtual, value);
        }

        public int UfAtualId
        {
            get => _ufAtualId;
            set => SetProperty(ref _ufAtualId, value);
        }

        [StringLength(100)]
        public string MunicipioAtual
        {
            get => _municipioAtual;
            set => SetStringProperty(ref _municipioAtual, value);
        }

        [StringLength(100)]
        public string Segmento
        {
            get => _segmento;
            set => SetStringProperty(ref _segmento, value);
        }

        [StringLength(100)]
        public string EspecieVeiculo
        {
            get => _especieVeiculo;
            set => SetStringProperty(ref _especieVeiculo, value);
        }

        public string? Chassi
        {
            get => _chassi;
            set => SetNullableStringProperty(ref _chassi, value);
        }

        public string? Motor
        {
            get => _motor;
            set => SetNullableStringProperty(ref _motor, value);
        }

        // Propriedades de combustível
        public bool UsaGasolina
        {
            get => _usaGasolina;
            set => SetProperty(ref _usaGasolina, value);
        }

        public bool UsaEtanol
        {
            get => _usaEtanol;
            set => SetProperty(ref _usaEtanol, value);
        }

        public bool UsaGNV
        {
            get => _usaGNV;
            set => SetProperty(ref _usaGNV, value);
        }

        public bool UsaRecargaEletrica
        {
            get => _usaRecargaEletrica;
            set => SetProperty(ref _usaRecargaEletrica, value);
        }

        public bool UsaDiesel
        {
            get => _usaDiesel;
            set => SetProperty(ref _usaDiesel, value);
        }

        public bool UsaHidrogenio
        {
            get => _usaHidrogenio;
            set => SetProperty(ref _usaHidrogenio, value);
        }

        // Construtor
        public Veiculo()
        {
            // Inicializar com valores padrão para evitar constraint violations
            UfOrigemId = 35; // SP como padrão
            UfAtualId = 35; // SP como padrão
        }

        // Métodos helper
        /// <summary>
        /// Método helper para sincronizar UfOrigemId com UfOrigem
        /// </summary>
        public void SincronizarUfOrigem(int ufId, string ufSigla)
        {
            UfOrigemId = ufId;
            UfOrigem = ufSigla;
        }

        /// <summary>
        /// Método helper para sincronizar UfAtualId com UfAtual
        /// </summary>
        public void SincronizarUfAtual(int ufId, string ufSigla)
        {
            UfAtualId = ufId;
            UfAtual = ufSigla;
        }

        /// <summary>
        /// Retorna uma lista dos combustíveis que o veículo utiliza.
        /// </summary>
        public List<string> CombustiveisUtilizados
        {
            get
            {
                var combustiveis = new List<string>();
                if (UsaGasolina) combustiveis.Add("Gasolina");
                if (UsaEtanol) combustiveis.Add("Etanol");
                if (UsaGNV) combustiveis.Add("GNV");
                if (UsaRecargaEletrica) combustiveis.Add("Elétrico");
                if (UsaDiesel) combustiveis.Add("Diesel");
                if (UsaHidrogenio) combustiveis.Add("Hidrogênio");
                return combustiveis;
            }
        }

        /// <summary>
        /// Representação string do veículo.
        /// </summary>
        public override string ToString()
        {
            return $"{Marca} {Modelo} - {Placa} ({AnoFabricacao})";
        }
    }
}
