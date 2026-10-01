using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IHasConstantValueExpression2 : IHasConstantValueExpression, IExpression2, IExpression, IExprement
	{
		Operator OpComparison { get; }
	}
}
