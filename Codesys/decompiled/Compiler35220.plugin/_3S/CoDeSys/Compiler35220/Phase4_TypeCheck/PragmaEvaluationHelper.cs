using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase4_TypeCheck
{
	// Token: 0x020002DC RID: 732
	public static class PragmaEvaluationHelper
	{
		// Token: 0x06002BF5 RID: 11253 RVA: 0x0009A244 File Offset: 0x00098444
		public static void visitVersionSupportingPragmaExpression(_IPragmaExpression pragmaExpr, Operator opComparison, Version currVersion, Version versionToTest)
		{
			switch (opComparison)
			{
			case Operator.Eq:
				goto IL_81;
			case Operator.Ne:
				goto IL_8F;
			case Operator.Ge:
				break;
			case Operator.Gt:
				goto IL_57;
			case Operator.Le:
				goto IL_73;
			case Operator.Lt:
				goto IL_65;
			default:
				switch (opComparison)
				{
				case Operator.Less:
					goto IL_65;
				case Operator.Greater:
					goto IL_57;
				case Operator.LessEqual:
					goto IL_73;
				case Operator.GreaterEqual:
					break;
				case Operator.Equal:
					goto IL_81;
				case Operator.NotEqual:
					goto IL_8F;
				default:
					return;
				}
				break;
			}
			pragmaExpr.Value = (currVersion >= versionToTest);
			return;
			IL_57:
			pragmaExpr.Value = (currVersion > versionToTest);
			return;
			IL_65:
			pragmaExpr.Value = (currVersion < versionToTest);
			return;
			IL_73:
			pragmaExpr.Value = (currVersion <= versionToTest);
			return;
			IL_81:
			pragmaExpr.Value = (currVersion == versionToTest);
			return;
			IL_8F:
			pragmaExpr.Value = (currVersion != versionToTest);
		}

		// Token: 0x06002BF6 RID: 11254 RVA: 0x0009A2F0 File Offset: 0x000984F0
		public static bool MatchesAnyType(TypeClass anyType, ICompiledType typeToCheck)
		{
			bool result = false;
			switch (anyType)
			{
			case TypeClass.Any:
				result = (TypeTable.IsInteger(typeToCheck.Class) || TypeTable.IsReal(typeToCheck.Class) || TypeTable.IsTimeOrDateType(typeToCheck.Class) || TypeTable.IsString(typeToCheck.Class) || TypeTable.IsBit(typeToCheck.Class));
				break;
			case TypeClass.AnyBit:
				result = TypeTable.IsBit(typeToCheck.Class);
				break;
			case TypeClass.AnyDate:
				result = TypeTable.IsTimeOrDateType(typeToCheck.Class);
				break;
			case TypeClass.AnyInt:
				result = TypeTable.IsInteger(typeToCheck.Class);
				break;
			case TypeClass.AnyNum:
				result = (TypeTable.IsInteger(typeToCheck.Class) || TypeTable.IsReal(typeToCheck.Class));
				break;
			case TypeClass.AnyReal:
				result = TypeTable.IsReal(typeToCheck.Class);
				break;
			default:
				if (anyType == TypeClass.AnyString)
				{
					result = TypeTable.IsString(typeToCheck.Class);
				}
				break;
			}
			return result;
		}

		// Token: 0x06002BF7 RID: 11255 RVA: 0x0009A3D8 File Offset: 0x000985D8
		public static bool MatchesWhereKindOfMatch(_IHasConstantValueExpression2 hasvalue, ILiteralValue constantLiteral, ILiteralValue literalLiteral)
		{
			bool result = false;
			switch (literalLiteral.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				result = PragmaEvaluationHelper.\u0001<long>(hasvalue.OpComparison, literalLiteral.SignedLong, constantLiteral.SignedLong);
				break;
			case KindOfLiteral.UnsignedInteger:
				result = PragmaEvaluationHelper.\u0001<ulong>(hasvalue.OpComparison, literalLiteral.UnsignedLong, constantLiteral.UnsignedLong);
				break;
			case KindOfLiteral.Float:
				result = PragmaEvaluationHelper.\u0001<double>(hasvalue.OpComparison, literalLiteral.Float, constantLiteral.Float);
				break;
			case KindOfLiteral.String:
				result = PragmaEvaluationHelper.\u0001<string>(hasvalue.OpComparison, literalLiteral.String, constantLiteral.String);
				break;
			case KindOfLiteral.Bool:
				result = PragmaEvaluationHelper.\u0001(hasvalue.OpComparison, literalLiteral.Bool, constantLiteral.Bool);
				break;
			}
			return result;
		}

		// Token: 0x06002BF8 RID: 11256 RVA: 0x0009A490 File Offset: 0x00098690
		public static bool MatchesWhereKindOfDontMatch(_IHasConstantValueExpression2 hasvalue, ILiteralValue constantLiteral, ILiteralValue literalLiteral)
		{
			bool result = false;
			switch (constantLiteral.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				if (literalLiteral.KindOf == KindOfLiteral.UnsignedInteger && 0L <= constantLiteral.SignedLong)
				{
					result = PragmaEvaluationHelper.\u0001<ulong>(hasvalue.OpComparison, literalLiteral.UnsignedLong, (ulong)constantLiteral.SignedLong);
				}
				break;
			case KindOfLiteral.UnsignedInteger:
				if (literalLiteral.KindOf == KindOfLiteral.SignedInteger && 0L <= literalLiteral.SignedLong)
				{
					result = PragmaEvaluationHelper.\u0001<ulong>(hasvalue.OpComparison, (ulong)literalLiteral.SignedLong, constantLiteral.UnsignedLong);
				}
				break;
			case KindOfLiteral.Bool:
				if (literalLiteral.KindOf == KindOfLiteral.SignedInteger && 0L <= literalLiteral.SignedLong)
				{
					ulong u = (ulong)(constantLiteral.Bool ? 1L : 0L);
					result = PragmaEvaluationHelper.\u0001<ulong>(hasvalue.OpComparison, (ulong)literalLiteral.SignedLong, u);
				}
				break;
			}
			return result;
		}

		// Token: 0x06002BF9 RID: 11257 RVA: 0x0009A558 File Offset: 0x00098758
		private static bool \u0001<\u0001>(Operator \u0002, \u0001 \u0003, \u0001 \u0004) where \u0001 : IComparable
		{
			switch (\u0002)
			{
			case Operator.Eq:
				break;
			case Operator.Ne:
				goto IL_64;
			case Operator.Ge:
				goto IL_7B;
			case Operator.Gt:
				goto IL_95;
			case Operator.Le:
				goto IL_AC;
			case Operator.Lt:
				goto IL_C6;
			default:
				switch (\u0002)
				{
				case Operator.Less:
					goto IL_C6;
				case Operator.Greater:
					goto IL_95;
				case Operator.LessEqual:
					goto IL_AC;
				case Operator.GreaterEqual:
					goto IL_7B;
				case Operator.Equal:
					break;
				case Operator.NotEqual:
					goto IL_64;
				default:
					return false;
				}
				break;
			}
			return \u0004.CompareTo(\u0003) == 0;
			IL_64:
			return \u0004.CompareTo(\u0003) != 0;
			IL_7B:
			return \u0004.CompareTo(\u0003) >= 0;
			IL_95:
			return \u0004.CompareTo(\u0003) > 0;
			IL_AC:
			return \u0004.CompareTo(\u0003) <= 0;
			IL_C6:
			return \u0004.CompareTo(\u0003) < 0;
		}

		// Token: 0x06002BFA RID: 11258 RVA: 0x0009A644 File Offset: 0x00098844
		private static bool \u0001(Operator \u0002, bool \u0003, bool \u0004)
		{
			if (\u0002 <= Operator.Ne)
			{
				if (\u0002 != Operator.Eq)
				{
					if (\u0002 != Operator.Ne)
					{
						return false;
					}
					goto IL_31;
				}
			}
			else if (\u0002 != Operator.Equal)
			{
				if (\u0002 != Operator.NotEqual)
				{
					return false;
				}
				goto IL_31;
			}
			return \u0003 == \u0004;
			IL_31:
			return \u0003 != \u0004;
		}
	}
}
