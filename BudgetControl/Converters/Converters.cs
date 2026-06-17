using System.Globalization;

namespace BudgetControl.Converters
{
    /// <summary>
    /// Retourne true si la string n'est pas vide/null
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value?.ToString());

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Inverse un booléen
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
    }

    /// <summary>
    /// Retourne true si l'Id est > 0 (mode édition)
    /// </summary>
    public class IdToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int id && id > 0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Titre du formulaire : "Nouvelle X" ou "Modifier X" selon l'Id
    /// </summary>
    public class IdToTitleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var entity = parameter?.ToString() ?? "entrée";
            return value is int id && id > 0
                ? $"Modifier {entity}"
                : $"Nouvelle {entity}";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Convertit un numéro de mois (1-12) en index (0-11) pour le Picker
    /// </summary>
    public class MonthToIndexConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int m ? m - 1 : 0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int idx ? idx + 1 : 1;
    }

    /// <summary>
    /// Normalise les revenus pour la barre de progression (0.0 à 1.0)
    /// Stocke le max dans App.RevenusMax pour aligner les deux barres
    /// </summary>
    public class RevenusProgressConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not decimal revenus) return 0.0;
            var max = Math.Max((double)revenus, App.DepensesMax);
            App.RevenusMax = (double)revenus;
            return max > 0 ? Math.Min((double)revenus / max, 1.0) : 0.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Normalise les dépenses pour la barre de progression (0.0 à 1.0)
    /// </summary>
    public class DepensesProgressConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not decimal depenses) return 0.0;
            var max = Math.Max(App.RevenusMax, (double)depenses);
            return max > 0 ? Math.Min((double)depenses / max, 1.0) : 0.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
