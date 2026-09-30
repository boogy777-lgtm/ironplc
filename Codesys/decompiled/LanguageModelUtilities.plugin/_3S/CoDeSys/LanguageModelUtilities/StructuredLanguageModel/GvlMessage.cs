using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	internal sealed class GvlMessage
	{
		internal string Message { get; private set; }

		internal Severity Severity { get; private set; }

		internal Guid ObjectGuid { get; private set; }

		internal IExprementPosition Position { get; private set; }

		internal short Length { get; private set; }

		internal GvlMessage(string stMessage, Severity severity, Guid gdObject, IExprementPosition exprementPosition, short sLength)
		{
			Message = stMessage;
			Severity = severity;
			ObjectGuid = gdObject;
			Position = exprementPosition;
			Length = sLength;
		}
	}
}
