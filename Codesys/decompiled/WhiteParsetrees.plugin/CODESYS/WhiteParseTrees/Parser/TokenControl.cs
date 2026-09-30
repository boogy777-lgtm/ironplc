using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class TokenControl
	{
		private readonly string _stTransitionOperatorText;

		private readonly string _stNamespaceOperatorText;

		[System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken this[int ix]
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get
			{
				if (ix >= 0 && TokenList != null && ix < TokenList.Count)
				{
					return TokenList[ix];
				}
				return null;
			}
		}

		public int CurrentTokenIndex { get; set; }

		private IList<IWhiteToken> TokenList { get; }

		[System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken CurrentToken
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get
			{
				return this[CurrentTokenIndex];
			}
		}

		public int StartStatementTokenIndex { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken StartStatementToken
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get
			{
				return this[StartStatementTokenIndex];
			}
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken LastToken
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get
			{
				return this[TokenList.Count - 1];
			}
		}

		public TokenControl(IList<IWhiteToken> tokens)
		{
			TokenList = tokens;
			CurrentTokenIndex = -1;
			IScanner scanner = APEnvironment.LMServiceProvider.CreatorService.CreateScanner(string.Empty, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			_stTransitionOperatorText = scanner.GetOperatorText(Operator.Transition);
			_stNamespaceOperatorText = scanner.GetOperatorText(Operator.Namespace);
		}

		internal bool IsTokenWithTextTransition(IWhiteToken token)
		{
			return _stTransitionOperatorText.Equals(token.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		internal bool IsTokenWithTextNamespace(IWhiteToken token)
		{
			return _stNamespaceOperatorText.Equals(token.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		internal bool IsTokenToStartTransitionDeclaration(IWhiteToken lookAhead1)
		{
			if (!IsTokenWithTextTransition(lookAhead1))
			{
				return false;
			}
			return CheckNextTokenIdentifierOrAccessModifier();
		}

		private bool CheckNextTokenIdentifierOrAccessModifier()
		{
			LookAhead2(out var _, out var second);
			if (second == null)
			{
				return false;
			}
			WhiteTokenType type = second.Type;
			if (type == WhiteTokenType.Identifier || type == WhiteTokenType.Abstract || (uint)(type - 248) <= 4u)
			{
				return true;
			}
			return false;
		}

		internal bool IsTokenToStartNamespaceDeclaration(IWhiteToken lookAhead1)
		{
			if (!IsTokenWithTextNamespace(lookAhead1))
			{
				return false;
			}
			return CheckNextTokenIdentifierOrAccessModifier();
		}

		private bool IsWhiteSpace(WhiteTokenType tt, bool statementStart)
		{
			switch (tt)
			{
			case WhiteTokenType.Comment:
			case WhiteTokenType.DocComment:
			case WhiteTokenType.Pragma:
				return !statementStart;
			case WhiteTokenType.EndOfLine:
			case WhiteTokenType.Whitespace:
				return true;
			default:
				return false;
			}
		}

		public IWhiteToken Next(bool statementStart)
		{
			IWhiteToken whiteToken = BasicNext();
			while (IsWhiteSpace(whiteToken.Type, statementStart))
			{
				whiteToken = BasicNext();
			}
			return whiteToken;
		}

		public IWhiteToken Next()
		{
			return Next(statementStart: false);
		}

		public T Next<[System.Runtime.CompilerServices.Nullable(0)] T>() where T : IWhiteToken
		{
			IWhiteToken whiteToken = Next(statementStart: false);
			if (whiteToken is T)
			{
				return (T)whiteToken;
			}
			string message = WhiteParserMessages.UnexpectedToken(whiteToken, typeof(T));
			throw new ResynchronizeException(StartStatementTokenIndex, message, whiteToken);
		}

		public T Assert<[System.Runtime.CompilerServices.Nullable(0)] T>(IWhiteToken token) where T : IWhiteToken
		{
			if (token is T)
			{
				return (T)token;
			}
			string message = WhiteParserMessages.UnexpectedToken(token, typeof(T));
			throw new ResynchronizeException(StartStatementTokenIndex, message, token);
		}

		private IWhiteToken BasicNext()
		{
			CurrentTokenIndex++;
			if (CurrentToken == null)
			{
				string message = WhiteParserMessages.UnexpectedEndOfInput();
				throw new ResynchronizeException(StartStatementTokenIndex, message, TokenList.LastOrDefault());
			}
			return CurrentToken;
		}

		public IWhiteToken LookAhead1(bool bStatementStart = false)
		{
			int currentTokenIndex = CurrentTokenIndex;
			IWhiteToken result = Next(bStatementStart);
			CurrentTokenIndex = currentTokenIndex;
			return result;
		}

		public bool TryNext<T>([System.Runtime.CompilerServices.Nullable(2)][NotNullWhen(true)] out T token) where T : class, IWhiteToken
		{
			if (LookAhead1() is T val)
			{
				token = val;
				Next();
				return true;
			}
			token = null;
			return false;
		}

		public bool CheckNext<T>() where T : class, IWhiteToken
		{
			return LookAhead1() is T;
		}

		public void LookAhead2(out IWhiteToken first, out IWhiteToken second)
		{
			int currentTokenIndex = CurrentTokenIndex;
			first = Next();
			second = Next();
			CurrentTokenIndex = currentTokenIndex;
		}
	}
}
