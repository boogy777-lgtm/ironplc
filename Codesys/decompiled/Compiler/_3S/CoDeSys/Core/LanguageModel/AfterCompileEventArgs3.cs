using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class AfterCompileEventArgs3 : AfterCompileEventArgs2
	{
		private bool _bUpToDate;

		public bool UpToDate => _bUpToDate;

		public AfterCompileEventArgs3(Guid guidApplication, IMessage[] messages, bool bErrorsOccured, ILateCompileContext latecompilecontext, bool bUpToDate)
			: base(guidApplication, messages, bErrorsOccured, latecompilecontext)
		{
			_bUpToDate = bUpToDate;
		}
	}
}
