using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0016;
using \u001C;
using \u001F;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services.PreCompileSizeCalculation
{
	// Token: 0x0200012F RID: 303
	public class SignatureSizeCalculator
	{
		// Token: 0x0600159E RID: 5534 RVA: 0x0003EC44 File Offset: 0x0003CE44
		public SignatureSizeCalculator(_IPreCompileContext preCompileContext, PreCompileSizeCalculator preCompileSizeCalculator)
		{
			this.PreCompileContext = preCompileContext;
			this.\u0001 = new TypeSizeCalculator(this, this.PreCompileContext);
			this.\u0001 = preCompileSizeCalculator;
		}

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x0600159F RID: 5535 RVA: 0x0003EC6C File Offset: 0x0003CE6C
		private _IPreCompileContext PreCompileContext { get; }

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x060015A0 RID: 5536 RVA: 0x0003EC74 File Offset: 0x0003CE74
		public int PrecompilePointerSize
		{
			get
			{
				return this.PreCompileContext.PointerSize;
			}
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x0003EC84 File Offset: 0x0003CE84
		public int CalculateTypeSize(_IPreCompileContext precom, ISignature sign, IType type, IRecursionGuard recursionGuard)
		{
			LDictionary<ISignature, int> dicSizes = new LDictionary<ISignature, int>();
			return this.\u0001.CalculateTypeSize(precom, sign, type as ICompiledType, dicSizes, this.PreCompileContext._GetLibraryTable(), recursionGuard);
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x0003ECB8 File Offset: 0x0003CEB8
		public int CalculateSignatureSize(_IPreCompileContext precom, ISignature3 sign, IRecursionGuard recursionGuard)
		{
			LDictionary<ISignature, int> dicSizes = new LDictionary<ISignature, int>();
			_ILibraryTable ilibraryTable = this.PreCompileContext._GetLibraryTable();
			_IPrecompileScope iprecompileScope = new Helper().\u0001(this.PrecompilePointerSize, ilibraryTable, sign as _ISignature, this.PreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			iprecompileScope.SetPointerSize();
			IPrecompileScope4 prescope = iprecompileScope;
			return this.CalculatePrecompileSize(precom, sign, prescope, dicSizes, ilibraryTable, recursionGuard);
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x0003ED18 File Offset: 0x0003CF18
		public int CalculatePrecompileSize(_IPreCompileContext precom, ISignature3 sign, IPrecompileScope4 prescope, LDictionary<ISignature, int> dicSizes, _ILibraryTable libtable, IRecursionGuard recursionGuard)
		{
			int num;
			if (dicSizes.TryGetValue(sign, ref num))
			{
				return num;
			}
			dicSizes[sign] = -1;
			num = this.\u0001(precom, sign as _ISignature, prescope, dicSizes, libtable, recursionGuard);
			dicSizes[sign] = num;
			return num;
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x0003ED5C File Offset: 0x0003CF5C
		private int \u0001(_IPreCompileContext \u0002, _ISignature \u0003, IPrecompileScope4 \u0004, LDictionary<ISignature, int> \u0005, _ILibraryTable \u0006, IRecursionGuard \u0007)
		{
			Operator poutype = \u0003.POUType;
			if (poutype <= Operator.Type)
			{
				if (poutype != Operator.FunctionBlock)
				{
					if (poutype != Operator.Type)
					{
						return -1;
					}
					if (\u0003.GetFlag(SignatureFlag.Alias))
					{
						if (\u0003.All.Length != 0)
						{
							return this.\u0001.CalculateTypeSize(\u0002, \u0003, \u0003.All[0].Type as ICompiledType, \u0007);
						}
					}
					else if (\u0003.GetFlag(SignatureFlag.Union))
					{
						return this.\u0001(\u0002, \u0003, \u0005, \u0006, \u0007);
					}
				}
				return this.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
			}
			if (poutype != Operator.VarGlobal)
			{
				if (poutype == Operator.Interface)
				{
					return this.PrecompilePointerSize;
				}
			}
			else if (\u0003.GetFlag(SignatureFlag.Enum))
			{
				return this.\u0002(\u0002, \u0003, \u0005, \u0006, \u0007);
			}
			return -1;
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x0003EE18 File Offset: 0x0003D018
		private int \u0001(_IPreCompileContext \u0002, _ISignature \u0003, LDictionary<ISignature, int> \u0004, _ILibraryTable \u0005, IRecursionGuard \u0006)
		{
			IEnumerable<_IVariable> allVariables = \u0003.AllVariables;
			int num = 0;
			foreach (_IVariable ivariable in allVariables)
			{
				ICompiledType vartype = ivariable.Type as ICompiledType;
				int num2 = this.\u0001.CalculateTypeSize(\u0002, \u0003, vartype, \u0004, \u0005, \u0006);
				if (num2 == -1)
				{
					return -1;
				}
				num = Math.Max(num, num2);
			}
			return num;
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x0003EE94 File Offset: 0x0003D094
		private int \u0002(_IPreCompileContext \u0002, _ISignature \u0003, IPrecompileScope4 \u0004, LDictionary<ISignature, int> \u0005, _ILibraryTable \u0006, IRecursionGuard \u0007)
		{
			if (\u0003.POUType == Operator.Type && !\u0003.GetFlag(SignatureFlag.Structure))
			{
				return -1;
			}
			IExpression[] interfaceExpressions = \u0003.InterfaceExpressions;
			int num = \u001C.\u0007.\u0001(\u0003, interfaceExpressions, \u0004);
			bool flag;
			int num2 = this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, out flag);
			if (flag)
			{
				return -1;
			}
			int num3 = 0;
			if (\u0003.POUType == Operator.FunctionBlock)
			{
				num3 += this.PrecompilePointerSize;
			}
			if (\u0003.BaseExpression != null)
			{
				num3 = num2;
			}
			num3 += num * this.PrecompilePointerSize;
			ITargetSettings targetSettings = this.PreCompileContext.GetTargetSettings();
			int num4 = -1;
			uint num5 = 0U;
			int u = 1;
			if (\u0003.POUType == Operator.FunctionBlock)
			{
				u = this.PrecompilePointerSize;
			}
			int u2 = SignatureSizeCalculator.\u0001(\u0003);
			int u3 = SignatureSizeCalculator.\u0001(\u0003, targetSettings);
			\u001F.\u0005 u4 = new \u001F.\u0005(\u0002);
			IList<_IVariable> allVariables = \u0003.AllVariables;
			for (int i = 0; i < allVariables.Count; i++)
			{
				IVariable3 variable = allVariables[i];
				if (!SignatureSizeCalculator.\u0001(variable))
				{
					int num6 = num3;
					ICompiledType compiledType = u4.\u0001(variable);
					int num7 = this.\u0001.CalculateTypeSize(\u0002, \u0003, compiledType, \u0005, \u0006, \u0007);
					if (num7 == -1)
					{
						return -1;
					}
					if (!SignatureSizeCalculator.\u0001(compiledType, num6, ref num4, ref num5, ref num3))
					{
						num5 = 0U;
						num4 = -1;
						num6 = SignatureSizeCalculator.\u0001(\u0004, compiledType, u2, u3, ref u, variable, num6);
						num6 += num7;
						num3 = num6;
					}
				}
			}
			num3 = SignatureSizeCalculator.\u0001(num3, u);
			return num3;
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x0003EFF8 File Offset: 0x0003D1F8
		private static int \u0001(int \u0002, int \u0003)
		{
			while (\u0002 % \u0003 != 0)
			{
				\u0002++;
			}
			return \u0002;
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x0003F008 File Offset: 0x0003D208
		private static bool \u0001(IVariable3 \u0002)
		{
			return \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION) || \u0002.GetFlag(VarFlag.Absolut) || (\u0002.GetFlag(VarFlag.ReplacedConstant) || (((_IVariable)\u0002).IsProperty && !((_IVariable)\u0002).IsPropertyMonitor)) || (\u0002.Address != null && !\u0002.Address.Incomplete);
		}

		// Token: 0x060015A9 RID: 5545 RVA: 0x0003F074 File Offset: 0x0003D274
		private int \u0001(_IPreCompileContext \u0002, _ISignature \u0003, IPrecompileScope4 \u0004, LDictionary<ISignature, int> \u0005, _ILibraryTable \u0006, IRecursionGuard \u0007, out bool \u0008)
		{
			int num = 0;
			if (\u0003.BaseExpression != null)
			{
				ISignature3 signature = \u0004.FindSignatureGlobal(\u0003.BaseExpression) as ISignature3;
				if (signature == null)
				{
					\u0008 = true;
					return -1;
				}
				num = this.CalculatePrecompileSize(\u0002, signature, \u0004, \u0005, \u0006, \u0007);
				if (num == -1)
				{
					\u0008 = true;
					return -1;
				}
			}
			\u0008 = false;
			return num;
		}

		// Token: 0x060015AA RID: 5546 RVA: 0x0003F0C8 File Offset: 0x0003D2C8
		private static int \u0001(IPrecompileScope4 \u0002, ICompiledType \u0003, int \u0004, int \u0005, ref int \u0006, IVariable3 \u0007, int \u0008)
		{
			int num = new GranularityCalculator().GetGranularity(\u0003, \u0004, \u0002);
			if (num > \u0005)
			{
				num = \u0005;
			}
			if (num > \u0006)
			{
				\u0006 = num;
			}
			if (\u0008 % num != 0)
			{
				\u0008 = (\u0008 / num + 1) * num;
			}
			if (\u0007.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
			{
				\u0008 = int.Parse(\u0007.GetAttributeValue(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET));
			}
			return \u0008;
		}

		// Token: 0x060015AB RID: 5547 RVA: 0x0003F128 File Offset: 0x0003D328
		private static bool \u0001(ICompiledType \u0002, int \u0003, ref int \u0004, ref uint \u0005, ref int \u0006)
		{
			if (\u0002 != null && \u0002.Class == TypeClass.Bit)
			{
				if (\u0004 == -1)
				{
					\u0004 = \u0003;
					\u0003++;
				}
				if (\u0005 == 8U)
				{
					\u0005 = 0U;
					\u0004++;
					\u0003++;
				}
				\u0005 += 1U;
				\u0006 = \u0003;
				return true;
			}
			return false;
		}

		// Token: 0x060015AC RID: 5548 RVA: 0x0003F164 File Offset: 0x0003D364
		private int \u0002(_IPreCompileContext \u0002, _ISignature \u0003, LDictionary<ISignature, int> \u0004, _ILibraryTable \u0005, IRecursionGuard \u0006)
		{
			if (\u0003.All.Length != 0)
			{
				return this.\u0001.CalculateTypeSize(\u0002, \u0003, \u0003.All[0].Type as ICompiledType, \u0004, \u0005, \u0006);
			}
			return 4;
		}

		// Token: 0x060015AD RID: 5549 RVA: 0x0003F198 File Offset: 0x0003D398
		private static int \u0001(_ISignature \u0002, ITargetSettings \u0003)
		{
			int result = \u0016.\u0004.PackMode.GetIntValue(\u0003);
			int packMode = \u0002.PackMode;
			if (packMode != -1)
			{
				result = packMode;
			}
			return result;
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x0003F1C0 File Offset: 0x0003D3C0
		private static int \u0001(_ISignature \u0002)
		{
			int num = 0;
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE))
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE);
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

		// Token: 0x040003BD RID: 957
		private readonly PreCompileSizeCalculator \u0001;

		// Token: 0x040003BE RID: 958
		private readonly TypeSizeCalculator \u0001;

		// Token: 0x040003BF RID: 959
		[CompilerGenerated]
		private readonly _IPreCompileContext \u0001;
	}
}
