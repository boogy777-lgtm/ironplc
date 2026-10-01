using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext4 : ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		[Obsolete("Use ICompileContext10.GetReferencePositionsOfPOUEx instead")]
		List<ICodePosition> GetReferencePositionsOfPOU(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId);
	}
}
