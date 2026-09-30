using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000035 RID: 53
	internal readonly struct HasValuePragmaParser
	{
		// Token: 0x060003BB RID: 955 RVA: 0x00010F08 File Offset: 0x0000F108
		private HasValuePragmaParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00010F14 File Offset: 0x0000F114
		internal static _IExpression ParseHasValuePragma(ParserContext context, IToken tokenPragma)
		{
			HasValuePragmaParser hasValuePragmaParser = new HasValuePragmaParser(context);
			return hasValuePragmaParser.ParseHasValuePragma(tokenPragma);
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060003BD RID: 957 RVA: 0x00010F31 File Offset: 0x0000F131
		private ParserContext Context { get; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x060003BE RID: 958 RVA: 0x00010F39 File Offset: 0x0000F139
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x060003BF RID: 959 RVA: 0x00010F46 File Offset: 0x0000F146
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x00010F53 File Offset: 0x0000F153
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x060003C1 RID: 961 RVA: 0x00010F60 File Offset: 0x0000F160
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00010F6D File Offset: 0x0000F16D
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00010F80 File Offset: 0x0000F180
		private _IExpression ParseHasValuePragma(IToken tokenPragma)
		{
			_IHasValueExpression ihasValueExpression = this.LMItemFactory.CreateHasValueExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ihasValueExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				this.AddErrorST(ihasValueExpression, 26, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				ihasValueExpression.Define = pragmaToken.Identifier;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				this.AddErrorST(ihasValueExpression, 6, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 4)
			{
				this.AddErrorST(ihasValueExpression, 85, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				ihasValueExpression.DefineValue = pragmaToken.String;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(ihasValueExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return ihasValueExpression;
		}
	}
}
