using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITaskStackSizeProvider
	{
		uint GetOverriddenStackSize(int nProjectHandle, Guid guidApplication);
	}
}
