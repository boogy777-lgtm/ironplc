using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200005D RID: 93
	internal readonly struct VariableDeclarationParser
	{
		// Token: 0x060005DB RID: 1499 RVA: 0x00018F45 File Offset: 0x00017145
		private VariableDeclarationParser(ParserContext context, IToken token)
		{
			this.Context = context;
			this._vds = this.Context.LMItemFactory.CreateVariableDeclarationStatement(token);
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x00018F68 File Offset: 0x00017168
		internal static _IVariableDeclarationStatement ParseVariableDeclaration(ParserContext context, IToken token)
		{
			VariableDeclarationParser variableDeclarationParser = new VariableDeclarationParser(context, token);
			return variableDeclarationParser.ParseVariableDeclaration(token);
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060005DD RID: 1501 RVA: 0x00018F86 File Offset: 0x00017186
		private ParserContext Context { get; }

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x00018F8E File Offset: 0x0001718E
		private Operator OpCurrentVariableList
		{
			get
			{
				return this.DeclarationParser.OpCurrentVariableList;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060005DF RID: 1503 RVA: 0x00018F9B File Offset: 0x0001719B
		private DeclarationParser DeclarationParser
		{
			get
			{
				return this.Context.DeclarationParser;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x00018FA8 File Offset: 0x000171A8
		private bool AllowPaths
		{
			get
			{
				return this.DeclarationParser.AllowPaths;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060005E1 RID: 1505 RVA: 0x00018FB5 File Offset: 0x000171B5
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060005E2 RID: 1506 RVA: 0x00018FC2 File Offset: 0x000171C2
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060005E3 RID: 1507 RVA: 0x00018FCF File Offset: 0x000171CF
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00018FDC File Offset: 0x000171DC
		private void ParseReSyncIF()
		{
			this.Scanner.ParseReSyncIF();
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00018FE9 File Offset: 0x000171E9
		private Operator MatchOperator(_IExprement exp, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, exp, ops);
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00018FFE File Offset: 0x000171FE
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x0001900B File Offset: 0x0001720B
		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x0001901D File Offset: 0x0001721D
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060005E9 RID: 1513 RVA: 0x0001902B File Offset: 0x0001722B
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00019038 File Offset: 0x00017238
		private _IExpression ParseInitialisation()
		{
			return this.ExpressionParser.ParseInitialisation();
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00019045 File Offset: 0x00017245
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00019058 File Offset: 0x00017258
		private _IVariableDeclarationStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			Operator @operator = 0;
			this.Scanner.SetPosition(tokenIdent);
			if (this.ParseNamesReturnError(ref @operator))
			{
				return this._vds;
			}
			if (@operator == 63 && this.ParseAddressReturnError())
			{
				return this._vds;
			}
			this._vds.Type = this.TypeParser.ParseType(true, false, tokenIdent);
			if (this._vds.Type != null)
			{
				_IExprement vds = this._vds;
				Operator[] array = new Operator[5];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.42DD65E4DCB54DCCDAF27E0C604C303FFC8E2A9AC7F3AAB8E518D32D25126804).FieldHandle);
				Operator operator2 = this.MatchOperator(vds, array);
				operator2 = this.TryParseInitInputAssignments(operator2);
				if (operator2 != 164)
				{
					if (operator2 == 172)
					{
						goto IL_E2;
					}
					if (operator2 != 185)
					{
						return this._vds;
					}
				}
				this._vds.Initial = this.ParseInitialisation();
				this.MatchOperator(this._vds, new Operator[]
				{
					172
				});
				this._vds.RefAssignInitialisation = (operator2 == 185);
			}
			IL_E2:
			return this._vds;
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00019150 File Offset: 0x00017350
		private Operator TryParseInitInputAssignments(Operator op)
		{
			if (op == 169)
			{
				this.MatchOperator(this._vds, new Operator[]
				{
					167
				});
				do
				{
					this.ParseInitialisationAssignments(this._vds);
				}
				while (this.MatchOperator(this._vds, new Operator[]
				{
					171,
					170
				}) == 171 && this.MatchOperator(this._vds, new Operator[]
				{
					167
				}) == 167);
				_IExprement vds = this._vds;
				Operator[] array = new Operator[3];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.2D358135687F0F628B688304D52E5723A20164C3AA49648347024C0989600031).FieldHandle);
				op = this.MatchOperator(vds, array);
			}
			else if (op == 167)
			{
				this.ParseInitialisationAssignments(this._vds);
				this._vds.OldInputAssigns = true;
				_IExprement vds2 = this._vds;
				Operator[] array2 = new Operator[3];
				RuntimeHelpers.InitializeArray(array2, fieldof(<PrivateImplementationDetails>.2D358135687F0F628B688304D52E5723A20164C3AA49648347024C0989600031).FieldHandle);
				op = this.MatchOperator(vds2, array2);
			}
			return op;
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x0001923C File Offset: 0x0001743C
		private bool ParseAddressReturnError()
		{
			IToken token;
			if (this.Next(out token) == 7)
			{
				DirectVariableLocation directVariableLocation;
				DirectVariableSize directVariableSize;
				int[] array;
				bool flag;
				this.Scanner.GetDirectVariable(token, ref directVariableLocation, ref directVariableSize, ref array, ref flag);
				this._vds.Address = this.LMItemFactory.CreateDirectVariable(directVariableLocation, directVariableSize, array);
			}
			else
			{
				if (token.Type != 8)
				{
					this.AddErrorST(this._vds, 30, new object[]
					{
						this.Scanner.GetTokenText(token)
					});
					this.ParseReSyncIF();
					return true;
				}
				DirectVariableLocation directVariableLocation2;
				this.Scanner.GetIncompleteDirectVariable(token, ref directVariableLocation2);
				this._vds.Address = this.LMItemFactory.CreateIncompleteDirectVariable(directVariableLocation2);
			}
			this.MatchOperator(this._vds, new Operator[]
			{
				163
			});
			return false;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x000192FC File Offset: 0x000174FC
		private bool ParseNamesReturnError(ref Operator opNext)
		{
			for (;;)
			{
				bool flag = false;
				_IExpression iexpression = null;
				IToken token;
				if (this.OpCurrentVariableList == 107 || this.OpCurrentVariableList == 108 || this.AllowPaths)
				{
					iexpression = this.ExpressionParser.ParseSTOperand(out flag);
				}
				else if (this.Next(out token) != 13)
				{
					this.AddErrorSTWithToken(this._vds, token, 26, new object[]
					{
						this.Scanner.GetTokenText(token)
					});
					flag = true;
				}
				else
				{
					iexpression = this.LMItemFactory.CreateVariableExpression(this.Scanner.GetIdentifier(token), token);
				}
				if (flag)
				{
					return false;
				}
				this._vds.AddName(iexpression);
				_IExprement vds = this._vds;
				Operator[] array = new Operator[3];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.A205CACBA426A69DF9426A237C8AAFC4187ADC4C430192D07A8ADADCEEAC1CA1).FieldHandle);
				opNext = this.MatchOperator(vds, array);
				if (opNext == 0)
				{
					break;
				}
				if (opNext != 171)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x000193C8 File Offset: 0x000175C8
		private void ParseInitialisationAssignments(_IVariableDeclarationStatement vds)
		{
			IToken token;
			if (this.Next(out token) != 15 || this.Scanner.GetOperator(token) != 168)
			{
				this.Scanner.SetPosition(token);
				IToken token2;
				bool bErrorLocal;
				Operator nextOperator;
				do
				{
					string empty = string.Empty;
					token2 = this.ParseInputName(ref empty);
					_IExpression iexpression = this.ParseInitialisation();
					bErrorLocal = this.CheckForInvalidInitialisation(iexpression);
					_IVariableExpression ivariableExpression = null;
					if (empty != string.Empty)
					{
						ivariableExpression = this.LMItemFactory.CreateVariableExpression(empty, token2);
					}
					vds.AddParam(iexpression, ivariableExpression);
					nextOperator = this.GetNextOperator(bErrorLocal, ref token2);
				}
				while (this.CheckForContinuation(vds, nextOperator, bErrorLocal, token2));
			}
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00019464 File Offset: 0x00017664
		private bool CheckForContinuation(_IVariableDeclarationStatement vds, Operator opTest, bool bErrorLocal, IToken tokenTest)
		{
			if (opTest == 171)
			{
				IToken token;
				if (this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 168)
				{
					return false;
				}
				this.Scanner.SetPosition(token);
				return true;
			}
			else
			{
				if (opTest == 168)
				{
					return false;
				}
				if (!bErrorLocal)
				{
					this.AddErrorSTWithToken(vds, tokenTest, 2, new object[]
					{
						this.Scanner.GetOperatorText(171),
						this.Scanner.GetOperatorText(168),
						this.Scanner.GetTokenText(tokenTest)
					});
					opTest = this.Scanner.ParseReSyncST(new Operator[]
					{
						171,
						168
					});
					if (opTest == 171)
					{
						return true;
					}
					if (opTest == 168)
					{
						return false;
					}
				}
				this.Scanner.SetPosition(tokenTest);
				return false;
			}
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x00019540 File Offset: 0x00017740
		private Operator GetNextOperator(bool bErrorLocal, ref IToken tokenTest)
		{
			Operator result = 0;
			if (bErrorLocal)
			{
				result = this.Scanner.ParseReSyncST(new Operator[]
				{
					171,
					168
				});
			}
			else if (this.Next(out tokenTest) == 15)
			{
				result = this.Scanner.GetOperator(tokenTest);
			}
			return result;
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00019590 File Offset: 0x00017790
		private bool CheckForInvalidInitialisation(_IExpression expParam)
		{
			bool result = false;
			if (expParam is IStructureInitialization)
			{
				this.AddErrorST(expParam, 303, Array.Empty<object>());
				result = true;
			}
			else if (expParam is IArrayInitialization)
			{
				this.AddErrorST(expParam, 304, Array.Empty<object>());
				result = true;
			}
			return result;
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x000195D8 File Offset: 0x000177D8
		private IToken ParseInputName(ref string stIdent)
		{
			IToken token;
			if (this.Next(out token) == 13)
			{
				IToken token2;
				if (this.Next(out token2) == 15)
				{
					if (this.Scanner.GetOperator(token2) == 164)
					{
						stIdent = this.Scanner.GetIdentifier(token);
					}
					else
					{
						this.Scanner.SetPosition(token);
					}
				}
				else
				{
					this.Scanner.SetPosition(token);
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return token;
		}

		// Token: 0x040000D8 RID: 216
		private readonly _IVariableDeclarationStatement _vds;
	}
}
