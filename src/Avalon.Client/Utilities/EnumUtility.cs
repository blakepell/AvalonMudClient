namespace Avalon.Utilities
{
    /// <summary>
    /// Enum utility that provides a method to return a enum value along with a description attribute provided by
    /// it for use with binding to ComboBoxes.
    /// </summary>
    public static class EnumUtility
    {
     
        /// <summary>
        /// Returns the value and description for the given enum.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static object[] GetValuesAndDescriptions(Type enumType)
        {
            var values = Enum.GetValues(enumType).Cast<object>();
            var valuesAndDescriptions = from value in values
                                        select new
                                        {
                                            Value = value,
                                            Description = value.GetType().GetMember(value.ToString() ?? string.Empty)[0].GetCustomAttributes(true).OfType<DescriptionAttribute>().Any() ?
                                                value.GetType().GetMember(value.ToString() ?? string.Empty)[0].GetCustomAttributes(true).OfType<DescriptionAttribute>().First().Description
                                                : value
                                        };

            var list = new List<object>();

            foreach (var description in valuesAndDescriptions)
            {
                list.Add(description);
            }

            return list.ToArray();
        }
    }
}
