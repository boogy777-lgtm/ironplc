using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013E RID: 318
	[ReleasedInterface]
	public interface IDocumentationProperty : IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060004DF RID: 1247
		// (set) Token: 0x060004E0 RID: 1248
		string Documentation { get; set; }
	}
}
