using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000033 RID: 51
	internal readonly struct HasConstantValueOrTypePragmaParser
	{
		// Token: 0x0600039D RID: 925 RVA: 0x00010636 File Offset: 0x0000E836
		private HasConstantValueOrTypePragmaParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00010640 File Offset: 0x0000E840
		internal static _IExpression ParseHasConstantValuePragma(ParserContext context, IToken tokenPragma)
		{
			HasConstantValueOrTypePragmaParser hasConstantValueOrTypePragmaParser = new HasConstantValueOrTypePragmaParser(context);
			return hasConstantValueOrTypePragmaParser.ParseHasConstantValuePragma(tokenPragma);
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00010660 File Offset: 0x0000E860
		internal static _IExpression ParseHasConstantTypePragma(ParserContext context, IToken tokenPragma)
		{
			HasConstantValueOrTypePragmaParser hasConstantValueOrTypePragmaParser = new HasConstantValueOrTypePragmaParser(context);
			return hasConstantValueOrTypePragmaParser.ParseHasConstantTypePragma(tokenPragma);
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x060003A0 RID: 928 RVA: 0x0001067D File Offset: 0x0000E87D
		private ParserContext Context { get; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060003A1 RID: 929 RVA: 0x00010685 File Offset: 0x0000E885
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x060003A2 RID: 930 RVA: 0x00010692 File Offset: 0x0000E892
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x060003A3 RID: 931 RVA: 0x0001069F File Offset: 0x0000E89F
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x060003A4 RID: 932 RVA: 0x000106AC File Offset: 0x0000E8AC
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060003A5 RID: 933 RVA: 0x000106B9 File Offset: 0x0000E8B9
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060003A6 RID: 934 RVA: 0x000106C6 File Offset: 0x0000E8C6
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x000106D3 File Offset: 0x0000E8D3
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x000106E4 File Offset: 0x0000E8E4
		private _IExpression ParseHasConstantValuePragma(IToken tokenPragma)
		{
			_IHasConstantValueExpression2 ihasConstantValueExpression = (_IHasConstantValueExpression2)this.LMItemFactory.CreateHasConstantValueExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ihasConstantValueExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			string text;
			ihasConstantValueExpression._Constant = this.ParsePragmaExpression(tokenPragma, out text);
			if (ihasConstantValueExpression._Constant == null)
			{
				this.AddErrorST(ihasConstantValueExpression, 7, new object[]
				{
					text,
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				new PragmaSourcePositionAdjuster(this.Scanner.CurrentToken.Position, this.PragmaScanner.PositionOffset, this.TokenFactory).AdjustSourcePositions(ihasConstantValueExpression._Constant);
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				this.AddErrorST(ihasConstantValueExpression, 6, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ihasConstantValueExpression._ConstantValue = this.ParsePragmaExpression(tokenPragma, out text);
			if (ihasConstantValueExpression._ConstantValue == null)
			{
				this.AddErrorST(ihasConstantValueExpression, 2, new object[]
				{
					Strings.Literal,
					Strings.Constant,
					text
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || (pragmaToken.Operator != 47 && pragmaToken.Operator != 41))
			{
				this.AddErrorST(ihasConstantValueExpression, 2, new object[]
				{
					")",
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				this.ParseHasConstantValuePragmaComparison(pragmaToken, ihasConstantValueExpression);
			}
			return ihasConstantValueExpression;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x00010884 File Offset: 0x0000EA84
		private _IExpression ParseHasConstantTypePragma(IToken tokenPragma)
		{
			_IHasConstantTypeExpression ihasConstantTypeExpression = this.LMItemFactory.CreateHasConstantTypeExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ihasConstantTypeExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			string text;
			ihasConstantTypeExpression._Constant = this.ParsePragmaExpression(tokenPragma, out text);
			if (ihasConstantTypeExpression._Constant == null)
			{
				this.AddErrorST(ihasConstantTypeExpression, 7, new object[]
				{
					text,
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				new PragmaSourcePositionAdjuster(this.Scanner.CurrentToken.Position, this.PragmaScanner.PositionOffset, this.TokenFactory).AdjustSourcePositions(ihasConstantTypeExpression._Constant);
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				this.AddErrorST(ihasConstantTypeExpression, 6, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			_ILiteralExpression iliteralExpression = this.ParsePragmaExpression(tokenPragma, out text) as _ILiteralExpression;
			if (iliteralExpression == null || iliteralExpression.ConstantType != null)
			{
				this.AddErrorST(ihasConstantTypeExpression, 317, new object[]
				{
					text
				});
			}
			else
			{
				ihasConstantTypeExpression._ConstantTypeReplaced = iliteralExpression.LiteralValue.Bool;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(ihasConstantTypeExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return ihasConstantTypeExpression;
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00010A0C File Offset: 0x0000EC0C
		private _IExpression ParsePragmaExpression(IToken tokenPragma, out string stParsedSourceSnippet)
		{
			IScanner9 scanner = this.Context.Scanner;
			this.Context.Scanner = this.PragmaScanner.OrgScanner;
			int sourceOffset = this.Scanner.SourceOffset;
			IExpression expression = this.Context.ExpressionParser.ParseExpression();
			int sourceOffset2 = this.Scanner.SourceOffset;
			_IExpression iexpression = expression as _IExpression;
			if (iexpression != null)
			{
				IMinimalPosition positionIntern = this.LMItemFactory.CreateMinimalPosition(tokenPragma.Position, iexpression.PositionIntern.PositionOffset + 1);
				iexpression.PositionIntern = positionIntern;
			}
			stParsedSourceSnippet = ((_IScanner)this.Scanner).GetInputSubString(sourceOffset, sourceOffset2 - sourceOffset);
			this.Context.Scanner = scanner;
			return iexpression;
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00010AB8 File Offset: 0x0000ECB8
		private void ParseHasConstantValuePragmaComparison(IPragmaToken token, _IHasConstantValueExpression2 expHelp)
		{
			Operator opComparison = 179;
			if (token.Operator == 47)
			{
				IToken token2;
				if (this.PragmaScanner.OrgScanner.GetNext(ref token2) != 15)
				{
					this.AddErrorST(expHelp, 24, new object[]
					{
						this.PragmaScanner.OrgScanner.GetTokenText(token2)
					});
				}
				else
				{
					opComparison = this.PragmaScanner.OrgScanner.GetOperator(token2);
					if (this.PragmaScanner.GetNext(ref token) != 3 || token.Operator != 41)
					{
						this.AddErrorST(expHelp, 6, new object[]
						{
							")",
							this.PragmaScanner.GetTokenText(token)
						});
					}
				}
			}
			expHelp._OpComparison = opComparison;
		}
	}
}
