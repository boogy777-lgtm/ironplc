using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000032 RID: 50
	internal readonly struct HasAttributePragmaParser
	{
		// Token: 0x06000393 RID: 915 RVA: 0x0001048C File Offset: 0x0000E68C
		private HasAttributePragmaParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00010498 File Offset: 0x0000E698
		internal static _IExpression ParseHasAttributePragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			HasAttributePragmaParser hasAttributePragmaParser = new HasAttributePragmaParser(context);
			return hasAttributePragmaParser.ParseHasAttributePragma(out bError, tokenPragma);
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000395 RID: 917 RVA: 0x000104B6 File Offset: 0x0000E6B6
		private ParserContext Context { get; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000396 RID: 918 RVA: 0x000104BE File Offset: 0x0000E6BE
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000397 RID: 919 RVA: 0x000104CB File Offset: 0x0000E6CB
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000398 RID: 920 RVA: 0x000104D8 File Offset: 0x0000E6D8
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000399 RID: 921 RVA: 0x000104E5 File Offset: 0x0000E6E5
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600039A RID: 922 RVA: 0x000104F2 File Offset: 0x0000E6F2
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00010502 File Offset: 0x0000E702
		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(this.Context, out bError, tokenPragma);
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00010514 File Offset: 0x0000E714
		private _IExpression ParseHasAttributePragma(out bool bError, IToken tokenPragma)
		{
			_IHasAttributeExpression ihasAttributeExpression = this.LMItemFactory.CreateHasAttributeExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ihasAttributeExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ihasAttributeExpression.ItemReference = this.ParseItemReference(out bError, tokenPragma);
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				this.AddErrorST(ihasAttributeExpression, 6, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 4)
			{
				this.AddErrorST(ihasAttributeExpression, 51, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				ihasAttributeExpression.Attribute = pragmaToken.String;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(ihasAttributeExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return ihasAttributeExpression;
		}
	}
}
