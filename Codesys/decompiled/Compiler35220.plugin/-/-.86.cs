using System;
using System.Diagnostics;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0003
{
	// Token: 0x02000113 RID: 275
	[DebuggerDisplay("{GetStringRepresentation()}")]
	internal sealed class \u0003 : IChangedLMObject2, IChangedLMObject
	{
		// Token: 0x06001436 RID: 5174 RVA: 0x0003B2C4 File Offset: 0x000394C4
		internal \u0003(string \u0017\u0002, Operator \u001A\u0005, string \u001B\u0005, EPouSetChange \u001C\u0005)
		{
			this.\u0001 = \u0017\u0002;
			this.\u0001 = \u001A\u0005;
			this.\u0002 = \u001B\u0005;
			this.\u0001 = \u001C\u0005;
			this.\u0003 = string.Empty;
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x06001437 RID: 5175 RVA: 0x0003B2F4 File Offset: 0x000394F4
		public string Name
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x06001438 RID: 5176 RVA: 0x0003B2FC File Offset: 0x000394FC
		public Operator POUType
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x06001439 RID: 5177 RVA: 0x0003B304 File Offset: 0x00039504
		public string Description
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x0600143A RID: 5178 RVA: 0x0003B30C File Offset: 0x0003950C
		public EPouSetChange Change
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x0600143B RID: 5179 RVA: 0x0003B314 File Offset: 0x00039514
		public bool OnlineChangePossible
		{
			get
			{
				return ((EPouSetChange)(-1) & this.\u0001) == EPouSetChange.Undefined;
			}
		}

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x0600143C RID: 5180 RVA: 0x0003B324 File Offset: 0x00039524
		// (set) Token: 0x0600143D RID: 5181 RVA: 0x0003B32C File Offset: 0x0003952C
		public string ChildName
		{
			get
			{
				return this.\u0003;
			}
			internal set
			{
				this.\u0003 = value;
			}
		}

		// Token: 0x0600143E RID: 5182 RVA: 0x0003B338 File Offset: 0x00039538
		public int \u0001()
		{
			return this.\u0004().GetHashCode();
		}

		// Token: 0x0600143F RID: 5183 RVA: 0x0003B348 File Offset: 0x00039548
		public bool \u0001(object \u0002)
		{
			\u0003 u = \u0002 as \u0003;
			return u != null && this.\u0004().Equals(u.\u0004());
		}

		// Token: 0x06001440 RID: 5184 RVA: 0x0003B374 File Offset: 0x00039574
		private string \u0004()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((this.\u0001 == null) ? string.Empty : this.\u0001);
			stringBuilder.Append("\t");
			stringBuilder.Append(this.\u0001);
			stringBuilder.Append("\t");
			stringBuilder.Append(this.\u0002);
			stringBuilder.Append("\t");
			stringBuilder.Append(this.\u0001.ToString());
			return stringBuilder.ToString();
		}

		// Token: 0x04000375 RID: 885
		private string \u0001;

		// Token: 0x04000376 RID: 886
		private Operator \u0001;

		// Token: 0x04000377 RID: 887
		private string \u0002;

		// Token: 0x04000378 RID: 888
		private EPouSetChange \u0001;

		// Token: 0x04000379 RID: 889
		private string \u0003;
	}
}
