using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct ErrorPragmaParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private ErrorPragmaParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IStatement ParseErrorPragma(ParserContext context, IToken token, IMinimalPosition errorpos, Severity sev)
		{
			return new ErrorPragmaParser(context).ParseErrorPragma(token, errorpos, sev);
		}

		internal static _IStatement ParseWarningPragma(ParserContext context, out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			return new ErrorPragmaParser(context).ParseWarningPragma(out bError, errorpos, tokenPragma);
		}

		internal static _IStatement ParseTextPragma(ParserContext context, out bool bError, IToken token, IMinimalPosition errorpos, Severity sev)
		{
			return new ErrorPragmaParser(context).ParseTextPragma(out bError, errorpos, token, sev);
		}

		private _IStatement ParseErrorPragma(IToken tokenPragma, IMinimalPosition errorpos, Severity sev)
		{
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.SingleByteString)
			{
				return null;
			}
			_IStatement iStatement = LMItemFactory.CreateEmptyStatement(tokenPragma);
			IPragmaToken ptStart;
			short num = ParseLength(out ptStart);
			if (num < 0)
			{
				PragmaScanner.SetPosition(ptStart);
				num = iStatement.PositionLength;
			}
			IMinimalPosition sourcepos = iStatement._Position;
			if (errorpos != null)
			{
				sourcepos = errorpos;
			}
			_ICompilerMessage cm = iStatement.AddMessage(token.String, sourcepos, sev, num, MessageId.None);
			ParseShowAttributes(out var _, cm);
			return iStatement;
		}

		private short ParseLength(out IPragmaToken ptStart)
		{
			short result = -1;
			if (PragmaScanner.GetNext(out ptStart) == PragmaTokenType.Operator && ptStart.Operator == PragmaOperator.Comma && PragmaScanner.GetNext(out var token) == PragmaTokenType.Identifier && token.Identifier == "Length" && PragmaScanner.GetNext(out token) == PragmaTokenType.Operator && token.Operator == PragmaOperator.assign && PragmaScanner.GetNext(out token) == PragmaTokenType.Integer)
			{
				result = (short)token.Integer;
			}
			return result;
		}

		private void ParseLength(ref short sLength, IPragmaToken pt)
		{
			if (pt.Identifier == "Length" && PragmaScanner.GetNext(out pt) == PragmaTokenType.Operator && pt.Operator == PragmaOperator.assign && PragmaScanner.GetNext(out pt) == PragmaTokenType.Integer)
			{
				sLength = (short)pt.Integer;
			}
		}

		private _IStatement ParseTextPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma, Severity severity)
		{
			bError = false;
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.SingleByteString)
			{
				return null;
			}
			_IStatement iStatement = LMItemFactory.CreateEmptyStatement(tokenPragma);
			IMinimalPosition sourcepos = iStatement._Position;
			if (errorpos != null)
			{
				sourcepos = errorpos;
			}
			_ICompilerMessage cm = iStatement.AddMessage(token.String, sourcepos, severity, iStatement.PositionLength, MessageId.None);
			ParseShowAttributes(out bError, cm);
			return iStatement;
		}

		private _IStatement ParseWarningPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			bError = false;
			if (PragmaScanner.GetNext(out var token) == PragmaTokenType.Operator)
			{
				if (PragmaScanner.GetNext(out var token2) != PragmaTokenType.Identifier)
				{
					return null;
				}
				if (token.Operator == PragmaOperator.restore)
				{
					return LMItemFactory.CreateWarningDisableRestorePragmaStatement(tokenPragma, bRestore: true, token2.Identifier);
				}
				if (token.Operator == PragmaOperator.disable)
				{
					return LMItemFactory.CreateWarningDisableRestorePragmaStatement(tokenPragma, bRestore: false, token2.Identifier);
				}
				return null;
			}
			if (token.Type != PragmaTokenType.SingleByteString)
			{
				return null;
			}
			short sLength = -1;
			string stMessageGuid = null;
			IPragmaToken position = ParseOperands(ref sLength, ref stMessageGuid);
			if (sLength < 0)
			{
				PragmaScanner.SetPosition(position);
			}
			_IStatement iStatement = LMItemFactory.CreateEmptyStatement(tokenPragma);
			IMinimalPosition sourcepos = iStatement._Position;
			if (errorpos != null)
			{
				sourcepos = errorpos;
			}
			if (sLength < 0)
			{
				sLength = iStatement.PositionLength;
			}
			_ICompilerMessage iCompilerMessage = iStatement.AddMessage(token.String, sourcepos, Severity.Warning, sLength, MessageId.Wrn_Pragma);
			if (!string.IsNullOrEmpty(stMessageGuid))
			{
				iCompilerMessage.ObjectGuid = new Guid(stMessageGuid);
			}
			ParseShowAttributes(out bError, iCompilerMessage);
			return iStatement;
		}

		private IPragmaToken ParseOperands(ref short sLength, ref string stMessageGuid)
		{
			if (PragmaScanner.GetNext(out var token) == PragmaTokenType.Operator && token.Operator == PragmaOperator.Comma)
			{
				IPragmaToken token2;
				switch (PragmaScanner.GetNext(out token2))
				{
				case PragmaTokenType.Identifier:
					ParseLength(ref sLength, token2);
					break;
				case PragmaTokenType.Operator:
					if (token2.Operator == PragmaOperator.messageguid && PragmaScanner.GetNext(out token2) == PragmaTokenType.SingleByteString)
					{
						stMessageGuid = token2.String;
					}
					break;
				}
			}
			return token;
		}

		private void ParseShowAttributes(out bool bError, _ICompilerMessage cm)
		{
			bool flag = true;
			ShowAttribute showAttribute = ShowAttribute.None;
			bError = false;
			IPragmaToken token;
			while (flag && PragmaScanner.GetNext(out token) == PragmaTokenType.Operator)
			{
				switch (token.Operator)
				{
				case PragmaOperator.show_compile:
					showAttribute |= ShowAttribute.Compile;
					break;
				case PragmaOperator.show_precompile:
					showAttribute |= ShowAttribute.Precompile;
					break;
				default:
					flag = false;
					break;
				}
			}
			if (showAttribute != 0)
			{
				cm.ShowAttribute = showAttribute;
			}
		}
	}
}
