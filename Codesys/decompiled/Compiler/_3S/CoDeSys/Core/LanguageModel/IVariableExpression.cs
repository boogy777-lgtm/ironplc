using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableExpression : IExpression2, IExpression, IExprement
	{
		string Name { get; }

		int VariableId { get; }

		int SignatureId { get; }

		int ScopeId { get; }

		IVariable GetVariable(IScope scope);

		ISignature GetSignature(IScope scope);

		IScope GetScope(IScope scope);
	}
}
