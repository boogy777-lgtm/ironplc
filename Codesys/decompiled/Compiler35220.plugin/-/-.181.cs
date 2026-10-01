using System;
using System.Collections.Generic;
using \u0003;
using \u0019;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x020001EE RID: 494
	internal sealed class \u0008
	{
		// Token: 0x060021A2 RID: 8610 RVA: 0x00073B18 File Offset: 0x00071D18
		internal \u0008()
		{
			this.\u0001 = new Dictionary<byte, \u0019.\u0005>();
		}

		// Token: 0x060021A3 RID: 8611 RVA: 0x00073B2C File Offset: 0x00071D2C
		internal \u0019.\u0005 \u0001(byte \u0002)
		{
			\u0019.\u0005 u = null;
			if (!this.\u0001.TryGetValue(\u0002, out u))
			{
				u = new \u0019.\u0005(\u0002);
				this.\u0001(u);
			}
			return u;
		}

		// Token: 0x17000658 RID: 1624
		// (get) Token: 0x060021A4 RID: 8612 RVA: 0x00073B5C File Offset: 0x00071D5C
		internal \u0019.\u0005 PersistentInstancesForCheckAllPoolObjects
		{
			get
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = new \u0019.\u0005();
				}
				return this.\u0001;
			}
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x00073B78 File Offset: 0x00071D78
		private void \u0001(\u0019.\u0005 \u0002)
		{
			this.\u0001[\u0002.TaskId] = \u0002;
		}

		// Token: 0x17000659 RID: 1625
		// (get) Token: 0x060021A6 RID: 8614 RVA: 0x00073B8C File Offset: 0x00071D8C
		internal IEnumerable<\u0019.\u0005> Instances
		{
			get
			{
				foreach (\u0019.\u0005 u in this.\u0001.Values)
				{
					yield return u;
				}
				Dictionary<byte, \u0019.\u0005>.ValueCollection.Enumerator enumerator = default(Dictionary<byte, \u0019.\u0005>.ValueCollection.Enumerator);
				if (this.\u0001 != null)
				{
					yield return this.\u0001;
				}
				yield break;
				yield break;
			}
		}

		// Token: 0x1700065A RID: 1626
		// (get) Token: 0x060021A7 RID: 8615 RVA: 0x00073B9C File Offset: 0x00071D9C
		// (set) Token: 0x060021A8 RID: 8616 RVA: 0x00073BA4 File Offset: 0x00071DA4
		internal _ISignature Signature
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x060021A9 RID: 8617 RVA: 0x00073BB0 File Offset: 0x00071DB0
		internal void \u0001()
		{
			this.\u0001 = new Dictionary<string, string>();
			if (this.\u0001 != null)
			{
				foreach (_IVariable ivariable in this.\u0001.AllVariables)
				{
					if (ivariable.HasAttribute("map_to"))
					{
						this.\u0001.Add(ivariable.GetAttributeValue("map_to").ToUpperInvariant(), ivariable.VersionedName);
					}
				}
			}
		}

		// Token: 0x060021AA RID: 8618 RVA: 0x00073C3C File Offset: 0x00071E3C
		internal string \u0001(global::\u0003.\u000E \u0002)
		{
			string text = null;
			string text2 = \u0002.InstancePath.ToUpperInvariant();
			this.\u0001.TryGetValue(text2, out text);
			if (text == null && text2.StartsWith("__POOL."))
			{
				text2 = text2.Remove(0, 7);
				this.\u0001.TryGetValue(text2, out text);
			}
			return text;
		}

		// Token: 0x040005BF RID: 1471
		private _ISignature \u0001;

		// Token: 0x040005C0 RID: 1472
		private readonly Dictionary<byte, \u0019.\u0005> \u0001;

		// Token: 0x040005C1 RID: 1473
		private \u0019.\u0005 \u0001;

		// Token: 0x040005C2 RID: 1474
		private Dictionary<string, string> \u0001;
	}
}
