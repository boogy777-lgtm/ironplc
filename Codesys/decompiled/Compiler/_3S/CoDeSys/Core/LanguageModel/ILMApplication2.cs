using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMApplication2 : ILMApplication
	{
		int TargetOutputSize { get; set; }

		int TargetInputSize { get; set; }

		int TargetMemorySize { get; set; }
	}
}
