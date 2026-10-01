using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext15 : ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		void Define(string stDefineIdent, string stValue);

		void Undefine(string stDefineIdent);
	}
}
