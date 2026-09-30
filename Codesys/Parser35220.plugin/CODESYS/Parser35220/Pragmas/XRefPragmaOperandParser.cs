using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x0200003B RID: 59
	internal readonly struct XRefPragmaOperandParser
	{
		// Token: 0x06000424 RID: 1060 RVA: 0x000127D5 File Offset: 0x000109D5
		private XRefPragmaOperandParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x000127E0 File Offset: 0x000109E0
		internal static _IExpression ParseXRefPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			XRefPragmaOperandParser xrefPragmaOperandParser = new XRefPragmaOperandParser(context);
			return xrefPragmaOperandParser.ParseXRefPragma(out bError, tokenPragma);
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000426 RID: 1062 RVA: 0x000127FE File Offset: 0x000109FE
		private ParserContext Context { get; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x00012806 File Offset: 0x00010A06
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000428 RID: 1064 RVA: 0x00012813 File Offset: 0x00010A13
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000429 RID: 1065 RVA: 0x00012820 File Offset: 0x00010A20
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x0600042A RID: 1066 RVA: 0x0001282D File Offset: 0x00010A2D
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x0001283A File Offset: 0x00010A3A
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x0001284A File Offset: 0x00010A4A
		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(this.Context, out bError, tokenPragma);
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0001285C File Offset: 0x00010A5C
		private _IExpression ParseXRefPragma(out bool bError, IToken tokenPragma)
		{
			_IXRefExpression ixrefExpression = this.LMItemFactory.CreateXRefExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ixrefExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ixrefExpression.XRef = this.ParseItemReference(out bError, tokenPragma);
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 3 && pragmaToken.Operator == 22)
			{
				ixrefExpression.XRefFrom = this.ParseItemReference(out bError, tokenPragma);
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(ixrefExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return ixrefExpression;
		}
	}
}
