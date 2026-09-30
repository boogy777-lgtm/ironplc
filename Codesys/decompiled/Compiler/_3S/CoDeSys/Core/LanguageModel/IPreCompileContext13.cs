using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext13 : IPreCompileContext12, IPreCompileContext11, IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		IList<IPrecompilePositionInfo> GetDirectCrossReferencePositions(int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId);
	}
}
