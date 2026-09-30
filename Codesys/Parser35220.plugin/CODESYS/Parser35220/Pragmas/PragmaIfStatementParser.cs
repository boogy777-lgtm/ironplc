using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000037 RID: 55
	internal readonly struct PragmaIfStatementParser
	{
		// Token: 0x060003D4 RID: 980 RVA: 0x00011561 File Offset: 0x0000F761
		private PragmaIfStatementParser(ParserContext context, IToken tokenPragma)
		{
			this.Context = context;
			this._ifstatement = context.LMItemFactory.CreatePragmaIfStatement(null, tokenPragma);
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00011580 File Offset: 0x0000F780
		internal static _IPragmaIfStatement ParsePragmaIfStatement(ParserContext context, out bool bError, IToken tokenPragma)
		{
			PragmaIfStatementParser pragmaIfStatementParser = new PragmaIfStatementParser(context, tokenPragma);
			pragmaIfStatementParser.ParsePragmaIf(out bError, tokenPragma);
			return pragmaIfStatementParser._ifstatement;
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x000115A5 File Offset: 0x0000F7A5
		private ParserContext Context { get; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x000115AD File Offset: 0x0000F7AD
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060003D8 RID: 984 RVA: 0x000115BA File Offset: 0x0000F7BA
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x000115C7 File Offset: 0x0000F7C7
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060003DA RID: 986 RVA: 0x000115D4 File Offset: 0x0000F7D4
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060003DB RID: 987 RVA: 0x000115E1 File Offset: 0x0000F7E1
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x000115EE File Offset: 0x0000F7EE
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060003DD RID: 989 RVA: 0x000115FF File Offset: 0x0000F7FF
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0001160C File Offset: 0x0000F80C
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060003DF RID: 991 RVA: 0x0001161C File Offset: 0x0000F81C
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0001162C File Offset: 0x0000F82C
		private void ParsePragmaIf(out bool bError, IToken tokenPragma)
		{
			bError = false;
			bool flag;
			_IExpression iexpression = this.PragmaParser.ParsePragmaORExp(out flag, tokenPragma) ?? this.LMItemFactory.CreateErrorExpression(tokenPragma);
			this._ifstatement.Condition = iexpression;
			bool bElseFound = false;
			PragmaOperator opLastFound = 36;
			_IPragmaElseIf elseifCurrent = null;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 6 && (iexpression.MessagesList == null || iexpression.MessagesList.Count == 0))
			{
				this.AddErrorST(this._ifstatement, 9, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			IToken token = this.TokenFactory.CreateEmptyToken();
			for (;;)
			{
				_ISequenceStatement seq;
				PragmaOperator pragmaOperator;
				bError = this.ParseSequenceAndReturnError(bElseFound, out seq, ref token, out pragmaOperator);
				if (bError)
				{
					break;
				}
				if (token.Type == 21)
				{
					goto Block_5;
				}
				this.SetSequenceStatement(opLastFound, seq, elseifCurrent);
				if (pragmaOperator == 39)
				{
					return;
				}
				bElseFound = this.CheckForElseOrElseif(tokenPragma, pragmaOperator, bElseFound, ref elseifCurrent);
				opLastFound = pragmaOperator;
			}
			return;
			Block_5:
			this.AddErrorST(this._ifstatement, 8, new object[]
			{
				this.Scanner.GetOperatorText(69),
				this.Scanner.GetOperatorText(68),
				this.Scanner.GetOperatorText(75)
			});
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0001174C File Offset: 0x0000F94C
		private bool ParseSequenceAndReturnError(bool bElseFound, out _ISequenceStatement seq, ref IToken token, out PragmaOperator op)
		{
			seq = this.LMItemFactory.CreateSequenceStatement(token);
			IToken position;
			Operator @operator;
			for (;;)
			{
				this.Next(out token, true, true);
				if (this.CheckForPragmaReturnError(bElseFound, token, out op))
				{
					return false;
				}
				this.Scanner.SetPosition(token);
				if (token.Type == 21)
				{
					return false;
				}
				bool flag;
				_IStatement istatement = this.StatementParser.ParseSTStatement(out flag, false);
				seq.Add(istatement);
				if (flag)
				{
					@operator = this.StatementParser.ParseReSyncST(out position, Array.Empty<Operator>());
					if (@operator != 172)
					{
						break;
					}
				}
			}
			op = PragmaIfStatementParser.MapPragmaOperator(@operator);
			if (!PragmaIfStatementParser.CheckForFittingOperator(bElseFound, op))
			{
				this.Scanner.SetPosition(position);
				return true;
			}
			return false;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x000117EE File Offset: 0x0000F9EE
		private static bool CheckForFittingOperator(bool bElseFound, PragmaOperator op)
		{
			return (!bElseFound && (op == 39 || op == 37 || op == 38)) || (bElseFound && op == 39);
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x00011810 File Offset: 0x0000FA10
		private static PragmaOperator MapPragmaOperator(Operator opHelp)
		{
			PragmaOperator result;
			if (opHelp != 68)
			{
				if (opHelp != 69)
				{
					if (opHelp == 75)
					{
						result = 39;
					}
					else
					{
						result = 0;
					}
				}
				else
				{
					result = 37;
				}
			}
			else
			{
				result = 38;
			}
			return result;
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x00011840 File Offset: 0x0000FA40
		private bool CheckForPragmaReturnError(bool bElseFound, IToken token, out PragmaOperator op)
		{
			op = 0;
			if (token.Type == 4)
			{
				this.PragmaScanner.Reset(this.Scanner.GetPragma(token), token);
				IPragmaToken pragmaToken;
				this.PragmaScanner.GetNext(ref pragmaToken);
				op = pragmaToken.Operator;
				if (!bElseFound && (op == 39 || op == 37 || op == 38))
				{
					return true;
				}
				if (bElseFound && op == 39)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x000118AC File Offset: 0x0000FAAC
		private bool CheckForElseOrElseif(IToken tokenPragma, PragmaOperator op, bool bElseFound, ref _IPragmaElseIf elseifCurrent)
		{
			if (op != 37)
			{
				if (op == 38)
				{
					bElseFound = true;
				}
			}
			else
			{
				elseifCurrent = this.LMItemFactory.CreatePragmaElseIf();
				bool flag;
				_IExpression condition = this.PragmaParser.ParsePragmaORExp(out flag, tokenPragma) ?? this.LMItemFactory.CreateErrorExpression(this.Scanner.CurrentToken);
				IPragmaToken pragmaToken;
				if (this.PragmaScanner.GetNext(ref pragmaToken) != 6)
				{
					this.AddErrorST(this._ifstatement, 9, new object[]
					{
						"TODO"
					});
				}
				elseifCurrent.Condition = condition;
			}
			return bElseFound;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00011938 File Offset: 0x0000FB38
		private void SetSequenceStatement(PragmaOperator opLastFound, _ISequenceStatement seq, _IPragmaElseIf elseifCurrent)
		{
			switch (opLastFound)
			{
			case 36:
				this._ifstatement.IfThen = seq;
				return;
			case 37:
				if (elseifCurrent != null)
				{
					elseifCurrent.Controlled = seq;
					this._ifstatement.AddElseIf(elseifCurrent);
					return;
				}
				break;
			case 38:
				this._ifstatement.IfElse = seq;
				break;
			default:
				return;
			}
		}

		// Token: 0x040000A3 RID: 163
		private readonly _IPragmaIfStatement _ifstatement;
	}
}
