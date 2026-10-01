using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0080
{
	// Token: 0x020000C2 RID: 194
	internal sealed class \u0005
	{
		// Token: 0x06000E91 RID: 3729 RVA: 0x00027FBC File Offset: 0x000261BC
		internal static string[] \u0001(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
			{
				\u0001 = \u0006,
				\u0002 = \u0007,
				\u0003 = \u0008
			};
			LList<_IVariable> llist = new LList<_IVariable>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			LList<string> llist3 = new LList<string>();
			\u0080.\u0005.\u0001(\u0002, \u0003, llist3, llist, llist2, u);
			\u0004 = llist.OfType<IVariable>().ToArray<IVariable>();
			\u0005 = llist2.OfType<ISignature>().ToArray<ISignature>();
			return llist3.ToArray();
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x00028024 File Offset: 0x00026224
		internal static string[] \u0002(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
			{
				\u0001 = \u0006,
				\u0002 = \u0007,
				\u0003 = \u0008,
				\u0006 = true
			};
			LList<_IVariable> llist = new LList<_IVariable>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			LList<string> llist3 = new LList<string>();
			\u0080.\u0005.\u0001(\u0002, \u0003, llist3, llist, llist2, u);
			\u0004 = llist.OfType<IVariable>().ToArray<IVariable>();
			\u0005 = llist2.OfType<ISignature>().ToArray<ISignature>();
			return llist3.ToArray();
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x00028094 File Offset: 0x00026294
		private static void \u0001(_ICompileContext \u0002, ISignature \u0003, LList<string> \u0004, LList<_IVariable> \u0005, LList<_ISignature> \u0006, \u0080.\u0005.\u0001 \u0007)
		{
			if (\u0003 == null)
			{
				return;
			}
			LList<string> llist = InstancePathService.\u0001(\u0002, \u0003 as _ISignature, Array.Empty<int>(), \u0080.\u0005.\u0001(), \u0005, \u0006, \u0007);
			\u0004.AddRange(llist);
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x000280C8 File Offset: 0x000262C8
		internal static IEnumerable<IInstancePathInfo> \u0001(_ICompileContext \u0002, ISignature \u0003, bool \u0004)
		{
			LList<_IVariable> llist = new LList<_IVariable>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			LList<string> llist3 = InstancePathService.\u0001(\u0002, \u0003 as _ISignature, Array.Empty<int>(), llist, llist2, \u0004);
			LList<IInstancePathInfo> llist4 = new LList<IInstancePathInfo>();
			for (int i = 0; i < llist.Count; i++)
			{
				llist4.Add(new \u0080.\u0005.\u0003(llist3[i], llist[i], llist2[i]));
			}
			return llist4;
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x00028138 File Offset: 0x00026338
		internal static \u0080.\u0005.\u0002 \u0001()
		{
			return new \u0080.\u0005.\u0002();
		}

		// Token: 0x04000281 RID: 641
		internal static readonly int \u0001 = 40;

		// Token: 0x020000C3 RID: 195
		internal sealed class \u0001
		{
			// Token: 0x06000E98 RID: 3736 RVA: 0x00028154 File Offset: 0x00026354
			internal \u0001()
			{
			}

			// Token: 0x06000E99 RID: 3737 RVA: 0x00028174 File Offset: 0x00026374
			internal \u0001(\u0080.\u0005.\u0001 \u0091\u0004)
			{
				this.\u0001 = \u0091\u0004.\u0001;
				this.\u0002 = \u0091\u0004.\u0002;
				this.\u0003 = \u0091\u0004.\u0003;
				this.\u0004 = \u0091\u0004.\u0004;
				this.\u0005 = \u0091\u0004.\u0005;
				this.\u0006 = \u0091\u0004.\u0006;
			}

			// Token: 0x04000282 RID: 642
			internal bool \u0001;

			// Token: 0x04000283 RID: 643
			internal bool \u0002 = true;

			// Token: 0x04000284 RID: 644
			internal bool \u0003 = true;

			// Token: 0x04000285 RID: 645
			internal bool \u0004 = true;

			// Token: 0x04000286 RID: 646
			internal bool \u0005;

			// Token: 0x04000287 RID: 647
			internal bool \u0006;
		}

		// Token: 0x020000C4 RID: 196
		internal sealed class \u0002
		{
			// Token: 0x17000442 RID: 1090
			// (get) Token: 0x06000E9A RID: 3738 RVA: 0x000281E4 File Offset: 0x000263E4
			internal LRUCache<\u0080.\u0005.\u0004, \u0080.\u0005.\u0005> Cache { get; }

			// Token: 0x06000E9B RID: 3739 RVA: 0x000281EC File Offset: 0x000263EC
			internal \u0002()
			{
				this.Cache = new LRUCache<\u0080.\u0005.\u0004, \u0080.\u0005.\u0005>(\u0080.\u0005.\u0001);
			}

			// Token: 0x04000288 RID: 648
			[CompilerGenerated]
			private readonly LRUCache<\u0080.\u0005.\u0004, \u0080.\u0005.\u0005> \u0001;
		}

		// Token: 0x020000C5 RID: 197
		private sealed class \u0003 : IInstancePathInfo
		{
			// Token: 0x06000E9C RID: 3740 RVA: 0x00028204 File Offset: 0x00026404
			internal \u0003(string \u0003\u0004, IVariable \u0093\u0004, ISignature \u0094\u0004)
			{
				this.InstancePath = \u0003\u0004;
				this.VarInstance = \u0093\u0004;
				this.DeclaringSignature = \u0094\u0004;
			}

			// Token: 0x17000443 RID: 1091
			// (get) Token: 0x06000E9D RID: 3741 RVA: 0x00028224 File Offset: 0x00026424
			// (set) Token: 0x06000E9E RID: 3742 RVA: 0x0002822C File Offset: 0x0002642C
			public string InstancePath { get; private set; }

			// Token: 0x17000444 RID: 1092
			// (get) Token: 0x06000E9F RID: 3743 RVA: 0x00028238 File Offset: 0x00026438
			// (set) Token: 0x06000EA0 RID: 3744 RVA: 0x00028240 File Offset: 0x00026440
			public IVariable VarInstance { get; private set; }

			// Token: 0x17000445 RID: 1093
			// (get) Token: 0x06000EA1 RID: 3745 RVA: 0x0002824C File Offset: 0x0002644C
			// (set) Token: 0x06000EA2 RID: 3746 RVA: 0x00028254 File Offset: 0x00026454
			public ISignature DeclaringSignature { get; private set; }

			// Token: 0x04000289 RID: 649
			[CompilerGenerated]
			private string \u0001;

			// Token: 0x0400028A RID: 650
			[CompilerGenerated]
			private IVariable \u0001;

			// Token: 0x0400028B RID: 651
			[CompilerGenerated]
			private ISignature \u0001;
		}

		// Token: 0x020000C6 RID: 198
		internal sealed class \u0004
		{
			// Token: 0x06000EA3 RID: 3747 RVA: 0x00028260 File Offset: 0x00026460
			internal \u0004(int \u0095\u0004, int \u0096\u0004, int \u0097\u0004, \u0080.\u0005.\u0001 \u0098\u0004)
			{
				this.\u0001 = \u0095\u0004;
				this.\u0002 = \u0096\u0004;
				this.\u0003 = \u0097\u0004;
				this.\u0001 = \u0098\u0004.\u0001;
				this.\u0002 = \u0098\u0004.\u0002;
				this.\u0003 = \u0098\u0004.\u0003;
				this.\u0004 = \u0098\u0004.\u0004;
				this.\u0005 = \u0098\u0004.\u0005;
			}

			// Token: 0x06000EA4 RID: 3748 RVA: 0x000282CC File Offset: 0x000264CC
			public bool \u0001(object \u0002)
			{
				if (\u0002 == null || base.GetType() != \u0002.GetType())
				{
					return false;
				}
				\u0080.\u0005.\u0004 u = (\u0080.\u0005.\u0004)\u0002;
				return this.\u0001 == u.\u0001 && this.\u0002 == u.\u0002 && this.\u0003 == u.\u0003 && this.\u0001 == u.\u0001 && this.\u0002 == u.\u0002 && this.\u0003 == u.\u0003 && this.\u0004 == u.\u0004 && this.\u0005 == u.\u0005;
			}

			// Token: 0x06000EA5 RID: 3749 RVA: 0x0002836C File Offset: 0x0002656C
			public int \u0001()
			{
				return this.\u0001.GetHashCode() ^ this.\u0002.GetHashCode() * 13 ^ this.\u0003.GetHashCode() * 13 ^ this.\u0001.GetHashCode() * 13 ^ this.\u0002.GetHashCode() * 13 ^ this.\u0003.GetHashCode() * 13 ^ this.\u0004.GetHashCode() * 13 ^ this.\u0005.GetHashCode() * 13;
			}

			// Token: 0x0400028C RID: 652
			private readonly int \u0001;

			// Token: 0x0400028D RID: 653
			private readonly int \u0002;

			// Token: 0x0400028E RID: 654
			private readonly int \u0003;

			// Token: 0x0400028F RID: 655
			private readonly bool \u0001;

			// Token: 0x04000290 RID: 656
			private readonly bool \u0002;

			// Token: 0x04000291 RID: 657
			private readonly bool \u0003;

			// Token: 0x04000292 RID: 658
			private readonly bool \u0004;

			// Token: 0x04000293 RID: 659
			private readonly bool \u0005;
		}

		// Token: 0x020000C7 RID: 199
		internal sealed class \u0005
		{
			// Token: 0x06000EA6 RID: 3750 RVA: 0x00028408 File Offset: 0x00026608
			internal \u0005(InstancePathsContainer \u0099\u0004)
			{
				this.\u0001 = \u0099\u0004;
			}

			// Token: 0x04000294 RID: 660
			internal InstancePathsContainer \u0001;
		}
	}
}
