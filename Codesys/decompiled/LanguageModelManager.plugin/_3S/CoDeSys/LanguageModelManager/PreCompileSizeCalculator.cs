using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Legacy;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003A RID: 58
	public class PreCompileSizeCalculator
	{
		// Token: 0x060002BD RID: 701 RVA: 0x0000AA96 File Offset: 0x00009A96
		public PreCompileSizeCalculator(PreCompileContext preCompileContext)
		{
			this._legacyPreCompileSizeCalculator = new PreCompileSizeCalculator(preCompileContext);
			this._preCompileContext = preCompileContext;
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000AAB4 File Offset: 0x00009AB4
		public bool CalculateVariableSizes(List<ISignature> signaturelist, List<IVariable> varlist, out List<int> sizes)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, out sizes);
			}
			IList<int> list;
			bool result = VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateVariableSizes(this._preCompileContext, signaturelist, varlist, new RecursionGuard(), out list);
			sizes = new List<int>();
			if (list != null)
			{
				sizes.AddRange(list);
			}
			return result;
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000AB02 File Offset: 0x00009B02
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, recursionGuard, out sizes);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateVariableSizes(this._preCompileContext, signaturelist, varlist, recursionGuard, out sizes);
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000AB31 File Offset: 0x00009B31
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, new RecursionGuard(), out sizes);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateVariableSizes(this._preCompileContext, signaturelist, varlist, new RecursionGuard(), out sizes);
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000AB66 File Offset: 0x00009B66
		public int CalculateTypeSize(ISignature sign, IType type)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.CalculateTypeSize(sign, type);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateTypeSize(this._preCompileContext, sign, type, new RecursionGuard());
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000AB94 File Offset: 0x00009B94
		public int CalculateTypeSize(ISignature sign, IType type, IRecursionGuard recursionGuard)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.SignatureSizeCalculator.CalculateTypeSize(sign, type, recursionGuard);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateTypeSize(this._preCompileContext, sign, type, recursionGuard);
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000ABC4 File Offset: 0x00009BC4
		public int CalculateSignatureSize(ISignature3 sign)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.CalculateSignatureSize(sign);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateSignatureSize(this._preCompileContext, sign, new RecursionGuard());
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000ABF0 File Offset: 0x00009BF0
		public int CalculateSignatureSize(ISignature3 sign, IRecursionGuard recursionGuard)
		{
			if (VersionedCompilerFactory._PreCompileSizeCalculator_OrNull == null)
			{
				return this._legacyPreCompileSizeCalculator.SignatureSizeCalculator.CalculateSignatureSize(sign, recursionGuard);
			}
			return VersionedCompilerFactory._PreCompileSizeCalculator_OrNull.CalculateSignatureSize(this._preCompileContext, sign, recursionGuard);
		}

		// Token: 0x0400006E RID: 110
		private readonly PreCompileSizeCalculator _legacyPreCompileSizeCalculator;

		// Token: 0x0400006F RID: 111
		private readonly PreCompileContext _preCompileContext;
	}
}
