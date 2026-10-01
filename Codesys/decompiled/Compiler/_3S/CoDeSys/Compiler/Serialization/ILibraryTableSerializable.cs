using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ILibraryTableSerializable
	{
		IEnumerable<ILibInfoSerializable> LibInfoSerializable { get; set; }
	}
}
