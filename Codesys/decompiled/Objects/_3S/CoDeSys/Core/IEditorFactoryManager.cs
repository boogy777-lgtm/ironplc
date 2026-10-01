using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000013 RID: 19
	[ReleasedInterface]
	public interface IEditorFactoryManager
	{
		// Token: 0x0600004A RID: 74
		IEditorFactory[] GetFactories();

		// Token: 0x0600004B RID: 75
		IEditorFactory GetFactory(Guid factoryGuid);

		// Token: 0x0600004C RID: 76
		IEditorFactory[] GetFactories(Type objectType, Type[] embeddedObjectTypes);

		// Token: 0x0600004D RID: 77
		IEditorFactory GetDefaultFactory(Type objectType, Type[] embeddedObjectTypes);

		// Token: 0x0600004E RID: 78
		void SetDefaultFactory(Type objectType, Type[] embeddedObjectTypes, IEditorFactory factory);
	}
}
