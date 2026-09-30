using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeclarationInfo3 : IDeclarationInfo2, IDeclarationInfo
	{
		Guid GVLGuid { get; }
	}
}
