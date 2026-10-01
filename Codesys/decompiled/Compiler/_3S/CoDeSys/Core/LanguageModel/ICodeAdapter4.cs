using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter4 : ICodeAdapter3, ICodeAdapter2, ICodeAdapter
	{
		bool IsLikeScalar(ICompiledType ctype);
	}
}
