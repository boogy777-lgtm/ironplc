using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ITextualPreCompCrossReferencesSerializable
	{
		IList<Guid> SharedGuidTable { get; }

		IEnumerable<ITextualPreCompCrossReferenceSerializable> CrossReferences { get; }
	}
}
