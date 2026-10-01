using System;
using System.Collections.Generic;
using \u0015;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x02000146 RID: 326
	internal static class \u0007
	{
		// Token: 0x060016B0 RID: 5808 RVA: 0x000451E4 File Offset: 0x000433E4
		private static \u0002 \u0001(\u0002 \u0002, _ISignature \u0003)
		{
			if (!string.IsNullOrEmpty((\u0003 != null) ? \u0003.LibraryId : null))
			{
				return \u0002.\u0002(\u0003);
			}
			return \u0002;
		}

		// Token: 0x060016B1 RID: 5809 RVA: 0x00045204 File Offset: 0x00043404
		private static \u0007.\u0001 \u0001(this \u0002 \u0002, _ISignature \u0003, _IType \u0004, out \u0002 \u0005)
		{
			bool flag = true;
			\u0005 = \u0002;
			HashSet<_ISignature> hashSet = new HashSet<_ISignature>();
			while (\u0005 != null && \u0003 != null && \u0003.GetFlag(SignatureFlag.Alias) && hashSet.Add(\u0003))
			{
				_IType type = \u0003.AllVariables[0]._Type;
				if (flag)
				{
					\u0005 = \u0007.\u0001(\u0005, \u0003);
					flag = false;
				}
				if (TypeClass.Userdef != type.Class)
				{
					return new \u0007.\u0001
					{
						\u0001 = type,
						\u0001 = null
					};
				}
				\u0003 = (\u0005.FindSignature(type as IUserdefType) as _ISignature);
				\u0004 = type;
				\u0005 = \u0007.\u0001(\u0005, \u0003);
			}
			return new \u0007.\u0001
			{
				\u0001 = \u0004,
				\u0001 = \u0003
			};
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x000452BC File Offset: 0x000434BC
		public static _ISignature \u0001(this \u0002 \u0002, _ISignature \u0003, out \u0002 \u0004)
		{
			return \u0002.\u0001(\u0003, null, out \u0004).\u0001;
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x000452CC File Offset: 0x000434CC
		public static _IType \u0001(this \u0002 \u0002, _IType \u0003, out \u0002 \u0004)
		{
			_IUserdefType iuserdefType = \u0003 as _IUserdefType;
			if (iuserdefType != null && \u0002 != null)
			{
				_ISignature u = \u0002.FindSignature(iuserdefType) as _ISignature;
				return \u0002.\u0001(u, iuserdefType, out \u0004).\u0001;
			}
			\u0004 = \u0002;
			return \u0003;
		}

		// Token: 0x060016B4 RID: 5812 RVA: 0x00045308 File Offset: 0x00043508
		public static _IType \u0001(this \u0002 \u0002, _ISignature \u0003, _IType \u0004, out \u0002 \u0005)
		{
			return \u0002.\u0001(\u0003, \u0004, out \u0005).\u0001;
		}

		// Token: 0x060016B5 RID: 5813 RVA: 0x00045318 File Offset: 0x00043518
		public static _IType \u0001(this \u0002 \u0002)
		{
			if (TypeTable.GetSize2(TypeClass.Pointer, \u0002) != 8)
			{
				return TypeTable.UDInt;
			}
			return TypeTable.ULInt;
		}

		// Token: 0x02000147 RID: 327
		private struct \u0001
		{
			// Token: 0x040003FA RID: 1018
			public _IType \u0001;

			// Token: 0x040003FB RID: 1019
			public _ISignature \u0001;
		}
	}
}
