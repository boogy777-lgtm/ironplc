using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class FindSubelementsArrayHelper
	{
		private static int GetIntValue(IExpression expr, IPrecompileScope scope, out bool bValid)
		{
			return PreCompileUtilities._GetIntValue(expr, scope, out bValid);
		}

		internal static IIdentifierInfo[] FindSubelementsArrayType(FindSubelementsFlags flags, IPrecompileScope6 pscope, ICompiledType ctype)
		{
			if (ctype != null && ctype.DeRefType != null && ctype.DeRefType is IArrayType && (flags & FindSubelementsFlags.IncludeArrayElements) != 0)
			{
				return FindSubelementsArrayType(pscope, ctype);
			}
			return Array.Empty<IIdentifierInfo>();
		}

		private static IIdentifierInfo[] FindSubelementsArrayType(IPrecompileScope6 pscope, ICompiledType ctype)
		{
			IArrayType arrayType = (IArrayType)ctype.DeRefType;
			switch (arrayType.Dimensions.Length)
			{
			case 1:
				return FindSubelementsArrayType1Dim(pscope, arrayType);
			case 2:
				return FindSubelementsArrayType2Dim(pscope, arrayType);
			case 3:
				return FindSubelementsArrayType3Dim(pscope, arrayType);
			default:
				return Array.Empty<IIdentifierInfo>();
			}
		}

		private static IIdentifierInfo[] FindSubelementsArrayType1Dim(IPrecompileScope6 pscope, IArrayType arrayType)
		{
			bool bValid;
			int intValue = GetIntValue(arrayType.Dimensions[0].LowerBorder, pscope, out bValid);
			bool bValid2;
			int intValue2 = GetIntValue(arrayType.Dimensions[0].UpperBorder, pscope, out bValid2);
			if (bValid && bValid2)
			{
				List<IIdentifierInfo> list = new List<IIdentifierInfo>();
				for (int i = intValue; i <= intValue2; i++)
				{
					IIdentifierInfo item = new IdentifierInfo($"[{i}]", string.Empty, IdentifierInfoFlag.Variable, arrayType.Base);
					list.Add(item);
				}
				return list.ToArray();
			}
			return Array.Empty<IIdentifierInfo>();
		}

		private static IIdentifierInfo[] FindSubelementsArrayType2Dim(IPrecompileScope6 pscope, IArrayType arrayType)
		{
			bool bValid;
			int intValue = GetIntValue(arrayType.Dimensions[0].LowerBorder, pscope, out bValid);
			bool bValid2;
			int intValue2 = GetIntValue(arrayType.Dimensions[0].UpperBorder, pscope, out bValid2);
			bool bValid3;
			int intValue3 = GetIntValue(arrayType.Dimensions[1].LowerBorder, pscope, out bValid3);
			bool bValid4;
			int intValue4 = GetIntValue(arrayType.Dimensions[1].UpperBorder, pscope, out bValid4);
			if (bValid && bValid2 && bValid3 && bValid4)
			{
				List<IIdentifierInfo> list = new List<IIdentifierInfo>();
				for (int i = intValue; i <= intValue2; i++)
				{
					for (int j = intValue3; j <= intValue4; j++)
					{
						IIdentifierInfo item = new IdentifierInfo($"[{i}, {j}]", string.Empty, IdentifierInfoFlag.Variable, arrayType.Base);
						list.Add(item);
					}
				}
				return list.ToArray();
			}
			return Array.Empty<IIdentifierInfo>();
		}

		private static IIdentifierInfo[] FindSubelementsArrayType3Dim(IPrecompileScope6 pscope, IArrayType arrayType)
		{
			bool bValid;
			int intValue = GetIntValue(arrayType.Dimensions[0].LowerBorder, pscope, out bValid);
			bool bValid2;
			int intValue2 = GetIntValue(arrayType.Dimensions[0].UpperBorder, pscope, out bValid2);
			bool bValid3;
			int intValue3 = GetIntValue(arrayType.Dimensions[1].LowerBorder, pscope, out bValid3);
			bool bValid4;
			int intValue4 = GetIntValue(arrayType.Dimensions[1].UpperBorder, pscope, out bValid4);
			bool bValid5;
			int intValue5 = GetIntValue(arrayType.Dimensions[2].LowerBorder, pscope, out bValid5);
			bool bValid6;
			int intValue6 = GetIntValue(arrayType.Dimensions[2].UpperBorder, pscope, out bValid6);
			if (bValid && bValid2 && bValid3 && bValid4 && bValid5 && bValid6)
			{
				List<IIdentifierInfo> list = new List<IIdentifierInfo>();
				for (int i = intValue; i <= intValue2; i++)
				{
					for (int j = intValue3; j <= intValue4; j++)
					{
						for (int k = intValue5; k <= intValue6; k++)
						{
							IIdentifierInfo item = new IdentifierInfo($"[{i}, {j}, {k}]", string.Empty, IdentifierInfoFlag.Variable, arrayType.Base);
							list.Add(item);
						}
					}
				}
				return list.ToArray();
			}
			return Array.Empty<IIdentifierInfo>();
		}
	}
}
