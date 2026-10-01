using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IInterfaceOffsetEntry : IVFTableEntry2, IVFTableEntry
	{
		new string Name { get; set; }

		bool CPP { get; set; }

		new int Id { get; }

		int HierarchyOffset { get; set; }

		int InstancePointerOffset { get; set; }
	}
}
