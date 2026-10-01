using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IWarningDisableRestorePragmaStatement : IStatement, IExprement
	{
		bool Restore { get; }

		string Id { get; }
	}
}
