using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class UseNewestCompilerVersionChangingEventArgs : UseNewestCompilerVersionEventArgsBase
	{
		private Exception _ex;

		public Exception Exception => _ex;

		public UseNewestCompilerVersionChangingEventArgs(bool bOldValue, bool bNewValue)
			: base(bOldValue, bNewValue)
		{
		}

		public void Cancel(Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}
			if (_ex == null)
			{
				_ex = ex;
			}
		}
	}
}
