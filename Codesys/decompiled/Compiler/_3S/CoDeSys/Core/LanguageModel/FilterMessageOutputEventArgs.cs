using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class FilterMessageOutputEventArgs : MessageOutputEventArgs
	{
		private IMessageCategory _messageCategory;

		private IMessage _message;

		private bool _bIgnore;

		public IMessageCategory MessageCategory => _messageCategory;

		public IMessage Message => _message;

		public bool Ignore
		{
			get
			{
				return _bIgnore;
			}
			set
			{
				_bIgnore = value;
			}
		}

		public FilterMessageOutputEventArgs(IMessageCategory messageCategory, IMessage message)
		{
			_messageCategory = messageCategory;
			_message = message;
		}
	}
}
