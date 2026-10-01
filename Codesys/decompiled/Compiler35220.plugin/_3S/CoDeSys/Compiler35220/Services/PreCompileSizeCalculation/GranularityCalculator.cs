using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services.PreCompileSizeCalculation
{
	// Token: 0x0200012D RID: 301
	public class GranularityCalculator : IGranularityCalculator
	{
		// Token: 0x06001593 RID: 5523 RVA: 0x0003E934 File Offset: 0x0003CB34
		private int \u0001(_ISignature \u0002, int \u0003, IPrecompileScope4 \u0004)
		{
			IPrecompileScope4 u = (IPrecompileScope4)\u0004.NewLocalScope(\u0002);
			bool flag;
			int num = this.\u0001(\u0002, \u0003, u, out flag);
			if (flag)
			{
				return num;
			}
			num = this.\u0001(\u0002, \u0003, u, num);
			return GranularityCalculator.\u0001(\u0002, num);
		}

		// Token: 0x06001594 RID: 5524 RVA: 0x0003E974 File Offset: 0x0003CB74
		private static int \u0001(_ISignature \u0002, int \u0003)
		{
			if (\u0002.POUType == Operator.FunctionBlock && TypeTable.PointerSize(null) > \u0003)
			{
				\u0003 = TypeTable.PointerSize(null);
			}
			return \u0003;
		}

		// Token: 0x06001595 RID: 5525 RVA: 0x0003E994 File Offset: 0x0003CB94
		private int \u0001(_ISignature \u0002, int \u0003, IPrecompileScope4 \u0004, int \u0005)
		{
			if (\u0002.BaseExpression != null)
			{
				ISignature signature = \u0004.FindSignatureGlobal(\u0002.BaseExpression);
				int num = this.\u0001(signature as _ISignature, \u0003, \u0004);
				if (num > \u0005)
				{
					\u0005 = num;
				}
			}
			return \u0005;
		}

		// Token: 0x06001596 RID: 5526 RVA: 0x0003E9D0 File Offset: 0x0003CBD0
		private int \u0001(_ISignature \u0002, int \u0003, IPrecompileScope4 \u0004, out bool \u0005)
		{
			IEnumerable<_IVariable> allVariables = \u0002.AllVariables;
			int num = 1;
			\u0005 = false;
			foreach (_IVariable ivariable in allVariables)
			{
				int granularity = this.GetGranularity(ivariable.Type as ICompiledType, \u0003, \u0004);
				if (granularity == -1)
				{
					\u0005 = true;
					return -1;
				}
				if (granularity > num)
				{
					num = granularity;
				}
			}
			return num;
		}

		// Token: 0x06001597 RID: 5527 RVA: 0x0003EA48 File Offset: 0x0003CC48
		public int GetGranularity(ICompiledType type, int iMinSize, IPrecompileScope4 prescope)
		{
			TypeClass @class = type.Class;
			if (@class == TypeClass.String)
			{
				return 1;
			}
			if (@class == TypeClass.WString)
			{
				return 2;
			}
			switch (@class)
			{
			case TypeClass.Subrange:
			{
				_ISubrangeType isubrangeType = (_ISubrangeType)type;
				return this.GetGranularity(isubrangeType._Base, iMinSize, prescope);
			}
			case TypeClass.Enum:
				return this.GetGranularity(((_IEnumType)type)._Base, iMinSize, prescope);
			case TypeClass.Array:
			{
				_IArrayType iarrayType = (_IArrayType)type.DeRefType;
				return this.GetGranularity(iarrayType.BaseType, iMinSize, prescope);
			}
			case TypeClass.Userdef:
				return this.\u0001(type, iMinSize, prescope);
			}
			int size = TypeTable.GetSize(type.Class, null);
			if (size < iMinSize)
			{
				return iMinSize;
			}
			return size;
		}

		// Token: 0x06001598 RID: 5528 RVA: 0x0003EAEC File Offset: 0x0003CCEC
		private int \u0001(ICompiledType \u0002, int \u0003, IPrecompileScope4 \u0004)
		{
			_IUserdefType iuserdefType = (_IUserdefType)\u0002.DeRefType;
			ISignature signature = \u0004.FindSignatureGlobal(iuserdefType.NameExpression);
			if (signature == null)
			{
				return -1;
			}
			int num;
			if (signature.POUType == Operator.Interface || signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				num = TypeTable.PointerSize(null);
			}
			else
			{
				num = this.\u0001(signature as _ISignature, \u0003, \u0004);
			}
			int packMode = ((_ISignature)signature).PackMode;
			if (packMode != -1 && num > packMode)
			{
				num = packMode;
			}
			return num;
		}
	}
}
