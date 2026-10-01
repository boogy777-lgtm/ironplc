using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompilerVersionSettings
	{
		Version CompilerVersionToUseInternal();

		string MapFromInternalToOEMTextSave(Version compilerVersion);

		string MapFromInternalToOEMText(Version theEvilVersion);
	}
}
