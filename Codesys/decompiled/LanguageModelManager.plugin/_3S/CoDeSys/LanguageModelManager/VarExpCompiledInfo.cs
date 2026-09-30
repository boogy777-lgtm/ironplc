using System;
using System.Reflection;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000078 RID: 120
	public class VarExpCompiledInfo
	{
		// Token: 0x170001AF RID: 431
		// (get) Token: 0x060007AD RID: 1965 RVA: 0x000130D5 File Offset: 0x000120D5
		// (set) Token: 0x060007AE RID: 1966 RVA: 0x000130DD File Offset: 0x000120DD
		[Obfuscation(Feature = "rename")]
		public int ISignatureId { get; set; } = Common.InvalidID;

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x060007AF RID: 1967 RVA: 0x000130E6 File Offset: 0x000120E6
		// (set) Token: 0x060007B0 RID: 1968 RVA: 0x000130EE File Offset: 0x000120EE
		[Obfuscation(Feature = "rename")]
		public int IScopeId { get; set; } = Common.InvalidID;

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x060007B1 RID: 1969 RVA: 0x000130F7 File Offset: 0x000120F7
		// (set) Token: 0x060007B2 RID: 1970 RVA: 0x000130FF File Offset: 0x000120FF
		[Obfuscation(Feature = "rename")]
		public int IVariableId { get; set; } = Common.InvalidID;

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x060007B3 RID: 1971 RVA: 0x00013108 File Offset: 0x00012108
		// (set) Token: 0x060007B4 RID: 1972 RVA: 0x00013110 File Offset: 0x00012110
		[Obfuscation(Feature = "rename")]
		public VarExprFlag Varexprflag { get; set; }

		// Token: 0x04000103 RID: 259
		[Obfuscation(Feature = "rename")]
		internal IVariableExprInfo m_expinfo;

		// Token: 0x04000104 RID: 260
		[Obfuscation(Feature = "rename")]
		internal ICompiledType m_ctype;
	}
}
