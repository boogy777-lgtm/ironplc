using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027D RID: 637
	public class TypeSizeCalculator
	{
		// Token: 0x06002ABF RID: 10943 RVA: 0x0006EDF6 File Offset: 0x0006DDF6
		public TypeSizeCalculator(PreCompileSizeCalculator preCompileSizeCalculator, PreCompileContext preCompileContext)
		{
			this._preCompileSizeCalculator = preCompileSizeCalculator;
			this._preCompileContext = preCompileContext;
		}

		// Token: 0x06002AC0 RID: 10944 RVA: 0x0006EE0C File Offset: 0x0006DE0C
		public int CalculateTypeSize(ISignature sign, ICompiledType vartype, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			IPrecompileScope4 prescope;
			if (libtable == null)
			{
				prescope = (this._preCompileContext.CreatePrecompileScope(sign.ObjectGuid) as IPrecompileScope4);
			}
			else
			{
				prescope = CompilerProxy.CreatePrecompileScope(this._preCompileSizeCalculator.SignatureSizeCalculator.PrecompilePointerSize, libtable, sign as _ISignature, this._preCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			}
			switch (vartype.Class)
			{
			case TypeClass.Bool:
			case TypeClass.Bit:
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Pointer:
			case TypeClass.Reference:
			case TypeClass.LTime:
			case TypeClass.BitConst:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return TypeTable.GetSize(vartype.Class, null);
			case TypeClass.String:
				return TypeSizeCalculator.CalculateStringSize(vartype, recursionGuard, prescope);
			case TypeClass.WString:
				return TypeSizeCalculator.CalculateWStringSize(vartype, recursionGuard, prescope);
			case TypeClass.Subrange:
			case TypeClass.Enum:
				return TypeTable.GetSize(vartype.DeRefType.Class, null);
			case TypeClass.Array:
				return this.CalculateArrayTypeSize(sign, vartype, dicSizes, libtable, recursionGuard, prescope);
			case TypeClass.Userdef:
				return this.CalculateUserdefTypeSize(vartype, dicSizes, libtable, recursionGuard, prescope);
			case TypeClass.Lazy:
				return -1;
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
				if ((!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351740 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800) && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351810)
				{
					return -1;
				}
				return this._preCompileContext.PointerSize;
			case TypeClass.__Vector:
				return this.CalculateVectorTypeSize(sign, vartype, dicSizes, libtable, recursionGuard, prescope);
			}
			return -1;
		}

		// Token: 0x06002AC1 RID: 10945 RVA: 0x0006EFEC File Offset: 0x0006DFEC
		private int CalculateVectorTypeSize(ISignature sign, ICompiledType vartype, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			_IVectorType ivectorType = vartype as _IVectorType;
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(ivectorType))
				{
					return -1;
				}
				recursionGuard.Add(ivectorType);
			}
			int num = 1;
			if (ivectorType != null && !SizeCalculatorHelpFunctions.GetConstExpressionValue(ivectorType.Dimension as IExpression2, prescope, recursionGuard, out num))
			{
				return -1;
			}
			int num2 = this.CalculateTypeSize(sign, (ivectorType != null) ? ivectorType.BaseType : null, dicSizes, libtable, recursionGuard);
			if (num2 == -1)
			{
				return -1;
			}
			return num * num2;
		}

		// Token: 0x06002AC2 RID: 10946 RVA: 0x0006F064 File Offset: 0x0006E064
		private int CalculateArrayTypeSize(ISignature sign, ICompiledType vartype, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			ArrayType arrayType = vartype as ArrayType;
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(arrayType))
				{
					return -1;
				}
				recursionGuard.Add(arrayType);
			}
			int arrayRanges = this.GetArrayRanges(arrayType, recursionGuard, prescope);
			if (arrayRanges == -1)
			{
				return -1;
			}
			int num = this.CalculateTypeSize(sign, (arrayType != null) ? arrayType.BaseType : null, dicSizes, libtable, recursionGuard);
			if (num == -1)
			{
				return -1;
			}
			return arrayRanges * num;
		}

		// Token: 0x06002AC3 RID: 10947 RVA: 0x0006F0D0 File Offset: 0x0006E0D0
		private int GetArrayRanges(ArrayType arr, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			int num = 0;
			if (((arr != null) ? arr.Dimensions : null) != null)
			{
				foreach (IArrayDimension arrayDimension in arr.Dimensions)
				{
					int num2;
					int num3;
					if (!SizeCalculatorHelpFunctions.GetConstExpressionValue(arrayDimension.LowerBorder as IExpression2, prescope, recursionGuard, out num2) || !SizeCalculatorHelpFunctions.GetConstExpressionValue(arrayDimension.UpperBorder as IExpression2, prescope, recursionGuard, out num3))
					{
						return -1;
					}
					int num4 = num3 - num2 + 1;
					if (num4 < 0)
					{
						return -1;
					}
					if (num == 0)
					{
						num = 1;
					}
					num *= num4;
				}
			}
			return num;
		}

		// Token: 0x06002AC4 RID: 10948 RVA: 0x0006F150 File Offset: 0x0006E150
		private int CalculateUserdefTypeSize(ICompiledType vartype, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			UserdefType userdefType = vartype as UserdefType;
			ISignature3 signature = ((prescope != null) ? prescope.FindSignatureGlobal((userdefType != null) ? userdefType.NameExpression : null) : null) as ISignature3;
			if (signature == null)
			{
				return -1;
			}
			IPrecompileScope4 prescope2 = prescope;
			if (!string.IsNullOrEmpty(signature.LibraryPath) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				IPreCompileContext4 libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
				if (libraryContext != null)
				{
					prescope2 = (libraryContext.CreatePrecompileScope(signature.ObjectGuid) as IPrecompileScope4);
				}
			}
			return this._preCompileSizeCalculator.SignatureSizeCalculator.CalculatePrecompileSize(signature, prescope2, dicSizes, libtable, recursionGuard);
		}

		// Token: 0x06002AC5 RID: 10949 RVA: 0x0006F1EC File Offset: 0x0006E1EC
		private static int CalculateStringSize(ICompiledType vartype, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			StringType stringType = vartype as StringType;
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(stringType))
				{
					return -1;
				}
				recursionGuard.Add(stringType);
			}
			if (((stringType != null) ? stringType.Length : null) == null)
			{
				return 81;
			}
			int num;
			if (!SizeCalculatorHelpFunctions.GetConstExpressionValue(stringType.Length, prescope, recursionGuard, out num))
			{
				return -1;
			}
			return num + 1;
		}

		// Token: 0x06002AC6 RID: 10950 RVA: 0x0006F244 File Offset: 0x0006E244
		private static int CalculateWStringSize(ICompiledType vartype, IRecursionGuard recursionGuard, IPrecompileScope4 prescope)
		{
			WStringType wstringType = vartype as WStringType;
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(wstringType))
				{
					return -1;
				}
				recursionGuard.Add(wstringType);
			}
			if (((wstringType != null) ? wstringType.Length : null) == null)
			{
				return 162;
			}
			int num;
			if (!SizeCalculatorHelpFunctions.GetConstExpressionValue(wstringType.Length, prescope, recursionGuard, out num))
			{
				return -1;
			}
			return 2 * (num + 1);
		}

		// Token: 0x04000824 RID: 2084
		private readonly PreCompileSizeCalculator _preCompileSizeCalculator;

		// Token: 0x04000825 RID: 2085
		private readonly PreCompileContext _preCompileContext;
	}
}
