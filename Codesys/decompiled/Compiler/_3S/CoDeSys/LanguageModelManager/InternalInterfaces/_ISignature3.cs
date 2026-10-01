using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISignature3 : _ISignature2, _ISignature, ISignature7, ISignature6, ISignature5, ISignature4, ISignature3, ISignature2, ISignature
	{
		IEnumerable<ISourcePosition> UnusedDeclarationPositions { get; set; }

		void SetAllVariablesWithoutSideEffects(IList<_IVariable> list);
	}
}
