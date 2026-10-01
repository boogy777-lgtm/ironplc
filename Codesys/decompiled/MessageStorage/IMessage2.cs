using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000003 RID: 3
	[ReleasedInterface]
	public interface IMessage2 : IMessage
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000008 RID: 8
		Color FontColor { get; }
	}
}
