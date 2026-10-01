using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class CompilerVersionChangingEventArgs : CompilerVersionEventArgsBase
	{
		private Exception _ex;

		public Exception Exception => _ex;

		public CompilerVersionChangingEventArgs(Version previousVersion, Version newVersion)
			: base(previousVersion, newVersion)
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
