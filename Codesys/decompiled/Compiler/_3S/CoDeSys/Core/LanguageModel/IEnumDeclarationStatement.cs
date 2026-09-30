using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IEnumDeclarationStatement : IStatement, IExprement
	{
		IExpression Value { get; }

		string Name { get; }
	}
}
