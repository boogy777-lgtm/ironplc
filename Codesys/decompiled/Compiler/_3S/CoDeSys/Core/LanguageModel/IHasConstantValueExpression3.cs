using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IHasConstantValueExpression3 : IHasConstantValueExpression2, IHasConstantValueExpression, IExpression2, IExpression, IExprement
	{
		IExpression ConstantValueExpression { get; }
	}
}
