using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200005B RID: 91
	internal readonly struct TypeDeclarationParser
	{
		// Token: 0x060005A9 RID: 1449 RVA: 0x00017DD4 File Offset: 0x00015FD4
		private TypeDeclarationParser(ParserContext context, IToken token)
		{
			this.Context = context;
			this._tds = this.Context.LMItemFactory.CreateTypeDeclarationStatement(token);
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x00017DF4 File Offset: 0x00015FF4
		internal static _ITypeDeclarationStatement ParseTypeDeclaration(ParserContext context, IToken token)
		{
			TypeDeclarationParser typeDeclarationParser = new TypeDeclarationParser(context, token);
			return typeDeclarationParser.ParseTypeDeclaration();
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x060005AB RID: 1451 RVA: 0x00017E11 File Offset: 0x00016011
		private ParserContext Context { get; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x00017E19 File Offset: 0x00016019
		private DeclarationParser DeclarationParser
		{
			get
			{
				return this.Context.DeclarationParser;
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x00017E26 File Offset: 0x00016026
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x00017E33 File Offset: 0x00016033
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x00017E40 File Offset: 0x00016040
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x00017E4D File Offset: 0x0001604D
		private Operator MatchOperator(_IExprement exp, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, exp, ops);
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x060005B1 RID: 1457 RVA: 0x00017E62 File Offset: 0x00016062
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00017E6F File Offset: 0x0001606F
		private void Next(out IToken token)
		{
			this.Scanner.Next(out token);
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x00017E7E File Offset: 0x0001607E
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00017E8B File Offset: 0x0001608B
		private _IExpression ParseInitialisation()
		{
			return this.ExpressionParser.ParseInitialisation();
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00017E98 File Offset: 0x00016098
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00017EA8 File Offset: 0x000160A8
		private _ITypeDeclarationStatement ParseTypeDeclaration()
		{
			IToken token;
			this.Next(out token);
			SignatureFlag signatureFlag = this.ReadAccessSpecifiers(token);
			this.Next(out token);
			if (token.Type != 13)
			{
				this.AddErrorST(this._tds, 26, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				return this._tds;
			}
			string identifier = this.Scanner.GetIdentifier(token);
			this._tds.Name = identifier;
			_IVariableExpression nameExpression = this.LMItemFactory.CreateVariableExpression(identifier, token);
			this._tds.NameExpression = nameExpression;
			Operator @operator = this.MatchOperator(this._tds, new Operator[]
			{
				163,
				116
			});
			@operator = this.ReadBaseType(@operator);
			if (@operator != 163)
			{
				return this._tds;
			}
			this.Next(out token);
			bool flag = true;
			if (token.Type == 15)
			{
				if (this.ReadKindOfDeclarationReturnDone(token, signatureFlag, identifier, ref flag))
				{
					return this._tds;
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			if (flag)
			{
				this._tds.Flags = 4L;
				this._tds.Type = this.TypeParser.ParseType();
			}
			this._tds.Flags = (this._tds.Flags | signatureFlag);
			if (this.MatchOperator(this._tds, new Operator[]
			{
				164,
				172
			}) == 164)
			{
				_IExpression initial = this.ParseInitialisation();
				this._tds.Initial = initial;
				this.MatchOperator(this._tds, new Operator[]
				{
					172
				});
			}
			return this._tds;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00018044 File Offset: 0x00016244
		private bool ReadKindOfDeclarationReturnDone(IToken token, SignatureFlag sfAccess, string stName, ref bool bAlias)
		{
			Operator @operator = this.Scanner.GetOperator(token);
			if (@operator == 99)
			{
				this._tds.Flags = (1L | sfAccess);
				this._tds.Declarations = this.DeclarationParser.ParseVariableList(token);
				this.MatchOperator(this._tds, new Operator[]
				{
					78
				});
				return true;
			}
			if (@operator != 100)
			{
				if (@operator != 167)
				{
					this.Scanner.SetPosition(token);
				}
				else
				{
					this._tds.Flags = (2L | sfAccess);
					this._tds.Declarations = EnumListParser.ParseEnumList(this.Context, token, stName);
					bAlias = false;
				}
				return false;
			}
			this._tds.Flags = (16385L | sfAccess);
			this._tds.Declarations = this.DeclarationParser.ParseVariableList(token);
			this.MatchOperator(this._tds, new Operator[]
			{
				79
			});
			return true;
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x00018136 File Offset: 0x00016336
		private Operator ReadBaseType(Operator opTest)
		{
			if (opTest == 116)
			{
				this._tds.Extends = this.ExpressionParser.ParseQualifiedNameExpression(this._tds);
				opTest = this.MatchOperator(this._tds, new Operator[]
				{
					163
				});
			}
			return opTest;
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x00018178 File Offset: 0x00016378
		private SignatureFlag ReadAccessSpecifiers(IToken token)
		{
			SignatureFlag sfAccess = 0L;
			sfAccess = this.ReadPublicPrivatProtected(token, sfAccess);
			return this.ReadFinalAbstract(sfAccess);
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0001819C File Offset: 0x0001639C
		private SignatureFlag ReadFinalAbstract(SignatureFlag sfAccess)
		{
			IToken token;
			this.Next(out token);
			if (token.Type == 15)
			{
				Operator @operator = this.Scanner.GetOperator(token);
				if (@operator != 224)
				{
					if (@operator != 230)
					{
						this.Scanner.SetPosition(token);
					}
					else
					{
						sfAccess |= 8796093022208L;
					}
				}
				else
				{
					sfAccess |= 140737488355328L;
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return sfAccess;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00018214 File Offset: 0x00016414
		private SignatureFlag ReadPublicPrivatProtected(IToken token, SignatureFlag sfAccess)
		{
			if (token.Type == 15)
			{
				if (this.Scanner.GetOperator(token) == 229)
				{
					sfAccess = 4398046511104L;
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
			return sfAccess;
		}

		// Token: 0x040000D5 RID: 213
		private readonly _ITypeDeclarationStatement _tds;
	}
}
