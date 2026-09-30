using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200004A RID: 74
	internal readonly struct StructureInitializationParser
	{
		// Token: 0x060004F9 RID: 1273 RVA: 0x00015AF1 File Offset: 0x00013CF1
		private StructureInitializationParser(ParserContext context, InitializationParser initializationParser)
		{
			this.Context = context;
			this.InitializationParser = initializationParser;
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060004FA RID: 1274 RVA: 0x00015B01 File Offset: 0x00013D01
		private ParserContext Context { get; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060004FB RID: 1275 RVA: 0x00015B09 File Offset: 0x00013D09
		private InitializationParser InitializationParser { get; }

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x060004FC RID: 1276 RVA: 0x00015B11 File Offset: 0x00013D11
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x060004FD RID: 1277 RVA: 0x00015B1E File Offset: 0x00013D1E
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x00015B2B File Offset: 0x00013D2B
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x00015B39 File Offset: 0x00013D39
		private void ParseReSyncST(params Operator[] ops)
		{
			this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x00015B48 File Offset: 0x00013D48
		private void ParseReSyncIF()
		{
			this.Scanner.ParseReSyncIF();
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x00015B55 File Offset: 0x00013D55
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x00015B62 File Offset: 0x00013D62
		private Operator MatchOperator(params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, true, ops);
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00015B78 File Offset: 0x00013D78
		private void MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			this.Scanner.MatchOperator(this.ErrorHandler, null, bGenerateError, ops);
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00015B90 File Offset: 0x00013D90
		internal static _IExpression ParseStructInitialisation(ParserContext context, InitializationParser initializationParser, ref bool bBreakInit, bool bInterface)
		{
			StructureInitializationParser structureInitializationParser = new StructureInitializationParser(context, initializationParser);
			return structureInitializationParser.ParseStructInitialisation(ref bBreakInit, bInterface);
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00015BB0 File Offset: 0x00013DB0
		private _IExpression ParseStructInitialisation(ref bool bBreakInit, bool bInterface)
		{
			IToken token;
			this.Next(out token);
			this.Scanner.SetPosition(token);
			IToken token2;
			_IExpression result;
			if (this.CheckForEmptyOrNoStructureInitialization(bInterface, token, out token2, out result))
			{
				return result;
			}
			_IStructureInitialization istructureInitialization = this.LMItemFactory.CreateStructureInitialisation(token2);
			IToken token3;
			while (this.AssertIdentifier(ref bBreakInit, bInterface, istructureInitialization, out token3))
			{
				_IVariableExpression ivariableExpression = this.LMItemFactory.CreateVariableExpression(this.Scanner.GetIdentifier(token3), token3);
				IToken token4;
				if (!this.AssertAssignment(ref bBreakInit, bInterface, istructureInitialization, out token4))
				{
					return istructureInitialization;
				}
				_IExpression rvalue = this.InitializationParser.ParseInitialisation(ref bBreakInit);
				if (bBreakInit)
				{
					return istructureInitialization;
				}
				_IAssignmentExpression iassignmentExpression = this.LMItemFactory.CreateAssignmentExpression(ivariableExpression, token4);
				iassignmentExpression._RValue = rvalue;
				istructureInitialization.AddInitValue(iassignmentExpression);
				if (this.MatchOperator(new Operator[]
				{
					171,
					168
				}) != 171)
				{
					return istructureInitialization;
				}
			}
			return istructureInitialization;
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x00015C88 File Offset: 0x00013E88
		private bool AssertAssignment(ref bool bBreakInit, bool bInterface, _IStructureInitialization structinit, out IToken tokenAssignment)
		{
			if (this.Next(out tokenAssignment) != 15 || this.Scanner.GetOperator(tokenAssignment) != 164)
			{
				if (bInterface)
				{
					this.ErrorHandler.AddError(tokenAssignment, 6, new object[]
					{
						164,
						this.Scanner.GetTokenText(tokenAssignment)
					});
				}
				else
				{
					this.ErrorHandler.AddErrorST(structinit, 6, new object[]
					{
						164,
						this.Scanner.GetTokenText(tokenAssignment)
					});
				}
				if (bInterface)
				{
					this.ParseReSyncIF();
				}
				else
				{
					this.ParseReSyncST(Array.Empty<Operator>());
				}
				bBreakInit = true;
				return false;
			}
			return true;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00015D40 File Offset: 0x00013F40
		private bool AssertIdentifier(ref bool bBreakInit, bool bInterface, _IStructureInitialization structinit, out IToken tokenIdentifier)
		{
			if (this.Next(out tokenIdentifier) != 13)
			{
				if (bInterface)
				{
					this.ErrorHandler.AddError(tokenIdentifier, 26, new object[]
					{
						this.Scanner.GetTokenText(tokenIdentifier)
					});
				}
				else
				{
					this.ErrorHandler.AddErrorST(structinit, 26, new object[]
					{
						this.Scanner.GetTokenText(tokenIdentifier)
					});
				}
				if (bInterface)
				{
					this.ParseReSyncIF();
				}
				else
				{
					this.ParseReSyncST(Array.Empty<Operator>());
				}
				bBreakInit = true;
				return false;
			}
			return true;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00015DC8 File Offset: 0x00013FC8
		private bool CheckForEmptyOrNoStructureInitialization(bool bInterface, IToken tokenSave, out IToken tokenPosition, out _IExpression structureInitialisation)
		{
			structureInitialisation = null;
			if (!this.NextTokenIsStructureOperator(out tokenPosition))
			{
				this.Scanner.SetPosition(tokenSave);
				if (!bInterface)
				{
					structureInitialisation = null;
					return true;
				}
				IToken position;
				if (!this.LookaheadForStructureInitialization(out tokenPosition, out position))
				{
					this.Scanner.SetPosition(tokenSave);
					if (this.LookaheadForEmptyInitialization(out tokenPosition))
					{
						structureInitialisation = this.LMItemFactory.CreateStructureInitialisation(tokenPosition);
						return true;
					}
					this.Scanner.SetPosition(tokenSave);
					structureInitialisation = null;
					return true;
				}
				else
				{
					this.Scanner.SetPosition(position);
				}
			}
			else
			{
				this.MatchOperator(true, new Operator[]
				{
					167
				});
				IToken position2;
				if (this.LookaheadForEmptyStructureInitialization(out position2))
				{
					structureInitialisation = this.LMItemFactory.CreateStructureInitialisation(tokenPosition);
					return true;
				}
				this.Scanner.SetPosition(position2);
			}
			return false;
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00015E87 File Offset: 0x00014087
		private bool LookaheadForEmptyStructureInitialization(out IToken tokenHelp)
		{
			return this.Next(out tokenHelp) == 15 && this.Scanner.GetOperator(tokenHelp) == 168;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x00015EAC File Offset: 0x000140AC
		private bool LookaheadForEmptyInitialization(out IToken tokenPosition)
		{
			IToken token;
			return this.Next(out tokenPosition) == 15 && this.Scanner.GetOperator(tokenPosition) == 167 && this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 168;
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00015EFC File Offset: 0x000140FC
		private bool LookaheadForStructureInitialization(out IToken tokenPosition, out IToken tokenIdent)
		{
			tokenPosition = null;
			tokenIdent = null;
			IToken token;
			return this.Next(out tokenPosition) == 15 && this.Scanner.GetOperator(tokenPosition) == 167 && this.Next(out tokenIdent) == 13 && this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 164;
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x00015F5A File Offset: 0x0001415A
		private bool NextTokenIsStructureOperator(out IToken tokenPosition)
		{
			return this.Next(out tokenPosition) == 15 && this.Scanner.GetOperator(tokenPosition) == 99;
		}
	}
}
