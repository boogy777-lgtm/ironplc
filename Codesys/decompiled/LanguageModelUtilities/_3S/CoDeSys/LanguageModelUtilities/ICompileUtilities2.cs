using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities2 : ICompileUtilities
	{
		IDataLocationInformation GetInformationForDataLocation(Guid guidApplication, IDataLocation codeLocation, IDataLocation instanceLocation, bool bException);
	}
}
