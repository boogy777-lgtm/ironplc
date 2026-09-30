using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IWarningHelper2 : _IWarningHelper, ILMWarningConfiguration, ILMWarningConfiguration2
	{
		bool IsWarningAsError(MessageId messageId);

		ICollection<int> GetWarningAsErrorIds();

		void WriteWarningAsErrorIds(ICollection<int> hsWarningAsErrorIds);
	}
}
