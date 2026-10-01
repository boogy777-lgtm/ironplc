using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct ReturnStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ReturnStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IReturnStatement Parse(ParserContext context, out bool bError, IToken tokenTry)
		{
			return new ReturnStatementParser(context).ParseReturn(out bError, tokenTry);
		}

		private _IReturnStatement ParseReturn(out bool bError, IToken tokenRet)
		{
			bError = false;
			_IReturnStatement iReturnStatement = LMItemFactory.CreateReturnStatement(tokenRet);
			Scanner.Next(out var token);
			if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.LeftParenthesis)
			{
				_IExpression iExpression = ExpressionParser.ParseAssignExp(out bError) ?? LMItemFactory.CreateErrorExpression(token);
				StatementParser.CheckForOperator(iExpression, Operator.RightParenthesis, bError, out var exprError);
				if (exprError != null)
				{
					foreach (_ICompilerMessage item in iExpression.MessagesList.Where((_ICompilerMessage m) => Severity.Error == m.Severity))
					{
						exprError.AddError(item.Text, item.MessageId);
					}
					iReturnStatement._Condition = exprError;
				}
				else
				{
					iReturnStatement._Condition = iExpression;
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			return iReturnStatement;
		}
	}
}
