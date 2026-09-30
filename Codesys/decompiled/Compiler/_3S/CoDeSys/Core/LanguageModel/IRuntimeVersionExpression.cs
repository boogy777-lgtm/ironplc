using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRuntimeVersionExpression : IExpression, IExprement
	{
		Version VersionToTest { get; }

		Operator OpComparison { get; }
	}
}
