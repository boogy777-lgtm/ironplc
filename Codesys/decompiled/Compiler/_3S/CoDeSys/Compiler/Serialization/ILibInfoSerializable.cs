using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ILibInfoSerializable
	{
		string LibraryId { get; set; }

		string Namespace { get; set; }

		string ReferencingLibrary { get; set; }

		bool OutOfPool { get; set; }

		int Id { get; set; }

		bool QualifiedOnly { get; set; }

		bool PublishSymbols { get; set; }
	}
}
