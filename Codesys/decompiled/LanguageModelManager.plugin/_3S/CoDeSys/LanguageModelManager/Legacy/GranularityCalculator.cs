using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027B RID: 635
	public static class GranularityCalculator
	{
		// Token: 0x06002AB6 RID: 10934 RVA: 0x0006EA98 File Offset: 0x0006DA98
		private static int GetGranularity(_ISignature sign, int iMinSize, IPrecompileScope4 prescope)
		{
			IPrecompileScope4 scope;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				scope = (IPrecompileScope4)prescope.NewLocalScope(sign);
			}
			else
			{
				scope = prescope;
			}
			bool flag;
			int num = GranularityCalculator.GetGranularityOfVariables(sign, iMinSize, scope, out flag);
			if (flag)
			{
				return num;
			}
			num = GranularityCalculator.GetGranularityOfBaseType(sign, iMinSize, scope, num);
			return GranularityCalculator.AdaptFunctionBlockMinimalGranularity(sign, num);
		}

		// Token: 0x06002AB7 RID: 10935 RVA: 0x0006EAEA File Offset: 0x0006DAEA
		private static int AdaptFunctionBlockMinimalGranularity(_ISignature sign, int nGranularity)
		{
			if (sign.POUType == Operator.FunctionBlock && TypeTable.PointerSize(null) > nGranularity)
			{
				nGranularity = TypeTable.PointerSize(null);
			}
			return nGranularity;
		}

		// Token: 0x06002AB8 RID: 10936 RVA: 0x0006EB08 File Offset: 0x0006DB08
		private static int GetGranularityOfBaseType(_ISignature sign, int iMinSize, IPrecompileScope4 scope, int nGranularity)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33220 && sign.BaseExpression != null)
			{
				int granularity = GranularityCalculator.GetGranularity(scope.FindSignatureGlobal(sign.BaseExpression) as _ISignature, iMinSize, scope);
				if (granularity > nGranularity)
				{
					nGranularity = granularity;
				}
			}
			return nGranularity;
		}

		// Token: 0x06002AB9 RID: 10937 RVA: 0x0006EB50 File Offset: 0x0006DB50
		private static int GetGranularityOfVariables(_ISignature sign, int iMinSize, IPrecompileScope4 scope, out bool bError)
		{
			IEnumerable<_IVariable> allVariables = sign.AllVariables;
			int num = 1;
			bError = false;
			foreach (_IVariable ivariable in allVariables)
			{
				int granularity = GranularityCalculator.GetGranularity(ivariable.Type as ICompiledType, iMinSize, scope);
				if (granularity == -1)
				{
					bError = true;
					return -1;
				}
				if (granularity > num)
				{
					num = granularity;
				}
			}
			return num;
		}

		// Token: 0x06002ABA RID: 10938 RVA: 0x0006EBC0 File Offset: 0x0006DBC0
		public static int GetGranularity(ICompiledType type, int iMinSize, IPrecompileScope4 prescope)
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
				return GranularityCalculator.GetGranularity(((SubrangeType)type).BaseType, iMinSize, prescope);
			case TypeClass.Enum:
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
				{
					return GranularityCalculator.GetGranularity(((_IEnumType)type)._Base, iMinSize, prescope);
				}
				break;
			case TypeClass.Array:
				return GranularityCalculator.GetGranularity(((ArrayType)type.DeRefType).BaseType, iMinSize, prescope);
			case TypeClass.Userdef:
				return GranularityCalculator.CalculateUserdefTypeGranularity(type, iMinSize, prescope);
			}
			int size = TypeTable.GetSize(type.Class, null);
			if (size < iMinSize)
			{
				return iMinSize;
			}
			return size;
		}

		// Token: 0x06002ABB RID: 10939 RVA: 0x0006EC70 File Offset: 0x0006DC70
		private static int CalculateUserdefTypeGranularity(ICompiledType type, int iMinSize, IPrecompileScope4 prescope)
		{
			UserdefType userdefType = (UserdefType)type.DeRefType;
			ISignature signature = prescope.FindSignatureGlobal(userdefType.NameExpression);
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
				num = GranularityCalculator.GetGranularity(signature as _ISignature, iMinSize, prescope);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				int packMode = ((_ISignature)signature).PackMode;
				if (packMode != -1 && num > packMode)
				{
					num = packMode;
				}
			}
			return num;
		}
	}
}
