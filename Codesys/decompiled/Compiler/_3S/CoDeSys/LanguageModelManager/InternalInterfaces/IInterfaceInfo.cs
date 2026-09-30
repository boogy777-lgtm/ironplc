using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IInterfaceInfo
	{
		int InterfaceId { get; set; }

		string OrgName { get; set; }

		int OwnIndex { get; set; }

		int ParentInterfaceIndex { get; set; }

		int DerivedInterfaceIndex { get; set; }

		bool IsBaseInterfaceInfo { get; set; }

		bool IsEqualParent { get; set; }

		bool NoInit { get; set; }
	}
}
