using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;

namespace CODESYS.Parser35210.Scanner
{
	public class MultiStringScanner : InternalScanner, IMultiStringScanner, _IScanner5, _IScanner4, _IScanner3, _IScanner2, _IScanner, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner, IScanner7, IScanner8, IScanner9
	{
		private class MultiStringToken : _IToken, IToken
		{
			private Token _token;

			internal int StringIndex { get; set; }

			public TokenType Type => _token.Type;

			public int SourceOffset => _token.SourceOffset;

			public long Position => _token.Position;

			public short PositionOffset => _token.PositionOffset;

			public int Length => _token.Length;

			public int SourceLine => _token.SourceLine;

			public int SourceColumn => _token.SourceColumn;

			public long CharactersToSkipSeen
			{
				get
				{
					return _token.CharactersToSkipSeen;
				}
				set
				{
					_token.CharactersToSkipSeen = value;
				}
			}

			internal MultiStringToken(IToken token)
			{
				_token = (Token)(object)token;
			}
		}

		private IList<string> _strings;

		private int _nCurrentIndex;

		public MultiStringScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService sos)
			: base(typeTable, overflowChecker, sos)
		{
		}

		public void InitializeMulti(IList<string> strings)
		{
			_strings = strings;
			_nCurrentIndex = 0;
			Initialize(_strings[_nCurrentIndex]);
		}

		public override void SetPosition(IToken token)
		{
			if (token is MultiStringToken multiStringToken && _nCurrentIndex != multiStringToken.StringIndex)
			{
				_nCurrentIndex = multiStringToken.StringIndex;
				Initialize(_strings[_nCurrentIndex]);
			}
			base.SetPosition(token);
		}

		public override TokenType GetNext(out IToken token)
		{
			TokenType next = base.GetNext(out token);
			if (next == TokenType.End && _nCurrentIndex < _strings.Count - 1)
			{
				_nCurrentIndex++;
				Initialize(_strings[_nCurrentIndex]);
				next = GetNext(out token);
				if (token is Token)
				{
					MultiStringToken multiStringToken = (MultiStringToken)(token = new MultiStringToken(token));
				}
				((MultiStringToken)token).StringIndex = _nCurrentIndex;
			}
			return next;
		}
	}
}
