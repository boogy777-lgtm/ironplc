using System;
using System.Collections.Generic;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200005A RID: 90
	internal class SyntaxElementParser
	{
		// Token: 0x1700015A RID: 346
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x00017C60 File Offset: 0x00015E60
		private ParserContext Context { get; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600059F RID: 1439 RVA: 0x00017C68 File Offset: 0x00015E68
		private List<SyntaxElement> SyntaxElements { get; }

		// Token: 0x060005A0 RID: 1440 RVA: 0x00017C70 File Offset: 0x00015E70
		private _IStatement NextStatement()
		{
			return this.ParseSTStatement(true);
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060005A1 RID: 1441 RVA: 0x00017C79 File Offset: 0x00015E79
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x00017C86 File Offset: 0x00015E86
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x00017C94 File Offset: 0x00015E94
		private _IStatement ParseSTStatement(bool bTopLevel)
		{
			this.StatementParser.ContextualOperatorHandler.TryRecognizeContextualDeclarationOperator();
			bool flag;
			return this.StatementParser.ParseSTStatement(out flag, bTopLevel);
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x00017CBF File Offset: 0x00015EBF
		private SyntaxElementParser(ParserContext context)
		{
			this.Context = context;
			this.SyntaxElements = new List<SyntaxElement>();
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x00017CD9 File Offset: 0x00015ED9
		public static List<SyntaxElement> ParseSyntaxElements(ParserContext context)
		{
			SyntaxElementParser syntaxElementParser = new SyntaxElementParser(context);
			syntaxElementParser._ParseSyntaxElements();
			return syntaxElementParser.SyntaxElements;
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x00017CEC File Offset: 0x00015EEC
		private void _ParseSyntaxElements()
		{
			for (;;)
			{
				for (EndOfPOUElement item = this.CheckForEndOfPOU(); item != null; item = this.CheckForEndOfPOU())
				{
					this.SyntaxElements.Add(item);
				}
				_IStatement istatement = this.NextStatement();
				if (istatement == null)
				{
					break;
				}
				this.SyntaxElements.Add(new StatementElement(istatement));
			}
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x00017D38 File Offset: 0x00015F38
		private EndOfPOUElement CheckForEndOfPOU()
		{
			Operator @operator;
			IToken token;
			if (this.TryNextOperator(this.Scanner, out @operator, out token))
			{
				if (@operator <= 283)
				{
					switch (@operator)
					{
					case 70:
					case 73:
					case 74:
					case 76:
						break;
					case 71:
					case 72:
					case 75:
						goto IL_63;
					default:
						if (@operator - 281 > 2)
						{
							goto IL_63;
						}
						break;
					}
				}
				else if (@operator != 288 && @operator != 291)
				{
					goto IL_63;
				}
				return new EndOfPOUElement(@operator, token);
			}
			IL_63:
			this.Scanner.SetPosition(token);
			return null;
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x00017DB5 File Offset: 0x00015FB5
		private bool TryNextOperator(IScanner9 scanner, out Operator op, out IToken token)
		{
			op = 0;
			if (scanner.Next(out token, true, true) != 15)
			{
				return false;
			}
			op = scanner.GetOperator(token);
			return true;
		}
	}
}
