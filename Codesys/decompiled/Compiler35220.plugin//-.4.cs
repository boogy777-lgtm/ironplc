using System;
using _3S.CoDeSys.Utilities;

namespace \u0082
{
	// Token: 0x020000DD RID: 221
	internal sealed class \u0004
	{
		// Token: 0x06000FB3 RID: 4019 RVA: 0x0002B688 File Offset: 0x00029888
		internal void \u0001(string \u0002)
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001.Add(\u0002);
			}
		}

		// Token: 0x06000FB4 RID: 4020 RVA: 0x0002B6D0 File Offset: 0x000298D0
		internal LList<string> \u0001()
		{
			object u = this.\u0001;
			LList<string> result;
			lock (u)
			{
				LList<string> u2 = this.\u0001;
				this.\u0001 = new LList<string>();
				result = u2;
			}
			return result;
		}

		// Token: 0x06000FB5 RID: 4021 RVA: 0x0002B720 File Offset: 0x00029920
		internal void \u0001(Exception \u0002)
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001.Add(\u0002);
			}
		}

		// Token: 0x06000FB6 RID: 4022 RVA: 0x0002B768 File Offset: 0x00029968
		internal Exception[] \u0001()
		{
			object u = this.\u0001;
			Exception[] result;
			lock (u)
			{
				result = this.\u0001.ToArray();
			}
			return result;
		}

		// Token: 0x040002AC RID: 684
		private LList<string> \u0001 = new LList<string>();

		// Token: 0x040002AD RID: 685
		private LList<Exception> \u0001 = new LList<Exception>();

		// Token: 0x040002AE RID: 686
		private object \u0001 = new object();
	}
}
