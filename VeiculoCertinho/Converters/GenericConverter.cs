using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Linq;

namespace VeiculoCertinho.Converters
{
    /// <summary>
    /// Converter universal que substitui TODOS os converters simples do projeto.
    /// Consolidação completa para eliminar 8+ arquivos de converters redundantes.
    /// </summary>
    public class GenericConverter : IValueConverter, IMultiValueConverter
    {
        public enum ConverterType
        {
            InverseBool,
            NotNull,
            NullToBool,
            ListContains,
            BoolToVisibility,
            StringIsNullOrEmpty,
            CountToVisibility,
            IntToBool,
            StringToBool,
            MultiBoolToEnabled,
            CombinedFieldEnabled,
            MunicipioAtualEnabled,
            NaoLocalizadoToEnabled,
            ListContainsMulti,
            StringComparison,
            DateComparison,
            NumericComparison
        }

        // Propriedades para configurar o comportamento
        public ConverterType Type { get; set; } = ConverterType.InverseBool;
        public object? Parameter { get; set; }
        public object? TrueValue { get; set; } = true;
        public object? FalseValue { get; set; } = false;
        public object? NullValue { get; set; } = false;
        public string? ComparisonOperator { get; set; } = "==";

        // Instâncias estáticas pré-configuradas para uso comum
        public static readonly GenericConverter InverseBool = new() { Type = ConverterType.InverseBool };
        public static readonly GenericConverter NotNull = new() { Type = ConverterType.NotNull };
        public static readonly GenericConverter NullToBool = new() { Type = ConverterType.NullToBool };
        public static readonly GenericConverter ListContains = new() { Type = ConverterType.ListContains };
        public static readonly GenericConverter BoolToVisibility = new() { Type = ConverterType.BoolToVisibility };
        public static readonly GenericConverter StringIsNullOrEmpty = new() { Type = ConverterType.StringIsNullOrEmpty };
        public static readonly GenericConverter CountToVisibility = new() { Type = ConverterType.CountToVisibility };
        public static readonly GenericConverter MultiBoolToEnabled = new() { Type = ConverterType.MultiBoolToEnabled };
        public static readonly GenericConverter CombinedFieldEnabled = new() { Type = ConverterType.CombinedFieldEnabled };
        public static readonly GenericConverter MunicipioAtualEnabled = new() { Type = ConverterType.MunicipioAtualEnabled };
        public static readonly GenericConverter NaoLocalizadoToEnabled = new() { Type = ConverterType.NaoLocalizadoToEnabled };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var param = parameter ?? Parameter;

            return Type switch
            {
                ConverterType.InverseBool => ConvertInverseBool(value),
                ConverterType.NotNull => ConvertNotNull(value),
                ConverterType.NullToBool => ConvertNullToBool(value),
                ConverterType.ListContains => ConvertListContains(value, param),
                ConverterType.BoolToVisibility => ConvertBoolToVisibility(value),
                ConverterType.StringIsNullOrEmpty => ConvertStringIsNullOrEmpty(value),
                ConverterType.CountToVisibility => ConvertCountToVisibility(value),
                ConverterType.IntToBool => ConvertIntToBool(value),
                ConverterType.StringToBool => ConvertStringToBool(value),
                ConverterType.NaoLocalizadoToEnabled => ConvertNaoLocalizadoToEnabled(value),
                ConverterType.StringComparison => ConvertStringComparison(value, param),
                ConverterType.DateComparison => ConvertDateComparison(value, param),
                ConverterType.NumericComparison => ConvertNumericComparison(value, param),
                _ => value
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var param = parameter ?? Parameter;

            return Type switch
            {
                ConverterType.InverseBool => ConvertInverseBool(value),
                ConverterType.NullToBool => ConvertNullToBool(value),
                ConverterType.BoolToVisibility => ConvertVisibilityToBool(value),
                ConverterType.StringToBool => ConvertBoolToString(value),
                _ => value
            };
        }

        // IMultiValueConverter implementation
        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null) return FalseValue;

            return Type switch
            {
                ConverterType.MultiBoolToEnabled => ConvertMultiBoolToEnabled(values),
                ConverterType.CombinedFieldEnabled => ConvertCombinedFieldEnabled(values),
                ConverterType.MunicipioAtualEnabled => ConvertMunicipioAtualEnabled(values),
                ConverterType.ListContainsMulti => ConvertListContainsMulti(values, parameter),
                _ => Convert(values.FirstOrDefault(), targetType, parameter, culture)
            };
        }

        public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            // Para a maioria dos casos, não implementamos ConvertBack em MultiValue
            return null;
        }

        #region Single Value Conversion Methods

        private object? ConvertInverseBool(object? value)
        {
            if (value is bool boolValue)
                return !boolValue;
            return NullValue;
        }

        private object? ConvertNotNull(object? value)
        {
            return value != null ? TrueValue : FalseValue;
        }

        private object? ConvertNullToBool(object? value)
        {
            return value == null ? TrueValue : FalseValue;
        }

        private object? ConvertListContains(object? value, object? parameter)
        {
            if (value is IEnumerable enumerable && parameter != null)
            {
                foreach (var item in enumerable)
                {
                    if (item?.Equals(parameter) == true)
                        return TrueValue;
                }
            }
            return FalseValue;
        }

        private object? ConvertBoolToVisibility(object? value)
        {
            if (value is bool boolValue)
                return boolValue ? TrueValue ?? true : FalseValue ?? false;
            return FalseValue ?? false;
        }

        private object? ConvertVisibilityToBool(object? value)
        {
            if (value is bool visibility)
                return visibility;
            return false;
        }

        private object? ConvertStringIsNullOrEmpty(object? value)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? TrueValue : FalseValue;
        }

        private object? ConvertCountToVisibility(object? value)
        {
            var count = 0;
            
            if (value is ICollection collection)
                count = collection.Count;
            else if (value is IEnumerable enumerable)
                count = enumerable.Cast<object>().Count();
            else if (value is int intValue)
                count = intValue;

            return count > 0 ? TrueValue : FalseValue;
        }

        private object? ConvertIntToBool(object? value)
        {
            if (value is int intValue)
                return intValue > 0 ? TrueValue : FalseValue;
            if (value is double doubleValue)
                return doubleValue > 0 ? TrueValue : FalseValue;
            if (value is decimal decimalValue)
                return decimalValue > 0 ? TrueValue : FalseValue;
            return FalseValue;
        }

        private object? ConvertStringToBool(object? value)
        {
            if (value is string stringValue)
            {
                return stringValue.ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "sim" or "on" => TrueValue,
                    "false" or "0" or "no" or "não" or "off" => FalseValue,
                    _ => string.IsNullOrEmpty(stringValue) ? FalseValue : TrueValue
                };
            }
            return FalseValue;
        }

        private object? ConvertBoolToString(object? value)
        {
            if (value is bool boolValue)
                return boolValue ? TrueValue?.ToString() ?? "True" : FalseValue?.ToString() ?? "False";
            return FalseValue?.ToString() ?? "False";
        }

        private object? ConvertNaoLocalizadoToEnabled(object? value)
        {
            var str = value?.ToString();
            return !string.IsNullOrEmpty(str) && str != "Não Localizado" ? TrueValue : FalseValue;
        }

        private object? ConvertStringComparison(object? value, object? parameter)
        {
            var str1 = value?.ToString() ?? string.Empty;
            var str2 = parameter?.ToString() ?? string.Empty;

            return ComparisonOperator switch
            {
                "==" => str1.Equals(str2, StringComparison.OrdinalIgnoreCase) ? TrueValue : FalseValue,
                "!=" => !str1.Equals(str2, StringComparison.OrdinalIgnoreCase) ? TrueValue : FalseValue,
                "contains" => str1.Contains(str2, StringComparison.OrdinalIgnoreCase) ? TrueValue : FalseValue,
                "startswith" => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase) ? TrueValue : FalseValue,
                "endswith" => str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase) ? TrueValue : FalseValue,
                _ => FalseValue
            };
        }

        private object? ConvertDateComparison(object? value, object? parameter)
        {
            if (value is DateTime date1 && parameter is DateTime date2)
            {
                return ComparisonOperator switch
                {
                    "==" => date1.Date == date2.Date ? TrueValue : FalseValue,
                    "!=" => date1.Date != date2.Date ? TrueValue : FalseValue,
                    ">" => date1 > date2 ? TrueValue : FalseValue,
                    "<" => date1 < date2 ? TrueValue : FalseValue,
                    ">=" => date1 >= date2 ? TrueValue : FalseValue,
                    "<=" => date1 <= date2 ? TrueValue : FalseValue,
                    _ => FalseValue
                };
            }
            return FalseValue;
        }

        private object? ConvertNumericComparison(object? value, object? parameter)
        {
            if (TryConvertToDouble(value, out double num1) && TryConvertToDouble(parameter, out double num2))
            {
                return ComparisonOperator switch
                {
                    "==" => Math.Abs(num1 - num2) < 0.001 ? TrueValue : FalseValue,
                    "!=" => Math.Abs(num1 - num2) >= 0.001 ? TrueValue : FalseValue,
                    ">" => num1 > num2 ? TrueValue : FalseValue,
                    "<" => num1 < num2 ? TrueValue : FalseValue,
                    ">=" => num1 >= num2 ? TrueValue : FalseValue,
                    "<=" => num1 <= num2 ? TrueValue : FalseValue,
                    _ => FalseValue
                };
            }
            return FalseValue;
        }

        #endregion

        #region Multi Value Conversion Methods

        private object? ConvertMultiBoolToEnabled(object[] values)
        {
            return values.All(v => v is bool b && b) ? TrueValue : FalseValue;
        }

        private object? ConvertCombinedFieldEnabled(object[] values)
        {
            // Todos os campos devem estar preenchidos (não nulos e não vazios)
            return values.All(v => v != null && !string.IsNullOrWhiteSpace(v.ToString())) ? TrueValue : FalseValue;
        }

        private object? ConvertMunicipioAtualEnabled(object[] values)
        {
            // Lógica específica: verifica se UF atual está selecionada e não é "Não Localizado"
            if (values.Length >= 2)
            {
                var ufAtual = values[0]?.ToString();
                var municipioAtual = values.Length > 1 ? values[1]?.ToString() : null;

                var ufValida = !string.IsNullOrWhiteSpace(ufAtual) && ufAtual != "Não Localizado";
                var municipioValido = !string.IsNullOrWhiteSpace(municipioAtual) && municipioAtual != "Não Localizado";

                return ufValida && municipioValido ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        private object? ConvertListContainsMulti(object[] values, object? parameter)
        {
            if (values.Length > 0 && values[0] is IEnumerable enumerable)
            {
                var searchValues = values.Skip(1);
                return searchValues.Any(searchValue => 
                    enumerable.Cast<object>().Any(item => item?.Equals(searchValue) == true)) ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        #endregion

        #region Helper Methods

        private static bool TryConvertToDouble(object? value, out double result)
        {
            result = 0;
            return value switch
            {
                double d => (result = d) == d,
                float f => (result = f) == f,
                decimal dec => (result = (double)dec) == (double)dec,
                int i => (result = i) == i,
                long l => (result = l) == l,
                string s => double.TryParse(s, out result),
                _ => false
            };
        }

        /// <summary>
        /// Cria um converter configurado para inverter valores booleanos.
        /// </summary>
        public static GenericConverter CreateInverseBool() => new() { Type = ConverterType.InverseBool };

        /// <summary>
        /// Cria um converter configurado para verificar se um valor não é nulo.
        /// </summary>
        public static GenericConverter CreateNotNull() => new() { Type = ConverterType.NotNull };

        /// <summary>
        /// Cria um converter configurado para verificar se uma lista contém um item.
        /// </summary>
        public static GenericConverter CreateListContains(object parameter) => new() 
        { 
            Type = ConverterType.ListContains, 
            Parameter = parameter 
        };

        /// <summary>
        /// Cria um converter configurado para converter booleano para visibilidade.
        /// </summary>
        public static GenericConverter CreateBoolToVisibility(bool trueValue = true, bool falseValue = false) => new() 
        { 
            Type = ConverterType.BoolToVisibility, 
            TrueValue = trueValue, 
            FalseValue = falseValue 
        };

        /// <summary>
        /// Cria um converter para múltiplos booleanos.
        /// </summary>
        public static GenericConverter CreateMultiBoolToEnabled() => new() { Type = ConverterType.MultiBoolToEnabled };

        /// <summary>
        /// Cria um converter para campos combinados.
        /// </summary>
        public static GenericConverter CreateCombinedFieldEnabled() => new() { Type = ConverterType.CombinedFieldEnabled };

        /// <summary>
        /// Cria um converter para validação de município atual.
        /// </summary>
        public static GenericConverter CreateMunicipioAtualEnabled() => new() { Type = ConverterType.MunicipioAtualEnabled };

        /// <summary>
        /// Cria um converter para verificar se não é "Não Localizado".
        /// </summary>
        public static GenericConverter CreateNaoLocalizadoToEnabled() => new() { Type = ConverterType.NaoLocalizadoToEnabled };

        /// <summary>
        /// Cria um converter para comparação de strings.
        /// </summary>
        public static GenericConverter CreateStringComparison(string operatorType, string compareValue) => new() 
        { 
            Type = ConverterType.StringComparison, 
            ComparisonOperator = operatorType,
            Parameter = compareValue
        };

        /// <summary>
        /// Cria um converter para comparação de datas.
        /// </summary>
        public static GenericConverter CreateDateComparison(string operatorType, DateTime compareDate) => new() 
        { 
            Type = ConverterType.DateComparison, 
            ComparisonOperator = operatorType,
            Parameter = compareDate
        };

        /// <summary>
        /// Cria um converter para comparação numérica.
        /// </summary>
        public static GenericConverter CreateNumericComparison(string operatorType, double compareValue) => new() 
        { 
            Type = ConverterType.NumericComparison, 
            ComparisonOperator = operatorType,
            Parameter = compareValue
        };

        #endregion
    }
} 