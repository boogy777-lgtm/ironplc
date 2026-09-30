using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IInterfaceOnlineVarRef : IReferencingOnlineVarRef, IOnlineVarRef
	{
		IInterfaceMonitoringHelper InterfaceMonitoringHelper { get; }

		EInterfaceOnlineVarRefState InterfaceOnlineVarRefState { get; }

		string StaticInstancePath { get; }
	}
}
