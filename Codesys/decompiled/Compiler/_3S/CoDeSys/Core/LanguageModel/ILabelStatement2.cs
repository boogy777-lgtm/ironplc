using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILabelStatement2 : ILabelStatement, IStatement, IExprement
	{
		string OrgText { get; }
	}
}
