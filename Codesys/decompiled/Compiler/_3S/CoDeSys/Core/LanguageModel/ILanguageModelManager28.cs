using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager28 : ILanguageModelManager27, ILanguageModelManager26, ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		int GetGranularity(IPreCompileContext precom, ICompiledType type);

		byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations);

		IExpressionTypifier CreateTypifier(Guid guidApplication, IScope scope, bool bContributeToCompile, bool bInterpretPragmas);

		IExternalReference[] GetExternalReferences(ICompileContext comcon, bool bCompactDownload);
	}
}
