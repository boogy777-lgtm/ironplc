using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x020001AC RID: 428
	internal sealed class \u0004 : IEquatable<\u0004>
	{
		// Token: 0x06001EBD RID: 7869 RVA: 0x0006315C File Offset: 0x0006135C
		internal \u0004(_ICompilerMessage \u0095\u0003)
		{
			this.\u0001 = \u0095\u0003.MessageId;
			this.\u0001 = \u0095\u0003.Position;
			this.\u0001 = \u0095\u0003.PositionOffset;
			this.\u0001 = \u0095\u0003.Text;
			this.\u0001 = \u0095\u0003.Severity;
			this.\u0001 = \u0095\u0003.ObjectGuid;
		}

		// Token: 0x06001EBE RID: 7870 RVA: 0x000631B8 File Offset: 0x000613B8
		public bool \u0001(\u0004 \u0002)
		{
			return \u0002 != null && this.\u0001 == \u0002.\u0001 && this.\u0001 == \u0002.\u0001 && this.\u0001 == \u0002.\u0001 && this.\u0001 == \u0002.\u0001 && this.\u0001 == \u0002.\u0001 && this.\u0001 == \u0002.\u0001;
		}

		// Token: 0x06001EBF RID: 7871 RVA: 0x00063228 File Offset: 0x00061428
		public bool \u0001(object \u0002)
		{
			return this.\u0001(\u0002 as \u0004);
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x00063238 File Offset: 0x00061438
		public int \u0001()
		{
			return (((((60746322 * -1437678473 + ((int)this.\u0001).GetHashCode()) * -1437678473 + this.\u0001.GetHashCode()) * -1437678473 + this.\u0001.GetHashCode()) * -1437678473 + EqualityComparer<string>.Default.GetHashCode(this.\u0001)) * -1437678473 + ((int)this.\u0001).GetHashCode()) * -1437678473 + this.\u0001.GetHashCode();
		}

		// Token: 0x0400050E RID: 1294
		private readonly MessageId \u0001;

		// Token: 0x0400050F RID: 1295
		private readonly long \u0001;

		// Token: 0x04000510 RID: 1296
		private readonly short \u0001;

		// Token: 0x04000511 RID: 1297
		private readonly string \u0001;

		// Token: 0x04000512 RID: 1298
		private readonly Severity \u0001;

		// Token: 0x04000513 RID: 1299
		private readonly Guid \u0001;
	}
}
