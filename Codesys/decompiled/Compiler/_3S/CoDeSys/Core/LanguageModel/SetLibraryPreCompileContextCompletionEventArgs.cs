using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class SetLibraryPreCompileContextCompletionEventArgs : EventArgs
	{
		private string _stLibraryId;

		private bool _bLast;

		private object _callerData;

		public string LibraryId => _stLibraryId;

		public bool Last => _bLast;

		public object CallerData => _callerData;

		public SetLibraryPreCompileContextCompletionEventArgs(string stLibraryId, bool bLast, object callerData)
		{
			_stLibraryId = stLibraryId;
			_bLast = bLast;
			_callerData = callerData;
		}
	}
}
