using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IInterfaceHierarchy
	{
		IInterfaceInfo this[int index] { get; }

		IEnumerable<IInterfaceInfo> Interfaces { get; }

		IEnumerable<IInterfaceInfo> GetInterfacesInVFTableOrder();

		string GetUniqueInterfaceVariableName(IInterfaceInfo ii);
	}
}
