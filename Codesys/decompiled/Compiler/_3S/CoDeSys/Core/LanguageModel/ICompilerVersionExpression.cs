using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionExpression
	{
		Version VersionToTest { get; }

		Operator OpComparison { get; }
	}
}
