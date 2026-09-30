using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeReference2 : ITypeReference, IExpression2, IExpression, IExprement
	{
		IExpression InstanceExpression { get; }
	}
}
