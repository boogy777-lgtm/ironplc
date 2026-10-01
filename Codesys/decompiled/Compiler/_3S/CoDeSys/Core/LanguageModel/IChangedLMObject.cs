using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IChangedLMObject
	{
		string Name { get; }

		Operator POUType { get; }

		string Description { get; }

		EPouSetChange Change { get; }

		bool OnlineChangePossible { get; }
	}
}
