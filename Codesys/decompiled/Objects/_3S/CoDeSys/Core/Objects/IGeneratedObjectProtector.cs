using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000141 RID: 321
	[ReleasedInterface]
	public interface IGeneratedObjectProtector
	{
		// Token: 0x060004E5 RID: 1253
		void CheckPermissionToModify(object editor, MetaObjectCancelEventArgs args);
	}
}
