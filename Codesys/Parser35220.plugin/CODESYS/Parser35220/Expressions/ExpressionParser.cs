using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200003F RID: 63
	internal class ExpressionParser
	{
		// Token: 0x06000454 RID: 1108 RVA: 0x0001303C File Offset: 0x0001123C
		internal ExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x0001304B File Offset: 0x0001124B
		internal void SetInSTCode()
		{
			this.InitializationParser.InSTCode = true;
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000456 RID: 1110 RVA: 0x00013059 File Offset: 0x00011259
		private ParserContext Context { get; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x00013061 File Offset: 0x00011261
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x0001306E File Offset: 0x0001126E
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x0001307B File Offset: 0x0001127B
		private OperandParser OperandParser
		{
			get
			{
				return this.Context.OperandParser;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x00013088 File Offset: 0x00011288
		private InitializationParser InitializationParser
		{
			get
			{
				return this.Context.InitializationParser;
			}
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00013095 File Offset: 0x00011295
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x000130A3 File Offset: 0x000112A3
		internal _IExpression ParseORExp(out bool bError)
		{
			return InfixOperationParser.ParseORExp(this.Context, out bError);
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x000130B1 File Offset: 0x000112B1
		public _IExpression ParseInitialisation()
		{
			return this.InitializationParser.ParseInitialisation();
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x000130BE File Offset: 0x000112BE
		internal _IExpression ParseInitialisationExp(out bool bError)
		{
			return this.InitializationParser.ParseInitialisationExp(out bError);
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x000130CC File Offset: 0x000112CC
		internal IExpression ParseExpression()
		{
			bool flag;
			IExpression result = this.ParseAssignment(out flag);
			if (flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x000130E8 File Offset: 0x000112E8
		public _IExpression ParseAssignExp(out bool bError)
		{
			if (this.Context.StatementParser.ParseInitializationExpression)
			{
				return this.ParseInitialisationExp(out bError);
			}
			return this.ParseAssignment(out bError);
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0001310C File Offset: 0x0001130C
		internal _IExpression ParseAssignment(out bool bError)
		{
			_IExpression iexpression = this.ParseORExp(out bError);
			IToken token;
			int num = this.Next(out token);
			Operator @operator = 0;
			if ((num == 15 && (@operator = this.Scanner.GetOperator(token)) == 164) || @operator == 165 || @operator == 166 || @operator == 185 || @operator == 189)
			{
				int sourceOffset = this.Scanner.SourceOffset;
				_IExpression rvalue = this.ParseAssignment(out bError);
				_IAssignmentExpression iassignmentExpression = this.LMItemFactory.CreateAssignmentExpression(iexpression, token);
				iassignmentExpression._RValue = rvalue;
				iassignmentExpression.KindOf = @operator;
				iassignmentExpression._Position = iexpression._Position;
				iexpression = iassignmentExpression;
				int sourceOffset2 = this.Scanner.SourceOffset;
				iassignmentExpression.PositionLength = (short)(sourceOffset2 - sourceOffset);
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return iexpression;
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x000131C9 File Offset: 0x000113C9
		internal _IExpression ParseVarAccess(_IExpression exp, out bool bError, _IToken startToken, ref _IToken endToken)
		{
			return this.OperandParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x000131DB File Offset: 0x000113DB
		internal IExpression ParseOperand()
		{
			return this.OperandParser.ParseOperand();
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x000131E8 File Offset: 0x000113E8
		public _IExpression ParseSTOperand(out bool bError)
		{
			return this.OperandParser.ParseSTOperand(out bError);
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x000131F6 File Offset: 0x000113F6
		internal _IExpression ParseSTOperandWithPosition(out bool bError, _IToken startToken, out _IToken endToken)
		{
			return this.OperandParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00013208 File Offset: 0x00011408
		internal _IExpression ParseSTPrefixOperator(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			endToken = token;
			lenghtOfExpWithoutParanthesis = 0;
			if (op <= 154)
			{
				if (op <= 121)
				{
					switch (op)
					{
					case 1:
					case 28:
					case 29:
					case 33:
					case 34:
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
					case 42:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
					case 48:
					case 49:
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
					case 56:
					case 57:
					case 58:
					case 59:
						break;
					case 2:
						goto IL_377;
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					case 9:
					case 10:
					case 11:
					case 12:
					case 13:
					case 14:
					case 15:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
					case 21:
					case 22:
					case 23:
					case 24:
					case 25:
					case 26:
					case 27:
					case 30:
					case 31:
					case 32:
						goto IL_3B0;
					case 40:
					case 41:
						return MinMaxOperatorParser.ParseStatic(this.Context, out bError, op, token, startToken, out endToken);
					default:
						if (op - 120 > 1)
						{
							goto IL_3B0;
						}
						return ThisAndBaseExpressionParser.Parse(this.Context, out bError, op, token, startToken, out endToken);
					}
				}
				else
				{
					if (op == 133)
					{
						return UnaryNotParser.Parse(this.Context, out bError, op, token, startToken, out endToken);
					}
					if (op - 153 > 1)
					{
						goto IL_3B0;
					}
				}
			}
			else if (op <= 162)
			{
				if (op - 157 <= 1)
				{
					return UnaryPlusMinusParser.Parse(this.Context, out bError, op, token, startToken, out endToken);
				}
				if (op != 162)
				{
					goto IL_3B0;
				}
				goto IL_377;
			}
			else
			{
				if (op == 167)
				{
					return ParenthesizedExpressionParser.ParseParenthesizedExpressionStatic(this.Context, out bError, token, out endToken, out lenghtOfExpWithoutParanthesis);
				}
				switch (op)
				{
				case 184:
					return ConversionExpressionParser.ParseStatic(this.Context, out bError, op, token, startToken, out endToken);
				case 185:
				case 186:
				case 187:
				case 189:
				case 206:
				case 207:
				case 208:
				case 209:
				case 210:
				case 211:
				case 212:
				case 213:
				case 214:
				case 215:
				case 216:
				case 217:
				case 218:
				case 219:
					goto IL_3B0;
				case 188:
				case 190:
				case 191:
				case 193:
				case 194:
				case 195:
				case 196:
				case 197:
				case 198:
				case 199:
				case 201:
				case 203:
				case 204:
				case 205:
				case 220:
				case 221:
				case 222:
					break;
				case 192:
					goto IL_377;
				case 200:
					return NewExpressionParser.Parse(this.Context, out bError, token, out endToken);
				case 202:
					return ImplicitCastOperatorParser.ParseStatic(this.Context, out bError, op, token, startToken, out endToken);
				default:
					switch (op)
					{
					case 233:
					case 236:
					case 242:
					case 245:
					case 246:
					case 247:
					case 248:
					case 249:
					case 252:
					case 253:
					case 254:
					case 256:
					case 263:
					case 264:
					case 265:
					case 266:
					case 267:
					case 268:
					case 269:
					case 270:
					case 277:
					case 278:
					case 279:
						break;
					case 234:
					case 235:
					case 237:
					case 238:
					case 239:
					case 240:
					case 241:
					case 243:
					case 244:
					case 250:
					case 257:
					case 258:
					case 259:
					case 260:
					case 261:
					case 262:
					case 271:
					case 272:
					case 273:
					case 274:
					case 275:
					case 276:
						goto IL_3B0;
					case 251:
						goto IL_377;
					case 255:
						return CurrentTaskExpressionParser.ParseStatic(this.Context, out bError, op, token, startToken, out endToken);
					default:
						goto IL_3B0;
					}
					break;
				}
			}
			return PrefixedOperatorParser.Parse(this.Context, out bError, op, token, startToken, out endToken);
			IL_377:
			return ScopeExpressionParser.ParseStatic(this.Context, out bError, op, token, startToken, out endToken);
			IL_3B0:
			bError = true;
			return null;
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x000135C9 File Offset: 0x000117C9
		internal _IExpression ParseQualifiedNameExpression(_IExprement expForError)
		{
			return this.OperandParser.ParseQualifiedNameExpression(expForError);
		}
	}
}
