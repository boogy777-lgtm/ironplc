using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.Nodes.Tokens;
using CODESYS.WhiteParseTrees.Services;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class TokenStream
	{
		public static IList<IWhiteToken> ReadTokenStream(string stCodeInput)
		{
			IScanner7 scanner = TryCreateScannerForText(stCodeInput);
			scanner.AllowNestedComments = APEnvironment.LanguageModelManagerConsolidated.AllowNestedComments;
			scanner.AllowMultipleUnderlines = true;
			IWhiteToken whiteToken = null;
			List<IWhiteToken> list = new List<IWhiteToken>();
			do
			{
				scanner.GetNext(out var token);
				try
				{
					IWhiteToken whiteToken2 = TokenFactory.CreateToken(token, scanner);
					if (whiteToken is INonSyntacticToken nonSyntacticToken)
					{
						whiteToken2.Leading = nonSyntacticToken;
						nonSyntacticToken.Trailing = whiteToken2;
					}
					whiteToken = whiteToken2;
					list.Add(whiteToken2);
				}
				catch
				{
					ErrorToken item = new ErrorToken(scanner.GetTokenText(token));
					list.Add(item);
				}
			}
			while (whiteToken != null && whiteToken.Type != WhiteTokenType.End);
			return list;
		}

		public static IList<IWhiteToken> ReadTokenStream(string stCodeInput, out SourcePositionMap sourcePositionMap)
		{
			IScanner7 scanner = TryCreateScannerForText(stCodeInput);
			scanner.AllowNestedComments = APEnvironment.LanguageModelManagerConsolidated.AllowNestedComments;
			scanner.AllowMultipleUnderlines = true;
			IWhiteToken whiteToken = null;
			sourcePositionMap = new SourcePositionMap();
			List<IWhiteToken> list = new List<IWhiteToken>();
			int num = 0;
			do
			{
				scanner.GetNext(out var token);
				if (token.PositionOffset == 0)
				{
					sourcePositionMap.AddNewStartPosition(num, token.Position);
				}
				num += token.Length;
				IWhiteToken whiteToken2 = TokenFactory.CreateToken(token, scanner);
				if (whiteToken is INonSyntacticToken nonSyntacticToken)
				{
					whiteToken2.Leading = nonSyntacticToken;
					nonSyntacticToken.Trailing = whiteToken2;
				}
				whiteToken = whiteToken2;
				list.Add(whiteToken2);
			}
			while (whiteToken.Type != WhiteTokenType.End);
			return list;
		}

		private static IScanner7 TryCreateScannerForText(string stCodeInput)
		{
			return (APEnvironment.LMServiceProvider.CreatorService.CreateScanner(stCodeInput, bIncludeComments: true, bIncludeEndOfLines: true, bIncludePragmas: true, bIncludeWhitespaces: true) as IScanner7) ?? throw new TooOldCompilerversionException(new Version(3, 5, 16, 0));
		}
	}
}
