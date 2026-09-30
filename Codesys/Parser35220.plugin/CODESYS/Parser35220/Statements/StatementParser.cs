using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Pragmas;
using CODESYS.Parser35220.Scanner;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x0200002D RID: 45
	public class StatementParser
	{
		// Token: 0x06000313 RID: 787 RVA: 0x0000E753 File Offset: 0x0000C953
		internal StatementParser(ParserContext context)
		{
			this.Context = context;
			this.ContextualOperatorHandler = new ContextualOperatorHandler(context);
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000314 RID: 788 RVA: 0x0000E792 File Offset: 0x0000C992
		private ParserContext Context { get; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000315 RID: 789 RVA: 0x0000E79A File Offset: 0x0000C99A
		private PragmaStatementParser PragmaStatementParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000316 RID: 790 RVA: 0x0000E7A7 File Offset: 0x0000C9A7
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000317 RID: 791 RVA: 0x0000E7B4 File Offset: 0x0000C9B4
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0000E7C1 File Offset: 0x0000C9C1
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000E7CF File Offset: 0x0000C9CF
		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600031A RID: 794 RVA: 0x0000E7DF File Offset: 0x0000C9DF
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0000E7EC File Offset: 0x0000C9EC
		private void MatchOperator(_IExprement exp, params Operator[] ops)
		{
			this.Scanner.MatchOperator(this.ErrorHandler, exp, ops);
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000E802 File Offset: 0x0000CA02
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000E812 File Offset: 0x0000CA12
		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x0600031E RID: 798 RVA: 0x0000E824 File Offset: 0x0000CA24
		// (set) Token: 0x0600031F RID: 799 RVA: 0x0000E82C File Offset: 0x0000CA2C
		internal bool Flow { get; set; } = true;

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000320 RID: 800 RVA: 0x0000E835 File Offset: 0x0000CA35
		// (set) Token: 0x06000321 RID: 801 RVA: 0x0000E83D File Offset: 0x0000CA3D
		internal bool Bp { get; set; } = true;

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000322 RID: 802 RVA: 0x0000E846 File Offset: 0x0000CA46
		// (set) Token: 0x06000323 RID: 803 RVA: 0x0000E84E File Offset: 0x0000CA4E
		internal bool InCase { get; set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000324 RID: 804 RVA: 0x0000E857 File Offset: 0x0000CA57
		// (set) Token: 0x06000325 RID: 805 RVA: 0x0000E85F File Offset: 0x0000CA5F
		internal bool InDeclaration { get; set; }

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000326 RID: 806 RVA: 0x0000E868 File Offset: 0x0000CA68
		// (set) Token: 0x06000327 RID: 807 RVA: 0x0000E870 File Offset: 0x0000CA70
		internal bool Implicit { get; set; }

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000328 RID: 808 RVA: 0x0000E879 File Offset: 0x0000CA79
		// (set) Token: 0x06000329 RID: 809 RVA: 0x0000E881 File Offset: 0x0000CA81
		internal bool ImplicitAnyway { get; set; }

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x0600032A RID: 810 RVA: 0x0000E88A File Offset: 0x0000CA8A
		internal bool ParseInitializationExpression
		{
			get
			{
				return this.Implicit || this.ImplicitAnyway;
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000E89C File Offset: 0x0000CA9C
		// (set) Token: 0x0600032C RID: 812 RVA: 0x0000E8A4 File Offset: 0x0000CAA4
		internal bool InsideDeclarationVarDecl { get; set; }

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x0600032D RID: 813 RVA: 0x0000E8AD File Offset: 0x0000CAAD
		// (set) Token: 0x0600032E RID: 814 RVA: 0x0000E8B5 File Offset: 0x0000CAB5
		internal ContextualOperatorHandler ContextualOperatorHandler { get; private set; }

		// Token: 0x0600032F RID: 815 RVA: 0x0000E8C0 File Offset: 0x0000CAC0
		internal bool CheckOptionalOperator(Operator op)
		{
			IToken token;
			if (this.Scanner.Next(out token) != 15 || this.Scanner.GetOperator(token) != op)
			{
				this.Scanner.SetPosition(token);
				return false;
			}
			return true;
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000E8FC File Offset: 0x0000CAFC
		internal void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			exprError = null;
			IToken token;
			if (this.Scanner.Next(out token) != 15 || this.Scanner.GetOperator(token) != op)
			{
				if (!bErrorLocal)
				{
					exprError = this.LMItemFactory.CreateErrorExpression(token);
					this.AddErrorSTWithToken(exprement, token, 6, new object[]
					{
						this.Scanner.GetOperatorText(op),
						this.Scanner.GetTokenText(token)
					});
					this.Scanner.SetPosition(token);
				}
				if (this.ParseReSyncST(new Operator[]
				{
					op
				}) != op)
				{
					if (bErrorLocal)
					{
						this.AddErrorSTWithToken(exprement, token, 6, new object[]
						{
							this.Scanner.GetOperatorText(op),
							this.Scanner.GetTokenText(token)
						});
					}
					this.Scanner.SetPosition(token);
				}
			}
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0000E9CA File Offset: 0x0000CBCA
		private Operator ParseReSyncST(params Operator[] ops)
		{
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0000E9D8 File Offset: 0x0000CBD8
		internal Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			this.Scanner.Next(out tokenPos);
			this.Scanner.SetPosition(tokenPos);
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0000EA00 File Offset: 0x0000CC00
		internal void SetStatementFlags(_IStatement statement)
		{
			statement.SetFlag(1L, this.Flow);
			statement.SetFlag(2L, this.Bp);
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0000EA20 File Offset: 0x0000CC20
		internal _IStatement ParseSTStatement(out bool bError, bool bTopLevel)
		{
			this._stackDepth++;
			IToken token;
			this.Scanner.GetNext(ref token);
			this.Scanner.SetPosition(token);
			int sourceOffset = this.Scanner.SourceOffset;
			_IStatement istatement = this.ParseSTStatementHelp(out bError, bTopLevel);
			int sourceOffset2 = this.Scanner.SourceOffset;
			if (istatement != null)
			{
				_IToken itoken = (_IToken)token;
				long num = ((_IToken)this.Scanner.CurrentToken).CharactersToSkipSeen - itoken.CharactersToSkipSeen;
				istatement.PositionLength = (short)((long)(sourceOffset2 - sourceOffset) - num);
			}
			this._stackDepth--;
			return istatement;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0000EABD File Offset: 0x0000CCBD
		internal void RecognizeContextualDeclarationOperator(bool bDo, Operator eWhichOperator)
		{
			((InternalScanner)this.Context.Scanner).RecognizeContextualOperator(bDo, eWhichOperator);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0000EAD8 File Offset: 0x0000CCD8
		private _IStatement ParseSTStatementHelp(out bool bError, bool bTopLevel)
		{
			bool bSemicolonNecessary = true;
			IToken token;
			TokenType tokenType = this.Next(out token, true, true);
			bError = false;
			_IStatement istatement;
			switch (tokenType)
			{
			case 2:
			{
				string comment = this.Scanner.GetComment(token);
				istatement = this.LMItemFactory.CreateCommentStatement(comment, token);
				bSemicolonNecessary = false;
				goto IL_16E;
			}
			case 3:
			{
				string docComment = this.Scanner.GetDocComment(token);
				_ICommentStatement icommentStatement = this.LMItemFactory.CreateCommentStatement(docComment, token);
				icommentStatement.DocComment = true;
				istatement = icommentStatement;
				bSemicolonNecessary = false;
				goto IL_16E;
			}
			case 4:
				istatement = this.ParseSTStatementHelpPragma(out bError, token);
				bSemicolonNecessary = false;
				goto IL_16E;
			case 5:
			case 6:
				break;
			case 7:
			{
				this.Scanner.SetPosition(token);
				_IExpression iexpression = this.ParseAssignExp(out bError);
				istatement = this.LMItemFactory.CreateExpressionStatement(iexpression, token);
				goto IL_16E;
			}
			default:
				switch (tokenType)
				{
				case 13:
					istatement = this.TryParseDeclarationOrLabel(ref bError, token, ref bSemicolonNecessary);
					goto IL_16E;
				case 14:
					istatement = this.TryParseCaseLabelStatement(token, ref bSemicolonNecessary);
					goto IL_16E;
				case 15:
					istatement = this.ParseOperatorStatement(ref bError, token, ref bSemicolonNecessary);
					goto IL_16E;
				default:
					if (tokenType == 21)
					{
						if (bTopLevel)
						{
							return null;
						}
						istatement = this.LMItemFactory.CreateErrorStatement(token);
						this.AddErrorSTWithToken(istatement, token, 9, new object[]
						{
							((_IScanner)this.Scanner).GetTokenText(token, 1)
						});
						goto IL_16E;
					}
					break;
				}
				break;
			}
			istatement = this.LMItemFactory.CreateErrorStatement(token);
			this.AddErrorSTWithToken(istatement, token, 9, new object[]
			{
				((_IScanner)this.Scanner).GetTokenText(token, 1)
			});
			IL_16E:
			this.ParseSemicolonIfNecessary(bSemicolonNecessary, istatement);
			if (istatement != null)
			{
				istatement.SetFlag(1L, this.Flow);
			}
			if (istatement != null)
			{
				istatement.SetFlag(2L, this.Bp);
			}
			if (istatement != null)
			{
				istatement.SetFlag(32L, this.ShouldCreateBpForCurrentStatement());
			}
			if (istatement != null && !(istatement is IPragmaStatement))
			{
				this.RestoreBp();
			}
			return istatement;
		}

		// Token: 0x06000337 RID: 823 RVA: 0x0000ECA4 File Offset: 0x0000CEA4
		internal _IStatement TryParseVariableDeclarationList()
		{
			_IStatement istatement = null;
			IToken token;
			TokenType tokenType = this.Next(out token, true, true);
			switch (tokenType)
			{
			case 2:
			{
				string comment = this.Scanner.GetComment(token);
				istatement = this.LMItemFactory.CreateCommentStatement(comment, token);
				break;
			}
			case 3:
			{
				string docComment = this.Scanner.GetDocComment(token);
				_ICommentStatement icommentStatement = this.LMItemFactory.CreateCommentStatement(docComment, token);
				icommentStatement.DocComment = true;
				istatement = icommentStatement;
				break;
			}
			case 4:
			{
				bool flag;
				istatement = this.ParseSTStatementHelpPragma(out flag, token);
				break;
			}
			default:
				if (tokenType == 15)
				{
					istatement = this._TryParseVariableDeclarationList(token);
				}
				break;
			}
			if (istatement == null)
			{
				this.Scanner.SetPosition(token);
			}
			else
			{
				istatement.SetFlag(1L, this.Flow);
				istatement.SetFlag(2L, this.Bp);
				istatement.SetFlag(32L, this.ShouldCreateBpForCurrentStatement());
				if (!(istatement is IPragmaStatement))
				{
					this.RestoreBp();
				}
			}
			return istatement;
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0000ED7C File Offset: 0x0000CF7C
		private _IStatement _TryParseVariableDeclarationList(IToken token)
		{
			_IStatement istatement = null;
			Operator @operator = this.Scanner.GetOperator(token);
			if (@operator <= 114)
			{
				if (@operator != 105 && @operator - 107 > 7)
				{
					return istatement;
				}
			}
			else
			{
				if (@operator == 172)
				{
					return this.LMItemFactory.CreateEmptyStatement(token);
				}
				if (@operator != 244)
				{
					return istatement;
				}
			}
			istatement = this.ParseVariableList(token);
			this.MatchOperator(istatement, new Operator[]
			{
				81
			});
			return istatement;
		}

		// Token: 0x06000339 RID: 825 RVA: 0x0000EDE8 File Offset: 0x0000CFE8
		private static bool IsStartEndOperatorMatch(Operator startOp, Operator endOp)
		{
			if (startOp <= 88)
			{
				if (startOp == 60)
				{
					return 70 == endOp;
				}
				if (startOp == 87)
				{
					return 73 == endOp;
				}
				if (startOp == 88)
				{
					return 74 == endOp;
				}
			}
			else if (startOp <= 118)
			{
				if (startOp == 93)
				{
					return 76 == endOp;
				}
				if (startOp == 118)
				{
					return 281 == endOp;
				}
			}
			else
			{
				if (startOp == 119)
				{
					return 283 == endOp;
				}
				switch (startOp)
				{
				case 284:
				case 285:
					return 282 == endOp;
				case 287:
					return 288 == endOp;
				case 290:
					return 291 == endOp;
				}
			}
			return false;
		}

		// Token: 0x0600033A RID: 826 RVA: 0x0000EE94 File Offset: 0x0000D094
		private bool CheckEndOperator(Operator endOp)
		{
			if (this.m_BeginOpStack.Count < 1)
			{
				return false;
			}
			if (StatementParser.IsStartEndOperatorMatch(this.m_BeginOpStack.Peek(), endOp))
			{
				this.m_BeginOpStack.Pop();
				return true;
			}
			return false;
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0000EEC8 File Offset: 0x0000D0C8
		private _IStatement ParseOperatorStatement(ref bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			Operator @operator = this.Scanner.GetOperator(token);
			_IStatement istatement;
			if (@operator <= 172)
			{
				if (@operator <= 141)
				{
					if (@operator <= 60)
					{
						if (@operator != 2)
						{
							if (@operator != 60)
							{
								goto IL_4AF;
							}
							goto IL_344;
						}
					}
					else
					{
						switch (@operator)
						{
						case 65:
							istatement = this.ParseCase(out bError, token);
							bSemicolonNecessary = false;
							return istatement;
						case 66:
						case 67:
						case 68:
						case 69:
						case 71:
						case 72:
						case 75:
						case 77:
						case 78:
						case 79:
						case 80:
						case 81:
						case 82:
						case 86:
						case 90:
						case 91:
						case 92:
						case 94:
						case 95:
						case 97:
						case 99:
						case 100:
						case 101:
						case 102:
						case 104:
						case 106:
						case 116:
						case 117:
							goto IL_4AF;
						case 70:
						case 73:
						case 74:
						case 76:
							goto IL_377;
						case 83:
							return this.LMItemFactory.CreateExitStatement(token);
						case 84:
							return this.LMItemFactory.CreateContinueStatement(token);
						case 85:
							istatement = this.ParseFor(out bError, token);
							bSemicolonNecessary = false;
							return istatement;
						case 87:
						case 88:
						case 93:
						case 118:
						case 119:
							goto IL_30C;
						case 89:
							istatement = this.ParseIf(out bError, token);
							bSemicolonNecessary = false;
							return istatement;
						case 96:
							istatement = this.ParseRepeat(out bError, token);
							bSemicolonNecessary = false;
							return istatement;
						case 98:
							return this.ParseReturn(out bError, token);
						case 103:
							if (this.InsideDeclarationVarDecl)
							{
								return this.CreateUnexpectedTokenErrorStatement(token);
							}
							istatement = this.ParseTypeDeclaration(token);
							this.MatchOperator(istatement, new Operator[]
							{
								80
							});
							bSemicolonNecessary = false;
							return istatement;
						case 105:
						case 107:
						case 108:
						case 109:
						case 110:
						case 111:
						case 112:
						case 113:
						case 114:
							goto IL_2EA;
						case 115:
							istatement = this.ParseWhile(out bError, token);
							bSemicolonNecessary = false;
							return istatement;
						case 120:
						case 121:
							break;
						default:
							if (@operator != 141)
							{
								goto IL_4AF;
							}
							return this.ParseConditionalCall(out bError, token);
						}
					}
				}
				else if (@operator <= 158)
				{
					if (@operator == 143)
					{
						return this.ParseJump(out bError, token);
					}
					if (@operator - 157 > 1)
					{
						goto IL_4AF;
					}
					goto IL_2AF;
				}
				else if (@operator != 162)
				{
					if (@operator != 172)
					{
						goto IL_4AF;
					}
					istatement = this.LMItemFactory.CreateEmptyStatement(token);
					bSemicolonNecessary = false;
					return istatement;
				}
				else
				{
					if (!this.InDeclaration)
					{
						return this.ParseExpressionStatement(out bError, token);
					}
					istatement = this.ParseVariableDeclaration(token);
					bSemicolonNecessary = false;
					return istatement;
				}
			}
			else if (@operator <= 238)
			{
				if (@operator <= 205)
				{
					switch (@operator)
					{
					case 192:
						goto IL_2AF;
					case 193:
					case 194:
					case 195:
					case 197:
					case 200:
						goto IL_4AF;
					case 196:
					case 198:
					case 199:
					case 201:
					case 202:
						break;
					default:
						if (@operator != 205)
						{
							goto IL_4AF;
						}
						return this.ParseWaitStatement(out bError, token);
					}
				}
				else if (@operator - 221 > 1)
				{
					if (@operator != 238)
					{
						goto IL_4AF;
					}
					istatement = this.ParseTryCatchStatement(out bError, token);
					bSemicolonNecessary = false;
					return istatement;
				}
			}
			else if (@operator <= 255)
			{
				switch (@operator)
				{
				case 242:
				case 246:
				case 247:
					break;
				case 243:
				case 245:
					goto IL_4AF;
				case 244:
					goto IL_2EA;
				default:
					switch (@operator)
					{
					case 251:
					case 255:
						return this.ParseCaseLabelOrExpressionStatement(out bError, token, ref bSemicolonNecessary);
					case 252:
					case 253:
						goto IL_4AF;
					case 254:
						break;
					default:
						goto IL_4AF;
					}
					break;
				}
			}
			else if (@operator != 270)
			{
				switch (@operator)
				{
				case 281:
				case 282:
				case 283:
				case 288:
				case 291:
					goto IL_377;
				case 284:
				case 285:
					goto IL_30C;
				case 286:
					istatement = this.ParseImplementationBlock(out bError, token);
					bSemicolonNecessary = false;
					return istatement;
				case 287:
				case 290:
					goto IL_344;
				case 289:
					goto IL_4AF;
				default:
					goto IL_4AF;
				}
			}
			return this.ParseExpressionStatement(out bError, token);
			IL_2AF:
			return this.TryParseCaseLabelStatement(token, ref bSemicolonNecessary);
			IL_2EA:
			istatement = this.ParseVariableList(token);
			this.MatchOperator(istatement, new Operator[]
			{
				81
			});
			bSemicolonNecessary = false;
			return istatement;
			IL_30C:
			if (this.InsideDeclarationVarDecl)
			{
				return this.CreateUnexpectedTokenErrorStatement(token);
			}
			this.m_BeginOpStack.Push(@operator);
			istatement = this.ParsePOUDeclaration(token, this.Scanner.GetOperator(token));
			bSemicolonNecessary = false;
			return istatement;
			IL_344:
			this.m_BeginOpStack.Push(@operator);
			this.ContextualOperatorHandler.TreatContextualDeclarationOperatorAsIdentifier();
			istatement = this.ParsePOUDeclaration(token, this.Scanner.GetOperator(token));
			bSemicolonNecessary = false;
			return istatement;
			IL_377:
			if (!this.CheckEndOperator(@operator))
			{
				istatement = this.LMItemFactory.CreateErrorStatement(token);
				this.AddErrorSTWithToken(istatement, token, 9, new object[]
				{
					((_IScanner)this.Scanner).GetTokenText(token, 1)
				});
				return istatement;
			}
			istatement = this.LMItemFactory.CreateEmptyStatement(token);
			bSemicolonNecessary = false;
			return istatement;
			IL_4AF:
			return this.CreateUnexpectedTokenErrorStatement(token);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0000F38D File Offset: 0x0000D58D
		private _IStatement ParseImplementationBlock(out bool bError, IToken token)
		{
			return ImplementationBlockParser.Parse(this.Context, out bError, token);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0000F39C File Offset: 0x0000D59C
		private _IStatement ParseExpressionStatement(out bool bError, IToken token)
		{
			this.Scanner.SetPosition(token);
			_IExpression iexpression = this.ParseAssignExp(out bError);
			return this.LMItemFactory.CreateExpressionStatement(iexpression, token);
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0000F3CC File Offset: 0x0000D5CC
		private _IStatement TryParseCaseLabelStatement(IToken token, ref bool bSemicolonNecessary)
		{
			if (this.InCase)
			{
				this.Scanner.SetPosition(token);
				_ICaseLabelStatement icaseLabelStatement = this.ParseCaseLabel(token);
				if (icaseLabelStatement != null)
				{
					_IStatement result = icaseLabelStatement;
					bSemicolonNecessary = false;
					return result;
				}
			}
			return this.CreateUnexpectedTokenErrorStatement(token);
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0000F404 File Offset: 0x0000D604
		private _IStatement CreateUnexpectedTokenErrorStatement(IToken token)
		{
			_IStatement istatement = this.LMItemFactory.CreateErrorStatement(token);
			this.AddErrorSTWithToken(istatement, token, 9, new object[]
			{
				((_IScanner)this.Scanner).GetTokenText(token, 1)
			});
			return istatement;
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0000F444 File Offset: 0x0000D644
		private _IStatement ParseCaseLabelOrExpressionStatement(out bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			this.Scanner.SetPosition(token);
			bError = false;
			_IStatement result;
			if (this.InCase)
			{
				_ICaseLabelStatement icaseLabelStatement = this.ParseCaseLabel(token);
				if (icaseLabelStatement != null)
				{
					result = icaseLabelStatement;
					bSemicolonNecessary = false;
					return result;
				}
				this.Scanner.SetPosition(token);
			}
			_IExpression iexpression = this.ParseAssignExp(out bError);
			if (this.InDeclaration)
			{
				result = this.ParseVariableDeclaration(token);
				bSemicolonNecessary = false;
			}
			else
			{
				result = this.LMItemFactory.CreateExpressionStatement(iexpression, token);
			}
			return result;
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0000F4B4 File Offset: 0x0000D6B4
		private _IStatement ParseWaitStatement(out bool bError, IToken token)
		{
			_IStatement result = null;
			this.Scanner.SetPosition(token);
			_IOperatorExpression ioperatorExpression = this.ParseAssignExp(out bError) as _IOperatorExpression;
			if (ioperatorExpression != null)
			{
				_IWhileStatement iwhileStatement = this.LMItemFactory.CreateWhileStatement();
				if (ioperatorExpression._OperandsList.Count == 1)
				{
					_IOperatorExpression ioperatorExpression2 = this.LMItemFactory.CreateOperatorExpression(133);
					ioperatorExpression2.AddOperand(ioperatorExpression[0]);
					iwhileStatement._Condition = ioperatorExpression2;
				}
				else
				{
					_IErrorExpression ierrorExpression = this.LMItemFactory.CreateErrorExpression(token);
					this.AddErrorST(ierrorExpression, 22, new object[]
					{
						this.Scanner.GetOperatorText(205),
						1
					});
					iwhileStatement._Condition = ierrorExpression;
				}
				_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement();
				_ICallExpression icallExpression = this.LMItemFactory.CreateCallExpression(this.LMItemFactory.CreateVariableExpression("SynchWait", token), token);
				isequenceStatement.Add(this.LMItemFactory.CreateExpressionStatement(icallExpression, token));
				iwhileStatement._Controlled = isequenceStatement;
				result = iwhileStatement;
			}
			return result;
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0000F5B4 File Offset: 0x0000D7B4
		private void ParseSemicolonIfNecessary(bool bSemicolonNecessary, _IStatement statement)
		{
			IToken token;
			if (bSemicolonNecessary && (this.Next(out token) != 15 || this.Scanner.GetOperator(token) != 172))
			{
				if (token.Type == 21)
				{
					this.AddErrorSTWithToken(statement, token, 190, new object[]
					{
						this.Scanner.GetTokenText(token)
					});
					return;
				}
				this.AddErrorSTWithToken(statement, token, 189, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				this.Scanner.SetPosition(token);
			}
		}

		// Token: 0x06000343 RID: 835 RVA: 0x0000F640 File Offset: 0x0000D840
		private _IStatement TryParseDeclarationOrLabel(ref bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			if (this.InDeclaration)
			{
				_IStatement istatement;
				if (this.Scanner.GetIdentifier(token) == "FUNCTIONBLOCK")
				{
					istatement = this.ParsePOUDeclaration(token, 88);
					this.AddErrorSTWithToken(istatement, token, 98, Array.Empty<object>());
				}
				else
				{
					istatement = this.ParseVariableDeclaration(token);
					bSemicolonNecessary = false;
				}
				return istatement;
			}
			IToken token2;
			if (!this.InCase && this.Next(out token2) == 15 && this.Scanner.GetOperator(token2) == 163)
			{
				string identifier = this.Scanner.GetIdentifier(token);
				_IStatement istatement = this.LMItemFactory.CreateLabelStatement(identifier, token);
				bSemicolonNecessary = false;
				return istatement;
			}
			this.Scanner.SetPosition(token);
			if (this.InCase)
			{
				_ICaseLabelStatement icaseLabelStatement = this.ParseCaseLabel(token);
				if (icaseLabelStatement != null)
				{
					_IStatement istatement = icaseLabelStatement;
					bSemicolonNecessary = false;
					return istatement;
				}
				this.Scanner.SetPosition(token);
			}
			_IExpression iexpression = this.ParseAssignExp(out bError);
			return this.LMItemFactory.CreateExpressionStatement(iexpression, token);
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0000F728 File Offset: 0x0000D928
		private _IStatement ParseSTStatementHelpPragma(out bool bError, IToken token)
		{
			return this.PragmaStatementParser.ParsePragma(out bError, token);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0000F737 File Offset: 0x0000D937
		private _ICaseLabelStatement ParseCaseLabel(IToken tokenCaseLabel)
		{
			return CaseStatementParser.ParseCaseLabel(this.Context, tokenCaseLabel);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0000F745 File Offset: 0x0000D945
		private _IStatement ParseCase(out bool bError, IToken tokenCase)
		{
			return CaseStatementParser.ParseCaseStatement(this.Context, out bError, tokenCase);
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0000F754 File Offset: 0x0000D954
		private _IStatement ParsePOUDeclaration(IToken tokenPOUType, Operator opParam)
		{
			return this.Context.DeclarationParser.ParsePOUDeclaration(tokenPOUType, opParam);
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0000F768 File Offset: 0x0000D968
		private _IStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			return this.Context.DeclarationParser.ParseVariableDeclaration(tokenIdent);
		}

		// Token: 0x06000349 RID: 841 RVA: 0x0000F77B File Offset: 0x0000D97B
		private _IStatement ParseTypeDeclaration(IToken tokenType)
		{
			return this.Context.DeclarationParser.ParseTypeDeclaration(tokenType);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0000F78E File Offset: 0x0000D98E
		private _IStatement ParseVariableList(IToken tokenVar)
		{
			return this.Context.DeclarationParser.ParseVariableList(tokenVar);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0000F7A1 File Offset: 0x0000D9A1
		private _IReturnStatement ParseReturn(out bool bError, IToken tokenRet)
		{
			return ReturnStatementParser.Parse(this.Context, out bError, tokenRet);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0000F7B0 File Offset: 0x0000D9B0
		private _ITryCatchStatement ParseTryCatchStatement(out bool bError, IToken tokenRet)
		{
			return TryCatchStatementParser.Parse(this.Context, out bError, tokenRet);
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0000F7BF File Offset: 0x0000D9BF
		private _IStatement ParseIf(out bool bError, IToken tokenIf)
		{
			return IfStatementParser.Parse(this.Context, out bError, tokenIf);
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0000F7CE File Offset: 0x0000D9CE
		private _IStatement ParseWhile(out bool bError, IToken tokenWhile)
		{
			return WhileStatementParser.Parse(this.Context, out bError, tokenWhile);
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0000F7DD File Offset: 0x0000D9DD
		private _IStatement ParseFor(out bool bError, IToken tokenFor)
		{
			return ForStatementParser.Parse(this.Context, out bError, tokenFor);
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0000F7EC File Offset: 0x0000D9EC
		private _IStatement ParseRepeat(out bool bError, IToken tokenRepeat)
		{
			return RepeatStatementParser.Parse(this.Context, out bError, tokenRepeat);
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0000F7FB File Offset: 0x0000D9FB
		private _IStatement ParseConditionalCall(out bool bError, IToken tokenCalc)
		{
			return ConditionalCallParser.Parse(this.Context, out bError, tokenCalc);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0000F80A File Offset: 0x0000DA0A
		private _IStatement ParseJump(out bool bError, IToken tokenJump)
		{
			return JumpStatementParser.Parse(this.Context, out bError, tokenJump);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0000F819 File Offset: 0x0000DA19
		private bool ShouldCreateBpForCurrentStatement()
		{
			return this.m_BpStack.Count <= 0 || this.m_BpStack.Peek().Item1;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0000F83B File Offset: 0x0000DA3B
		internal void EnableBp()
		{
			this.m_BpStack.Push(new Tuple<bool, int>(true, this._stackDepth - 1));
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0000F856 File Offset: 0x0000DA56
		internal void DisableBp()
		{
			this.m_BpStack.Push(new Tuple<bool, int>(false, this._stackDepth - 1));
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000F871 File Offset: 0x0000DA71
		internal void RestoreBp()
		{
			if (this.m_BpStack.Count > 0 && this.m_BpStack.Peek().Item2 >= this._stackDepth)
			{
				this.m_BpStack.Pop();
			}
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0000F8A5 File Offset: 0x0000DAA5
		private _IExpression ParseAssignExp(out bool bError)
		{
			if (this.Implicit || this.ImplicitAnyway)
			{
				return this.Context.ExpressionParser.ParseInitialisationExp(out bError);
			}
			return this.Context.ExpressionParser.ParseAssignment(out bError);
		}

		// Token: 0x04000096 RID: 150
		private readonly LStack<Operator> m_BeginOpStack = new LStack<Operator>();

		// Token: 0x04000097 RID: 151
		private int _stackDepth;

		// Token: 0x04000098 RID: 152
		private readonly LStack<Tuple<bool, int>> m_BpStack = new LStack<Tuple<bool, int>>();
	}
}
