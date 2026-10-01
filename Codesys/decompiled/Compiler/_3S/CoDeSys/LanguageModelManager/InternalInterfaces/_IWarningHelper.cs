using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IWarningHelper : ILMWarningConfiguration
	{
		bool IsWarningMessageDisabled(MessageId id);

		bool IsOEMDisabledId(MessageId id);

		bool IsWarning(MessageId messageId);

		bool IsError(MessageId messageId);

		ICollection<int> GetDisabledWarningIds();

		void WriteDisabledWarningIds(ICollection<int> hsDisabledWarningIds);
	}
}
