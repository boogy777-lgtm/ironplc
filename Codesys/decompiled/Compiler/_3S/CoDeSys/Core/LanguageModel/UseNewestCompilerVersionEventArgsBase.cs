using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public abstract class UseNewestCompilerVersionEventArgsBase : EventArgs
	{
		private bool _bOldValue;

		private bool _bNewValue;

		public bool UseNewestCompilerVersionBeforeChange => _bOldValue;

		public bool UseNewestCompilerVersionAfterChange => _bNewValue;

		public UseNewestCompilerVersionEventArgsBase(bool bOldValue, bool bNewValue)
		{
			_bOldValue = bOldValue;
			_bNewValue = bNewValue;
		}
	}
}
