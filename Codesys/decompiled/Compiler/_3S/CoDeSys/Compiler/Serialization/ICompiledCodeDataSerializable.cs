using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompiledCodeDataSerializable : ICompiledCodeSerializable
	{
		int RelatedId { get; set; }
	}
}
