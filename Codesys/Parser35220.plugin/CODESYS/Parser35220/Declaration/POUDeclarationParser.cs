using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Scanner;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000050 RID: 80
	public class POUDeclarationParser
	{
		// Token: 0x06000544 RID: 1348 RVA: 0x00016738 File Offset: 0x00014938
		private POUDeclarationParser(ParserContext context, IToken token, Operator op)
		{
			this.Context = context;
			if (op == 118)
			{
				this._pds = this.Context.LMItemFactory.CreateMethodDeclarationStatement(token);
				return;
			}
			this._pds = this.Context.LMItemFactory.CreatePOUDeclarationStatement(token);
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00016786 File Offset: 0x00014986
		internal static _IPOUDeclarationStatement ParsePOUDeclaration(ParserContext context, IToken token, Operator op)
		{
			return new POUDeclarationParser(context, token, op).ParsePOUDeclaration(op);
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x00016796 File Offset: 0x00014996
		private ParserContext Context { get; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000547 RID: 1351 RVA: 0x0001679E File Offset: 0x0001499E
		private DeclarationParser DeclarationParser
		{
			get
			{
				return this.Context.DeclarationParser;
			}
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000548 RID: 1352 RVA: 0x000167AB File Offset: 0x000149AB
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000549 RID: 1353 RVA: 0x000167B8 File Offset: 0x000149B8
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x000167C5 File Offset: 0x000149C5
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x0600054B RID: 1355 RVA: 0x000167D2 File Offset: 0x000149D2
		private InternalScanner InternalScanner
		{
			get
			{
				return (InternalScanner)this.Context.Scanner;
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x000167E4 File Offset: 0x000149E4
		private void MatchOperator(_IExprement exp, params Operator[] ops)
		{
			this.Scanner.MatchOperator(this.ErrorHandler, exp, ops);
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x0600054D RID: 1357 RVA: 0x000167FA File Offset: 0x000149FA
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x00016807 File Offset: 0x00014A07
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x00016815 File Offset: 0x00014A15
		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000550 RID: 1360 RVA: 0x00016825 File Offset: 0x00014A25
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000551 RID: 1361 RVA: 0x00016832 File Offset: 0x00014A32
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000552 RID: 1362 RVA: 0x0001683F File Offset: 0x00014A3F
		// (set) Token: 0x06000553 RID: 1363 RVA: 0x0001684C File Offset: 0x00014A4C
		private bool InsideDeclarationVarDecl
		{
			get
			{
				return this.StatementParser.InsideDeclarationVarDecl;
			}
			set
			{
				this.StatementParser.InsideDeclarationVarDecl = value;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000554 RID: 1364 RVA: 0x0001685A File Offset: 0x00014A5A
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x00016867 File Offset: 0x00014A67
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x00016878 File Offset: 0x00014A78
		private _IPOUDeclarationStatement ParsePOUDeclaration(Operator opParam)
		{
			bool insideDeclarationVarDecl = this.InsideDeclarationVarDecl;
			this.InsideDeclarationVarDecl = false;
			this.ParsePOUDeclarationInternal(opParam);
			this.InsideDeclarationVarDecl = insideDeclarationVarDecl;
			return this._pds;
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x000168A8 File Offset: 0x00014AA8
		private void ParsePOUDeclarationInternal(Operator opParam)
		{
			this._pds.Class = opParam;
			IToken token;
			this.ScanAccessSpecifier(out token);
			if (!this.CheckForName(token))
			{
				return;
			}
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(this.TokenFactory.Empty());
			string identifier = this.Scanner.GetIdentifier(token);
			this._pds.Name = identifier;
			_IVariableExpression nameExpression = this.LMItemFactory.CreateVariableExpression(identifier, token);
			this._pds.NameExpression = nameExpression;
			Operator @operator = this.TryParseGenericVarList(isequenceStatement, out token);
			if (@operator != null)
			{
				if (@operator != 116)
				{
					if (@operator != 117)
					{
						if (@operator == 163)
						{
							this._pds.Type = this.TypeParser.ParseType();
						}
						else
						{
							this.Scanner.SetPosition(token);
						}
					}
					else
					{
						this.ParseImplementsList();
					}
				}
				else
				{
					this.ParseExtendsList();
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			for (;;)
			{
				_IStatement istatement = this.StatementParser.TryParseVariableDeclarationList();
				if (istatement == null)
				{
					break;
				}
				isequenceStatement.Add(istatement);
			}
			this._pds.Declarations = isequenceStatement;
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x000169AC File Offset: 0x00014BAC
		private void ParseImplementsList()
		{
			_IExpression iexpression = this.ExpressionParser.ParseQualifiedNameExpression(this._pds);
			if (iexpression != null)
			{
				this._pds.AddInterfaceImplementation(iexpression);
			}
			IToken token;
			if (this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 171)
			{
				this.ParseImplementsList();
				return;
			}
			this.Scanner.SetPosition(token);
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x00016A0C File Offset: 0x00014C0C
		private void ParseExtendsList()
		{
			_IType itype = this.TypeParser.ParseUserdefType();
			_IExpression iexpression = null;
			IGenericUserdefType genericUserdefType = itype as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				iexpression = this.LMItemFactory.CreateTypeExpression(itype);
				iexpression._Position = ((_IExpression)genericUserdefType.NameExpression)._Position;
			}
			else
			{
				_IUserdefType iuserdefType = itype as _IUserdefType;
				if (iuserdefType != null)
				{
					iexpression = (iuserdefType.NameExpression as _IExpression);
				}
			}
			if (iexpression != null)
			{
				this._pds.Extends.Add(iexpression);
			}
			IToken token;
			if (this.Next(out token) != 15)
			{
				this.Scanner.SetPosition(token);
				return;
			}
			if (this.Scanner.GetOperator(token) == 117)
			{
				this.ParseImplementsList();
				return;
			}
			if (this.Scanner.GetOperator(token) == 171)
			{
				this.ParseExtendsList();
				return;
			}
			this.Scanner.SetPosition(token);
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00016ADC File Offset: 0x00014CDC
		private Operator TryParseGenericVarList(_ISequenceStatement seq, out IToken token)
		{
			Operator @operator = 0;
			this.Next(out token, true, true);
			this.Scanner.SetPosition(token);
			IToken token2;
			if (this.Next(out token2, false, false) == 15)
			{
				@operator = this.Scanner.GetOperator(token2);
			}
			if (@operator == 280)
			{
				_IStatement istatement = this.DeclarationParser.ParseVariableList(token2);
				this.MatchOperator(istatement, new Operator[]
				{
					81
				});
				@operator = 0;
				if (this.Next(out token) == 15)
				{
					@operator = this.Scanner.GetOperator(token);
				}
				if (this.Context._bReportSP18Feature)
				{
					this.Context.AddUnsupportedFeatureError(istatement, Strings.CompilerFeature_GenericConstantVariable, ParserContext.CompilerVersion18);
				}
				seq.Add(istatement);
			}
			return @operator;
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x00016B8A File Offset: 0x00014D8A
		private bool CheckForName(IToken token)
		{
			if (token.Type != 13)
			{
				this.AddErrorST(this._pds, 26, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				return false;
			}
			return true;
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x00016BBC File Offset: 0x00014DBC
		private void ScanAccessSpecifier(out IToken token)
		{
			this.RecognizeContextualMethodDeclarationOperators(true);
			this.Next(out token);
			token = this.ScanAccessSpecifier2(token);
			this.RecognizeContextualMethodDeclarationOperators(false);
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x00016BDE File Offset: 0x00014DDE
		private void RecognizeContextualMethodDeclarationOperators(bool bDo)
		{
			if (this._pds is _IMethodDeclarationStatement)
			{
				this.InternalScanner.RecognizeContextualOperator(bDo, 289);
				this.InternalScanner.RecognizeContextualOperator(bDo, 225);
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x00016C10 File Offset: 0x00014E10
		private IToken ScanAccessSpecifier2(IToken token)
		{
			if (token.Type == 15)
			{
				Operator @operator = this.Scanner.GetOperator(token);
				switch (@operator)
				{
				case 224:
					this._pds.SetAccessFlag(140737488355328L);
					goto IL_D5;
				case 226:
					goto IL_D5;
				case 227:
					this._pds.SetAccessFlag(1099511627776L);
					goto IL_D5;
				case 228:
					this._pds.SetAccessFlag(2199023255552L);
					goto IL_D5;
				case 229:
					this._pds.SetAccessFlag(4398046511104L);
					goto IL_D5;
				case 230:
					this._pds.SetAccessFlag(8796093022208L);
					goto IL_D5;
				}
				if (!this.SetMethodFlags(@operator))
				{
					this.Scanner.SetPosition(token);
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			IL_D5:
			this.Next(out token);
			if (token.Type == 15)
			{
				Operator operator2 = this.Scanner.GetOperator(token);
				if (operator2 != 224)
				{
					if (operator2 != 230)
					{
						if (!this.SetMethodFlags(operator2))
						{
							this.Scanner.SetPosition(token);
						}
					}
					else
					{
						this._pds.SetAccessFlag(8796093022208L);
					}
				}
				else
				{
					this._pds.SetAccessFlag(140737488355328L);
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			this.Next(out token);
			return token;
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x00016D80 File Offset: 0x00014F80
		private bool SetMethodFlags(Operator op)
		{
			_IMethodDeclarationStatement imethodDeclarationStatement = this._pds as _IMethodDeclarationStatement;
			if (imethodDeclarationStatement != null)
			{
				if (op == 225)
				{
					imethodDeclarationStatement.Override = true;
					return true;
				}
				if (op == 289)
				{
					imethodDeclarationStatement.Overload = true;
					return true;
				}
			}
			return false;
		}

		// Token: 0x040000C5 RID: 197
		private readonly _IPOUDeclarationStatement _pds;
	}
}
