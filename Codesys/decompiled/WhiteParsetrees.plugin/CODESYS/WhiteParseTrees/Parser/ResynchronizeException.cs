using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CODESYS.WhiteParseTrees.Parser
{
	[Serializable]
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ResynchronizeException : Exception
	{
		internal int StartStatementTokenIndex { get; }

		private string _Message { get; set; }

		internal IWhiteToken ErrorToken { get; set; }

		[System.Runtime.CompilerServices.Nullable(1)]
		public override string Message
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get
			{
				return _Message ?? string.Empty;
			}
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		protected ResynchronizeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_Message = base.Message;
		}

		internal ResynchronizeException(int startStatementTokenIndex)
		{
			StartStatementTokenIndex = startStatementTokenIndex;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		internal ResynchronizeException(int startStatementTokenIndex, string message, [System.Runtime.CompilerServices.Nullable(2)] IWhiteToken errorToken)
			: this(startStatementTokenIndex)
		{
			_Message = message;
			ErrorToken = errorToken;
		}
	}
}
