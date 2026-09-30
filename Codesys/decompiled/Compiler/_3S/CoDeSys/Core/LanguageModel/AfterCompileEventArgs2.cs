using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class AfterCompileEventArgs2 : AfterCompileEventArgs
	{
		private ILateCompileContext _lateCompileContext;

		public ILateCompileContext LateCompileContext => _lateCompileContext;

		public AfterCompileEventArgs2(Guid guidApplication, IMessage[] messages, bool bErrorsOccured, ILateCompileContext latecompilecontext)
			: base(guidApplication, messages, bErrorsOccured)
		{
			_lateCompileContext = latecompilecontext;
		}
	}
}
