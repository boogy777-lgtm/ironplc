using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000041 RID: 65
	[ReleasedInterface]
	public interface IEditorFactory
	{
		// Token: 0x06000117 RID: 279
		bool AcceptsObjectType(Type objectType, Type[] embeddedObjectTypes);

		// Token: 0x06000118 RID: 280
		IEditor Create();
	}
}
