using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013F RID: 319
	[ReleasedInterface]
	public interface IEmbeddedObject : IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060004E1 RID: 1249
		// (set) Token: 0x060004E2 RID: 1250
		IObject OwnerObject { get; set; }

		// Token: 0x060004E3 RID: 1251
		string GetPositionText(long nPosition);

		// Token: 0x060004E4 RID: 1252
		string GetContentString(ref long nPosition, ref int nLength, bool bWord);
	}
}
