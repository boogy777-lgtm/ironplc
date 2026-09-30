using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INamespaceConflictChecker
	{
		IEnumerable<INamespaceConflictIssue> CheckConflictingNamespace(string stNamespace);
	}
}
