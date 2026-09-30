using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000034 RID: 52
	internal readonly struct HasTypePragmaParser
	{
		// Token: 0x060003AC RID: 940 RVA: 0x00010B6B File Offset: 0x0000ED6B
		private HasTypePragmaParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060003AD RID: 941 RVA: 0x00010B74 File Offset: 0x0000ED74
		internal static _IExpression ParseHasTypePragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			HasTypePragmaParser hasTypePragmaParser = new HasTypePragmaParser(context);
			return hasTypePragmaParser.ParseHasTypePragma(out bError, tokenPragma);
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00010B94 File Offset: 0x0000ED94
		internal static _IExpression ParseIsEnumTypePragma(ParserContext context, IToken tokenPragma)
		{
			HasTypePragmaParser hasTypePragmaParser = new HasTypePragmaParser(context);
			return hasTypePragmaParser.ParseIsEnumTypePragma(tokenPragma);
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060003AF RID: 943 RVA: 0x00010BB1 File Offset: 0x0000EDB1
		private ParserContext Context { get; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060003B0 RID: 944 RVA: 0x00010BB9 File Offset: 0x0000EDB9
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060003B1 RID: 945 RVA: 0x00010BC6 File Offset: 0x0000EDC6
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060003B2 RID: 946 RVA: 0x00010BD3 File Offset: 0x0000EDD3
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x00010BE0 File Offset: 0x0000EDE0
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060003B4 RID: 948 RVA: 0x00010BED File Offset: 0x0000EDED
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060003B5 RID: 949 RVA: 0x00010BFA File Offset: 0x0000EDFA
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00010C07 File Offset: 0x0000EE07
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00010C18 File Offset: 0x0000EE18
		private _IExpression ParseHasTypePragma(out bool bError, IToken tokenPragma)
		{
			_IHasTypeExpression ihasTypeExpression = this.LMItemFactory.CreateHasTypeExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(ihasTypeExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ihasTypeExpression.Variable = (this.ParseItemReference(out bError, tokenPragma) as _IVariableReference);
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				this.AddErrorST(ihasTypeExpression, 6, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ICompiledType compiledType = this.ParseType();
			_IUserdefType iuserdefType = compiledType as _IUserdefType;
			if (iuserdefType != null)
			{
				new PragmaSourcePositionAdjuster(this.Scanner.CurrentToken.Position, this.PragmaScanner.PositionOffset, this.TokenFactory).AdjustSourcePositions(iuserdefType.NameExpression as _IExpression);
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3)
			{
				this.AddErrorST(ihasTypeExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
				ihasTypeExpression.ReferencedType = compiledType;
				return ihasTypeExpression;
			}
			if (pragmaToken.Operator == 47)
			{
				if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || (pragmaToken.Operator != 48 && pragmaToken.Operator != 49))
				{
					this.AddErrorST(ihasTypeExpression, 6, new object[]
					{
						"FALSE",
						this.PragmaScanner.GetTokenText(pragmaToken)
					});
				}
				else if (pragmaToken.Operator == 49)
				{
					_IHasCompatibleTypeExpression ihasCompatibleTypeExpression = this.LMItemFactory.CreateHasCompatibleTypeExpression();
					ihasCompatibleTypeExpression.AssignFrom(ihasTypeExpression);
					ihasTypeExpression = ihasCompatibleTypeExpression;
				}
				this.PragmaScanner.GetNext(ref pragmaToken);
			}
			if (pragmaToken.Type != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(ihasTypeExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ihasTypeExpression.ReferencedType = compiledType;
			return ihasTypeExpression;
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00010E08 File Offset: 0x0000F008
		private ICompiledType ParseType()
		{
			IScanner9 scanner = this.Context.Scanner;
			this.Context.Scanner = this.PragmaScanner.OrgScanner;
			ICompiledType result = this.Context.TypeParser.ParseType();
			this.Context.Scanner = scanner;
			return result;
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00010E54 File Offset: 0x0000F054
		private _IExpression ParseIsEnumTypePragma(IToken tokenPragma)
		{
			_IIsEnumTypeExpression iisEnumTypeExpression = this.LMItemFactory.CreateIsEnumTypeExpression(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorST(iisEnumTypeExpression, 6, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			iisEnumTypeExpression.ReferencedType = this.ParseType();
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorST(iisEnumTypeExpression, 6, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return iisEnumTypeExpression;
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00010EF9 File Offset: 0x0000F0F9
		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(this.Context, out bError, tokenPragma);
		}
	}
}
