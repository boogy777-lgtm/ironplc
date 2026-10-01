using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IParseTreeService
	{
		IParseTreeProvider CreateParseTreeProvider(_ICompiledPOU2 cpou, IGreenTreeConverter converter, ITreeFactory treeFactory);
	}
}
