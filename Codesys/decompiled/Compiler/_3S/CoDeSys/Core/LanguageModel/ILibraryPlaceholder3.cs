using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholder3 : ILibraryPlaceholder2, ILibraryPlaceholder
	{
		bool Optional { get; set; }
	}
}
