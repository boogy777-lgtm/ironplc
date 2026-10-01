using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPouReference2 : IPouReference, IExpression2, IExpression, IExprement
	{
		IExpression InstanceExpression { get; }
	}
}
