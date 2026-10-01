using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000151 RID: 337
	[ReleasedInterface]
	public interface IOrderedSubObjects
	{
		// Token: 0x06000508 RID: 1288
		bool AcceptsChildObject(Type childObjectType, int nIndex);

		// Token: 0x06000509 RID: 1289
		int GetChildIndex(Guid subObjectGuid);

		// Token: 0x0600050A RID: 1290
		void AddChild(Guid subObjectGuid, int nIndex);

		// Token: 0x0600050B RID: 1291
		void RemoveChild(IMetaObject moRemoved);
	}
}
