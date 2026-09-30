using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DF RID: 223
	[ReleasedInterface]
	public interface IIncompleteDeserializationInfo
	{
		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000376 RID: 886
		bool UnknownObject { get; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000377 RID: 887
		Type Type { get; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000378 RID: 888
		string UnknownSerializableValueName { get; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000379 RID: 889
		string Warning { get; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600037A RID: 890
		string Cancellation { get; }
	}
}
