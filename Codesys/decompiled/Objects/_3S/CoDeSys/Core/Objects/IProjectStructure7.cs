using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200010B RID: 267
	[ReleasedInterface]
	public interface IProjectStructure7 : IProjectStructure6, IProjectStructure5, IProjectStructure4, IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x06000427 RID: 1063
		IEnumerable<IVirtualPSNodeAdditionResult<TVirtualPSNode>> AddMultipleObjects<TVirtualPSNode>(IEnumerable<IVirtualPSRoot<TVirtualPSNode>> roots, ObjectAdditionProgressHandler<TVirtualPSNode> additionHandler, bool setPastedObject) where TVirtualPSNode : IVirtualPSNode;
	}
}
