using System;
using System.Collections.Generic;
using System.Linq;
using \u001C;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000C0 RID: 192
	internal static class InstancePathService
	{
		// Token: 0x06000E85 RID: 3717 RVA: 0x00027D18 File Offset: 0x00025F18
		internal static LList<string> \u0001(_IArrayType \u0002, IScope5 \u0003)
		{
			LList<string> llist = new LList<string>();
			bool flag;
			string[] components = \u0002.GetComponents(\u0003, out flag);
			if (\u0002.Base is _IArrayType)
			{
				LList<string> llist2 = InstancePathService.\u0001(\u0002.Base as _IArrayType, \u0003);
				foreach (string str in components)
				{
					foreach (string str2 in llist2)
					{
						llist.Add(str + str2);
					}
				}
			}
			else
			{
				llist.AddRange(components);
			}
			return llist;
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x00027DC4 File Offset: 0x00025FC4
		internal static LList<string> \u0001(_ICompileContext \u0002, _ISignature \u0003, int[] \u0004, \u0080.\u0005.\u0002 \u0005, LList<_IVariable> \u0006, LList<_ISignature> \u0007, \u0080.\u0005.\u0001 \u0008)
		{
			InstancePathsContainer instancePathsContainer = new \u001C.\u0003(\u0008, \u0005, \u0004, \u0002).\u0001(\u0003);
			LList<string> llist = new LList<string>();
			llist.AddRange(instancePathsContainer.InstancePaths);
			if (\u0006 != null)
			{
				\u0006.AddRange(instancePathsContainer.InstancePathInformations.Select(new Func<InstancePathInformation, _IVariable>(InstancePathService.<>c.<>9.\u0001)).Where(new Func<_IVariable, bool>(InstancePathService.<>c.<>9.\u0001)));
			}
			if (\u0007 != null)
			{
				\u0007.AddRange(instancePathsContainer.InstancePathInformations.Select(new Func<InstancePathInformation, _ISignature>(InstancePathService.<>c.<>9.\u0001)).Where(new Func<_ISignature, bool>(InstancePathService.<>c.<>9.\u0001)));
			}
			return llist;
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x00027EA8 File Offset: 0x000260A8
		internal static InstancePathsContainer \u0001(_ICompileContext \u0002, _ISignature \u0003, int[] \u0004, \u0080.\u0005.\u0002 \u0005, \u0080.\u0005.\u0001 \u0006)
		{
			return new \u001C.\u0003(\u0006, \u0005, \u0004, \u0002).\u0001(\u0003);
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x00027EBC File Offset: 0x000260BC
		internal static LList<string> \u0001(_ICompileContext \u0002, _ISignature \u0003, LList<_IVariable> \u0004, LList<_ISignature> \u0005)
		{
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
			{
				\u0001 = true,
				\u0002 = false,
				\u0003 = false,
				\u0004 = false,
				\u0005 = true
			};
			return InstancePathService.\u0001(\u0002, \u0003, Array.Empty<int>(), \u0080.\u0005.\u0001(), \u0004, \u0005, u);
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x00027F08 File Offset: 0x00026108
		internal static IEnumerable<string> \u0001(_ICompileContext \u0002, _ISignature \u0003, \u0080.\u0005.\u0001 \u0004)
		{
			LList<_IVariable> u = new LList<_IVariable>();
			LList<_ISignature> u2 = new LList<_ISignature>();
			return InstancePathService.\u0001(\u0002, \u0003, Array.Empty<int>(), \u0080.\u0005.\u0001(), u, u2, \u0004).ToArray();
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x00027F3C File Offset: 0x0002613C
		internal static LList<string> \u0001(_ICompileContext \u0002, _ISignature \u0003, int[] \u0004, LList<_IVariable> \u0005, LList<_ISignature> \u0006, bool \u0007)
		{
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
			{
				\u0001 = true,
				\u0002 = false,
				\u0003 = \u0007,
				\u0004 = false
			};
			return InstancePathService.\u0001(\u0002, \u0003, \u0004, \u0080.\u0005.\u0001(), \u0005, \u0006, u);
		}
	}
}
