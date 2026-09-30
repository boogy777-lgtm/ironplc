using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableExpression2 : IVariableExpression, IExpression2, IExpression, IExprement
	{
		IVariable GetVariable(IPrecompileScope scope);

		ISignature GetSignature(IPrecompileScope scope);
	}
}
