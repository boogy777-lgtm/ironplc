using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILateCompileContext
	{
		bool AddLateLanguageModelForPOU(ILMPOU lmpou);

		bool AddLateLanguageModelForGVL(ILMGlobVarlist lmgvl);

		bool AddLateLanguageModelForDUT(ILMDataType lmdut);
	}
}
