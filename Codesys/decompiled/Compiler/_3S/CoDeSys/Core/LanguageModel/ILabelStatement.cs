using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILabelStatement : IStatement, IExprement
	{
		string Text { get; }
	}
}
