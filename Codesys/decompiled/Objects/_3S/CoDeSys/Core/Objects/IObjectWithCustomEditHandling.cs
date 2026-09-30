using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000FA RID: 250
	[ReleasedInterface]
	public interface IObjectWithCustomEditHandling
	{
		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060003EB RID: 1003
		bool PreventImmediateEditing { get; }
	}
}
