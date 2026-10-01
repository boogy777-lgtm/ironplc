using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal class CodeStatementChecker : DummyBaseClass, IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		private ParserContext Context { get; }

		private CodeStatementChecker(ParserContext context)
		{
			Context = context;
		}

		internal static void CheckForUnexpectedStatementsInImplementation(_IStatement statement, ParserContext context)
		{
			CodeStatementChecker ivisit = new CodeStatementChecker(context);
			statement.Accept(ivisit);
		}

		private void CheckNotExpected(_IStatement statement, _ISequenceStatement sequence, int index)
		{
			if (statement is _IPOUDeclarationStatement || statement is _ITypeDeclarationStatement || statement is _IVariableDeclarationListStatement || statement is _IVariableDeclarationStatement || statement is _IEnumDeclarationStatement || statement is IEnumDeclarationListStatement)
			{
				_IStatement iStatement = Context.LMItemFactory.CreateErrorStatement();
				iStatement._Position = statement._Position;
				iStatement.LengthIntern = statement.LengthIntern;
				Context.ErrorHandler.AddErrorST(iStatement, MessageId.Err_UnexpectedStatement);
				sequence._StatementList[index] = iStatement;
			}
		}

		public void visit(_ISequenceStatement seq)
		{
			for (int i = 0; i < seq._StatementList.Count; i++)
			{
				_IStatement statement = seq._StatementList[i];
				seq._StatementList[i].Accept(this);
				CheckNotExpected(statement, seq, i);
			}
		}

		public void visit(_ICompiledPOU cpou)
		{
			cpou.GetParseTree().Accept(this);
		}

		public void visit(_IWhileStatement whilst)
		{
			whilst._Controlled.Accept(this);
		}

		public void visit(_IRepeatStatement repeat)
		{
			repeat._Controlled.Accept(this);
		}

		public void visit(_IForStatement forloop)
		{
			forloop._Controlled.Accept(this);
		}

		public void visit(_IIfStatement ifst)
		{
			ifst._IfThen.Accept(this);
			ifst._IfElse?.Accept(this);
			foreach (_IElseIf item in ifst._ElseIf)
			{
				item._Controlled.Accept(this);
			}
		}

		public void visit(_ICaseStatement casest)
		{
			foreach (_ICase @case in casest._Cases)
			{
				@case._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
		}
	}
}
