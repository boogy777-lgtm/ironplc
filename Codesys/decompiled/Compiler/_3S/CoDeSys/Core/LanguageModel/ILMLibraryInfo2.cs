using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMLibraryInfo2 : ILMLibraryInfo
	{
		bool QualifiedOnlyLocal { get; set; }
	}
}
