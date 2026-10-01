using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200010D RID: 269
	[ReleasedInterface]
	public interface IProjectStructure9 : IProjectStructure8, IProjectStructure7, IProjectStructure6, IProjectStructure5, IProjectStructure4, IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x0600042A RID: 1066
		IEnumerable<IVirtualPSNodeAdditionResult<TVirtualPSNode>> AddMultipleObjects<TVirtualPSNode>(IEnumerable<IVirtualPSRoot<TVirtualPSNode>> roots, ObjectAdditionProgressHandler<TVirtualPSNode> additionHandler, bool setPastedObject, ActionContext context) where TVirtualPSNode : IVirtualPSNode;
	}
}
