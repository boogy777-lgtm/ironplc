using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities12 : IPreCompileUtilities11, IPreCompileUtilities10, IPreCompileUtilities9, IPreCompileUtilities8, IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		string PrependNamespaceToVariableTypename(int projHandle, Guid objGuid, IType type, string stVarName);
	}
}
