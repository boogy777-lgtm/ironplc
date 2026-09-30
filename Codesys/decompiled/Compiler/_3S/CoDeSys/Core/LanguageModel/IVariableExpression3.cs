using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableExpression3 : IVariableExpression2, IVariableExpression, IExpression2, IExpression, IExprement
	{
		int PrecompileVariableId { get; }

		int PrecompileSignatureId { get; }
	}
}
