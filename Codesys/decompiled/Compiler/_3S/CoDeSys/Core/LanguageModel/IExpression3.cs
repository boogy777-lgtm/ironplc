using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpression3 : IExpression2, IExpression, IExprement
	{
		IVariable GetVariable(IScope scope);

		ISignature GetSignature(IScope scope);
	}
}
