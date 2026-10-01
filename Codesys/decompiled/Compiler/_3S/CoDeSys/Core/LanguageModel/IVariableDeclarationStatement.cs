using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableDeclarationStatement : IStatement, IExprement
	{
		IExpression[] VariableNames { get; }

		IType DeclaredType { get; }

		IExpression InitializationExpression { get; }

		IDirectVariable AddressLocation { get; }
	}
}
