using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMultipleIndexInitialization : IExpression, IExprement
	{
		IExpression Number { get; }

		IExpression Value { get; }

		int NumberInt(out bool bValid, IScope scope);

		int NumberInt(out bool bValid, IPrecompileScope scope);
	}
}
