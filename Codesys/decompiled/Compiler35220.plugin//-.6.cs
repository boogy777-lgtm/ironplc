using System;
using System.Runtime.CompilerServices;
using \u0006;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0083
{
	// Token: 0x020002DB RID: 731
	internal sealed class \u0006
	{
		// Token: 0x17000795 RID: 1941
		// (get) Token: 0x06002BEE RID: 11246 RVA: 0x0009A094 File Offset: 0x00098294
		// (set) Token: 0x06002BEF RID: 11247 RVA: 0x0009A09C File Offset: 0x0009829C
		private IScope5 Scope { get; set; }

		// Token: 0x06002BF0 RID: 11248 RVA: 0x0009A0A8 File Offset: 0x000982A8
		internal \u0006(IScope5 \u009B\u0002)
		{
			this.Scope = \u009B\u0002;
		}

		// Token: 0x06002BF1 RID: 11249 RVA: 0x0009A0B8 File Offset: 0x000982B8
		private bool \u0001(ICompiledType \u0002)
		{
			if (\u0002.Class != TypeClass.Enum)
			{
				return false;
			}
			ISignature signature = this.Scope.FindSignature(\u0002 as IEnumType);
			return signature != null && signature.HasAttribute("strict");
		}

		// Token: 0x06002BF2 RID: 11250 RVA: 0x0009A0F4 File Offset: 0x000982F4
		private void \u0001(ICompiledType \u0002, ICompiledType \u0003, ref ICompiledType \u0004, ref ICompiledType \u0005)
		{
			if (\u0003.Class == TypeClass.Reference)
			{
				IReferenceType2 referenceType = \u0003 as IReferenceType2;
				if (referenceType != null && this.\u0001(referenceType.OriginalBase))
				{
					\u0005 = referenceType.OriginalBase;
					if (\u0002.Class == TypeClass.Enum)
					{
						\u0004 = \u0002;
						return;
					}
					if (\u0002.Class != TypeClass.Reference)
					{
						return;
					}
					IReferenceType2 referenceType2 = \u0002 as IReferenceType2;
					if (referenceType2 != null && referenceType2.OriginalBase.Class == TypeClass.Enum)
					{
						\u0004 = referenceType2.OriginalBase;
						return;
					}
					return;
				}
			}
			if (this.\u0001(\u0003))
			{
				\u0005 = \u0003;
				if (\u0002.Class == TypeClass.Enum)
				{
					\u0004 = \u0002;
				}
			}
		}

		// Token: 0x06002BF3 RID: 11251 RVA: 0x0009A180 File Offset: 0x00098380
		private bool \u0001(ICompiledType \u0002, ICompiledType \u0003, bool \u0004)
		{
			if (!\u0004)
			{
				return \u0011.\u0002(\u0002.DeRefType, \u0003, this.Scope);
			}
			ICompiledType deRefType = \u0002.DeRefType;
			ICompiledType deRefType2 = \u0003.DeRefType;
			this.\u0001(\u0002, \u0003, ref deRefType, ref deRefType2);
			if (TypeTable.IsAnyType(deRefType2.Class))
			{
				return PragmaEvaluationHelper.MatchesAnyType(deRefType2.Class, deRefType);
			}
			return ((_IType)deRefType2).IsEqual(deRefType, this.Scope);
		}

		// Token: 0x06002BF4 RID: 11252 RVA: 0x0009A1EC File Offset: 0x000983EC
		public void \u0001(_IHasTypeExpression \u0002)
		{
			ICompiledType compiledType = null;
			if (\u0002.Variable != null)
			{
				compiledType = \u0002.Variable.Type;
			}
			\u0002.ReferencedType = \u0002.Type;
			ICompiledType referencedType = \u0002.ReferencedType;
			if (compiledType != null && referencedType != null)
			{
				\u0002.Value = this.\u0001(compiledType, referencedType, \u0002.Exact);
				return;
			}
			\u0002.Value = false;
		}

		// Token: 0x04000859 RID: 2137
		[CompilerGenerated]
		private IScope5 \u0001;
	}
}
