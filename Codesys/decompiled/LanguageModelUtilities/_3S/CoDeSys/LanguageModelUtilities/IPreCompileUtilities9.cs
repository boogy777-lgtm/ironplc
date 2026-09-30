using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities9 : IPreCompileUtilities8, IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		IEvaluationContext4 CreateContext4(int nProj, int nAttrProj, Guid gdScope, Guid gdLocalScope, IGetLibInformation libInfo);

		IEvaluationContext4 GetContextFromSignature(int nProjAttracting, ISignature sig, ISignature sigLocal, IGetLibInformation libInfo);

		string MakeValidIdentifier(string stInput, bool bAllowUnicodeCharacters);
	}
}
