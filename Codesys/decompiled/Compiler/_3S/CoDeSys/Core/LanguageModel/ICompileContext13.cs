using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext13 : ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		string GetDisassembly(ICompiledPOU cpou);

		IVariable AddWatchVariable(string stName, ICompiledType type);

		IVariable AddWatchVariable(string stName, ICompiledType type, IDataLocation requestedLocation);

		IVariable GetWatchVariable(string stName);

		void RemoveWatchVariables();

		string[] SubElements(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, out bool bValid);
	}
}
