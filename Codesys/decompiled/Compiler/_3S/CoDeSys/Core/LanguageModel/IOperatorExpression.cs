using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IOperatorExpression : IExpression2, IExpression, IExprement
	{
		Operator Code { get; }

		IExpression[] Operands { get; }
	}
}
