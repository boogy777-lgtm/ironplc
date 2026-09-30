using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal class SyntaxElementParser
	{
		private ParserContext Context { get; }

		private List<SyntaxElement> SyntaxElements { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private IScanner9 Scanner => Context.Scanner;

		private _IStatement NextStatement()
		{
			return ParseSTStatement(bTopLevel: true);
		}

		private _IStatement ParseSTStatement(bool bTopLevel)
		{
			bool bError;
			return StatementParser.ParseSTStatement(out bError, bTopLevel);
		}

		private SyntaxElementParser(ParserContext context)
		{
			Context = context;
			SyntaxElements = new List<SyntaxElement>();
		}

		public static List<SyntaxElement> ParseSyntaxElements(ParserContext context)
		{
			SyntaxElementParser syntaxElementParser = new SyntaxElementParser(context);
			syntaxElementParser._ParseSyntaxElements();
			return syntaxElementParser.SyntaxElements;
		}

		private void _ParseSyntaxElements()
		{
			while (true)
			{
				EndOfPOUElement endOfPOUElement = CheckForEndOfPOU();
				if (endOfPOUElement != null)
				{
					SyntaxElements.Add(endOfPOUElement);
				}
				_IStatement iStatement = NextStatement();
				if (iStatement != null)
				{
					SyntaxElements.Add(new StatementElement(iStatement));
					continue;
				}
				break;
			}
		}

		private EndOfPOUElement CheckForEndOfPOU()
		{
			if (TryNextOperator(Scanner, out var op, out var token))
			{
				switch (op)
				{
				case Operator.EndAction:
				case Operator.EndFunction:
				case Operator.EndFunctionBlock:
				case Operator.EndProgram:
				case Operator.EndMethod:
				case Operator.EndInterface:
					return new EndOfPOUElement(op, token);
				}
			}
			Scanner.SetPosition(token);
			return null;
		}

		private bool TryNextOperator(IScanner9 scanner, out Operator op, out IToken token)
		{
			op = Operator.None;
			if (scanner.Next(out token, bWithPragma: true, bWithComment: true) != TokenType.Operator)
			{
				return false;
			}
			op = scanner.GetOperator(token);
			return true;
		}
	}
}
