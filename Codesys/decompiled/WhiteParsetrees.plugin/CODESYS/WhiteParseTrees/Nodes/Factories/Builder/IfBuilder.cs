using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class IfBuilder : IIfBuilder, IStatementBuilder<IWhiteIfStatement>, IIfConditionStep, IIfThenStatementStep, IIfElseIfStatementStep, IIfElseStatementStep
	{
		private IIfToken If { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression Condition
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IThenToken Then { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteSequenceStatement ThenStatement
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteElseIfStatement> ElseIfStatement
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		private IElseToken Else { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteSequenceStatement ElseStatement
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IEndIfToken EndIf { get; set; }

		public IfBuilder()
		{
			If = TokenFactory<IIfToken>.Create("IF");
			Then = TokenFactory<IThenToken>.Create("THEN");
			Else = TokenFactory<IElseToken>.Create("ELSE");
			EndIf = TokenFactory<IEndIfToken>.Create("END_IF");
		}

		public IWhiteIfStatement Build()
		{
			if (Condition == null)
			{
				throw new BuilderException("Condition not assigned");
			}
			if (ThenStatement == null)
			{
				throw new BuilderException("ThenStatement not assigned");
			}
			if (ElseIfStatement == null)
			{
				throw new BuilderException("ElseIfStatement not assigned");
			}
			if (ElseStatement == null)
			{
				throw new BuilderException("ElseStatement not assigned");
			}
			IElseToken @else = ((ElseStatement.Count > 0) ? Else : null);
			return new WhiteIfStatement(If, Condition, Then, ThenStatement, ElseIfStatement, @else, ElseStatement, EndIf);
		}

		public IIfThenStatementStep WithCondition(IWhiteExpression condition)
		{
			Condition = condition;
			return this;
		}

		public IIfElseIfStatementStep WithThenStatement(IWhiteSequenceStatement thenStatement)
		{
			ThenStatement = thenStatement;
			return this;
		}

		public IIfElseStatementStep WithElseIfStatement(IEnumerable<IWhiteElseIfStatement> elseIfStatements)
		{
			ElseIfStatement = elseIfStatements;
			return this;
		}

		public IIfBuilder WithElseStatement(IWhiteSequenceStatement elseStatement)
		{
			ElseStatement = elseStatement;
			return this;
		}
	}
}
