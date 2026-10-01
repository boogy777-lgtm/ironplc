using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPlaceholderInfo
	{
		ILibraryPlaceholder PlaceholderInfo { get; }

		ILibParameterTable ParamTable { get; set; }
	}
}
