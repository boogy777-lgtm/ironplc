using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027E RID: 638
	public class SignatureSizeCalculator
	{
		// Token: 0x06002AC7 RID: 10951 RVA: 0x0006F2A1 File Offset: 0x0006E2A1
		public SignatureSizeCalculator(PreCompileContext preCompileContext, PreCompileSizeCalculator preCompileSizeCalculator)
		{
			this.PreCompileContext = preCompileContext;
			this._typeSizeCalculator = new TypeSizeCalculator(preCompileSizeCalculator, this.PreCompileContext);
			this._preCompileSizeCalculator = preCompileSizeCalculator;
		}

		// Token: 0x17000BF1 RID: 3057
		// (get) Token: 0x06002AC8 RID: 10952 RVA: 0x0006F2C9 File Offset: 0x0006E2C9
		private PreCompileContext PreCompileContext { get; }

		// Token: 0x17000BF2 RID: 3058
		// (get) Token: 0x06002AC9 RID: 10953 RVA: 0x0006F2D4 File Offset: 0x0006E2D4
		public int PrecompilePointerSize
		{
			get
			{
				if ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351740 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351810)
				{
					return this.PreCompileContext.PointerSize;
				}
				return 4;
			}
		}

		// Token: 0x06002ACA RID: 10954 RVA: 0x0006F324 File Offset: 0x0006E324
		public int CalculateTypeSize(ISignature sign, IType type, IRecursionGuard recursionGuard)
		{
			LDictionary<ISignature, int> dicSizes = new LDictionary<ISignature, int>();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
			{
				return this._typeSizeCalculator.CalculateTypeSize(sign, type as ICompiledType, dicSizes, this.PreCompileContext._GetLibraryTable(), recursionGuard);
			}
			return this._typeSizeCalculator.CalculateTypeSize(sign, type as ICompiledType, dicSizes, null, recursionGuard);
		}

		// Token: 0x06002ACB RID: 10955 RVA: 0x0006F380 File Offset: 0x0006E380
		public int CalculateSignatureSize(ISignature3 sign, IRecursionGuard recursionGuard)
		{
			LDictionary<ISignature, int> dicSizes = new LDictionary<ISignature, int>();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
			{
				_ILibraryTable libtable = this.PreCompileContext._GetLibraryTable();
				_IPrecompileScope iprecompileScope = CompilerProxy.CreatePrecompileScope(this.PrecompilePointerSize, libtable, sign as _ISignature, this.PreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
				iprecompileScope.SetPointerSize();
				IPrecompileScope4 prescope = iprecompileScope;
				return this.CalculatePrecompileSize(sign, prescope, dicSizes, libtable, recursionGuard);
			}
			IPrecompileScope4 prescope2 = this.PreCompileContext.CreatePrecompileScope(Guid.Empty) as IPrecompileScope4;
			return this.CalculatePrecompileSize(sign, prescope2, dicSizes, null, recursionGuard);
		}

		// Token: 0x06002ACC RID: 10956 RVA: 0x0006F40C File Offset: 0x0006E40C
		public int CalculatePrecompileSize(ISignature3 sign, IPrecompileScope4 prescope, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			int num;
			if (dicSizes.TryGetValue(sign, ref num))
			{
				return num;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35430 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV345120 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000))
			{
				dicSizes[sign] = -1;
			}
			num = this.CalculatePrecompileSize1(sign as _ISignature, prescope, dicSizes, libtable, recursionGuard);
			dicSizes[sign] = num;
			return num;
		}

		// Token: 0x06002ACD RID: 10957 RVA: 0x0006F480 File Offset: 0x0006E480
		private int CalculatePrecompileSize1(_ISignature sign, IPrecompileScope4 prescope, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			Operator poutype = sign.POUType;
			if (poutype <= Operator.Type)
			{
				if (poutype != Operator.FunctionBlock)
				{
					if (poutype != Operator.Type)
					{
						return -1;
					}
					if (sign.GetFlag(SignatureFlag.Alias))
					{
						if (sign.All.Length != 0)
						{
							return this._preCompileSizeCalculator.CalculateTypeSize(sign, sign.All[0].Type as ICompiledType);
						}
					}
					else if (sign.GetFlag(SignatureFlag.Union))
					{
						return this.CalculateSizeUnion(sign, dicSizes, libtable, recursionGuard);
					}
				}
				return this.CalculateSignatureSize(sign, prescope, dicSizes, libtable, recursionGuard);
			}
			if (poutype != Operator.VarGlobal)
			{
				if (poutype == Operator.Interface)
				{
					return this.PrecompilePointerSize;
				}
			}
			else if (sign.GetFlag(SignatureFlag.Enum))
			{
				return this.CalculatePrecompileSize_Enum(sign, dicSizes, libtable, recursionGuard);
			}
			return -1;
		}

		// Token: 0x06002ACE RID: 10958 RVA: 0x0006F52C File Offset: 0x0006E52C
		private int CalculateSizeUnion(_ISignature sign, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			IEnumerable<_IVariable> allVariables = sign.AllVariables;
			int num = 0;
			foreach (_IVariable ivariable in allVariables)
			{
				ICompiledType vartype = ivariable.Type as ICompiledType;
				int num2 = this._typeSizeCalculator.CalculateTypeSize(sign, vartype, dicSizes, libtable, recursionGuard);
				if (num2 == -1)
				{
					return -1;
				}
				num = Help.max(new int[]
				{
					num,
					num2
				});
			}
			return num;
		}

		// Token: 0x06002ACF RID: 10959 RVA: 0x0006F5B4 File Offset: 0x0006E5B4
		private ICompiledType GetResolvedXType(ICompiledType ctype)
		{
			if ((!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351740 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800) && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351810)
			{
				return ctype;
			}
			switch (ctype.Class)
			{
			case TypeClass.UXInt:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.UDInt;
				}
				return TypeTable.ULInt;
			case TypeClass.XWord:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.DWord;
				}
				return TypeTable.LWord;
			case TypeClass.XInt:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.DInt;
				}
				return TypeTable.LInt;
			default:
				return ctype;
			}
		}

		// Token: 0x06002AD0 RID: 10960 RVA: 0x0006F66C File Offset: 0x0006E66C
		private int CalculateSignatureSize(_ISignature sign, IPrecompileScope4 prescope, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			if (sign.POUType == Operator.Type && !sign.GetFlag(SignatureFlag.Structure))
			{
				return -1;
			}
			IExpression[] interfaceExpressions = sign.InterfaceExpressions;
			int num = SizeCalculatorHelpFunctions.CalculateNumOfInterfaces(sign, interfaceExpressions, prescope);
			bool flag;
			int num2 = this.CalculateBaseSignatureSize(sign, prescope, dicSizes, libtable, recursionGuard, out flag);
			if (flag)
			{
				return -1;
			}
			int num3 = 0;
			if (sign.POUType == Operator.FunctionBlock)
			{
				num3 += this.PrecompilePointerSize;
			}
			if (sign.BaseExpression != null)
			{
				num3 = num2;
			}
			num3 += num * this.PrecompilePointerSize;
			ITargetSettings targetSettings = this.PreCompileContext.GetTargetSettings();
			int num4 = -1;
			uint num5 = 0U;
			int iGranularityGlob = 1;
			if (sign.POUType == Operator.FunctionBlock)
			{
				iGranularityGlob = this.PrecompilePointerSize;
			}
			int iMinSize = SignatureSizeCalculator.EvaluateMinSizeAttribute(sign);
			int iPackMode = SignatureSizeCalculator.EvaluatePackMode(sign, targetSettings);
			IList<_IVariable> allVariables = sign.AllVariables;
			for (int i = 0; i < allVariables.Count; i++)
			{
				IVariable3 variable = allVariables[i];
				if (!SignatureSizeCalculator.VariableToIgnore(variable))
				{
					int num6 = num3;
					ICompiledType compiledType = variable.Type as ICompiledType;
					if (CompilerProxy._TypeTable.IsXType(compiledType))
					{
						compiledType = this.GetResolvedXType(compiledType);
					}
					int num7 = this._typeSizeCalculator.CalculateTypeSize(sign, compiledType, dicSizes, libtable, recursionGuard);
					if (num7 == -1)
					{
						return -1;
					}
					if (!SignatureSizeCalculator.CalculateBitOffset(compiledType, num6, ref num4, ref num5, ref num3))
					{
						num5 = 0U;
						num4 = -1;
						num6 = SignatureSizeCalculator.AdaptLocalOffsetForGranularity(prescope, compiledType, iMinSize, iPackMode, ref iGranularityGlob, variable, num6);
						num6 += num7;
						num3 = num6;
					}
				}
			}
			num3 = SignatureSizeCalculator.AlignToGranularity(num3, iGranularityGlob);
			return num3;
		}

		// Token: 0x06002AD1 RID: 10961 RVA: 0x0006F7DF File Offset: 0x0006E7DF
		private static int AlignToGranularity(int iOffset, int iGranularityGlob)
		{
			while (iOffset % iGranularityGlob != 0)
			{
				iOffset++;
			}
			return iOffset;
		}

		// Token: 0x06002AD2 RID: 10962 RVA: 0x0006F7F0 File Offset: 0x0006E7F0
		private static bool VariableToIgnore(IVariable3 var)
		{
			return var.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION) || var.GetFlag(VarFlag.Absolut) || (var.GetFlag(VarFlag.ReplacedConstant) || (((_IVariable)var).IsProperty && !((_IVariable)var).IsPropertyMonitor)) || (var.Address != null && !var.Address.Incomplete);
		}

		// Token: 0x06002AD3 RID: 10963 RVA: 0x0006F85C File Offset: 0x0006E85C
		private int CalculateBaseSignatureSize(_ISignature sign, IPrecompileScope4 prescope, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard, out bool bError)
		{
			int num = 0;
			if (sign.BaseExpression != null)
			{
				ISignature3 signature = prescope.FindSignatureGlobal(sign.BaseExpression) as ISignature3;
				if (signature == null)
				{
					bError = true;
					return -1;
				}
				num = this.CalculatePrecompileSize(signature, prescope, dicSizes, libtable, recursionGuard);
				if (num == -1)
				{
					bError = true;
					return -1;
				}
			}
			bError = false;
			return num;
		}

		// Token: 0x06002AD4 RID: 10964 RVA: 0x0006F8AC File Offset: 0x0006E8AC
		private static int AdaptLocalOffsetForGranularity(IPrecompileScope4 prescope, ICompiledType vartype, int iMinSize, int iPackMode, ref int iGranularityGlob, IVariable3 var, int iLocalOffset)
		{
			int num = GranularityCalculator.GetGranularity(vartype, iMinSize, prescope);
			if (num > iPackMode)
			{
				num = iPackMode;
			}
			if (num > iGranularityGlob)
			{
				iGranularityGlob = num;
			}
			if (iLocalOffset % num != 0)
			{
				iLocalOffset = (iLocalOffset / num + 1) * num;
			}
			if (var.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
			{
				iLocalOffset = int.Parse(var.GetAttributeValue(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET));
			}
			return iLocalOffset;
		}

		// Token: 0x06002AD5 RID: 10965 RVA: 0x0006F905 File Offset: 0x0006E905
		private static bool CalculateBitOffset(ICompiledType vartype, int iLocalOffset, ref int iOffsetLastBitOffset, ref uint uiBitNum, ref int iOffset)
		{
			if (vartype != null && vartype.Class == TypeClass.Bit)
			{
				if (iOffsetLastBitOffset == -1)
				{
					iOffsetLastBitOffset = iLocalOffset;
					iLocalOffset++;
				}
				if (uiBitNum == 8U)
				{
					uiBitNum = 0U;
					iOffsetLastBitOffset++;
					iLocalOffset++;
				}
				uiBitNum += 1U;
				iOffset = iLocalOffset;
				return true;
			}
			return false;
		}

		// Token: 0x06002AD6 RID: 10966 RVA: 0x0006F940 File Offset: 0x0006E940
		private int CalculatePrecompileSize_Enum(_ISignature sign, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			if (sign.All.Length != 0)
			{
				return this._typeSizeCalculator.CalculateTypeSize(sign, sign.All[0].Type as ICompiledType, dicSizes, libtable, recursionGuard);
			}
			return 4;
		}

		// Token: 0x06002AD7 RID: 10967 RVA: 0x0006F970 File Offset: 0x0006E970
		private static int EvaluatePackMode(_ISignature sign, ITargetSettings tarset)
		{
			int result = LocalTargetSettings.PackMode.GetIntValue(tarset);
			int packMode = sign.PackMode;
			if (packMode != -1)
			{
				result = packMode;
			}
			return result;
		}

		// Token: 0x06002AD8 RID: 10968 RVA: 0x0006F998 File Offset: 0x0006E998
		private static int EvaluateMinSizeAttribute(_ISignature sign)
		{
			int num = 0;
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE))
			{
				string attributeValue = sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE);
				try
				{
					num = int.Parse(attributeValue);
				}
				catch
				{
					num = 0;
				}
				if (num != 1 && num != 2 && num != 4 && num != 8)
				{
					num = 0;
				}
			}
			return num;
		}

		// Token: 0x04000826 RID: 2086
		private readonly PreCompileSizeCalculator _preCompileSizeCalculator;

		// Token: 0x04000827 RID: 2087
		private readonly TypeSizeCalculator _typeSizeCalculator;
	}
}
