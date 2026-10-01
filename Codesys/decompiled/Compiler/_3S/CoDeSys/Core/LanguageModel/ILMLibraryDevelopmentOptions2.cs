using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMLibraryDevelopmentOptions2 : ILMLibraryDevelopmentOptions
	{
		ECheckAllPoolObjectsTargetPointerSize CheckAllPoolObjectsTargetPointerSize { get; set; }
	}
}
