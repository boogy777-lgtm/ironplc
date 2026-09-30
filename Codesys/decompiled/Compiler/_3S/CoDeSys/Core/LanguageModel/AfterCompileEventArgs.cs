using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AfterCompileEventArgs : CompileEventArgs
	{
		private IMessage[] _messages;

		private bool _bErrorsOccured;

		public bool ErrorsOccured => _bErrorsOccured;

		public IMessage[] Messages => _messages;

		public AfterCompileEventArgs(Guid guidApplication, IMessage[] messages, bool bErrorsOccured)
			: base(guidApplication)
		{
			_messages = messages;
			_bErrorsOccured = bErrorsOccured;
		}
	}
}
