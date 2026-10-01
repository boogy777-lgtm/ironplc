using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services.PreCompileSizeCalculation
{
	// Token: 0x0200012E RID: 302
	public class PreCompileSizeCalculator : IPreCompileSizeCalculator
	{
		// Token: 0x0600159A RID: 5530 RVA: 0x0003EB68 File Offset: 0x0003CD68
		public bool CalculateVariableSizes(_IPreCompileContext precom, IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes)
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
			_ILibraryTable libtable = precom._GetLibraryTable();
			TypeSizeCalculator typeSizeCalculator = new TypeSizeCalculator(new SignatureSizeCalculator(precom, this), precom);
			for (int i = 0; i < varlist.Count; i++)
			{
				ICompiledType vartype = varlist[i].Type as ICompiledType;
				sizes.Add(typeSizeCalculator.CalculateTypeSize(precom, signaturelist[i], vartype, dicSizes, libtable, recursionGuard));
			}
			return true;
		}

		// Token: 0x0600159B RID: 5531 RVA: 0x0003EC14 File Offset: 0x0003CE14
		public int CalculateTypeSize(_IPreCompileContext precom, ISignature sign, IType type, IRecursionGuard recursionGuard)
		{
			return new SignatureSizeCalculator(precom, this).CalculateTypeSize(precom, sign, type, recursionGuard);
		}

		// Token: 0x0600159C RID: 5532 RVA: 0x0003EC28 File Offset: 0x0003CE28
		public int CalculateSignatureSize(_IPreCompileContext precom, ISignature3 sign, IRecursionGuard recursionGuard)
		{
			return new SignatureSizeCalculator(precom, this).CalculateSignatureSize(precom, sign, recursionGuard);
		}
	}
}
