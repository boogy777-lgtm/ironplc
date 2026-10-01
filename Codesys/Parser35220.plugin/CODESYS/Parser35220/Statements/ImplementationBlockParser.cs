using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Scanner;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000026 RID: 38
	internal readonly struct ImplementationBlockParser
	{
		// Token: 0x06000297 RID: 663 RVA: 0x0000DC23 File Offset: 0x0000BE23
		private ImplementationBlockParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000DC2C File Offset: 0x0000BE2C
		internal static _IStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			ImplementationBlockParser implementationBlockParser = new ImplementationBlockParser(context);
			return implementationBlockParser.ParseImplementationBlock(out bError, token);
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000299 RID: 665 RVA: 0x0000DC4A File Offset: 0x0000BE4A
		private ParserContext Context { get; }

		// Token: 0x0600029A RID: 666 RVA: 0x0000DC54 File Offset: 0x0000BE54
		private _IStatement ParseImplementationBlock(out bool bError, IToken token)
		{
			_IToken itoken;
			_IExpression iexpression = PrefixedOperatorParser.Parse(this.Context, out bError, 286, (_IToken)token, null, out itoken);
			IOperatorExpression operatorExpression = iexpression as IOperatorExpression;
			if (operatorExpression == null)
			{
				bError = true;
				return this.Context.LMItemFactory.CreateExpressionStatement(iexpression);
			}
			string text;
			if (operatorExpression.Operands.Length > 1)
			{
				_IStringLiteralExpression istringLiteralExpression = operatorExpression.Operands[1] as _IStringLiteralExpression;
				if (istringLiteralExpression != null)
				{
					text = istringLiteralExpression.StringValue;
					goto IL_65;
				}
			}
			text = "__END_IMPLEMENTATION";
			IL_65:
			IToken token2;
			((_IScanner6)this.Context.Scanner).ReadTokenUntilTerminator(text, ref token2);
			_IEmbeddedLanguageStatement iembeddedLanguageStatement = this.Context.LMItemFactory.CreateEmbeddedLanguageStatement();
			iembeddedLanguageStatement.SetPosition(token);
			_IEmbeddedLanguageStatement iembeddedLanguageStatement2 = iembeddedLanguageStatement;
			_IStringLiteralExpression istringLiteralExpression2 = operatorExpression.Operands[0] as _IStringLiteralExpression;
			iembeddedLanguageStatement2.Kind = ((istringLiteralExpression2 != null) ? istringLiteralExpression2.StringValue : null);
			if (token2 != null)
			{
				iembeddedLanguageStatement.MergePosition(token2);
				InternalScanner internalScanner = this.Context.Scanner as InternalScanner;
				if (internalScanner != null)
				{
					iembeddedLanguageStatement.RawSegment = new ArraySegment<char>(internalScanner.RawInput, token2.SourceOffset, token2.Length);
				}
				else
				{
					iembeddedLanguageStatement.RawSegment = new ArraySegment<char>(((_IScanner)this.Context.Scanner).GetInputSubString(token2.SourceOffset, token2.Length).ToCharArray());
				}
			}
			else
			{
				this.Context.ErrorHandler.AddErrorST(iembeddedLanguageStatement, 588, new object[]
				{
					text
				});
			}
			return iembeddedLanguageStatement;
		}
	}
}
