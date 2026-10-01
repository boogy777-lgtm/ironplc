using System;
using CODESYS.Parser;
using CODESYS.Parser35220.PragmaScanner;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000039 RID: 57
	public class PragmaStatementParser
	{
		// Token: 0x060003FE RID: 1022 RVA: 0x00011E26 File Offset: 0x00010026
		internal PragmaStatementParser(ParserContext context, IScanner9 scannerForPragmaParser)
		{
			this.PragmaScanner = new PragmaScanner(scannerForPragmaParser);
			this.Context = context;
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x00011E41 File Offset: 0x00010041
		private ParserContext Context { get; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000400 RID: 1024 RVA: 0x00011E49 File Offset: 0x00010049
		private _ILanguageModelBuilder7 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000401 RID: 1025 RVA: 0x00011E56 File Offset: 0x00010056
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000402 RID: 1026 RVA: 0x00011E63 File Offset: 0x00010063
		// (set) Token: 0x06000403 RID: 1027 RVA: 0x00011E75 File Offset: 0x00010075
		private Guid MessageGuid
		{
			get
			{
				return this.Context.InternalParser.MessageGuid;
			}
			set
			{
				this.Context.InternalParser.MessageGuid = value;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x00011E88 File Offset: 0x00010088
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x00011E95 File Offset: 0x00010095
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x00011EA2 File Offset: 0x000100A2
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x00011EB4 File Offset: 0x000100B4
		internal _IStatement ParsePragma(out bool bError, IToken token)
		{
			_IStatement istatement = this.ParsePragmaIntern(out bError, token);
			if (istatement == null)
			{
				return this.CreatePragmaStatement(token);
			}
			return istatement;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x00011ED6 File Offset: 0x000100D6
		private _IStatement CreatePragmaStatement(IToken token)
		{
			_IPragmaStatement ipragmaStatement = this.LMItemFactory.CreatePragmaStatement(token);
			ipragmaStatement.Text = this.Scanner.GetPragma(token);
			return ipragmaStatement;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00011EF8 File Offset: 0x000100F8
		private _IStatement ParsePragmaIntern(out bool bError, IToken tokenPragma)
		{
			string pragma = this.Scanner.GetPragma(tokenPragma);
			return this.ParsePragmaIntern(out bError, null, pragma, tokenPragma);
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x00011F1C File Offset: 0x0001011C
		internal _IStatement ParsePragmaIntern(out bool bError, IMinimalPosition errorpos, string stPragma, IToken tokenPragma)
		{
			this.PragmaScanner.Reset(stPragma, tokenPragma);
			bError = false;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3)
			{
				return null;
			}
			PragmaOperator @operator = pragmaToken.Operator;
			switch (@operator)
			{
			case 1:
				this.StatementParser.Flow = true;
				break;
			case 2:
				this.StatementParser.Flow = false;
				break;
			case 3:
				this.StatementParser.Bp = true;
				break;
			case 4:
				this.StatementParser.Bp = false;
				break;
			case 5:
			case 7:
			case 12:
			case 15:
				break;
			case 6:
				return this.ParseBreakPointStatement(tokenPragma);
			case 8:
				return this.ParseErrorPragma(tokenPragma, errorpos, 2);
			case 9:
				return this.ParseWarningPragma(out bError, errorpos, tokenPragma);
			case 10:
				return this.ParseTextPragma(out bError, errorpos, tokenPragma, 8);
			case 11:
				return this.ParseTextPragma(out bError, errorpos, tokenPragma, 16);
			case 13:
				return this.ParseDefinePragma(tokenPragma);
			case 14:
				return this.ParseUndefinePragma(tokenPragma);
			default:
				switch (@operator)
				{
				case 28:
				{
					bool flag = this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 30;
					this.Scanner.AllowMultipleUnderlines = (flag || this.Context.StatementParser.ImplicitAnyway);
					this.Context.StatementParser.Implicit = flag;
					return this.LMItemFactory.CreateImplicitCodeSectionPragma(tokenPragma, stPragma, flag);
				}
				case 29:
				case 30:
				case 32:
				case 34:
				case 35:
					break;
				case 31:
					return this.ParseMessageGuidPragma(tokenPragma);
				case 33:
					return this.ParsePragmaAssert(out bError, tokenPragma);
				case 36:
					return this.ParsePragmaIf(out bError, tokenPragma);
				case 37:
				case 38:
				case 39:
				{
					_IPragmaStatement ipragmaStatement = this.LMItemFactory.CreatePragmaStatement(tokenPragma);
					ipragmaStatement.Text = stPragma;
					this.AddErrorST(ipragmaStatement, 81, new object[]
					{
						this.PragmaScanner.GetTokenText(pragmaToken)
					});
					return ipragmaStatement;
				}
				default:
					switch (@operator)
					{
					case 53:
						this.Context.DeclarationParser.AllowPaths = true;
						break;
					case 56:
						return this.ParseErrorPragma(tokenPragma, errorpos, 1);
					case 57:
						this.StatementParser.EnableBp();
						break;
					case 58:
						this.StatementParser.DisableBp();
						break;
					}
					break;
				}
				break;
			}
			return null;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x0001216C File Offset: 0x0001036C
		private _IStatement ParseTextPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma, Severity severity)
		{
			return ErrorPragmaParser.ParseTextPragma(this.Context, out bError, tokenPragma, errorpos, severity);
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0001217E File Offset: 0x0001037E
		private _IStatement ParseWarningPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			return ErrorPragmaParser.ParseWarningPragma(this.Context, out bError, errorpos, tokenPragma);
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00012190 File Offset: 0x00010390
		private _IStatement ParseBreakPointStatement(IToken tokenPragma)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 2)
			{
				return null;
			}
			long integer = pragmaToken.Integer;
			long invalidPosition = Helper.InvalidPosition;
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 3 && pragmaToken.Operator == 7 && this.PragmaScanner.GetNext(ref pragmaToken) == 2)
			{
				return null;
			}
			return this.LMItemFactory.CreateBreakPointStatement(tokenPragma, integer, invalidPosition);
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x000121F8 File Offset: 0x000103F8
		private _IStatement ParseUndefinePragma(IToken tokenPragma)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				return null;
			}
			string identifier = pragmaToken.Identifier;
			return this.LMItemFactory.CreateDefineStatement(tokenPragma, false, identifier);
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x0001222C File Offset: 0x0001042C
		private _IStatement ParseDefinePragma(IToken tokenPragma)
		{
			string text = null;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 1)
			{
				return null;
			}
			string identifier = pragmaToken.Identifier;
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 4)
			{
				text = pragmaToken.String;
			}
			return this.LMItemFactory.CreateDefineStatement(tokenPragma, true, identifier, text);
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0001227C File Offset: 0x0001047C
		private _IStatement ParseMessageGuidPragma(IToken tokenPragma)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 4)
			{
				Guid messageGuid;
				try
				{
					messageGuid = new Guid(pragmaToken.String);
				}
				catch
				{
					messageGuid = Guid.Empty;
				}
				this.MessageGuid = messageGuid;
				_IMessageGuidPragmaStatement imessageGuidPragmaStatement = this.LMItemFactory.CreateMessageGuidPragmaStatement(tokenPragma, this.MessageGuid);
				imessageGuidPragmaStatement.Text = this.Scanner.GetTokenText(tokenPragma);
				return imessageGuidPragmaStatement;
			}
			return null;
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x000122F0 File Offset: 0x000104F0
		private _IStatement ParseErrorPragma(IToken tokenPragma, IMinimalPosition errorpos, Severity sev)
		{
			return ErrorPragmaParser.ParseErrorPragma(this.Context, tokenPragma, errorpos, sev);
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00012300 File Offset: 0x00010500
		internal _IExpression ParsePragmaORExp(out bool bError, IToken tokenPragma)
		{
			return PragmaOperandParser.ParsePragmaORExp(this.Context, out bError, tokenPragma);
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00012310 File Offset: 0x00010510
		private _IStatement ParsePragmaAssert(out bool bError, IToken tokenPragma)
		{
			bError = false;
			string empty = string.Empty;
			_IExpression conditionExpression = this.GetConditionExpression(tokenPragma, ref empty);
			return this.LMItemFactory.CreatePragmaAssertion(conditionExpression, empty, tokenPragma);
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00012340 File Offset: 0x00010540
		private _IExpression GetConditionExpression(IToken tokenPragma, ref string stErrorMessage)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				return null;
			}
			bool flag;
			_IExpression result = this.ParsePragmaORExp(out flag, tokenPragma);
			if (flag)
			{
				return result;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 47)
			{
				return result;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 4)
			{
				return result;
			}
			stErrorMessage = pragmaToken.String;
			return result;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x000123AF File Offset: 0x000105AF
		private _IStatement ParsePragmaIf(out bool bError, IToken tokenPragma)
		{
			return PragmaIfStatementParser.ParsePragmaIfStatement(this.Context, out bError, tokenPragma);
		}

		// Token: 0x040000A6 RID: 166
		internal readonly IPragmaScanner PragmaScanner;
	}
}
