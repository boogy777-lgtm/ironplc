using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHelper4 : ICompilerHelper3, ICompilerHelper2, ICompilerHelper
	{
		IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature);

		IEnumerable<uint> CreateChecksumListByVariableOffsetsForSignature(ISignature sign, Guid appGuid);
	}
}
