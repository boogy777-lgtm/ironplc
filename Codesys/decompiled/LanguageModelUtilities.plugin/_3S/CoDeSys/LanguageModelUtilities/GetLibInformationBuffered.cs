using System;
using System.Collections.Generic;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class GetLibInformationBuffered : IGetLibInformation2, IGetLibInformation
	{
		private Dictionary<int, List<ILibManItem>> m_libStructure;

		private Dictionary<Guid, Dictionary<int, List<ILibManItem>>> m_libStructureApp;

		private Dictionary<ILibManItem, IManagedLibrary> m_libItems;

		public GetLibInformationBuffered()
		{
			m_libStructure = new Dictionary<int, List<ILibManItem>>();
			m_libStructureApp = new Dictionary<Guid, Dictionary<int, List<ILibManItem>>>();
			m_libItems = new Dictionary<ILibManItem, IManagedLibrary>();
		}

		public IEnumerable<ILibManItem> GetProjectLibs(int nProj)
		{
			if (m_libStructure.TryGetValue(nProj, out var value))
			{
				return value;
			}
			value = new List<ILibManItem>(LibraryHelpers.GetAllLibManItemsInProject(nProj, null));
			m_libStructure[nProj] = value;
			return value;
		}

		public IEnumerable<ILibManItem> GetProjectLibs(int nProj, Guid gdApp)
		{
			if (m_libStructureApp.TryGetValue(gdApp, out var value))
			{
				if (value.TryGetValue(nProj, out var value2))
				{
					return value2;
				}
				return value[nProj] = new List<ILibManItem>(LibraryHelpers.GetAllLibManItemsInProject(nProj, gdApp));
			}
			value = new Dictionary<int, List<ILibManItem>>();
			return value[nProj] = new List<ILibManItem>(LibraryHelpers.GetAllLibManItemsInProject(nProj, gdApp));
		}

		public IManagedLibrary GetManagedLib(ILibManItem lmi)
		{
			if (m_libItems.TryGetValue(lmi, out var value))
			{
				return value;
			}
			value = LibraryHelpers.GetManagedLibFromLibManItem(lmi);
			m_libItems[lmi] = value;
			return value;
		}
	}
}
