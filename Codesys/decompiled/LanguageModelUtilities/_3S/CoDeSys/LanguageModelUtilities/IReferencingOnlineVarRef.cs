using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IReferencingOnlineVarRef : IOnlineVarRef
	{
		event OnlineVarRefEventHandler ValueChanged;

		event OnlineVarRefEventHandler ReferencedInstanceAvailable;

		void Initialize(IInterfaceMonitoringHelper IInterfaceMonitoringHelper, IVarRef owningVarRef, Guid gdApplication);

		bool DetermineInstanceSignatureIfPossible(bool bCheckAddressChange, out bool bAddressChanged, out ulong ulAddressInstance, out ISignature signInstance);
	}
}
