using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDefineStatement : IStatement, IExprement
	{
		bool Define { get; }

		string Ident { get; }

		string Value { get; }
	}
}
