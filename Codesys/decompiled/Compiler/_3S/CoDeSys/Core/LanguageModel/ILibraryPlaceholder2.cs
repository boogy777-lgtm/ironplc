using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholder2 : ILibraryPlaceholder
	{
		bool QualifiedOnlyLocal { get; set; }
	}
}
