using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext14 : IPreCompileContext13, IPreCompileContext12, IPreCompileContext11, IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		bool IsDefined(string stDefineIdent);

		bool DefineHasValue(string stDefineIdent, string stValue);
	}
}
