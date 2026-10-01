using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ElseIfBuilder : IElseIfBuilder, IStatementBuilder<IWhiteElseIfStatement>, IElseIfConditionStep, IElseIfThenStep
	{
		private IElseIfToken ElseIf { get; set; }

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

		public ElseIfBuilder()
		{
			ElseIf = TokenFactory<IElseIfToken>.Create("ELSIF");
			Then = TokenFactory<IThenToken>.Create("THEN");
		}

		public IWhiteElseIfStatement Build()
		{
			if (Condition == null)
			{
				throw new BuilderException("Condition not assigned");
			}
			if (ThenStatement == null)
			{
				throw new BuilderException("ThenStatement not assigned");
			}
			return new WhiteElseIfStatement(ElseIf, Condition, Then, ThenStatement);
		}

		public IElseIfThenStep WithCondition(IWhiteExpression condition)
		{
			Condition = condition;
			return this;
		}

		public IElseIfBuilder WithThen(IWhiteSequenceStatement then)
		{
			ThenStatement = then;
			return this;
		}
	}
}
