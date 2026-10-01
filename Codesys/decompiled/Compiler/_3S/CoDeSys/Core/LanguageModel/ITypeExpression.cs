using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeExpression : IExpression2, IExpression, IExprement
	{
		ICompiledType2 ExpressionType { get; }
	}
}
