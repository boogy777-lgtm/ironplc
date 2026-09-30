using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000109 RID: 265
	[ReleasedInterface]
	public interface IProjectStructure5 : IProjectStructure4, IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x06000423 RID: 1059
		IEnumerable<IVirtualPSNodeAdditionResult<TVirtualPSNode>> AddMultipleObjects<TVirtualPSNode>(IEnumerable<IVirtualPSRoot<TVirtualPSNode>> roots, ObjectAdditionProgressHandler<TVirtualPSNode> additionHandler) where TVirtualPSNode : IVirtualPSNode;
	}
}
