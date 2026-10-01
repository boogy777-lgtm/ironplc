using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x0200028E RID: 654
	internal sealed class \u0012
	{
		// Token: 0x17000741 RID: 1857
		// (get) Token: 0x0600293D RID: 10557 RVA: 0x0008FA00 File Offset: 0x0008DC00
		private \u0011 Context { get; }

		// Token: 0x17000742 RID: 1858
		// (get) Token: 0x0600293E RID: 10558 RVA: 0x0008FA08 File Offset: 0x0008DC08
		private StructAndArrayInitReplacer Visitor { get; }

		// Token: 0x0600293F RID: 10559 RVA: 0x0008FA10 File Offset: 0x0008DC10
		public \u0012(\u0011 \u0083\u0005, StructAndArrayInitReplacer \u001B\u0006)
		{
			this.Context = \u0083\u0005;
			this.Visitor = \u001B\u0006;
		}

		// Token: 0x06002940 RID: 10560 RVA: 0x0008FA28 File Offset: 0x0008DC28
		public _IStatement \u0001(_IAssignmentExpression \u0002)
		{
			if (\u0002._RValue is _IArrayInitialization)
			{
				ICodegenerator3 codegenerator = this.Context.CodeGen as ICodegenerator3;
				bool emulateVectors = codegenerator == null || !codegenerator.GetProperty(CodegeneratorProperties.SupportsVectorOperations);
				_ISignature sign = (this.Context._Scope.MethodSignature as _ISignature) ?? (this.Context._Scope.LocalSignature as _ISignature);
				return new ArrayInitialisationCodeGenerator(this.Visitor, this.Context, emulateVectors).HandleArrayInitialisation(\u0002, sign);
			}
			return null;
		}

		// Token: 0x0400078F RID: 1935
		[CompilerGenerated]
		private readonly \u0011 \u0001;

		// Token: 0x04000790 RID: 1936
		[CompilerGenerated]
		private readonly StructAndArrayInitReplacer \u0001;
	}
}
