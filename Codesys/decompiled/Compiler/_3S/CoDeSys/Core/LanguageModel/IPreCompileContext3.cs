using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext3 : IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		[Obsolete("This feature is deprecated.")]
		IAuxiliaryCompileInformationList AuxiliaryCompileInformationList { get; }

		IPreCompileContext3 GetLibraryByNamespace(string stNamespace, ITargetSettings tarset);

		string GetNamespaceOfLibrary(IPreCompileContext precom, ITargetSettings tarset);
	}
}
