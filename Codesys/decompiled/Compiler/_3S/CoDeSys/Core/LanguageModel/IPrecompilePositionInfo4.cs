using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompilePositionInfo4 : IPrecompilePositionInfo3, IPrecompilePositionInfo2, IPrecompilePositionInfo
	{
		Guid MessageGuid { get; }
	}
}
