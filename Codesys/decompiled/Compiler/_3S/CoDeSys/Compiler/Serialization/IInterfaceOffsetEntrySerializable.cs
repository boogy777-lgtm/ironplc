using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IInterfaceOffsetEntrySerializable
	{
		int Id { get; set; }

		string Name { get; set; }

		bool CPP { get; set; }

		int HierarchyOffset { get; set; }

		int InstancePointerOffset { get; set; }
	}
}
