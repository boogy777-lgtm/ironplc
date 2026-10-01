using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICrossReferenceSerializable
	{
		int CodeId { get; set; }
	}
}
