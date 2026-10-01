using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;

namespace CODESYS.Parser35210.PragmaScanner
{
	internal class PragmaToken : IPragmaToken
	{
		internal PragmaTokenType _type;

		internal PragmaOperator _pop;

		internal string _stIdent;

		internal long _lInteger;

		internal string _stString;

		public PragmaTokenType Type => _type;

		public IToken OrgToken { get; }

		public long Integer => _lInteger;

		public string Identifier => _stIdent;

		public string String => _stString;

		public PragmaOperator Operator => _pop;

		public PragmaToken(IToken tokenOrg)
		{
			OrgToken = tokenOrg;
		}
	}
}
