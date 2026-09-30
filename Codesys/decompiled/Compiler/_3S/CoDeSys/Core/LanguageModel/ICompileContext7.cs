using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext7 : ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		[Obsolete("Use ICompileContext10.GetAllCompiledPOUsEx instead")]
		List<ICompiledPOU4> GetAllCompiledPOUs();

		[Obsolete("Use ICompileContext10.GetCompiledPOUsToCompileEx instead")]
		List<ICompiledPOU4> GetCompiledPOUsToCompile();

		[Obsolete("Use ICompileContext10.GetAllSignaturesFlatEx instead")]
		List<ISignature4> GetAllSignaturesFlat();
	}
}
