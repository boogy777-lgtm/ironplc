using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISequenceStatement2 : ISequenceStatement, IStatement, IExprement
	{
		void RemoveStatement(int i);
	}
}
