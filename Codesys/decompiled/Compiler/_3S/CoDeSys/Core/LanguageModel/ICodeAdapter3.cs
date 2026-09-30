using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter3 : ICodeAdapter2, ICodeAdapter
	{
		IArea[] GetAreasRecursive();
	}
}
