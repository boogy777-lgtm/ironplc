using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000009 RID: 9
	[ReleasedInterface]
	public interface IMessageCategory2 : IMessageCategory
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000014 RID: 20
		bool DeleteOnPrimaryProjectSwitch { get; }
	}
}
