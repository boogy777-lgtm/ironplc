using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal class InitializationParser
	{
		internal bool InSTCode { get; set; }

		private ParserContext Context { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		internal InitializationParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		public _IExpression ParseInitialisation()
		{
			bool bBreakInit = false;
			return ParseInitialisation(ref bBreakInit);
		}

		internal _IExpression ParseInitialisation(ref bool bBreakInit)
		{
			_IExpression iExpression = ParseStructInitialisation(ref bBreakInit, bInterface: true);
			if (bBreakInit)
			{
				return iExpression;
			}
			if (iExpression != null)
			{
				return iExpression;
			}
			iExpression = ParseArrayInitialisation(ref bBreakInit);
			if (bBreakInit)
			{
				return iExpression;
			}
			if (iExpression != null)
			{
				return iExpression;
			}
			bool bError;
			return ExpressionParser.ParseAssignment(out bError);
		}

		internal _IExpression ParseInitialisationExp(out bool bError)
		{
			_IExpression iExpression = ExpressionParser.ParseORExp(out bError);
			IToken token;
			TokenType num = Next(out token);
			Operator @operator = Operator.None;
			if ((num == TokenType.Operator && (@operator = Scanner.GetOperator(token)) == Operator.Assign) || @operator == Operator.SetAssign || @operator == Operator.ResetAssign || @operator == Operator.RefAssign || (@operator == Operator.FupAssign && !InSTCode))
			{
				_IExpression rValue = ParseAnyInitialisationExpression(out bError);
				_IAssignmentExpression iAssignmentExpression = LMItemFactory.CreateAssignmentExpression(iExpression, token);
				iAssignmentExpression._RValue = rValue;
				iAssignmentExpression.KindOf = @operator;
				iExpression = iAssignmentExpression;
			}
			else
			{
				Scanner.SetPosition(token);
			}
			return iExpression;
		}

		private _IExpression ParseAnyInitialisationExpression(out bool bError)
		{
			bError = false;
			bool bBreakInit = false;
			_IExpression iExpression = ParseStructInitialisation(ref bBreakInit, bInterface: false);
			if (iExpression != null)
			{
				return iExpression;
			}
			iExpression = ParseArrayInitialisation(ref bBreakInit);
			if (iExpression != null)
			{
				return iExpression;
			}
			return ExpressionParser.ParseAssignment(out bError);
		}

		private _IExpression ParseStructInitialisation(ref bool bBreakInit, bool bInterface)
		{
			return StructureInitializationParser.ParseStructInitialisation(Context, this, ref bBreakInit, bInterface);
		}

		private _IExpression ParseArrayInitialisation(ref bool bBreakInit)
		{
			return ArrayInitializationParser.ParseArrayInitialisation(Context, this, ref bBreakInit);
		}
	}
}
