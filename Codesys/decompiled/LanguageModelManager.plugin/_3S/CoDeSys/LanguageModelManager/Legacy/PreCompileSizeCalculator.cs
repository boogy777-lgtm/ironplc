using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027F RID: 639
	public class PreCompileSizeCalculator
	{
		// Token: 0x06002AD9 RID: 10969 RVA: 0x0006F9F4 File Offset: 0x0006E9F4
		public PreCompileSizeCalculator(PreCompileContext preCompileContext)
		{
			this.SignatureSizeCalculator = new SignatureSizeCalculator(preCompileContext, this);
			this._typeSizeCalculator = new TypeSizeCalculator(this, preCompileContext);
			this._preCompileContext = preCompileContext;
		}

		// Token: 0x17000BF3 RID: 3059
		// (get) Token: 0x06002ADA RID: 10970 RVA: 0x0006FA1D File Offset: 0x0006EA1D
		public SignatureSizeCalculator SignatureSizeCalculator { get; }

		// Token: 0x06002ADB RID: 10971 RVA: 0x0006FA28 File Offset: 0x0006EA28
		public bool CalculateVariableSizes(List<ISignature> signaturelist, List<IVariable> varlist, out List<int> sizes)
		{
			IList<int> source;
			bool result = this.CalculateVariableSizes(signaturelist, varlist, out source);
			sizes = source.ToList<int>();
			return result;
		}

		// Token: 0x06002ADC RID: 10972 RVA: 0x0006FA48 File Offset: 0x0006EA48
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes)
		{
			if (signaturelist == null)
			{
				throw new ArgumentNullException("signaturelist");
			}
			if (varlist == null)
			{
				throw new ArgumentNullException("varlist");
			}
			if (varlist.Count != signaturelist.Count)
			{
				throw new ArgumentException("signaturelist and varlist have not the same length");
			}
			sizes = new List<int>(varlist.Count);
			LDictionary<ISignature, int> dicSizes = new LDictionary<ISignature, int>();
			_ILibraryTable libtable = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
			{
				libtable = this._preCompileContext._GetLibraryTable();
			}
			for (int i = 0; i < varlist.Count; i++)
			{
				ICompiledType vartype = varlist[i].Type as ICompiledType;
				sizes.Add(this._typeSizeCalculator.CalculateTypeSize(signaturelist[i], vartype, dicSizes, libtable, recursionGuard));
			}
			return true;
		}

		// Token: 0x06002ADD RID: 10973 RVA: 0x0006FAFF File Offset: 0x0006EAFF
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes)
		{
			return this.CalculateVariableSizes(signaturelist, varlist, new RecursionGuard(), out sizes);
		}

		// Token: 0x06002ADE RID: 10974 RVA: 0x0006FB0F File Offset: 0x0006EB0F
		public int CalculateTypeSize(ISignature sign, IType type)
		{
			return this.SignatureSizeCalculator.CalculateTypeSize(sign, type, new RecursionGuard());
		}

		// Token: 0x06002ADF RID: 10975 RVA: 0x0006FB23 File Offset: 0x0006EB23
		public int CalculateSignatureSize(ISignature3 sign)
		{
			return this.SignatureSizeCalculator.CalculateSignatureSize(sign, new RecursionGuard());
		}

		// Token: 0x04000829 RID: 2089
		private readonly PreCompileContext _preCompileContext;

		// Token: 0x0400082A RID: 2090
		private readonly TypeSizeCalculator _typeSizeCalculator;
	}
}
