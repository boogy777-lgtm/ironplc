using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ForBuilder : IForBuilder, IStatementBuilder<IWhiteForStatement>, IForStartExpressiontStep, IForUpperBoundStep, IForStepSizeStep, IForControlledStep
	{
		private IForToken For { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteAssignmentExpression StartExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IToToken To { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression UpperBound
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IByToken By { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression StepWidth
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IDoToken Do { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteSequenceStatement Controlled
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IEndForToken EndFor { get; set; }

		public ForBuilder()
		{
			For = TokenFactory<IForToken>.Create("FOR");
			To = TokenFactory<IToToken>.Create("TO");
			By = TokenFactory<IByToken>.Create("BY");
			Do = TokenFactory<IDoToken>.Create("DO");
			EndFor = TokenFactory<IEndForToken>.Create("END_FOR");
		}

		public IWhiteForStatement Build()
		{
			if (StartExpression == null)
			{
				throw new BuilderException("StartExpression not assigned");
			}
			if (UpperBound == null)
			{
				throw new BuilderException("UpperBound not assigned");
			}
			if (Controlled == null)
			{
				throw new BuilderException("Controlled not assigned");
			}
			IByToken by = ((StepWidth != null) ? By : null);
			return new WhiteForStatement(For, StartExpression, To, UpperBound, by, StepWidth, Do, Controlled, EndFor);
		}

		IForUpperBoundStep IForStartExpressiontStep.WithStartExpression(IWhiteAssignmentExpression startExpression)
		{
			StartExpression = startExpression;
			return this;
		}

		public IForStepSizeStep WithUpperBound(IWhiteExpression upperBound)
		{
			UpperBound = upperBound;
			return this;
		}

		public IForControlledStep WithStepSize(IWhiteExpression stepWidth)
		{
			StepWidth = stepWidth;
			return this;
		}

		public IForBuilder WithControlled(IWhiteSequenceStatement controlled)
		{
			Controlled = controlled;
			return this;
		}
	}
}
