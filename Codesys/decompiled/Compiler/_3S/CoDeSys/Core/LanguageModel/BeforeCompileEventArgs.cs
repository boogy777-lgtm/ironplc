using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class BeforeCompileEventArgs : CompileEventArgs
	{
		private List<IMessage> _messages = new List<IMessage>();

		public IEnumerable<IMessage> Messages => _messages;

		public BeforeCompileEventArgs(Guid guidApplication)
			: base(guidApplication)
		{
		}

		public void AddMessage(IMessage msg)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			_messages.Add(msg);
		}
	}
}
