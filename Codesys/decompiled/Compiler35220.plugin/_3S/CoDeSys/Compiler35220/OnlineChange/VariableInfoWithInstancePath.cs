using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x02000363 RID: 867
	public class VariableInfoWithInstancePath : VariableInfo
	{
		// Token: 0x1700088C RID: 2188
		// (get) Token: 0x060033E6 RID: 13286 RVA: 0x000CBF3C File Offset: 0x000CA13C
		public ICollection<string> InstancePaths { get; }

		// Token: 0x060033E7 RID: 13287 RVA: 0x000CBF44 File Offset: 0x000CA144
		public VariableInfoWithInstancePath(int nVarId, int nSignId, VarFlag vfFlag, ICollection<string> instancePaths) : base(nVarId, nSignId, vfFlag)
		{
			this.InstancePaths = instancePaths;
		}

		// Token: 0x04000A07 RID: 2567
		[CompilerGenerated]
		private readonly ICollection<string> \u0001;
	}
}
