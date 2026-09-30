using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200005E RID: 94
	internal readonly struct VariableListParser
	{
		// Token: 0x060005F5 RID: 1525 RVA: 0x00019649 File Offset: 0x00017849
		private VariableListParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00019654 File Offset: 0x00017854
		internal static _IVariableDeclarationListStatement ParseVariableList(ParserContext context, IToken token)
		{
			VariableListParser variableListParser = new VariableListParser(context);
			return variableListParser.ParseVariableList(token);
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060005F7 RID: 1527 RVA: 0x00019671 File Offset: 0x00017871
		private ParserContext Context { get; }

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x00019679 File Offset: 0x00017879
		private Operator OpCurrentVariableList
		{
			get
			{
				return this.DeclarationParser.OpCurrentVariableList;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060005F9 RID: 1529 RVA: 0x00019686 File Offset: 0x00017886
		private DeclarationParser DeclarationParser
		{
			get
			{
				return this.Context.DeclarationParser;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x00019693 File Offset: 0x00017893
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060005FB RID: 1531 RVA: 0x000196A0 File Offset: 0x000178A0
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x000196AD File Offset: 0x000178AD
		private void ParseReSyncIF()
		{
			this.Scanner.ParseReSyncIF();
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x000196BA File Offset: 0x000178BA
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x000196C8 File Offset: 0x000178C8
		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x000196D8 File Offset: 0x000178D8
		private _IVariableDeclarationListStatement ParseVariableList(IToken tokenVar)
		{
			VarFlag flags = 2097152L;
			this.DeclarationParser.OpCurrentVariableList = this.Scanner.GetOperator(tokenVar);
			_IVariableDeclarationListStatement ivariableDeclarationListStatement = this.LMItemFactory.CreateVariableDeclarationListStatement(tokenVar);
			VariableListParser.SetVariableFlagByOperator(ref flags, this.OpCurrentVariableList);
			this.ScanForAdditionalFlags(ref flags);
			ivariableDeclarationListStatement.Flags = flags;
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement();
			IToken token;
			while (this.Next(out token, true, true) != 15 || (this.Scanner.GetOperator(token) != 81 && this.Scanner.GetOperator(token) != 78 && this.Scanner.GetOperator(token) != 79))
			{
				if (token.Type == 21)
				{
					IL_130:
					ivariableDeclarationListStatement.VariableDeclaration = isequenceStatement;
					this.DeclarationParser.OpCurrentVariableList = 0;
					return ivariableDeclarationListStatement;
				}
				this.Scanner.SetPosition(token);
				bool inDeclaration = this.Context.StatementParser.InDeclaration;
				this.Context.StatementParser.InDeclaration = true;
				bool flag;
				_IStatement istatement = this.Context.StatementParser.ParseSTStatement(out flag, false);
				this.Context.StatementParser.InDeclaration = inDeclaration;
				if (flag)
				{
					this.ParseReSyncIF();
				}
				isequenceStatement.Add(istatement);
				istatement.SetFlag(8L, this.DeclarationParser.Library);
			}
			this.Scanner.SetPosition(token);
			goto IL_130;
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x0001982C File Offset: 0x00017A2C
		private void ScanForAdditionalFlags(ref VarFlag vf)
		{
			IToken position;
			for (;;)
			{
				this.Next(out position, true, true);
				this.Scanner.SetPosition(position);
				IToken token;
				if (this.Next(out token) != 15)
				{
					break;
				}
				Operator @operator = this.Scanner.GetOperator(token);
				if (@operator <= 91)
				{
					if (@operator != 66)
					{
						if (@operator != 91)
						{
							goto Block_3;
						}
						vf |= 2048L;
					}
					else
					{
						vf |= 64L;
						vf |= 32L;
						if ((vf & 8L) == 8L)
						{
							vf &= -33L;
						}
					}
				}
				else if (@operator != 97)
				{
					if (@operator != 229)
					{
						goto Block_5;
					}
					if ((vf & 8192L) != 8192L)
					{
						goto IL_C4;
					}
					vf |= 34359738368L;
				}
				else
				{
					vf |= 1024L;
				}
			}
			this.Scanner.SetPosition(position);
			return;
			Block_3:
			Block_5:
			IL_C4:
			this.Scanner.SetPosition(position);
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x0001990C File Offset: 0x00017B0C
		private static void SetVariableFlagByOperator(ref VarFlag vf, Operator op)
		{
			switch (op)
			{
			case 99:
				vf = 513L;
				return;
			case 100:
				vf = 67109377L;
				return;
			case 101:
			case 102:
			case 103:
			case 104:
			case 106:
				break;
			case 105:
				vf = 1L;
				return;
			case 107:
				vf = 4096L;
				return;
			case 108:
				vf = 262160L;
				return;
			case 109:
				vf = 270336L;
				return;
			case 110:
				vf = 2L;
				return;
			case 111:
				vf = 8L;
				return;
			case 112:
				vf = 4L;
				return;
			case 113:
				vf = 2097152L;
				return;
			case 114:
				vf = 134479872L;
				return;
			default:
				if (op == 244)
				{
					vf = 137438953472L;
					return;
				}
				if (op != 280)
				{
					return;
				}
				vf = 2199023255552L;
				break;
			}
		}
	}
}
