using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU9 : ICompiledPOU8, ICompiledPOU6, ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU
	{
		void SetFlag(CompiledPOUFlags cpFlag, bool bSetTrue);
	}
}
