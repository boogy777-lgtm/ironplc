using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMObsoleteService
	{
		IScope CreateScope(ISignature isign, ICollection Signatures, Guid guidApplication);

		[Obsolete("Use ILMPreCompileService.IsHiddenVariable(ISignature6, IVariable, GUIHidingFlags) instead")]
		bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider);
	}
}
