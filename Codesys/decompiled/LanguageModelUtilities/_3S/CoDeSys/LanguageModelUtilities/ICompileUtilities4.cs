using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities4 : ICompileUtilities3, ICompileUtilities2, ICompileUtilities
	{
		ISignature FindSignature(ICompileContext comcon, int nProjectHandle, Guid guidObject);

		IDataLocationInformation GetInformationForDataLocation(Guid guidApplication, IDataLocation codeLocation, IDataLocation instanceLocation, ICallStackEntry2 oldLocation, bool bException);
	}
}
