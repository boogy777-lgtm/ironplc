using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000014 RID: 20
	[ReleasedInterface]
	public interface IEditorManager
	{
		// Token: 0x0600004F RID: 79
		IEditor[] GetEditors();

		// Token: 0x06000050 RID: 80
		IEditor[] GetEditors(int nProjectHandle, Guid objectGuid);

		// Token: 0x06000051 RID: 81
		IEditor[] GetEditors(IMetaObject metaObject);

		// Token: 0x06000052 RID: 82
		void AddEditor(IEditor editor);

		// Token: 0x06000053 RID: 83
		void SaveEditor(IEditor editor, bool bCommit);

		// Token: 0x06000054 RID: 84
		void SaveAllEditors(bool bCommit);

		// Token: 0x06000055 RID: 85
		void CloseEditor(IEditor editor);
	}
}
