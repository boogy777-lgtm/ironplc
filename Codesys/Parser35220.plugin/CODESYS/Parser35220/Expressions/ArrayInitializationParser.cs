using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200003C RID: 60
	internal readonly struct ArrayInitializationParser
	{
		// Token: 0x0600042E RID: 1070 RVA: 0x0001292B File Offset: 0x00010B2B
		private ArrayInitializationParser(ParserContext context, InitializationParser initializationParser)
		{
			this.Context = context;
			this.InitializationParser = initializationParser;
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x0600042F RID: 1071 RVA: 0x0001293B File Offset: 0x00010B3B
		private ParserContext Context { get; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000430 RID: 1072 RVA: 0x00012943 File Offset: 0x00010B43
		private InitializationParser InitializationParser { get; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000431 RID: 1073 RVA: 0x0001294B File Offset: 0x00010B4B
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000432 RID: 1074 RVA: 0x00012958 File Offset: 0x00010B58
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000433 RID: 1075 RVA: 0x00012965 File Offset: 0x00010B65
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00012972 File Offset: 0x00010B72
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x00012980 File Offset: 0x00010B80
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x0001298D File Offset: 0x00010B8D
		private Operator MatchOperator(params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, true, ops);
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x000129A3 File Offset: 0x00010BA3
		private Operator MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, bGenerateError, ops);
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x000129BC File Offset: 0x00010BBC
		internal static _IExpression ParseArrayInitialisation(ParserContext context, InitializationParser initializationParser, ref bool bBreakInit)
		{
			ArrayInitializationParser arrayInitializationParser = new ArrayInitializationParser(context, initializationParser);
			return arrayInitializationParser.ParseArrayInitialisation(ref bBreakInit);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x000129DC File Offset: 0x00010BDC
		private _IExpression ParseArrayInitialisation(ref bool bBreakInit)
		{
			IToken token;
			this.Next(out token);
			this.Scanner.SetPosition(token);
			IToken token2;
			if (!this.CheckForArrayInitialisation(token, out token2))
			{
				return null;
			}
			IToken position;
			_IExpression emptyArrayInitialization = this.GetEmptyArrayInitialization(token2, out position);
			if (emptyArrayInitialization != null)
			{
				return emptyArrayInitialization;
			}
			this.Scanner.SetPosition(position);
			_IArrayInitialization iarrayInitialization = this.LMItemFactory.CreateArrayInitialisation(token2);
			this.ParseInitializationExpressions(ref bBreakInit, iarrayInitialization);
			return iarrayInitialization;
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00012A40 File Offset: 0x00010C40
		private void ParseInitializationExpressions(ref bool bBreakInit, _IArrayInitialization arrinit)
		{
			Operator @operator;
			for (;;)
			{
				_IExpression iexpression = this.TryParseMultipleArrayInitialization(ref bBreakInit) ?? this.InitializationParser.ParseInitialisation(ref bBreakInit);
				if (bBreakInit)
				{
					break;
				}
				Operator[] array = new Operator[3];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.A0E4BDC5BDF9247ACE7897AFA7DA47B15CF96FBA97A86759B7EEDA56211701F1).FieldHandle);
				@operator = this.MatchOperator(array);
				iexpression = this.HandleCallExpression(iexpression);
				if (@operator != 167)
				{
					arrinit.AddInitValue(iexpression);
				}
				if (@operator != 171)
				{
					if (@operator != 167)
					{
						goto Block_4;
					}
					if (this.ParseMultipleArrayInit2(ref bBreakInit, arrinit, iexpression))
					{
						return;
					}
				}
			}
			return;
			Block_4:
			if (@operator != 170)
			{
				bBreakInit = true;
				return;
			}
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00012AC4 File Offset: 0x00010CC4
		private bool ParseMultipleArrayInit2(ref bool bBreakInit, _IArrayInitialization arrinit, _IExpression expr)
		{
			_IExpression value = this.InitializationParser.ParseInitialisation(ref bBreakInit);
			if (bBreakInit)
			{
				return true;
			}
			_IMultipleIndexInitialization imultipleIndexInitialization = this.LMItemFactory.CreateMultipleIndexInitialization(this.Scanner.CurrentToken);
			imultipleIndexInitialization._Number = expr;
			imultipleIndexInitialization._Value = value;
			arrinit.AddInitValue(imultipleIndexInitialization);
			this.MatchOperator(new Operator[]
			{
				168
			});
			return this.MatchOperator(new Operator[]
			{
				171,
				170
			}) != 171;
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00012B4B File Offset: 0x00010D4B
		private bool CheckForArrayInitialisation(IToken tokenPos, out IToken tokenHelp)
		{
			if (this.Next(out tokenHelp) != 15 || this.Scanner.GetOperator(tokenHelp) != 169)
			{
				this.Scanner.SetPosition(tokenPos);
				return false;
			}
			return true;
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x00012B7B File Offset: 0x00010D7B
		private _IExpression GetEmptyArrayInitialization(IToken tokenPos, out IToken tokenHelp2)
		{
			if (this.Next(out tokenHelp2) == 15 && this.Scanner.GetOperator(tokenHelp2) == 170)
			{
				return this.LMItemFactory.CreateArrayInitialisation(tokenPos);
			}
			return null;
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00012BAC File Offset: 0x00010DAC
		private _IExpression HandleCallExpression(_IExpression expr)
		{
			_ICallExpression icallExpression = expr as _ICallExpression;
			if (icallExpression != null && icallExpression.Inputs.Count == 1 && icallExpression.Inputs[0] == null && icallExpression.ParamExpressions.Count == 1)
			{
				_IMultipleIndexInitialization imultipleIndexInitialization = this.LMItemFactory.CreateMultipleIndexInitialization(this.Scanner.CurrentToken);
				imultipleIndexInitialization._Number = icallExpression._Callee;
				imultipleIndexInitialization._Value = icallExpression.ParamExpressions[0];
				expr = imultipleIndexInitialization;
			}
			return expr;
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00012C24 File Offset: 0x00010E24
		private _IExpression TryParseMultipleArrayInitialization(ref bool bBreakInit)
		{
			IToken position;
			if (this.Next(out position) != 13)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			this.Scanner.SetPosition(position);
			_IExpression iexpression = this.ExpressionParser.ParseQualifiedNameExpression(null);
			if (iexpression == null)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			if (this.MatchOperator(false, new Operator[]
			{
				167
			}) != 167)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			IToken token;
			this.Next(out token);
			if (token.Type == 15 && this.Scanner.GetOperator(token) == 168)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			this.Scanner.SetPosition(token);
			_IExpression iexpression2 = this.InitializationParser.ParseInitialisation(ref bBreakInit);
			if (iexpression2 == null)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			if (this.MatchOperator(false, new Operator[]
			{
				168
			}) != 168)
			{
				this.Scanner.SetPosition(position);
				return null;
			}
			return this.LMItemFactory.CreateMultipleIndexInitialization(iexpression, iexpression2);
		}
	}
}
