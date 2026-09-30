using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{TokenText}")]
	internal sealed class HumanReadableToken
	{
		private IToken _token;

		private string _stTokenText;

		internal IToken Token => _token;

		internal string TokenText => _stTokenText;

		internal HumanReadableToken(IToken token, string stTokenText)
		{
			_token = token;
			_stTokenText = stTokenText;
		}
	}
}
