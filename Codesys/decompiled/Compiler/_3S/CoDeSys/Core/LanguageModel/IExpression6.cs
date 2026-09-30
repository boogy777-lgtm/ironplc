using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpression6 : IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IExprement
	{
		IIdentifierInfo2 IdentifierInfo { get; }

		int PrecompileVariableId { get; }

		int PrecompileSignatureId { get; }
	}
}
