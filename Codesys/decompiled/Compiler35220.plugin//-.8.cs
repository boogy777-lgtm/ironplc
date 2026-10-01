using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0013;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0083
{
	// Token: 0x02000329 RID: 809
	internal sealed class \u0008
	{
		// Token: 0x170007FD RID: 2045
		// (get) Token: 0x06003030 RID: 12336 RVA: 0x000B7320 File Offset: 0x000B5520
		private \u0013.\u000E InitTypifier { get; }

		// Token: 0x170007FE RID: 2046
		// (get) Token: 0x06003031 RID: 12337 RVA: 0x000B7328 File Offset: 0x000B5528
		private TypesOnlyTypifier TypesOnlyTypifier { get; }

		// Token: 0x06003032 RID: 12338 RVA: 0x000B7330 File Offset: 0x000B5530
		internal \u0008(_ISignature \u001C\u0002, \u001B \u0082\u0005)
		{
			this.InitTypifier = new \u0013.\u000E(\u001C\u0002, \u0082\u0005);
			this.TypesOnlyTypifier = new TypesOnlyTypifier(\u001C\u0002, \u0082\u0005);
		}

		// Token: 0x06003033 RID: 12339 RVA: 0x000B7354 File Offset: 0x000B5554
		internal bool \u0001(IScope5 \u0002)
		{
			this.\u0003(\u0002);
			this.\u0002(\u0002);
			return true;
		}

		// Token: 0x06003034 RID: 12340 RVA: 0x000B7368 File Offset: 0x000B5568
		internal bool \u0002(IScope5 \u0002)
		{
			return this.InitTypifier.\u0001(\u0002);
		}

		// Token: 0x06003035 RID: 12341 RVA: 0x000B7378 File Offset: 0x000B5578
		internal bool \u0003(IScope5 \u0002)
		{
			return this.TypesOnlyTypifier.\u0001(\u0002);
		}

		// Token: 0x06003036 RID: 12342 RVA: 0x000B7388 File Offset: 0x000B5588
		internal static bool \u0001(_ISignature \u0002, IVariable \u0003)
		{
			return \u0002.POUType == Operator.FunctionBlock && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INST_VAR);
		}

		// Token: 0x04000931 RID: 2353
		[CompilerGenerated]
		private readonly \u0013.\u000E \u0001;

		// Token: 0x04000932 RID: 2354
		[CompilerGenerated]
		private readonly TypesOnlyTypifier \u0001;
	}
}
