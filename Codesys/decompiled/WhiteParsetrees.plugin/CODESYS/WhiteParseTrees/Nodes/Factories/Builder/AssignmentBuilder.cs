using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Expressions;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class AssignmentBuilder : IAssignmentBuilder, IExpressionBuilder<IWhiteAssignmentExpression>, ILValueStep, IAssignmentTokenStep, IRValueStep
	{
		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression LValue
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression RValue
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IAnyAssignmentToken AssignmentOperator { get; set; }

		public AssignmentBuilder()
		{
			AssignmentOperator = TokenFactory<IAnyAssignmentToken>.Create(":=");
		}

		public IWhiteAssignmentExpression Build()
		{
			if (LValue == null)
			{
				throw new BuilderException("LValue not assigned");
			}
			if (RValue == null)
			{
				throw new BuilderException("RValue not assigned");
			}
			return new WhiteAssignmentExpression(LValue, RValue, AssignmentOperator);
		}

		public IAssignmentTokenStep WithLValue(IWhiteExpression lvalue)
		{
			LValue = lvalue;
			return this;
		}

		public IAssignmentBuilder WithRValue(IWhiteExpression rvalue)
		{
			RValue = rvalue;
			return this;
		}

		public IRValueStep WithAssignmentToken(IAnyAssignmentToken token)
		{
			AssignmentOperator = token;
			return this;
		}
	}
}
