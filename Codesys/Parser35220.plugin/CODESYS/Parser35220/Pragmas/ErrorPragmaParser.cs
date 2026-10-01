using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000031 RID: 49
	internal readonly struct ErrorPragmaParser
	{
		// Token: 0x06000384 RID: 900 RVA: 0x0001008B File Offset: 0x0000E28B
		private ErrorPragmaParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00010094 File Offset: 0x0000E294
		internal static _IStatement ParseErrorPragma(ParserContext context, IToken token, IMinimalPosition errorpos, Severity sev)
		{
			ErrorPragmaParser errorPragmaParser = new ErrorPragmaParser(context);
			return errorPragmaParser.ParseErrorPragma(token, errorpos, sev);
		}

		// Token: 0x06000386 RID: 902 RVA: 0x000100B4 File Offset: 0x0000E2B4
		internal static _IStatement ParseWarningPragma(ParserContext context, out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			ErrorPragmaParser errorPragmaParser = new ErrorPragmaParser(context);
			return errorPragmaParser.ParseWarningPragma(out bError, errorpos, tokenPragma);
		}

		// Token: 0x06000387 RID: 903 RVA: 0x000100D4 File Offset: 0x0000E2D4
		internal static _IStatement ParseTextPragma(ParserContext context, out bool bError, IToken token, IMinimalPosition errorpos, Severity sev)
		{
			ErrorPragmaParser errorPragmaParser = new ErrorPragmaParser(context);
			return errorPragmaParser.ParseTextPragma(out bError, errorpos, token, sev);
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000388 RID: 904 RVA: 0x000100F5 File Offset: 0x0000E2F5
		private ParserContext Context { get; }

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000389 RID: 905 RVA: 0x000100FD File Offset: 0x0000E2FD
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x0600038A RID: 906 RVA: 0x0001010A File Offset: 0x0000E30A
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x0600038B RID: 907 RVA: 0x00010117 File Offset: 0x0000E317
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00010124 File Offset: 0x0000E324
		private _IStatement ParseErrorPragma(IToken tokenPragma, IMinimalPosition errorpos, Severity sev)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 4)
			{
				return null;
			}
			_IStatement istatement = this.LMItemFactory.CreateEmptyStatement(tokenPragma);
			IPragmaToken position;
			short num = this.ParseLength(out position);
			if (num < 0)
			{
				this.PragmaScanner.SetPosition(position);
				num = istatement.PositionLength;
			}
			IMinimalPosition minimalPosition = istatement._Position;
			if (errorpos != null)
			{
				minimalPosition = errorpos;
			}
			_ICompilerMessage cm = istatement.AddMessage(pragmaToken.String, minimalPosition, sev, num, 0);
			bool flag;
			this.ParseShowAttributes(out flag, cm);
			return istatement;
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0001019C File Offset: 0x0000E39C
		private short ParseLength(out IPragmaToken ptStart)
		{
			short result = -1;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref ptStart) == 3 && ptStart.Operator == 47 && this.PragmaScanner.GetNext(ref pragmaToken) == 1 && pragmaToken.Identifier == "Length" && this.PragmaScanner.GetNext(ref pragmaToken) == 3 && pragmaToken.Operator == 32 && this.PragmaScanner.GetNext(ref pragmaToken) == 2)
			{
				result = (short)pragmaToken.Integer;
			}
			return result;
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0001021C File Offset: 0x0000E41C
		private void ParseLength(ref short sLength, IPragmaToken pt)
		{
			if (pt.Identifier == "Length" && this.PragmaScanner.GetNext(ref pt) == 3 && pt.Operator == 32 && this.PragmaScanner.GetNext(ref pt) == 2)
			{
				sLength = (short)pt.Integer;
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00010270 File Offset: 0x0000E470
		private _IStatement ParseTextPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma, Severity severity)
		{
			bError = false;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 4)
			{
				return null;
			}
			_IStatement istatement = this.LMItemFactory.CreateEmptyStatement(tokenPragma);
			IMinimalPosition minimalPosition = istatement._Position;
			if (errorpos != null)
			{
				minimalPosition = errorpos;
			}
			_ICompilerMessage cm = istatement.AddMessage(pragmaToken.String, minimalPosition, severity, istatement.PositionLength, 0);
			this.ParseShowAttributes(out bError, cm);
			return istatement;
		}

		// Token: 0x06000390 RID: 912 RVA: 0x000102CC File Offset: 0x0000E4CC
		private _IStatement ParseWarningPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			bError = false;
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 3)
			{
				IPragmaToken pragmaToken2;
				if (this.PragmaScanner.GetNext(ref pragmaToken2) != 1)
				{
					return null;
				}
				if (pragmaToken.Operator == 55)
				{
					return this.LMItemFactory.CreateWarningDisableRestorePragmaStatement(tokenPragma, true, pragmaToken2.Identifier);
				}
				if (pragmaToken.Operator == 54)
				{
					return this.LMItemFactory.CreateWarningDisableRestorePragmaStatement(tokenPragma, false, pragmaToken2.Identifier);
				}
				return null;
			}
			else
			{
				if (pragmaToken.Type != 4)
				{
					return null;
				}
				short num = -1;
				string text = null;
				IPragmaToken position = this.ParseOperands(ref num, ref text);
				if (num < 0)
				{
					this.PragmaScanner.SetPosition(position);
				}
				_IStatement istatement = this.LMItemFactory.CreateEmptyStatement(tokenPragma);
				IMinimalPosition minimalPosition = istatement._Position;
				if (errorpos != null)
				{
					minimalPosition = errorpos;
				}
				if (num < 0)
				{
					num = istatement.PositionLength;
				}
				_ICompilerMessage icompilerMessage = istatement.AddMessage(pragmaToken.String, minimalPosition, 4, num, 373);
				if (!string.IsNullOrEmpty(text))
				{
					icompilerMessage.ObjectGuid = new Guid(text);
				}
				this.ParseShowAttributes(out bError, icompilerMessage);
				return istatement;
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000103CC File Offset: 0x0000E5CC
		private IPragmaToken ParseOperands(ref short sLength, ref string stMessageGuid)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) == 3 && pragmaToken.Operator == 47)
			{
				IPragmaToken pragmaToken2;
				PragmaTokenType next = this.PragmaScanner.GetNext(ref pragmaToken2);
				if (next == 1)
				{
					this.ParseLength(ref sLength, pragmaToken2);
				}
				else if (next == 3 && pragmaToken2.Operator == 31 && this.PragmaScanner.GetNext(ref pragmaToken2) == 4)
				{
					stMessageGuid = pragmaToken2.String;
				}
			}
			return pragmaToken;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00010438 File Offset: 0x0000E638
		private void ParseShowAttributes(out bool bError, _ICompilerMessage cm)
		{
			bool flag = true;
			ShowAttribute showAttribute = 0;
			bError = false;
			IPragmaToken pragmaToken;
			while (flag && this.PragmaScanner.GetNext(ref pragmaToken) == 3)
			{
				PragmaOperator @operator = pragmaToken.Operator;
				if (@operator != 34)
				{
					if (@operator != 35)
					{
						flag = false;
					}
					else
					{
						showAttribute |= 1;
					}
				}
				else
				{
					showAttribute |= 2;
				}
			}
			if (showAttribute != null)
			{
				cm.ShowAttribute = showAttribute;
			}
		}
	}
}
