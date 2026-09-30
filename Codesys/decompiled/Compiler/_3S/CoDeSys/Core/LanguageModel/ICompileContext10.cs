using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext10 : ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		int PointerSize { get; }

		IList<ICompiledPOU4> GetAllCompiledPOUsEx();

		IList<ICompiledPOU4> GetCompiledPOUsToCompileEx();

		IList<ISignature4> GetAllSignaturesFlatEx();

		IList<ICodePosition> GetReferencePositionsOfPOUEx(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId);

		IEnumerable<IInstancePathInfo> InstancePaths(ISignature sign, bool bWithDerivedFunctionBlocks);

		byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly);
	}
}
