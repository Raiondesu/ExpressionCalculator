using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ExpressionCalc
{
	public static class Extensions
	{
		public static string Description(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			var attribute = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute), false);
			return attribute != null ? attribute.Description : value.ToString();
		}

		public static T ToEnum<T>(this string description, T _default = default(T))
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new InvalidOperationException();
			foreach (var field in type.GetFields())
			{
				var attribute = Attribute.GetCustomAttribute(field,
					typeof(DescriptionAttribute)) as DescriptionAttribute;

				if (attribute != null && string.Equals(attribute.Description, description, StringComparison.CurrentCultureIgnoreCase))
						return (T) field.GetValue(null);

				if (string.Equals(field.Name, description, StringComparison.CurrentCultureIgnoreCase))
						return (T) field.GetValue(null);
			}
			return _default;
		}

		public static string[] GetDescriptions(this Enum value)
			=> Enum.GetValues(value.GetType()).OfType<Enum>()
				.Select(x => x.Description())
				.OrderByDescending(e => e.Length).ToArray();

		public static string GetLast(this string source, int tailLength = 5)
		{
			return tailLength >= source.Length ? source : source.Substring(source.Length - tailLength);
		}
	}
}