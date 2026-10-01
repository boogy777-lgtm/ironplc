using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000108 RID: 264
	[ReleasedInterface]
	public interface IProjectStructure4 : IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x14000025 RID: 37
		// (add) Token: 0x0600041C RID: 1052
		// (remove) Token: 0x0600041D RID: 1053
		event EventHandler<PSQueryRollbackEventArgs> QueryRollback;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x0600041E RID: 1054
		// (remove) Token: 0x0600041F RID: 1055
		event EventHandler<PSAbortEventArgs> TransactionAborted;

		// Token: 0x06000420 RID: 1056
		IEnumerable<IVirtualPSNodeAdditionResult<TVirtualPSNode>> AddMultipleObjects<TVirtualPSNode>(IEnumerable<IVirtualPSRoot<TVirtualPSNode>> roots, ObjectWasAddedHandler<TVirtualPSNode> additionHandler) where TVirtualPSNode : IVirtualPSNode;

		// Token: 0x06000421 RID: 1057
		IVirtualPSRoot<TVirtualPSNode> CreateVirtualPSRoot<TVirtualPSNode>(IPSNode parent, TVirtualPSNode node) where TVirtualPSNode : IVirtualPSNode;

		// Token: 0x06000422 RID: 1058
		IVirtualPSNode CreateVirtualPSNode(Guid objectGuid, Type objectType, string stName, IEnumerable<IVirtualPSNode> childNodes, IObject obj, IObjectProperty[] properties);
	}
}
