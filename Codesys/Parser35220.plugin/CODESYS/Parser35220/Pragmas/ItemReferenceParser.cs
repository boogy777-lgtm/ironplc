using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000036 RID: 54
	internal readonly struct ItemReferenceParser
	{
		// Token: 0x060003C4 RID: 964 RVA: 0x000110D0 File Offset: 0x0000F2D0
		private ItemReferenceParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x000110DC File Offset: 0x0000F2DC
		internal static _IItemReference ParseItemReference(ParserContext context, out bool bError, IToken tokenPragma)
		{
			ItemReferenceParser itemReferenceParser = new ItemReferenceParser(context);
			return itemReferenceParser.ParseItemReference(out bError, tokenPragma);
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060003C6 RID: 966 RVA: 0x000110FA File Offset: 0x0000F2FA
		private ParserContext Context { get; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x00011102 File Offset: 0x0000F302
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060003C8 RID: 968 RVA: 0x0001110F File Offset: 0x0000F30F
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x0001111C File Offset: 0x0000F31C
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060003CA RID: 970 RVA: 0x00011129 File Offset: 0x0000F329
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00011136 File Offset: 0x0000F336
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00011148 File Offset: 0x0000F348
		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			bError = false;
			IPragmaToken pragmaToken;
			PragmaTokenType next = this.PragmaScanner.GetNext(ref pragmaToken);
			_IItemReference result = null;
			if (next != 1)
			{
				if (next == 3)
				{
					switch (pragmaToken.Operator)
					{
					case 17:
						result = this.ParseInstancePath(out bError, tokenPragma);
						break;
					case 18:
						result = this.ParseTypeReference(tokenPragma);
						break;
					case 19:
						result = this.ParseTaskReference(tokenPragma);
						break;
					case 21:
						result = this.ParseResourceReference(tokenPragma);
						break;
					case 23:
						result = this.ParsePOUReference(tokenPragma);
						break;
					}
				}
			}
			else
			{
				result = this.LMItemFactory.CreateDefineReference(tokenPragma, pragmaToken.Identifier);
			}
			return result;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x000111E8 File Offset: 0x0000F3E8
		private _IItemReference ParseResourceReference(IToken tokenPragma)
		{
			_IResourceReference iresourceReference = this.LMItemFactory.CreateResourceReference(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 42)
			{
				this.AddErrorST(iresourceReference, 6, new object[]
				{
					":",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				this.AddErrorST(iresourceReference, 26, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				iresourceReference.ResourceName = pragmaToken.Identifier;
			}
			return iresourceReference;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00011280 File Offset: 0x0000F480
		private _IItemReference ParseTaskReference(IToken tokenPragma)
		{
			_ITaskReference itaskReference = this.LMItemFactory.CreateTaskReference(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 42)
			{
				this.AddErrorST(itaskReference, 6, new object[]
				{
					":",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				this.AddErrorST(itaskReference, 26, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			else
			{
				itaskReference.TaskName = pragmaToken.Identifier;
			}
			return itaskReference;
		}

		// Token: 0x060003CF RID: 975 RVA: 0x00011318 File Offset: 0x0000F518
		private _IItemReference ParsePOUReference(IToken tokenPragma)
		{
			_IPouReference ipouReference = this.LMItemFactory.CreatePouReference(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 42)
			{
				this.AddErrorST(ipouReference, 6, new object[]
				{
					":",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ipouReference.InstancePath = (this.ParsePragmaQualifiedNameExpression(ipouReference) ?? this.LMItemFactory.CreateVariableExpression(string.Empty));
			return ipouReference;
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00011394 File Offset: 0x0000F594
		private _IItemReference ParseTypeReference(IToken tokenPragma)
		{
			_ITypeReference itypeReference = this.LMItemFactory.CreateTypeReference(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 42)
			{
				this.AddErrorST(itypeReference, 6, new object[]
				{
					":",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			itypeReference.InstancePath = this.ParsePragmaQualifiedNameExpression(itypeReference);
			return itypeReference;
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x000113FC File Offset: 0x0000F5FC
		private _IItemReference ParseInstancePath(out bool bError, IToken tokenPragma)
		{
			_IVariableReference ivariableReference = this.LMItemFactory.CreateVariableReference(tokenPragma);
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 42)
			{
				this.AddErrorST(ivariableReference, 6, new object[]
				{
					":",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			ivariableReference.InstancePath = this.ParsePragmaInstancePath(out bError);
			return ivariableReference;
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00011464 File Offset: 0x0000F664
		private _IExpression ParsePragmaInstancePath(out bool bError)
		{
			IScanner9 scanner = this.Context.Scanner;
			this.Context.Scanner = this.PragmaScanner.OrgScanner;
			_IExpression iexpression = this.Context.ExpressionParser.ParseSTOperand(out bError);
			this.Context.Scanner = scanner;
			if (iexpression != null)
			{
				new PragmaSourcePositionAdjuster(scanner.CurrentToken.Position, this.PragmaScanner.PositionOffset, this.Context.TokenFactory).AdjustSourcePositions(iexpression);
			}
			return iexpression;
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x000114E4 File Offset: 0x0000F6E4
		private _IExpression ParsePragmaQualifiedNameExpression(_IExprement expForError)
		{
			IScanner9 scanner = this.Context.Scanner;
			this.Context.Scanner = this.PragmaScanner.OrgScanner;
			_IExpression iexpression = this.Context.ExpressionParser.ParseQualifiedNameExpression(expForError);
			this.Context.Scanner = scanner;
			if (iexpression != null)
			{
				new PragmaSourcePositionAdjuster(scanner.CurrentToken.Position, this.PragmaScanner.PositionOffset, this.Context.TokenFactory).AdjustSourcePositions(iexpression);
			}
			return iexpression;
		}
	}
}
