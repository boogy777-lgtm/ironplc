using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IOnlineChangeDetails3 : IOnlineChangeDetails2, IOnlineChangeDetails
	{
		IDictionary<string, IVariableInfo> InterfacesToRelink { get; }

		IDictionary<string, IVariableInfo> InstancesToMove { get; }

		long TotalNumberOfRelinkTests { get; }
	}
}
