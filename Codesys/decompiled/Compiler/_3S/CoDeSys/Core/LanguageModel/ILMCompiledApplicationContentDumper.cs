using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationContentDumper
	{
		string DumpCode(ICompiledPOU cpou);

		string GetDisassembly(ICompiledPOU cpou);

		string DumpDataManager();
	}
}
