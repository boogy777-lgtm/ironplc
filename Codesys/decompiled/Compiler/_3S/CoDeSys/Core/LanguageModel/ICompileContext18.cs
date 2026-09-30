using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext18 : ICompileContext17, ICompileContext16, ICompileContext15, ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		bool IsDefined(string stDefineIdent);

		bool DefineHasValue(string stDefineIdent, string stValue);

		string[] SubElementsWithRange(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid);
	}
}
