using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000134 RID: 308
	[ReleasedInterface]
	public interface IArchivable
	{
		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060004CD RID: 1229
		string[] SerializableValueNames { get; }

		// Token: 0x060004CE RID: 1230
		object GetSerializableValue(string stValueName);

		// Token: 0x060004CF RID: 1231
		void SetSerializableValue(string stValueName, object value);

		// Token: 0x060004D0 RID: 1232
		void BeforeSerialize();

		// Token: 0x060004D1 RID: 1233
		void AfterDeserialize();
	}
}
