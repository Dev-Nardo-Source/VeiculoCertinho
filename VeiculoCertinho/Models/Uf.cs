using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa uma Unidade Federativa (Estado).
    /// </summary>
    public class Uf : BaseModel
    {
        private string _nome = string.Empty;
        private string _sigla = string.Empty;

        /// <summary>
        /// Nome completo da UF.
        /// </summary>
        [Required(ErrorMessage = "Nome da UF é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string Nome
        {
            get => _nome;
            set => SetStringProperty(ref _nome, value);
        }

        /// <summary>
        /// Sigla da UF (ex: SP, RJ, ES).
        /// </summary>
        [Required(ErrorMessage = "Sigla da UF é obrigatória")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Sigla deve ter exatamente 2 caracteres")]
        public string Sigla
        {
            get => _sigla;
            set => SetStringProperty(ref _sigla, value?.ToUpper());
        }

        /// <summary>
        /// Texto para exibição em listas e combos (Sigla - Nome).
        /// </summary>
        public string DisplayText => $"{Sigla} - {Nome}";

        /// <summary>
        /// Representação string da UF.
        /// </summary>
        public override string ToString()
        {
            return DisplayText;
        }
    }
} 