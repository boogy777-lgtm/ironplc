using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableDeclarationListStatement : IStatement, IExprement
	{
		VarFlag Flags { get; }

		IVariableDeclarationStatement[] Declarations { get; }
	}
}
