using System;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public static class ExtensionMethods
	{
		public static bool Contains(this string @this, string value, StringComparison comparisonType)
		{
			if (@this == null)
			{
				throw new ArgumentNullException("this");
			}
			return @this.IndexOf(value, comparisonType) >= 0;
		}
	}
}
