using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class LibraryContextDeletedEventArgs : EventArgs
	{
		public string LibraryId { get; private set; }

		public LibraryContextDeletedEventArgs(string stLibraryId)
		{
			LibraryId = stLibraryId ?? "";
		}
	}
}
