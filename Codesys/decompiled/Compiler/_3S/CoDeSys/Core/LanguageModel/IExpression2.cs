using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpression2 : IExpression, IExprement
	{
		ILiteralValue Literal(IPrecompileScope scope);
	}
}
