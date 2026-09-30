using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICastExpression : IExpression2, IExpression, IExprement
	{
		IExpression ExprWithType { get; }

		IExpression Base { get; }

		ICompiledType ExplicitelySpecifiedType { get; }
	}
}
