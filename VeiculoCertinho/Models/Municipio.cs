using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um município.
    /// </summary>
    public class Municipio : BaseModel
    {
        private string _nome = string.Empty;
        private int _ufId;

        /// <summary>
        /// Nome do município.
        /// </summary>
        [Required(ErrorMessage = "Nome do município é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome
        {
            get => _nome;
            set => SetStringProperty(ref _nome, value);
        }

        /// <summary>
        /// ID da Unidade Federativa (UF) a qual o município pertence.
        /// </summary>
        [Required(ErrorMessage = "UF é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "UF deve ser selecionada")]
        public int UfId
        {
            get => _ufId;
            set => SetProperty(ref _ufId, value);
        }

        /// <summary>
        /// Texto para exibição em listas e combos.
        /// </summary>
        public string DisplayText => Nome;

        /// <summary>
        /// Representação string do município.
        /// </summary>
        public override string ToString()
        {
            return $"{Nome} (UF: {UfId})";
        }
    }
} 