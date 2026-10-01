using System;
using \u001C;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services.PreCompileSizeCalculation
{
	// Token: 0x02000131 RID: 305
	public class TypeSizeCalculator
	{
		// Token: 0x060015B2 RID: 5554 RVA: 0x0003F320 File Offset: 0x0003D520
		public TypeSizeCalculator(SignatureSizeCalculator signatureSizeCalculator, _IPreCompileContext preCompileContext)
		{
			this.\u0001 = signatureSizeCalculator;
			this.\u0001 = preCompileContext;
		}

		// Token: 0x060015B3 RID: 5555 RVA: 0x0003F338 File Offset: 0x0003D538
		public int CalculateTypeSize(_IPreCompileContext precom, ISignature sign, ICompiledType vartype, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			IPrecompileScope4 precompileScope;
			if (libtable == null)
			{
				precompileScope = (this.\u0001.CreatePrecompileScope(sign.ObjectGuid) as IPrecompileScope4);
			}
			else
			{
				precompileScope = new Helper().\u0001(precom.PointerSize, libtable, sign as _ISignature, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
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
				return TypeSizeCalculator.\u0001(vartype, recursionGuard, precompileScope);
			case TypeClass.WString:
				return TypeSizeCalculator.\u0002(vartype, recursionGuard, precompileScope);
			case TypeClass.Subrange:
			case TypeClass.Enum:
				return TypeTable.GetSize(vartype.DeRefType.Class, null);
			case TypeClass.Array:
				return this.\u0002(precom, sign, vartype, dicSizes, libtable, recursionGuard, precompileScope);
			case TypeClass.Userdef:
				return this.\u0001(precom, vartype, dicSizes, libtable, recursionGuard, precompileScope);
			case TypeClass.Lazy:
				return -1;
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
				return this.\u0001.PointerSize;
			case TypeClass.__Vector:
				return this.\u0001(precom, sign, vartype, dicSizes, libtable, recursionGuard, precompileScope);
			}
			return -1;
		}

		// Token: 0x060015B4 RID: 5556 RVA: 0x0003F4D8 File Offset: 0x0003D6D8
		private int \u0001(_IPreCompileContext \u0002, ISignature \u0003, ICompiledType \u0004, LDictionary<ISignature, int> \u0005, _ILibraryTable \u0006, IRecursionGuard \u0007, IPrecompileScope4 \u0008)
		{
			_IVectorType ivectorType = \u0004 as _IVectorType;
			if (\u0007 != null)
			{
				\u0007 = \u0007.Duplicate();
				if (\u0007.Has(ivectorType))
				{
					return -1;
				}
				\u0007.Add(ivectorType);
			}
			int num = 1;
			if (ivectorType != null && !\u0007.\u0001(ivectorType.Dimension as IExpression2, \u0008, \u0007, out num))
			{
				return -1;
			}
			int num2 = this.CalculateTypeSize(\u0002, \u0003, (ivectorType != null) ? ivectorType.BaseType : null, \u0005, \u0006, \u0007);
			if (num2 == -1)
			{
				return -1;
			}
			return num * num2;
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x0003F550 File Offset: 0x0003D750
		private int \u0002(_IPreCompileContext \u0002, ISignature \u0003, ICompiledType \u0004, LDictionary<ISignature, int> \u0005, _ILibraryTable \u0006, IRecursionGuard \u0007, IPrecompileScope4 \u0008)
		{
			_IArrayType iarrayType = \u0004 as _IArrayType;
			if (\u0007 != null)
			{
				\u0007 = \u0007.Duplicate();
				if (\u0007.Has(iarrayType))
				{
					return -1;
				}
				\u0007.Add(iarrayType);
			}
			int num = this.\u0001(iarrayType, \u0007, \u0008);
			if (num == -1)
			{
				return -1;
			}
			int num2 = this.CalculateTypeSize(\u0002, \u0003, (iarrayType != null) ? iarrayType.BaseType : null, \u0005, \u0006, \u0007);
			if (num2 == -1)
			{
				return -1;
			}
			return num * num2;
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x0003F5BC File Offset: 0x0003D7BC
		private int \u0001(_IArrayType \u0002, IRecursionGuard \u0003, IPrecompileScope4 \u0004)
		{
			int num = 0;
			if (((\u0002 != null) ? \u0002.Dimensions : null) != null)
			{
				foreach (IArrayDimension arrayDimension in \u0002.Dimensions)
				{
					int num2;
					int num3;
					if (!\u0007.\u0001(arrayDimension.LowerBorder as IExpression2, \u0004, \u0003, out num2) || !\u0007.\u0001(arrayDimension.UpperBorder as IExpression2, \u0004, \u0003, out num3))
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

		// Token: 0x060015B7 RID: 5559 RVA: 0x0003F63C File Offset: 0x0003D83C
		private int \u0001(_IPreCompileContext \u0002, ICompiledType \u0003, LDictionary<ISignature, int> \u0004, _ILibraryTable \u0005, IRecursionGuard \u0006, IPrecompileScope4 \u0007)
		{
			_IUserdefType iuserdefType = \u0003 as _IUserdefType;
			ISignature3 signature = ((\u0007 != null) ? \u0007.FindSignatureGlobal((iuserdefType != null) ? iuserdefType.NameExpression : null) : null) as ISignature3;
			if (signature == null)
			{
				return -1;
			}
			IPrecompileScope4 prescope = \u0007;
			if (!string.IsNullOrEmpty(signature.LibraryPath))
			{
				IPreCompileContext4 libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
				if (libraryContext != null)
				{
					prescope = (libraryContext.CreatePrecompileScope(signature.ObjectGuid) as IPrecompileScope4);
				}
			}
			return this.\u0001.CalculatePrecompileSize(\u0002, signature, prescope, \u0004, \u0005, \u0006);
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x0003F6C4 File Offset: 0x0003D8C4
		private static int \u0001(ICompiledType \u0002, IRecursionGuard \u0003, IPrecompileScope4 \u0004)
		{
			_IStringType istringType = \u0002 as _IStringType;
			if (\u0003 != null)
			{
				\u0003 = \u0003.Duplicate();
				if (\u0003.Has(istringType))
				{
					return -1;
				}
				\u0003.Add(istringType);
			}
			if (((istringType != null) ? istringType.Length : null) == null)
			{
				return 81;
			}
			int num;
			if (!\u0007.\u0001(istringType.Length, \u0004, \u0003, out num))
			{
				return -1;
			}
			return num + 1;
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x0003F71C File Offset: 0x0003D91C
		private static int \u0002(ICompiledType \u0002, IRecursionGuard \u0003, IPrecompileScope4 \u0004)
		{
			_IWStringType iwstringType = \u0002 as _IWStringType;
			if (\u0003 != null)
			{
				\u0003 = \u0003.Duplicate();
				if (\u0003.Has(iwstringType))
				{
					return -1;
				}
				\u0003.Add(iwstringType);
			}
			if (((iwstringType != null) ? iwstringType.Length : null) == null)
			{
				return 162;
			}
			int num;
			if (!\u0007.\u0001(iwstringType.Length, \u0004, \u0003, out num))
			{
				return -1;
			}
			return 2 * (num + 1);
		}

		// Token: 0x040003C0 RID: 960
		private readonly SignatureSizeCalculator \u0001;

		// Token: 0x040003C1 RID: 961
		private readonly _IPreCompileContext \u0001;
	}
}
