using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext3 : ICompileContext2, ICompileContext, ICompileContextCommon
	{
		[Obsolete("This feature is deprecated.")]
		IAuxiliaryCompileInformationList AuxiliaryCompileInformationList { get; }
	}
}
