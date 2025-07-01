using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Classe base para todos os models do sistema.
    /// Fornece funcionalidades comuns como INotifyPropertyChanged, propriedades de auditoria e validação.
    /// </summary>
    public abstract class BaseModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Identificador único da entidade.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Data de criação do registro.
        /// </summary>
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização do registro.
        /// </summary>
        public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifica mudanças de propriedade para o binding.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Define o valor de uma propriedade e notifica mudanças se necessário.
        /// </summary>
        /// <typeparam name="T">Tipo da propriedade</typeparam>
        /// <param name="field">Campo de apoio da propriedade</param>
        /// <param name="value">Novo valor</param>
        /// <param name="propertyName">Nome da propriedade (preenchido automaticamente)</param>
        /// <returns>True se o valor foi alterado, false caso contrário</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            UpdatedAt = DateTime.UtcNow;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Define o valor de uma propriedade string com tratamento de nulos e notifica mudanças.
        /// </summary>
        protected virtual bool SetStringProperty(ref string field, string? value, [CallerMemberName] string? propertyName = null)
        {
            var newValue = value ?? string.Empty;
            return SetProperty(ref field, newValue, propertyName);
        }

        /// <summary>
        /// Define o valor de uma propriedade string nullable e notifica mudanças.
        /// </summary>
        protected virtual bool SetNullableStringProperty(ref string? field, string? value, [CallerMemberName] string? propertyName = null)
        {
            return SetProperty(ref field, value, propertyName);
        }

        /// <summary>
        /// Valida se a entidade está em um estado válido.
        /// </summary>
        /// <returns>Lista de erros de validação (vazia se válido)</returns>
        public virtual List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            Validator.TryValidateObject(this, context, results, true);
            return results;
        }

        /// <summary>
        /// Verifica se a entidade é válida.
        /// </summary>
        public virtual bool IsValid => Validate().Count == 0;

        /// <summary>
        /// Cria uma cópia superficial da entidade.
        /// </summary>
        public virtual T Clone<T>() where T : BaseModel
        {
            return (T)MemberwiseClone();
        }

        /// <summary>
        /// Compara duas entidades baseado no ID.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is BaseModel other && other.GetType() == GetType())
            {
                return Id != 0 && Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Gera hash code baseado no ID.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Representação string da entidade.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
    }
} 