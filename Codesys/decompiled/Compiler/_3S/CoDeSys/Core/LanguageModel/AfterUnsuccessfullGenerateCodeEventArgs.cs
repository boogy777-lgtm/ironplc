using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AfterUnsuccessfullGenerateCodeEventArgs : CompileEventArgs
	{
		public bool ErrorsOccured { get; }

		public IMessage[] Messages { get; }

		public AfterUnsuccessfullGenerateCodeEventArgs(Guid guidApplication, IMessage[] messages, bool bErrorsOccured)
			: base(guidApplication)
		{
			Messages = messages;
			ErrorsOccured = bErrorsOccured;
		}
	}
}
