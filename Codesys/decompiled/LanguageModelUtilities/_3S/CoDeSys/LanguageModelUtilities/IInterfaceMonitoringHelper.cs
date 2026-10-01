using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IInterfaceMonitoringHelper
	{
		void ReadAllDataAreaAddresses(Guid guidApplication);

		bool GetAreaOffsetForAbsoluteAddess(Guid guidApplication, ulong ulAddress, out uint uiArea, out uint uiOffset);

		void DiscardAllDataAreaAddresses();

		string GetInterfaceInstancePath(Guid guidApplication, object interfaceValue, out bool bHidden);
	}
}
