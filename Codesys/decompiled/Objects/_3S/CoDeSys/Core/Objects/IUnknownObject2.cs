using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000156 RID: 342
	[ReleasedInterface]
	public interface IUnknownObject2 : IUnknownObject, IObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000513 RID: 1299
		string DeserializationIncompleteWarning { get; }

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06000514 RID: 1300
		string DeserializationIncompleteButtonText { get; }

		// Token: 0x06000515 RID: 1301
		void DeserializationIncompleteButtonAction();
	}
}
