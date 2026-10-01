using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000020 RID: 32
	[ReleasedInterface]
	public interface IProject7 : IProject6, IProject5, IProject4, IProject3, IProject2, IProject
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600009B RID: 155
		IEnumerable<IProjectAddOnInformation> AddOnsPresentDuringLastSave { get; }
	}
}
