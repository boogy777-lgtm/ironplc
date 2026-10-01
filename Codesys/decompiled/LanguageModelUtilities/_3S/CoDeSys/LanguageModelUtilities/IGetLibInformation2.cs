using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IGetLibInformation2 : IGetLibInformation
	{
		IEnumerable<ILibManItem> GetProjectLibs(int nProj, Guid gdApp);
	}
}
