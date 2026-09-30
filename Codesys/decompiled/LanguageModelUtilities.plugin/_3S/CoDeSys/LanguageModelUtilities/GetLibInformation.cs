using System;
using System.Collections.Generic;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class GetLibInformation : IGetLibInformation2, IGetLibInformation
	{
		public IEnumerable<ILibManItem> GetProjectLibs(int nProj)
		{
			return LibraryHelpers.GetAllLibManItemsInProject(nProj, null);
		}

		public IEnumerable<ILibManItem> GetProjectLibs(int nProj, Guid gdApp)
		{
			return LibraryHelpers.GetAllLibManItemsInProject(nProj, gdApp);
		}

		public IManagedLibrary GetManagedLib(ILibManItem lmi)
		{
			return LibraryHelpers.GetManagedLibFromLibManItem(lmi);
		}
	}
}
