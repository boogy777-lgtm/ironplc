using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IGetLibInformation
	{
		IEnumerable<ILibManItem> GetProjectLibs(int nProj);

		IManagedLibrary GetManagedLib(ILibManItem lmi);
	}
}
