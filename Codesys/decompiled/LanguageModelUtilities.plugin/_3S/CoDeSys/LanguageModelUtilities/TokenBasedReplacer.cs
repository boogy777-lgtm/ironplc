using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{5AD43649-7A58-4136-9D10-E30374E5B9C5}")]
	public class TokenBasedReplacer : ITokenBasedReplacer
	{
		public bool ContainsTokenInString(string stOrg, string stToken)
		{
			List<KeyValuePair<int, int>> list = DetermineReplacements(stOrg, stToken);
			if (list != null)
			{
				return list.Count > 0;
			}
			return false;
		}

		public string ReplaceTokenInString(string stOrg, string stToReplace, string stReplacement)
		{
			List<KeyValuePair<int, int>> list = DetermineReplacements(stOrg, stToReplace);
			if (list == null)
			{
				return stOrg;
			}
			string text = stOrg;
			list.Reverse();
			foreach (KeyValuePair<int, int> item in list)
			{
				text = text.Remove(item.Key, item.Value);
				text = text.Insert(item.Key, stReplacement);
			}
			return text;
		}

		private static List<KeyValuePair<int, int>> DetermineReplacements(string stOrg, string stToReplace)
		{
			List<KeyValuePair<int, int>> list = null;
			int num = stOrg.IndexOf(stToReplace);
			if (num >= 0)
			{
				int length = stToReplace.Length;
				list = new List<KeyValuePair<int, int>>();
				do
				{
					if (IsCompleteIdentifier(stOrg, num, length))
					{
						list.Add(new KeyValuePair<int, int>(num, length));
					}
				}
				while ((num = stOrg.IndexOf(stToReplace, num + length)) >= 0);
			}
			return list;
		}

		private static bool IsCompleteIdentifier(string stWholeString, int iIndexTokenCandidateBegin, int iToReplaceLen)
		{
			if (iIndexTokenCandidateBegin == 0 || !IsIdentifierChar(stWholeString[iIndexTokenCandidateBegin - 1]))
			{
				int num = iIndexTokenCandidateBegin + iToReplaceLen;
				if (num >= stWholeString.Length || !IsIdentifierChar(stWholeString[num]))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsIdentifierChar(char c)
		{
			if (!char.IsLetterOrDigit(c))
			{
				return c == '_';
			}
			return true;
		}
	}
}
