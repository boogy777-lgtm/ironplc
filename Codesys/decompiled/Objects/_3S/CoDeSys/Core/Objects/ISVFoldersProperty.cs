using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000133 RID: 307
	[ReleasedInterface]
	public interface ISVFoldersProperty : IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060004CA RID: 1226
		Guid[] StructuredViews { get; }

		// Token: 0x060004CB RID: 1227
		Guid GetParentObject(Guid structuredView);

		// Token: 0x060004CC RID: 1228
		bool SetParentObject(Guid structuredView, Guid parentObject);
	}
}
