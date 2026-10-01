using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Expressions;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class CallBuilder : ICallBuilder, IExpressionBuilder<IWhiteCallExpression>, ICalleeStep, ICalleeParamsStep
	{
		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression Callee
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private ILeftParenthesisToken LeftParenthesis { get; set; }

		private IEnumerable<IWhiteExpression> Params { get; set; }

		private IRightParenthesisToken RightParenthesis { get; set; }

		public CallBuilder()
		{
			Params = new List<IWhiteExpression>();
			LeftParenthesis = TokenFactory<ILeftParenthesisToken>.Create("(");
			RightParenthesis = TokenFactory<IRightParenthesisToken>.Create(")");
		}

		public IWhiteCallExpression Build()
		{
			if (Callee == null)
			{
				Callee = new WhiteEmptyExpression();
			}
			return new WhiteCallExpression(Callee, LeftParenthesis, Params, RightParenthesis);
		}

		public ICallBuilder WithParams(IEnumerable<IWhiteExpression> parameters)
		{
			Params = parameters;
			return this;
		}

		public ICalleeParamsStep WithCallee(IWhiteExpression callee)
		{
			Callee = callee;
			return this;
		}
	}
}
