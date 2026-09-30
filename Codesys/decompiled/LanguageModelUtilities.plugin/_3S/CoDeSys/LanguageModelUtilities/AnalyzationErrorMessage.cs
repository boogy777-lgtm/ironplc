using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class AnalyzationErrorMessage : IMessage
	{
		public short Length { get; }

		public Guid ObjectGuid { get; }

		public long Position { get; }

		public short PositionOffset { get; }

		public int ProjectHandle { get; }

		public Severity Severity => Severity.Error;

		public string Text { get; }

		public AnalyzationErrorMessage(ISignature declaringSign, IExpression exp, string stText)
		{
			ProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			ObjectGuid = ((declaringSign.MessageGuid != Guid.Empty) ? declaringSign.MessageGuid : declaringSign.ObjectGuid);
			Position = exp.Position.Position;
			PositionOffset = exp.Position.PositionOffset;
			Length = exp.Position.Length;
			Text = stText;
		}
	}
}
