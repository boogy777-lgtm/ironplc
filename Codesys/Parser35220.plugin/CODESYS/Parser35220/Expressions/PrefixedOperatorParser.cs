using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000048 RID: 72
	internal readonly struct PrefixedOperatorParser
	{
		// Token: 0x060004DD RID: 1245 RVA: 0x0001548C File Offset: 0x0001368C
		private PrefixedOperatorParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x00015495 File Offset: 0x00013695
		private ParserContext Context { get; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060004DF RID: 1247 RVA: 0x0001549D File Offset: 0x0001369D
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x000154AA File Offset: 0x000136AA
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x000154B7 File Offset: 0x000136B7
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x000154C5 File Offset: 0x000136C5
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x000154D2 File Offset: 0x000136D2
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x060004E4 RID: 1252 RVA: 0x000154DF File Offset: 0x000136DF
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x000154EC File Offset: 0x000136EC
		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			this.Next(out tokenPos);
			this.Scanner.SetPosition(tokenPos);
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x00015510 File Offset: 0x00013710
		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			PrefixedOperatorParser prefixedOperatorParser = new PrefixedOperatorParser(context);
			return prefixedOperatorParser.Parse(out bError, op, token, out endToken);
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x00015531 File Offset: 0x00013731
		private _IExpression Parse(out bool bError, Operator op, _IToken token, out _IToken endToken)
		{
			return this.ParsePrefixedOperator(out bError, op, token, out endToken);
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00015540 File Offset: 0x00013740
		private _IExpression ParsePrefixedOperator(out bool bError, Operator op, _IToken currentToken, out _IToken endToken)
		{
			bError = false;
			endToken = currentToken;
			_IOperatorExpression ioperatorExpression = this.LMItemFactory.CreateOperatorExpression(op, currentToken);
			_IExpression result;
			if (!this.CheckForLeftParenthesis(ref bError, ioperatorExpression, out result))
			{
				return result;
			}
			IToken token;
			if (this.Scanner.TryNextOperator(168, out token))
			{
				_IExpression result2 = ioperatorExpression;
				endToken = (token as _IToken);
				return result2;
			}
			this.Scanner.SetPosition(token);
			_IExpression result3;
			if (this.HandleSizeOfOperator(op, ref endToken, token, ioperatorExpression, out result3))
			{
				return result3;
			}
			this.ParseOperands(ref bError, ref endToken, token, ioperatorExpression);
			return ioperatorExpression;
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x000155B8 File Offset: 0x000137B8
		private void ParseOperands(ref bool bError, ref _IToken endToken, IToken tokenHelp, _IOperatorExpression opexp)
		{
			IToken position;
			for (;;)
			{
				bool flag;
				_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag) ?? this.LMItemFactory.CreateErrorExpression(tokenHelp);
				opexp.AddOperand(iexpression);
				Operator @operator;
				if (this.Next(out tokenHelp) != 15 || ((@operator = this.Scanner.GetOperator(tokenHelp)) != 171 && @operator != 168))
				{
					this.ErrorHandler.AddErrorSTWithToken(opexp, tokenHelp, 2, new object[]
					{
						this.Scanner.GetOperatorText(171),
						this.Scanner.GetOperatorText(168),
						this.Scanner.GetTokenText(tokenHelp)
					});
					@operator = this.ParseReSyncST(out position, new Operator[]
					{
						171,
						168
					});
					if (@operator != 171 && @operator != 168)
					{
						break;
					}
				}
				if (@operator != 171 && @operator == 168)
				{
					goto Block_6;
				}
			}
			this.Scanner.SetPosition(position);
			bError = true;
			return;
			Block_6:
			endToken = (tokenHelp as _IToken);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x000156C4 File Offset: 0x000138C4
		private bool HandleSizeOfOperator(Operator op, ref _IToken endToken, IToken tokenHelp, _IOperatorExpression opexp, out _IExpression sizeofexp)
		{
			sizeofexp = null;
			if (Helper.IsSizeOfOperator(op))
			{
				_IType itype = this.TypeParser.TryParseType();
				if (itype == null || itype.Class == 28)
				{
					this.Scanner.SetPosition(tokenHelp);
				}
				else
				{
					IToken token;
					if (this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 168)
					{
						_ITypeExpression itypeExpression = this.LMItemFactory.CreateTypeExpression(itype, tokenHelp);
						opexp.AddOperand(itypeExpression);
						sizeofexp = opexp;
						endToken = (token as _IToken);
						return true;
					}
					this.Scanner.SetPosition(tokenHelp);
				}
			}
			return false;
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00015758 File Offset: 0x00013958
		private bool CheckForLeftParenthesis(ref bool bError, _IOperatorExpression opexp, out _IExpression expression)
		{
			expression = null;
			IToken token;
			if (this.Scanner.TryNextOperator(167, out token))
			{
				return true;
			}
			this.ErrorHandler.AddErrorSTWithToken(opexp, token, 6, new object[]
			{
				this.Scanner.GetOperatorText(167),
				this.Scanner.GetTokenText(token)
			});
			Operator[] array = new Operator[3];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.0F804606BBAE4409CF04F7EAD25A0F6E0AA52F2C1779915C2E37AE5AD840520F).FieldHandle);
			if (this.ParseReSyncST(out token, array) == 168)
			{
				expression = opexp;
				return true;
			}
			this.Scanner.SetPosition(token);
			bError = true;
			expression = opexp;
			return false;
		}
	}
}
