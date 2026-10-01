using System;
using System.Reflection;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000139 RID: 313
	public class ObjectIdentification
	{
		// Token: 0x06001AB8 RID: 6840 RVA: 0x0004C2CF File Offset: 0x0004B2CF
		public ObjectIdentification(Guid guidObject, int nProjectHandle)
		{
			this._guidObject = guidObject;
			this._ProjectHandle = nProjectHandle;
		}

		// Token: 0x06001AB9 RID: 6841 RVA: 0x0004C2F7 File Offset: 0x0004B2F7
		public ObjectIdentification()
		{
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x0004C311 File Offset: 0x0004B311
		public override int GetHashCode()
		{
			return this._guidObject.GetHashCode() ^ this._ProjectHandle.GetHashCode();
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x0004C330 File Offset: 0x0004B330
		public override bool Equals(object obj)
		{
			if (obj is ObjectIdentification)
			{
				ObjectIdentification objectIdentification = obj as ObjectIdentification;
				return objectIdentification._guidObject == this._guidObject && objectIdentification._ProjectHandle == this._ProjectHandle;
			}
			return false;
		}

		// Token: 0x04000593 RID: 1427
		[Obfuscation(Feature = "rename")]
		public Guid _guidObject = Guid.Empty;

		// Token: 0x04000594 RID: 1428
		[Obfuscation(Feature = "rename")]
		public int _ProjectHandle = -1;
	}
}
