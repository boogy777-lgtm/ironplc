using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU5 : ICompiledPOU4, ICompiledPOU3, ICompiledPOU
	{
		int CodeGeneratorStackSize { set; }
	}
}
