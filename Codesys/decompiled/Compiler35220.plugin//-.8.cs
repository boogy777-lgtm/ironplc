using System;
using System.Runtime.CompilerServices;
using \u0007;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x02000149 RID: 329
	internal sealed class \u0008 : \u0005
	{
		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x060016F4 RID: 5876 RVA: 0x000460B0 File Offset: 0x000442B0
		private IScope5 ApplicationScope { get; }

		// Token: 0x060016F5 RID: 5877 RVA: 0x000460B8 File Offset: 0x000442B8
		public override IVariable[] \u0001(string \u0002, out ISignature[] \u0003)
		{
			IVariable[] array = base.\u0001(\u0002, out \u0003);
			if (array == null || array.Length < 1)
			{
				array = this.ApplicationScope.FindVariableGlobal(\u0002, out \u0003);
			}
			return array;
		}

		// Token: 0x060016F6 RID: 5878 RVA: 0x000460E8 File Offset: 0x000442E8
		internal override bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004)
		{
			bool flag = base.\u0001(\u0002, out \u0003, out \u0004);
			if (!flag)
			{
				flag = ((\u0005)this.ApplicationScope).\u0001(\u0002, out \u0003, out \u0004);
			}
			return flag;
		}

		// Token: 0x060016F7 RID: 5879 RVA: 0x00046118 File Offset: 0x00044318
		public override IScope \u0001(string \u0002)
		{
			IScope scope = base.\u0001(\u0002);
			if (scope == null)
			{
				scope = ((\u0005)this.ApplicationScope).\u0001(\u0002);
			}
			return scope;
		}

		// Token: 0x060016F8 RID: 5880 RVA: 0x00046144 File Offset: 0x00044344
		public \u0008(_ICompileContext \u0001\u0002, int \u0012\u0003, bool \u0013\u0003) : base(\u0001\u0002, \u0012\u0003, \u0013\u0003)
		{
			this.ApplicationScope = \u0007.\u0005.\u0001(\u0001\u0002);
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x0004615C File Offset: 0x0004435C
		public \u0008(_ICompileContext \u0001\u0002, _ISignature \u0014\u0003, bool \u0013\u0003) : base(\u0001\u0002, \u0014\u0003, \u0013\u0003)
		{
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x00046168 File Offset: 0x00044368
		public \u0008(_ICompileContext \u0001\u0002, _ISignature \u0014\u0003, bool \u0013\u0003, bool \u0015\u0003) : base(\u0001\u0002, \u0014\u0003, \u0013\u0003, \u0015\u0003)
		{
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x00046178 File Offset: 0x00044378
		public \u0008(_ICompileContext[] \u0016\u0003, int \u0012\u0003, bool \u0017\u0003) : base(\u0016\u0003, \u0012\u0003, \u0017\u0003)
		{
		}

		// Token: 0x040003FD RID: 1021
		[CompilerGenerated]
		private new readonly IScope5 \u0001;
	}
}
