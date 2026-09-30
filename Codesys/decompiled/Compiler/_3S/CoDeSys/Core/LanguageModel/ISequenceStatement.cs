using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISequenceStatement : IStatement, IExprement
	{
		IStatement[] Statements { get; }

		void AddStatement(IStatement state);

		void InsertStatement(int i, IStatement state);
	}
}
