using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class SetLibraryPreCompileContextCompletionPostProcessEventArgs : EventArgs
	{
		private string _stLibraryId;

		private bool _bLast;

		private object _callerData;

		private ParameterlessDelegate _postProcessDoneFunc;

		private ParameterlessDelegate _handledFunc;

		private ExitImmediatelyDelegate _exitImmediatelyFunc;

		public string LibraryId => _stLibraryId;

		public bool Last => _bLast;

		public object CallerData => _callerData;

		public SetLibraryPreCompileContextCompletionPostProcessEventArgs(string stLibraryId, bool bLast, object callerData, ParameterlessDelegate postProcessDoneFunc, ParameterlessDelegate handledFunc, ExitImmediatelyDelegate exitImmediatelyFunc)
		{
			_stLibraryId = stLibraryId;
			_bLast = bLast;
			_callerData = callerData;
			_postProcessDoneFunc = postProcessDoneFunc;
			_handledFunc = handledFunc;
			_exitImmediatelyFunc = exitImmediatelyFunc;
		}

		public void PostProcessDone()
		{
			_postProcessDoneFunc();
		}

		public void Handled()
		{
			_handledFunc();
		}

		public bool ExitImmediately()
		{
			return _exitImmediatelyFunc();
		}
	}
}
