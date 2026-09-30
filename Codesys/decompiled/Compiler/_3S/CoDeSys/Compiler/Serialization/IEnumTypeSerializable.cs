using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IEnumTypeSerializable
	{
		string Name { get; set; }

		void BeforeSerialize();

		void AfterDeserialize();
	}
}
