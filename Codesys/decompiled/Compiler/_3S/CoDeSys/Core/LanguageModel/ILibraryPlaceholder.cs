using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholder
	{
		string Name { get; }

		string Namespace { get; }

		string DefaultLibraryId { get; }

		bool PublishSymbols { get; }

		Guid Resolver { get; }
	}
}
