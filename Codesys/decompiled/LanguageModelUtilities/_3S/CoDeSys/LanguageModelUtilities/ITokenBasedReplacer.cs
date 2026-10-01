using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ITokenBasedReplacer
	{
		bool ContainsTokenInString(string stOrg, string stToken);

		string ReplaceTokenInString(string stOrg, string stToReplace, string stReplacement);
	}
}
