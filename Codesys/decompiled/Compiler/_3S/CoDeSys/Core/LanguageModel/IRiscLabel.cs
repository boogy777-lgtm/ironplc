using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscLabel
	{
		int InstructionIndex { get; }

		bool Defined { get; }

		string LabelString { get; }

		int[] Usages { get; }
	}
}
