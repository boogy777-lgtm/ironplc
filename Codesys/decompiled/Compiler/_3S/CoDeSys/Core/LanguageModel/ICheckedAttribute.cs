using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICheckedAttribute : IAttribute
	{
		string Description { get; }

		Version RequiredCompilerVersion { get; }

		AttributeScope Scope { get; }

		bool CheckValue(string value, AttributeScope scope, ISignature signature, IVariable variable, out string error);
	}
}
