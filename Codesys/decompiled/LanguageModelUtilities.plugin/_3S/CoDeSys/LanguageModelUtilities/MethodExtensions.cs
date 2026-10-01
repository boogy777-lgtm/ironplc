using System.Collections.Generic;
using System.Text;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public static class MethodExtensions
	{
		public static IEnumerable<string> Split(this string str, int chunkSize)
		{
			List<string> list = new List<string>();
			string[] array = str.Split(' ');
			if (array != null && array.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (stringBuilder.Length > chunkSize)
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
					stringBuilder.Append(text + " ");
				}
				if (stringBuilder.Length > 0)
				{
					list.Add(stringBuilder.ToString());
				}
			}
			return list;
		}
	}
}
