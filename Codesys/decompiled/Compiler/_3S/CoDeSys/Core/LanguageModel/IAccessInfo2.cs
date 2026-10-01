using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAccessInfo2 : IAccessInfo
	{
		Guid ApplicationGuid { get; }

		Guid MessageGuid { get; }
	}
}
