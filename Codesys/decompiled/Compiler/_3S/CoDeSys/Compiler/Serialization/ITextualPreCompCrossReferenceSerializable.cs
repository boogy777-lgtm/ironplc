using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ITextualPreCompCrossReferenceSerializable
	{
		string Name { get; }

		IDictionary<int, ICollection<int>> AccessingObjectsByMessageGuid { get; }
	}
}
