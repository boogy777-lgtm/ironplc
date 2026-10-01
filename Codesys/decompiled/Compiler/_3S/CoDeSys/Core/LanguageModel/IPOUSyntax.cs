using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPOUSyntax
	{
		_ISequenceStatement Declaration { get; }

		_ISequenceStatement Implementation { get; }

		IEnumerable<IPOUSyntax> SubPOUs { get; }
	}
}
