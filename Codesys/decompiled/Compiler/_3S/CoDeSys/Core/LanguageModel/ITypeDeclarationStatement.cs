using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeDeclarationStatement : IStatement, IExprement
	{
		IType AliasType { get; }

		string Name { get; }

		IExpression BaseType { get; }

		IExpression InitialValue { get; }

		IVariableDeclarationListStatement StructDeclarationList { get; }

		IEnumDeclarationListStatement EnumDeclarationList { get; }

		SignatureFlag Flags { get; }
	}
}
