using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DE RID: 222
	[ReleasedInterface]
	public interface ICompiledLibraryAmendableMetaObject
	{
		// Token: 0x06000375 RID: 885
		IMetaObject CloneWithAdditionalProperties(IEnumerable<IObjectProperty> properties);
	}
}
