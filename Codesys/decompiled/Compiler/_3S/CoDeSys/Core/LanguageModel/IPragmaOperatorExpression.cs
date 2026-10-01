using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPragmaOperatorExpression : IExpression2, IExpression, IExprement
	{
		Operator Operator { get; }

		IExpression[] AllOperands { get; }
	}
}
