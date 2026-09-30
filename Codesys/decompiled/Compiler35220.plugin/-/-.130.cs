using System;
using System.Runtime.CompilerServices;
using System.Xml;
using _3S.CoDeSys.Utilities;

namespace \u0005
{
	// Token: 0x0200017B RID: 379
	internal sealed class \u0003
	{
		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x060019BC RID: 6588 RVA: 0x000504C8 File Offset: 0x0004E6C8
		// (set) Token: 0x060019BD RID: 6589 RVA: 0x000504D0 File Offset: 0x0004E6D0
		public string Name { get; set; }

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x060019BE RID: 6590 RVA: 0x000504DC File Offset: 0x0004E6DC
		// (set) Token: 0x060019BF RID: 6591 RVA: 0x000504E4 File Offset: 0x0004E6E4
		public string Text { get; set; }

		// Token: 0x060019C0 RID: 6592 RVA: 0x000504F0 File Offset: 0x0004E6F0
		public void \u0001(string \u0002, string \u0003)
		{
			this.Attributes.Add(\u0002, \u0003);
		}

		// Token: 0x060019C1 RID: 6593 RVA: 0x00050500 File Offset: 0x0004E700
		public string \u0001(string \u0002)
		{
			string result = null;
			this.Attributes.TryGetValue(\u0002, ref result);
			return result;
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x00050520 File Offset: 0x0004E720
		public bool \u0001(string \u0002, ref string \u0003)
		{
			string text;
			if (!this.Attributes.TryGetValue(\u0002, ref text))
			{
				return false;
			}
			\u0003 = text;
			return true;
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x00050544 File Offset: 0x0004E744
		public bool \u0001(string \u0002, ref int \u0003)
		{
			string s;
			if (!this.Attributes.TryGetValue(\u0002, ref s))
			{
				return false;
			}
			\u0003 = XmlConvert.ToInt32(s);
			return true;
		}

		// Token: 0x060019C4 RID: 6596 RVA: 0x0005056C File Offset: 0x0004E76C
		public bool \u0001(string \u0002, ref bool \u0003)
		{
			string s;
			if (!this.Attributes.TryGetValue(\u0002, ref s))
			{
				return false;
			}
			\u0003 = XmlConvert.ToBoolean(s);
			return true;
		}

		// Token: 0x060019C5 RID: 6597 RVA: 0x00050594 File Offset: 0x0004E794
		public bool \u0001(string \u0002, ref Guid \u0003)
		{
			string g;
			if (!this.Attributes.TryGetValue(\u0002, ref g))
			{
				return false;
			}
			\u0003 = new Guid(g);
			return true;
		}

		// Token: 0x17000599 RID: 1433
		public \u0003 this[int \u0002]
		{
			get
			{
				if (this.ChildNodes.Count <= \u0002)
				{
					return null;
				}
				return this.ChildNodes[\u0002];
			}
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x000505E0 File Offset: 0x0004E7E0
		public \u0003 \u0001(string \u0002)
		{
			foreach (\u0003 u in this.ChildNodes)
			{
				if (u.Name == \u0002)
				{
					return u;
				}
			}
			return null;
		}

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x060019C8 RID: 6600 RVA: 0x0005063C File Offset: 0x0004E83C
		public LList<\u0003> ChildNodes { get; } = new LList<\u0003>();

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x060019C9 RID: 6601 RVA: 0x00050644 File Offset: 0x0004E844
		public LDictionary<string, string> Attributes { get; } = new LDictionary<string, string>();

		// Token: 0x060019CA RID: 6602 RVA: 0x0005064C File Offset: 0x0004E84C
		public void \u0001(\u0003 \u0002)
		{
			this.ChildNodes.Add(\u0002);
			\u0002.Parent = this;
		}

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x060019CB RID: 6603 RVA: 0x00050664 File Offset: 0x0004E864
		// (set) Token: 0x060019CC RID: 6604 RVA: 0x0005066C File Offset: 0x0004E86C
		public \u0003 Parent { get; set; }

		// Token: 0x04000476 RID: 1142
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x04000477 RID: 1143
		[CompilerGenerated]
		private string \u0002;

		// Token: 0x04000478 RID: 1144
		[CompilerGenerated]
		private readonly LList<\u0003> \u0001;

		// Token: 0x04000479 RID: 1145
		[CompilerGenerated]
		private readonly LDictionary<string, string> \u0001;

		// Token: 0x0400047A RID: 1146
		[CompilerGenerated]
		private \u0003 \u0001;
	}
}
