using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000030 RID: 48
	internal readonly struct DefinedPragmaOperandParser
	{
		// Token: 0x06000378 RID: 888 RVA: 0x0000FE4D File Offset: 0x0000E04D
		private DefinedPragmaOperandParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000379 RID: 889 RVA: 0x0000FE58 File Offset: 0x0000E058
		internal static _IExpression ParseDefinedPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			DefinedPragmaOperandParser definedPragmaOperandParser = new DefinedPragmaOperandParser(context);
			return definedPragmaOperandParser.ParseDefinedPragma(out bError, tokenPragma);
		}

		// Token: 0x0600037A RID: 890 RVA: 0x0000FE78 File Offset: 0x0000E078
		internal static _IExpression ParseProjectDefinedPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			DefinedPragmaOperandParser definedPragmaOperandParser = new DefinedPragmaOperandParser(context);
			return definedPragmaOperandParser.ParseProjectDefinedPragma(out bError, tokenPragma);
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x0600037B RID: 891 RVA: 0x0000FE96 File Offset: 0x0000E096
		private ParserContext Context { get; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0000FE9E File Offset: 0x0000E09E
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x0600037D RID: 893 RVA: 0x0000FEAB File Offset: 0x0000E0AB
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0000FEB8 File Offset: 0x0000E0B8
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x0600037F RID: 895 RVA: 0x0000FEC5 File Offset: 0x0000E0C5
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000380 RID: 896 RVA: 0x0000FED2 File Offset: 0x0000E0D2
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000381 RID: 897 RVA: 0x0000FEE2 File Offset: 0x0000E0E2
		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(this.Context, out bError, tokenPragma);
		}

		// Token: 0x06000382 RID: 898 RVA: 0x0000FEF4 File Offset: 0x0000E0F4
		private _IExpression ParseDefinedPragma(out bool bError, IToken tokenPragma)
		{
			_IDefinedExpression idefinedExpression = this.LMItemFactory.CreateDefinedExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(idefinedExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			idefinedExpression.ItemReference = this.ParseItemReference(out bError, tokenPragma);
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(idefinedExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return idefinedExpression;
		}

		// Token: 0x06000383 RID: 899 RVA: 0x0000FF9C File Offset: 0x0000E19C
		private _IExpression ParseProjectDefinedPragma(out bool bError, IToken tokenPragma)
		{
			_IProjectDefinedExpression iprojectDefinedExpression = this.LMItemFactory.CreateProjectDefinedExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(iprojectDefinedExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
				bError = true;
				return iprojectDefinedExpression;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				this.AddErrorST(iprojectDefinedExpression, 26, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
				bError = true;
				return iprojectDefinedExpression;
			}
			iprojectDefinedExpression.DefineReference = this.LMItemFactory.CreateDefineReference(tokenPragma, pragmaToken.Identifier);
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(iprojectDefinedExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
				bError = true;
			}
			bError = false;
			return iprojectDefinedExpression;
		}
	}
}
