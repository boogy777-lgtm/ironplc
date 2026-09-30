using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMLibraryInfo4 : ILMLibraryInfo3, ILMLibraryInfo2, ILMLibraryInfo
	{
		bool Optional { get; set; }
	}
}
