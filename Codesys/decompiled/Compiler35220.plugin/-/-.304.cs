using System;
using System.Linq;
using \u0015;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0006
{
	// Token: 0x02000334 RID: 820
	internal sealed class \u0011 : ITypeComparer
	{
		// Token: 0x06003166 RID: 12646 RVA: 0x000BE408 File Offset: 0x000BC608
		public bool \u0001(TypeClass \u0002, TypeClass \u0003, bool \u0004, bool \u0005, bool \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06003167 RID: 12647 RVA: 0x000BE418 File Offset: 0x000BC618
		public bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06003168 RID: 12648 RVA: 0x000BE424 File Offset: 0x000BC624
		public bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06003169 RID: 12649 RVA: 0x000BE430 File Offset: 0x000BC630
		public bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x0600316A RID: 12650 RVA: 0x000BE440 File Offset: 0x000BC640
		public bool \u0001(_IUserdefType \u0002, _IUserdefType \u0003, IScope2 \u0004, IScope2 \u0005)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600316B RID: 12651 RVA: 0x000BE44C File Offset: 0x000BC64C
		public bool \u0001(ISignature \u0002, ISignature \u0003, IScope2 \u0004)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600316C RID: 12652 RVA: 0x000BE458 File Offset: 0x000BC658
		public bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600316D RID: 12653 RVA: 0x000BE464 File Offset: 0x000BC664
		public bool \u0003(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0003(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600316E RID: 12654 RVA: 0x000BE470 File Offset: 0x000BC670
		public bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			return \u0011.\u0004(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600316F RID: 12655 RVA: 0x000BE47C File Offset: 0x000BC67C
		public bool \u0001(ISignature \u0002, ISignature \u0003, IScope2 \u0004, IScope2 \u0005)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06003170 RID: 12656 RVA: 0x000BE488 File Offset: 0x000BC688
		public bool \u0001(ICompiledType \u0002, ICommonScope \u0003)
		{
			return \u0011.\u0001(\u0002, \u0003);
		}

		// Token: 0x06003171 RID: 12657 RVA: 0x000BE494 File Offset: 0x000BC694
		public bool \u0001(_ILiteralExpression \u0002, ICompiledType \u0003, ICompiledType \u0004, ICommonScope \u0005, ICommonScope \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06003172 RID: 12658 RVA: 0x000BE4A4 File Offset: 0x000BC6A4
		public ICompiledType \u0001(ICompiledType \u0002, ICommonScope \u0003)
		{
			return \u0011.\u0001(\u0002, \u0003);
		}

		// Token: 0x06003173 RID: 12659 RVA: 0x000BE4B0 File Offset: 0x000BC6B0
		public bool \u0003(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			return \u0011.\u0005(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06003174 RID: 12660 RVA: 0x000BE4BC File Offset: 0x000BC6BC
		public bool \u0004(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			return \u0011.\u0008(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06003175 RID: 12661 RVA: 0x000BE4C8 File Offset: 0x000BC6C8
		public static bool \u0001(TypeClass \u0002, TypeClass \u0003, bool \u0004, bool \u0005, bool \u0006)
		{
			if (\u0004 && TypeTable.IsReal(\u0002) && TypeTable.IsReal(\u0003))
			{
				return true;
			}
			switch (\u0002)
			{
			case TypeClass.Byte:
			case TypeClass.USInt:
				if (\u0003 == TypeClass.Byte || \u0003 == TypeClass.USInt)
				{
					return true;
				}
				return false;
			case TypeClass.Word:
			case TypeClass.UInt:
				if (\u0003 == TypeClass.UInt || \u0003 == TypeClass.Word)
				{
					return true;
				}
				return false;
			case TypeClass.DWord:
			case TypeClass.UDInt:
				if (\u0003 == TypeClass.Pointer && !\u0006)
				{
					return true;
				}
				if (\u0005 && \u0003 == TypeClass.ULInt)
				{
					return true;
				}
				break;
			case TypeClass.LWord:
			case TypeClass.ULInt:
				goto IL_144;
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.String:
			case TypeClass.WString:
				return false;
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
				break;
			case TypeClass.Pointer:
				if (\u0006)
				{
					if (\u0003 == TypeClass.LWord || \u0003 == TypeClass.ULInt)
					{
						return true;
					}
					return false;
				}
				else
				{
					if (\u0003 == TypeClass.DWord || \u0003 == TypeClass.UDInt)
					{
						return true;
					}
					return false;
				}
				break;
			default:
				if (\u0002 == TypeClass.LTime)
				{
					goto IL_144;
				}
				if (\u0002 - TypeClass.LDate > 2)
				{
					return false;
				}
				if (\u0003 <= TypeClass.ULInt)
				{
					if (\u0003 != TypeClass.LWord && \u0003 != TypeClass.ULInt)
					{
						return false;
					}
				}
				else if (\u0003 != TypeClass.LTime && \u0003 - TypeClass.LDate > 2)
				{
					return false;
				}
				return true;
			}
			if (\u0003 <= TypeClass.LWord)
			{
				if (\u0003 != TypeClass.DWord)
				{
					if (\u0003 != TypeClass.LWord)
					{
						return false;
					}
					return \u0005;
				}
			}
			else
			{
				switch (\u0003)
				{
				case TypeClass.UDInt:
				case TypeClass.Time:
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
					break;
				case TypeClass.ULInt:
					return \u0005;
				case TypeClass.Real:
				case TypeClass.LReal:
				case TypeClass.String:
				case TypeClass.WString:
					return false;
				default:
					if (\u0003 != TypeClass.LTime)
					{
						return false;
					}
					return \u0005;
				}
			}
			return true;
			IL_144:
			if (\u0003 <= TypeClass.UDInt)
			{
				if (\u0003 != TypeClass.DWord)
				{
					if (\u0003 == TypeClass.LWord)
					{
						return true;
					}
					if (\u0003 != TypeClass.UDInt)
					{
						return false;
					}
				}
				return \u0005;
			}
			if (\u0003 <= TypeClass.Pointer)
			{
				if (\u0003 != TypeClass.ULInt)
				{
					if (\u0003 != TypeClass.Pointer)
					{
						return false;
					}
					if (\u0006 && (\u0002 == TypeClass.ULInt || \u0002 == TypeClass.LWord))
					{
						return true;
					}
					return false;
				}
			}
			else if (\u0003 != TypeClass.LTime && \u0003 - TypeClass.LDate > 2)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06003176 RID: 12662 RVA: 0x000BE660 File Offset: 0x000BC860
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004 as ICommonScope);
		}

		// Token: 0x06003177 RID: 12663 RVA: 0x000BE670 File Offset: 0x000BC870
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0004);
		}

		// Token: 0x06003178 RID: 12664 RVA: 0x000BE67C File Offset: 0x000BC87C
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005, false);
		}

		// Token: 0x06003179 RID: 12665 RVA: 0x000BE688 File Offset: 0x000BC888
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004, IScope5 \u0005, bool \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004 as ICommonScope, \u0005 as ICommonScope, \u0006);
		}

		// Token: 0x0600317A RID: 12666 RVA: 0x000BE6A0 File Offset: 0x000BC8A0
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, false);
		}

		// Token: 0x0600317B RID: 12667 RVA: 0x000BE6B0 File Offset: 0x000BC8B0
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006, bool \u0007)
		{
			if (\u0002 == null || \u0003 == null)
			{
				return false;
			}
			TypeClass @class = \u0002.Class;
			TypeClass class2 = \u0003.Class;
			if (class2 == TypeClass.AnyInt || class2 == TypeClass.AnyNum)
			{
				return \u0011.\u0008(\u0002, \u0003, \u0004, \u0005);
			}
			if (@class != class2)
			{
				return false;
			}
			switch (@class)
			{
			case TypeClass.String:
			{
				_IStringType type = \u0002 as _IStringType;
				_IStringType type2 = \u0003 as _IStringType;
				return \u0004.GetSize(type) == \u0005.GetSize(type2);
			}
			case TypeClass.WString:
			{
				_IWStringType type3 = \u0002 as _IWStringType;
				_IWStringType type4 = \u0003 as _IWStringType;
				return \u0004.GetSize(type3) == \u0005.GetSize(type4);
			}
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Params:
				break;
			case TypeClass.Pointer:
			case TypeClass.Reference:
				return \u0007 || \u0011.\u0001(\u0002.BaseType, \u0003.BaseType, \u0004, \u0005);
			case TypeClass.Subrange:
				return \u0011.\u0001(\u0002.BaseType, \u0003.BaseType, \u0004, \u0005) && \u0011.\u0001((_ISubrangeType)\u0002, (_ISubrangeType)\u0003, \u0004, \u0005, \u0006);
			case TypeClass.Enum:
				return \u0011.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006);
			case TypeClass.Array:
				return \u0011.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
			case TypeClass.Userdef:
				return \u0011.\u0002(\u0002, \u0003, \u0004, \u0005);
			default:
				if (@class == TypeClass.__Vector)
				{
					return \u0011.\u0003(\u0002, \u0003, \u0004, \u0005);
				}
				break;
			}
			return @class == class2;
		}

		// Token: 0x0600317C RID: 12668 RVA: 0x000BE7F8 File Offset: 0x000BC9F8
		private static bool \u0001(_ISubrangeType \u0002, _ISubrangeType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006)
		{
			ILiteralValue literalValue = \u0011.\u0001(\u0002._LowerBorder, \u0004);
			ILiteralValue literalValue2 = \u0011.\u0001(\u0003._LowerBorder, \u0005);
			ILiteralValue literalValue3 = \u0011.\u0001(\u0002._UpperBorder, \u0004);
			ILiteralValue literalValue4 = \u0011.\u0001(\u0003._UpperBorder, \u0005);
			if (literalValue == null || literalValue2 == null || literalValue3 == null || literalValue4 == null)
			{
				return false;
			}
			int num;
			int num2;
			int num3;
			int num4;
			if (!literalValue.GetInt(out num) || !literalValue2.GetInt(out num2) || !literalValue3.GetInt(out num3) || !literalValue4.GetInt(out num4))
			{
				return false;
			}
			if (\u0006)
			{
				return num3 - num == num4 - num2;
			}
			return num == num2 && num3 == num4;
		}

		// Token: 0x0600317D RID: 12669 RVA: 0x000BE890 File Offset: 0x000BCA90
		private static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006)
		{
			_IEnumType ienumType = \u0002 as _IEnumType;
			_IEnumType ienumType2 = \u0003 as _IEnumType;
			return ienumType.SignatureId == ienumType2.SignatureId && \u0011.\u0001(ienumType.DeRefType, ienumType2.DeRefType, \u0004, \u0005, \u0006);
		}

		// Token: 0x0600317E RID: 12670 RVA: 0x000BE8D0 File Offset: 0x000BCAD0
		private static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (\u0004 is IScope && \u0005 is IScope)
			{
				return \u0011.\u0001(\u0002 as _IUserdefType, \u0003 as _IUserdefType, \u0004 as IScope2, \u0005 as IScope2);
			}
			if (\u0004 is \u0015.\u0002 && \u0005 is \u0015.\u0002)
			{
				return \u0011.\u0001(\u0002 as _IUserdefType, \u0003 as _IUserdefType, \u0004 as \u0015.\u0002, \u0005 as \u0015.\u0002);
			}
			return \u0004.IsEqual(\u0002 as _IUserdefType, \u0003 as _IUserdefType);
		}

		// Token: 0x0600317F RID: 12671 RVA: 0x000BE94C File Offset: 0x000BCB4C
		private static bool \u0003(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (!\u0011.\u0001(\u0002.BaseType, \u0003.BaseType, \u0004, \u0005))
			{
				return false;
			}
			_IVectorType ivectorType = \u0002 as _IVectorType;
			_IVectorType ivectorType2 = \u0003 as _IVectorType;
			ILiteralValue literalValue = \u0004.GetLiteralValue(ivectorType.Dimension, true);
			ILiteralValue literalValue2 = \u0005.GetLiteralValue(ivectorType2.Dimension, true);
			int num;
			int num2;
			return literalValue != null && literalValue2 != null && literalValue.GetInt(out num) && literalValue2.GetInt(out num2) && num == num2;
		}

		// Token: 0x06003180 RID: 12672 RVA: 0x000BE9C8 File Offset: 0x000BCBC8
		private static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, bool \u0006, bool \u0007)
		{
			if (!\u0011.\u0001(\u0002.BaseType, \u0003.BaseType, \u0004, \u0005, false, \u0007))
			{
				return false;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			_IArrayType iarrayType2 = \u0003 as _IArrayType;
			if (iarrayType._Dimensions.Count != iarrayType2._Dimensions.Count)
			{
				return false;
			}
			for (int i = 0; i < iarrayType._Dimensions.Count; i++)
			{
				if (!\u0011.\u0001(\u0004, \u0005, \u0006, iarrayType, iarrayType2, i))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06003181 RID: 12673 RVA: 0x000BEA40 File Offset: 0x000BCC40
		private static bool \u0001(ICommonScope \u0002, ICommonScope \u0003, bool \u0004, _IArrayType \u0005, _IArrayType \u0006, int \u0007)
		{
			ILiteralValue literalValue = \u0011.\u0001(\u0005._Dimensions[\u0007]._LowerBorder, \u0002);
			ILiteralValue literalValue2 = \u0011.\u0001(\u0005._Dimensions[\u0007]._UpperBorder, \u0002);
			ILiteralValue literalValue3 = \u0011.\u0001(\u0006._Dimensions[\u0007]._LowerBorder, \u0003);
			ILiteralValue literalValue4 = \u0011.\u0001(\u0006._Dimensions[\u0007]._UpperBorder, \u0003);
			if (literalValue == null || literalValue2 == null || literalValue3 == null || literalValue4 == null)
			{
				return false;
			}
			int num;
			int num2;
			int num3;
			int num4;
			if (!literalValue.GetInt(out num) || !literalValue2.GetInt(out num2) || !literalValue3.GetInt(out num3) || !literalValue4.GetInt(out num4))
			{
				return false;
			}
			if (\u0004)
			{
				if (num2 - num != num4 - num3)
				{
					return false;
				}
			}
			else if (num2 != num4 || num != num3)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06003182 RID: 12674 RVA: 0x000BEB10 File Offset: 0x000BCD10
		private static ILiteralValue \u0001(_IExpression \u0002, ICommonScope \u0003)
		{
			ILiteralValue literalValue = \u0003.GetLiteralValue(\u0002, true);
			if (literalValue != null)
			{
				return literalValue;
			}
			IVariable variable = null;
			if (\u0003 is IPrecompileScope)
			{
				variable = \u0002.GetVariable(\u0003 as IPrecompileScope);
			}
			else if (\u0003 is IScope)
			{
				variable = \u0002.GetVariable(\u0003 as IScope);
			}
			if (((variable != null) ? variable.Type : null) == null)
			{
				return literalValue;
			}
			if (variable.Type is _IUserdefType && variable.Initial != null)
			{
				return \u0011.\u0001(\u0003, variable);
			}
			if (variable.Type is _IEnumType)
			{
				IType type = variable.Type;
				literalValue = ((ILMPreCompileService5)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService).GetEnumInitValue(variable, \u0003);
			}
			return literalValue;
		}

		// Token: 0x06003183 RID: 12675 RVA: 0x000BEBB8 File Offset: 0x000BCDB8
		private static ILiteralValue \u0001(ICommonScope \u0002, IVariable \u0003)
		{
			IUserdefType udtype = \u0003.Type as IUserdefType;
			ISignature signature = \u0002.FindSignature(udtype);
			if (signature == null || !signature.GetFlag(SignatureFlag.Enum))
			{
				return null;
			}
			_IExpression iexpression = \u0003.Initial as _IExpression;
			IVariable variable = null;
			IPrecompileScope precompileScope = \u0002 as IPrecompileScope;
			if (precompileScope != null)
			{
				variable = iexpression.GetVariable(precompileScope);
			}
			else
			{
				IScope scope = \u0002 as IScope;
				if (scope != null)
				{
					variable = iexpression.GetVariable(scope);
				}
			}
			if (variable != null && variable.GetFlag(VarFlag.Enum))
			{
				return ((ILMPreCompileService5)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService).GetEnumInitValue(variable, \u0002);
			}
			return null;
		}

		// Token: 0x06003184 RID: 12676 RVA: 0x000BEC50 File Offset: 0x000BCE50
		public static bool \u0001(_IUserdefType \u0002, _IUserdefType \u0003, IScope2 \u0004, IScope2 \u0005)
		{
			ISignature signature = null;
			if (\u0004 != null)
			{
				signature = \u0002.GetSignature(\u0004);
			}
			ISignature signature2 = null;
			if (\u0005 != null)
			{
				signature2 = \u0003.GetSignature(\u0005);
			}
			if (signature != null)
			{
				return \u0002.SignatureId == \u0003.SignatureId && signature2 != null && (\u0004 == \u0005 || !(signature as _ISignature).GetFlag(SignatureFlag.OnlineChanged));
			}
			if (signature2 != null)
			{
				return false;
			}
			IScope2 scope = \u0002.GetScope(\u0004);
			IScope2 scope2 = \u0003.GetScope(\u0005);
			return scope != null && scope2 != null && scope.Name == scope2.Name;
		}

		// Token: 0x06003185 RID: 12677 RVA: 0x000BECD8 File Offset: 0x000BCED8
		public static bool \u0001(_IUserdefType \u0002, _IUserdefType \u0003, \u0015.\u0002 \u0004, \u0015.\u0002 \u0005)
		{
			_ISignature isignature = null;
			if (\u0004 != null)
			{
				isignature = (\u0004.FindSignature(\u0002) as _ISignature);
			}
			_ISignature isignature2 = null;
			if (\u0005 != null)
			{
				isignature2 = (\u0005.FindSignature(\u0003) as _ISignature);
			}
			return isignature != null && isignature2 != null && isignature.PrecompileId == isignature2.PrecompileId;
		}

		// Token: 0x06003186 RID: 12678 RVA: 0x000BED20 File Offset: 0x000BCF20
		public static bool \u0001(ISignature \u0002, ISignature \u0003, IScope2 \u0004)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0004);
		}

		// Token: 0x06003187 RID: 12679 RVA: 0x000BED2C File Offset: 0x000BCF2C
		public static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004)
		{
			return \u0011.\u0002(\u0002, \u0003, \u0004 as ICommonScope);
		}

		// Token: 0x06003188 RID: 12680 RVA: 0x000BED3C File Offset: 0x000BCF3C
		public static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0008(\u0002, \u0003, \u0004, \u0004);
		}

		// Token: 0x06003189 RID: 12681 RVA: 0x000BED48 File Offset: 0x000BCF48
		public static bool \u0003(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004)
		{
			return \u0011.\u0003(\u0002, \u0003, \u0004 as ICommonScope);
		}

		// Token: 0x0600318A RID: 12682 RVA: 0x000BED58 File Offset: 0x000BCF58
		public static bool \u0003(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			return \u0011.\u0005(\u0002, \u0003, \u0004, \u0004);
		}

		// Token: 0x0600318B RID: 12683 RVA: 0x000BED64 File Offset: 0x000BCF64
		public static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004, IScope5 \u0005)
		{
			return \u0011.\u0004(\u0002, \u0003, \u0004 as ICommonScope, \u0005 as ICommonScope);
		}

		// Token: 0x0600318C RID: 12684 RVA: 0x000BED7C File Offset: 0x000BCF7C
		public static bool \u0004(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (\u0002.Class == TypeClass.Array && \u0003.Class == TypeClass.Array)
			{
				_IArrayType iarrayType = \u0002 as _IArrayType;
				_IArrayType iarrayType2 = \u0003 as _IArrayType;
				return iarrayType._Dimensions.Count == iarrayType2._Dimensions.Count && \u0011.\u0004(\u0002.BaseType, \u0003.BaseType, \u0004, \u0005);
			}
			return \u0011.\u0008(\u0002, \u0003, \u0004, \u0005) && (!TypeTable.IsInteger(\u0002.Class) || !TypeTable.IsReal(\u0003.Class)) && (!TypeTable.IsReal(\u0002.Class) || !TypeTable.IsInteger(\u0003.Class)) && TypeTable.IsSigned(\u0002.Class) == TypeTable.IsSigned(\u0003.Class);
		}

		// Token: 0x0600318D RID: 12685 RVA: 0x000BEE38 File Offset: 0x000BD038
		public static bool \u0001(ISignature \u0002, ISignature \u0003, IScope2 \u0004, IScope2 \u0005)
		{
			_ISignature isignature = \u0002 as _ISignature;
			_ISignature isignature2 = \u0003 as _ISignature;
			if (isignature == null || isignature2 == null)
			{
				return false;
			}
			if (isignature.Id == isignature2.Id)
			{
				return true;
			}
			if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID) && isignature2.HasAttribute(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID) && isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID) == isignature2.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID))
			{
				return true;
			}
			if (isignature.BaseSignatureId != Helper.InvalidId)
			{
				ISignature signature = \u0004[isignature.BaseSignatureId];
				if (\u0084.\u000E.\u0001((_ISignature)signature, \u0004))
				{
					return false;
				}
				if (\u0011.\u0001(signature, \u0003, \u0004, \u0005))
				{
					return true;
				}
			}
			if (isignature.InterfaceIds.Length != 0)
			{
				foreach (int nId in isignature.InterfaceIds)
				{
					ISignature signature2 = \u0004[nId];
					if (\u0084.\u000E.\u0001((_ISignature)signature2, \u0004))
					{
						return false;
					}
					if (\u0011.\u0001(signature2, \u0003, \u0004, \u0005))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600318E RID: 12686 RVA: 0x000BEF30 File Offset: 0x000BD130
		public static bool \u0001(ICompiledType \u0002, IScope5 \u0003)
		{
			return \u0011.\u0001(\u0002, \u0003 as ICommonScope);
		}

		// Token: 0x0600318F RID: 12687 RVA: 0x000BEF40 File Offset: 0x000BD140
		public static bool \u0001(ICompiledType \u0002, ICommonScope \u0003)
		{
			if (\u0002 == null)
			{
				return false;
			}
			if (\u0002.Class != TypeClass.Userdef)
			{
				return false;
			}
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			if (\u0003 == null)
			{
				return false;
			}
			ISignature signature = \u0003.FindSignature(iuserdefType);
			return signature != null && (signature.POUType == Operator.Interface || signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion));
		}

		// Token: 0x06003190 RID: 12688 RVA: 0x000BEF94 File Offset: 0x000BD194
		public static bool \u0001(_ILiteralExpression \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, IScope5 \u0006)
		{
			return \u0011.\u0001(\u0002.LiteralValue, \u0003, \u0004, \u0005 as ICommonScope, \u0006 as ICommonScope);
		}

		// Token: 0x06003191 RID: 12689 RVA: 0x000BEFB0 File Offset: 0x000BD1B0
		private static long \u0001(ILiteralValue \u0002)
		{
			bool flag;
			long result;
			if (\u0002.KindOf == KindOfLiteral.UnsignedInteger)
			{
				result = (long)\u0002.GetUnsignedLong(out flag);
			}
			else
			{
				result = \u0002.GetSignedLong(out flag);
			}
			if (!flag)
			{
				throw new InvalidCastException("Could not retrieve value from ILiteralValue.");
			}
			return result;
		}

		// Token: 0x06003192 RID: 12690 RVA: 0x000BEFEC File Offset: 0x000BD1EC
		private static ulong \u0001(ILiteralValue \u0002)
		{
			bool flag;
			ulong result;
			if (\u0002.KindOf == KindOfLiteral.UnsignedInteger)
			{
				result = \u0002.GetUnsignedLong(out flag);
			}
			else
			{
				result = (ulong)\u0002.GetSignedLong(out flag);
			}
			if (!flag)
			{
				throw new InvalidCastException("Could not retrieve value from ILiteralValue.");
			}
			return result;
		}

		// Token: 0x06003193 RID: 12691 RVA: 0x000BF028 File Offset: 0x000BD228
		public static bool \u0001(ILiteralValue \u0002, ICompiledType \u0003, ICompiledType \u0004, ICommonScope \u0005, ICommonScope \u0006)
		{
			if (\u0003 == null || \u0004 == null)
			{
				return false;
			}
			if (!\u0011.\u0001(\u0002, \u0004, \u0006))
			{
				return false;
			}
			TypeClass @class = \u0003.DeRefType.Class;
			TypeClass class2 = \u0004.DeRefType.Class;
			if (TypeTable.IsSigned(\u0003.DeRefType.Class) && \u0002.SignedLong < 0L && !TypeTable.IsSigned(\u0004.DeRefType.Class) && \u0004.DeRefType.Class != TypeClass.Pointer)
			{
				return false;
			}
			bool flag;
			bool result = \u0011.\u0001(\u0004, \u0005, \u0006, @class, class2, out flag);
			if (!flag)
			{
				return result;
			}
			if ((\u0002.KindOf == KindOfLiteral.SignedInteger || \u0002.KindOf == KindOfLiteral.UnsignedInteger) && \u0084.\u0004.\u0001(class2, @class, \u0002))
			{
				return false;
			}
			TypeTable.IsInteger(\u0003.Class);
			int num = -1;
			return (\u0002.GetInt(out num) && num == 0 && (\u0011.\u0001(\u0004, \u0006) || \u0004.Class == TypeClass.Reference)) || \u0011.\u0008(\u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06003194 RID: 12692 RVA: 0x000BF110 File Offset: 0x000BD310
		private static bool \u0001(ICompiledType \u0002, ICommonScope \u0003, ICommonScope \u0004, TypeClass \u0005, TypeClass \u0006, out bool \u0007)
		{
			\u0007 = false;
			switch (\u0005)
			{
			case TypeClass.Bool:
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
			{
				if (\u0006 == TypeClass.AnyBit)
				{
					return true;
				}
				if (\u0006 != TypeClass.Pointer || \u0005 == TypeClass.Bool)
				{
					goto IL_AB;
				}
				int size = TypeTable.GetSize2(TypeClass.Pointer, \u0004);
				if (size == 8 || (size == 4 && !TypeTable.IsLInteger2(\u0005, \u0004)))
				{
					return true;
				}
				goto IL_AB;
			}
			case TypeClass.Bit:
				break;
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.String:
			case TypeClass.WString:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
				goto IL_AB;
			case TypeClass.Pointer:
				if (\u0006 == TypeClass.Pointer)
				{
					return true;
				}
				goto IL_AB;
			default:
				if (\u0005 != TypeClass.BitConst)
				{
					goto IL_AB;
				}
				break;
			}
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0006);
			IL_AB:
			\u0007 = true;
			return true;
		}

		// Token: 0x06003195 RID: 12693 RVA: 0x000BF1D0 File Offset: 0x000BD3D0
		private static bool \u0001(ICompiledType \u0002, ICommonScope \u0003, ICommonScope \u0004, TypeClass \u0005)
		{
			return \u0005 == TypeClass.AnyBit || \u0005 == TypeClass.Bit || \u0005 == TypeClass.Bool || \u0005 == TypeClass.BitConst || \u0011.\u0008(TypeTable.Byte, \u0002, \u0003, \u0004);
		}

		// Token: 0x06003196 RID: 12694 RVA: 0x000BF1F4 File Offset: 0x000BD3F4
		private static bool \u0001(ILiteralValue \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			if (\u0003.Class == TypeClass.Subrange)
			{
				ILiteralValue literalValue = \u0004.GetLiteralValue((\u0003 as _ISubrangeType)._LowerBorder, true);
				ILiteralValue literalValue2 = \u0004.GetLiteralValue((\u0003 as _ISubrangeType)._UpperBorder, true);
				try
				{
					if (TypeTable.IsSigned(\u0003.DeRefType.Class))
					{
						long num = \u0011.\u0001(literalValue);
						long num2 = \u0011.\u0001(literalValue2);
						long num3 = \u0011.\u0001(\u0002);
						if (num3 < num || num3 > num2)
						{
							return false;
						}
					}
					else
					{
						ulong num4 = \u0011.\u0001(literalValue);
						ulong num5 = \u0011.\u0001(literalValue2);
						ulong num6 = \u0011.\u0001(\u0002);
						if (num6 < num4 || num6 > num5)
						{
							return false;
						}
					}
				}
				catch
				{
					return false;
				}
				return true;
			}
			return true;
		}

		// Token: 0x06003197 RID: 12695 RVA: 0x000BF2B0 File Offset: 0x000BD4B0
		public static bool \u0001(ILiteralValue \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, IScope5 \u0006)
		{
			return \u0011.\u0001(\u0002, \u0003, \u0004, \u0005 as ICommonScope, \u0006 as ICommonScope);
		}

		// Token: 0x06003198 RID: 12696 RVA: 0x000BF2C8 File Offset: 0x000BD4C8
		public static bool \u0001(_ILiteralExpression \u0002, ICompiledType \u0003, ICompiledType \u0004, ICommonScope \u0005, ICommonScope \u0006)
		{
			return \u0011.\u0001(\u0002.LiteralValue, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06003199 RID: 12697 RVA: 0x000BF2DC File Offset: 0x000BD4DC
		internal static ICompiledType \u0001(ICompiledType \u0002, IScope5 \u0003)
		{
			return \u0011.\u0001(\u0002, \u0003 as ICommonScope);
		}

		// Token: 0x0600319A RID: 12698 RVA: 0x000BF2EC File Offset: 0x000BD4EC
		internal static ICompiledType \u0001(ICompiledType \u0002, ICommonScope \u0003)
		{
			if (\u0002.DeRefType.Class != TypeClass.Userdef)
			{
				return \u0002;
			}
			IUserdefType2 userdefType = \u0002.DeRefType as IUserdefType2;
			_ISignature isignature;
			if (userdefType.SignatureId != -1)
			{
				isignature = (\u0003.FindSignature(userdefType) as _ISignature);
			}
			else
			{
				ISignature[] array = \u0003.FindSignature(userdefType.NameExpression);
				if (array == null || array.Length != 1)
				{
					return \u0002;
				}
				isignature = (array[0] as _ISignature);
			}
			if (isignature == null)
			{
				return \u0002;
			}
			if ((isignature.GetFlag(SignatureFlag.Alias) || isignature.GetFlag(SignatureFlag.Enum)) && isignature.AllVariables.Count > 0 && isignature.AllVariables.First<_IVariable>().CompiledType != null)
			{
				return isignature.AllVariables.First<_IVariable>().CompiledType;
			}
			return \u0002;
		}

		// Token: 0x0600319B RID: 12699 RVA: 0x000BF39C File Offset: 0x000BD59C
		public static bool \u0005(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (\u0002 == null || \u0003 == null)
			{
				return false;
			}
			\u0002 = \u0011.\u0001(\u0002, \u0004);
			\u0003 = \u0011.\u0001(\u0003, \u0005);
			TypeClass @class = \u0002.DeRefType.Class;
			TypeClass class2 = \u0003.DeRefType.Class;
			return (@class == TypeClass.Pointer && class2 != TypeClass.Pointer) || (class2 == TypeClass.Pointer && @class != TypeClass.Pointer);
		}

		// Token: 0x0600319C RID: 12700 RVA: 0x000BF3F4 File Offset: 0x000BD5F4
		private static TypeClass \u0001(ICommonScope \u0002, TypeClass \u0003)
		{
			TypeClass result = \u0003;
			if (\u0002 == null || (\u0002.PointerSize != 4 && \u0002.PointerSize != 8))
			{
				return result;
			}
			bool flag = \u0002.PointerSize == 8;
			switch (\u0003)
			{
			case TypeClass.UXInt:
				result = (flag ? TypeClass.ULInt : TypeClass.UDInt);
				break;
			case TypeClass.XWord:
				result = (flag ? TypeClass.LWord : TypeClass.DWord);
				break;
			case TypeClass.XInt:
				result = (flag ? TypeClass.LInt : TypeClass.DInt);
				break;
			}
			return result;
		}

		// Token: 0x0600319D RID: 12701 RVA: 0x000BF45C File Offset: 0x000BD65C
		private static bool \u0006(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (!\u0011.\u0001(\u0002.DeRefType.BaseType, \u0003.DeRefType.BaseType, \u0004, \u0005))
			{
				return false;
			}
			_IVectorType ivectorType = \u0002.DeRefType as _IVectorType;
			_IVectorType ivectorType2 = \u0003.DeRefType as _IVectorType;
			ILiteralValue literalValue = \u0004.GetLiteralValue(ivectorType.Dimension, true);
			ILiteralValue literalValue2 = \u0005.GetLiteralValue(ivectorType2.Dimension, true);
			int num;
			int num2;
			return literalValue != null && literalValue2 != null && literalValue.GetInt(out num) && literalValue2.GetInt(out num2) && num == num2;
		}

		// Token: 0x0600319E RID: 12702 RVA: 0x000BF4EC File Offset: 0x000BD6EC
		private static bool \u0007(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (!\u0011.\u0001(\u0002.DeRefType.BaseType, \u0003.DeRefType.BaseType, \u0004, \u0005))
			{
				return false;
			}
			_IArrayType iarrayType = \u0002.DeRefType as _IArrayType;
			_IArrayType iarrayType2 = \u0003.DeRefType as _IArrayType;
			if (iarrayType._Dimensions.Count != iarrayType2._Dimensions.Count)
			{
				return false;
			}
			for (int i = 0; i < iarrayType._Dimensions.Count; i++)
			{
				bool bAllocatedOk = true;
				ILiteralValue literalValue = \u0004.GetLiteralValue(iarrayType._Dimensions[i]._LowerBorder, bAllocatedOk);
				ILiteralValue literalValue2 = \u0005.GetLiteralValue(iarrayType2._Dimensions[i]._LowerBorder, bAllocatedOk);
				ILiteralValue literalValue3 = \u0004.GetLiteralValue(iarrayType._Dimensions[i]._UpperBorder, bAllocatedOk);
				ILiteralValue literalValue4 = \u0005.GetLiteralValue(iarrayType2._Dimensions[i]._UpperBorder, bAllocatedOk);
				if (literalValue == null || literalValue2 == null || literalValue3 == null || literalValue4 == null)
				{
					return \u0005.IsPrecompileScope;
				}
				int num;
				int num2;
				int num3;
				int num4;
				if (!literalValue.GetInt(out num) || !literalValue3.GetInt(out num2) || !literalValue2.GetInt(out num3) || !literalValue4.GetInt(out num4) || num2 - num != num4 - num3)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600319F RID: 12703 RVA: 0x000BF628 File Offset: 0x000BD828
		public static bool \u0008(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			if (\u0002 == null || \u0003 == null)
			{
				return false;
			}
			\u0002 = \u0011.\u0001(\u0002, \u0004);
			\u0003 = \u0011.\u0001(\u0003, \u0005);
			TypeClass typeClass = \u0002.DeRefType.Class;
			TypeClass typeClass2 = \u0003.DeRefType.Class;
			typeClass = \u0011.\u0001(\u0004, typeClass);
			typeClass2 = \u0011.\u0001(\u0005, typeClass2);
			bool result;
			if (\u0011.\u0001(typeClass, typeClass2, out result))
			{
				return result;
			}
			if (typeClass == typeClass2 && typeClass2 == TypeClass.Enum)
			{
				_IEnumType ienumType = \u0002.DeRefType as _IEnumType;
				_IEnumType ienumType2 = \u0003.DeRefType as _IEnumType;
				if (ienumType.SignatureId != -1)
				{
					return ienumType.SignatureId == ienumType2.SignatureId;
				}
				return ienumType.Name == ienumType2.Name;
			}
			else
			{
				if (typeClass == TypeClass.Enum)
				{
					typeClass = (\u0002.DeRefType as _IEnumType)._Base.Class;
				}
				if (typeClass2 == TypeClass.Enum)
				{
					typeClass2 = (\u0003.DeRefType as _IEnumType)._Base.Class;
				}
				if (\u0011.\u0002(\u0002, \u0003, \u0004, \u0005, typeClass, typeClass2, out result))
				{
					return result;
				}
				if (\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, typeClass, typeClass2, out result))
				{
					return result;
				}
				return \u0011.\u0001(typeClass, typeClass2);
			}
		}

		// Token: 0x060031A0 RID: 12704 RVA: 0x000BF734 File Offset: 0x000BD934
		private static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, TypeClass \u0006, TypeClass \u0007, out bool \u0008)
		{
			\u0008 = false;
			switch (\u0006)
			{
			case TypeClass.Bool:
				if (\u0007 <= TypeClass.Bit)
				{
					\u0008 = true;
					return true;
				}
				return false;
			case TypeClass.Bit:
				if (\u0007 <= TypeClass.Bit)
				{
					\u0008 = true;
					return true;
				}
				return false;
			case TypeClass.Byte:
			case TypeClass.SInt:
			case TypeClass.USInt:
				break;
			case TypeClass.Word:
			case TypeClass.Int:
			case TypeClass.UInt:
			case TypeClass.Enum:
				switch (\u0007)
				{
				case TypeClass.Word:
				case TypeClass.DWord:
				case TypeClass.LWord:
				case TypeClass.Int:
				case TypeClass.DInt:
				case TypeClass.LInt:
				case TypeClass.UInt:
				case TypeClass.UDInt:
				case TypeClass.ULInt:
				case TypeClass.Real:
				case TypeClass.LReal:
				case TypeClass.Pointer:
				case TypeClass.Enum:
					break;
				case TypeClass.SInt:
				case TypeClass.USInt:
				case TypeClass.String:
				case TypeClass.WString:
				case TypeClass.Time:
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
				case TypeClass.Reference:
				case TypeClass.Subrange:
					return false;
				default:
					if (\u0007 - TypeClass.AnyInt > 2 && \u0007 - TypeClass.UXInt > 2)
					{
						return false;
					}
					break;
				}
				\u0008 = true;
				return true;
			case TypeClass.DWord:
			case TypeClass.DInt:
			case TypeClass.UDInt:
				goto IL_266;
			case TypeClass.LWord:
			case TypeClass.LInt:
			case TypeClass.ULInt:
				goto IL_2DF;
			case TypeClass.Real:
			case TypeClass.LReal:
				if (\u0007 - TypeClass.Real <= 1 || \u0007 - TypeClass.AnyNum <= 1)
				{
					\u0008 = true;
					return true;
				}
				return false;
			case TypeClass.String:
				\u0008 = (\u0007 == TypeClass.String || \u0007 == TypeClass.AnyString || \u0007 == TypeClass.Any);
				return true;
			case TypeClass.WString:
				\u0008 = (\u0007 == TypeClass.WString || \u0007 == TypeClass.AnyString || \u0007 == TypeClass.Any);
				return true;
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.LTime:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				\u0008 = (\u0006 == \u0007 || \u0007 == TypeClass.AnyDate || \u0007 == TypeClass.Any);
				return true;
			case TypeClass.Pointer:
				if (\u0007 == TypeClass.Pointer)
				{
					\u0008 = true;
					return true;
				}
				if (TypeTable.GetSize2(\u0006, \u0004) == 8)
				{
					goto IL_2DF;
				}
				if (!TypeTable.IsXType(\u0002))
				{
					goto IL_266;
				}
				if (\u0007 == TypeClass.Pointer || \u0007 - TypeClass.UXInt <= 2)
				{
					\u0008 = true;
					return true;
				}
				return false;
			case TypeClass.Reference:
			case TypeClass.Subrange:
			case TypeClass.Params:
			case TypeClass.None:
			case TypeClass.XString:
			case TypeClass.AnyString:
				goto IL_403;
			case TypeClass.Array:
				\u0008 = false;
				if (\u0007 == TypeClass.Array)
				{
					\u0008 = \u0011.\u0007(\u0002, \u0003, \u0004, \u0005);
				}
				else if (\u0007 == TypeClass.VarLenArray)
				{
					\u0008 = true;
				}
				return true;
			case TypeClass.Userdef:
				\u0008 = \u0011.\u000E(\u0002, \u0003, \u0004, \u0005);
				return true;
			case TypeClass.Any:
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.AnyReal:
			case TypeClass.VarLenArray:
				\u0008 = false;
				return true;
			case TypeClass.Lazy:
				\u0008 = true;
				return true;
			case TypeClass.BitConst:
				if (\u0007 <= TypeClass.Bit)
				{
					\u0008 = true;
					return true;
				}
				break;
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
				if (\u0007 == TypeClass.Pointer || \u0007 - TypeClass.UXInt <= 2)
				{
					\u0008 = true;
					return true;
				}
				return false;
			case TypeClass.__Vector:
				\u0008 = false;
				if (\u0007 == TypeClass.__Vector)
				{
					\u0008 = \u0011.\u0006(\u0002, \u0003, \u0004, \u0005);
				}
				return true;
			default:
				goto IL_403;
			}
			switch (\u0007)
			{
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
			case TypeClass.Pointer:
			case TypeClass.Enum:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.AnyReal:
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
				\u0008 = true;
				return true;
			case TypeClass.String:
			case TypeClass.WString:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Reference:
			case TypeClass.Subrange:
			case TypeClass.Array:
			case TypeClass.Params:
			case TypeClass.Userdef:
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.Lazy:
			case TypeClass.LTime:
			case TypeClass.BitConst:
				return false;
			default:
				return false;
			}
			IL_266:
			if (\u0007 <= TypeClass.Pointer)
			{
				switch (\u0007)
				{
				case TypeClass.DWord:
				case TypeClass.LWord:
				case TypeClass.DInt:
				case TypeClass.LInt:
				case TypeClass.UDInt:
				case TypeClass.ULInt:
				case TypeClass.Real:
				case TypeClass.LReal:
					break;
				case TypeClass.SInt:
				case TypeClass.Int:
				case TypeClass.USInt:
				case TypeClass.UInt:
					return false;
				default:
					if (\u0007 != TypeClass.Pointer)
					{
						return false;
					}
					if (TypeTable.GetSize2(\u0007, \u0005) == 4)
					{
						\u0008 = true;
						return true;
					}
					return false;
				}
			}
			else if (\u0007 - TypeClass.AnyInt > 2 && \u0007 - TypeClass.UXInt > 2)
			{
				return false;
			}
			\u0008 = true;
			return true;
			IL_2DF:
			if (\u0007 <= TypeClass.LReal)
			{
				if (\u0007 != TypeClass.LWord && \u0007 != TypeClass.LInt && \u0007 - TypeClass.ULInt > 2)
				{
					return false;
				}
			}
			else if (\u0007 != TypeClass.Pointer)
			{
				if (\u0007 - TypeClass.AnyInt > 2 && \u0007 - TypeClass.UXInt > 2)
				{
					return false;
				}
			}
			else
			{
				if (TypeTable.GetSize2(\u0007, \u0005) == 8)
				{
					\u0008 = true;
					return true;
				}
				return false;
			}
			\u0008 = true;
			return true;
			IL_403:
			Debug.\u0001(false);
			return false;
		}

		// Token: 0x060031A1 RID: 12705 RVA: 0x000BFB4C File Offset: 0x000BDD4C
		private static bool \u0001(TypeClass \u0002, TypeClass \u0003, out bool \u0004)
		{
			\u0004 = false;
			if ((\u0002 <= TypeClass.LWord || \u0002 - TypeClass.USInt <= 3 || \u0002 - TypeClass.BitConst <= 2) && \u0003 == TypeClass.AnyBit)
			{
				\u0004 = true;
				return true;
			}
			return false;
		}

		// Token: 0x060031A2 RID: 12706 RVA: 0x000BFB70 File Offset: 0x000BDD70
		private static bool \u0002(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, TypeClass \u0006, TypeClass \u0007, out bool \u0008)
		{
			\u0008 = false;
			if (\u0006 == TypeClass.Pointer && \u0007 == TypeClass.Pointer)
			{
				_IPointerType ipointerType = \u0002 as _IPointerType;
				if (ipointerType != null)
				{
					_IPointerType ipointerType2 = \u0003 as _IPointerType;
					if (ipointerType2 != null)
					{
						ICompiledType @base = ipointerType2._Base;
						ICompiledType base2 = ipointerType._Base;
						for (;;)
						{
							_IArrayType iarrayType = base2 as _IArrayType;
							if (iarrayType == null || iarrayType.Class != TypeClass.Array)
							{
								break;
							}
							base2 = iarrayType._Base;
						}
						if (\u0011.\u0001(ipointerType2._Base, \u0005) && !TypeTable.IsNumber(base2.DeRefType.Class) && base2.DeRefType.Class != TypeClass.Pointer)
						{
							if (\u0011.\u0001(base2, \u0004))
							{
								\u0008 = \u0011.\u0008(base2, @base, \u0004, \u0005);
							}
							else
							{
								\u0008 = \u0011.\u0001(\u0002, \u0003, \u0004, \u0005);
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060031A3 RID: 12707 RVA: 0x000BFC34 File Offset: 0x000BDE34
		private static bool \u000E(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005)
		{
			_IUserdefType iuserdefType = \u0003.DeRefType as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			if (\u0011.\u0001(\u0002, \u0003, \u0004, \u0005))
			{
				return true;
			}
			_IUserdefType iuserdefType2 = \u0002.DeRefType as _IUserdefType;
			if (iuserdefType2 == null)
			{
				return false;
			}
			ISignature signature = \u0004.FindSignature(iuserdefType2);
			if (signature == null)
			{
				return false;
			}
			if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IVariable ivariable = signature["__Interface"] as _IVariable;
				if (ivariable == null)
				{
					return false;
				}
				iuserdefType2 = (ivariable._Type.BaseType as _IUserdefType);
				if (iuserdefType2 == null)
				{
					return false;
				}
				signature = \u0004.FindSignature(iuserdefType2);
			}
			ISignature signature2 = \u0005.FindSignature(iuserdefType);
			if (signature2 == null)
			{
				return false;
			}
			if (signature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IVariable ivariable2 = signature2["__Interface"] as _IVariable;
				if (ivariable2 == null)
				{
					return false;
				}
				iuserdefType = (ivariable2._Type.BaseType as _IUserdefType);
				if (iuserdefType == null)
				{
					return false;
				}
				signature2 = \u0005.FindSignature(iuserdefType);
			}
			return \u0004.IsImplicitConvertable(signature, signature2, \u0005);
		}

		// Token: 0x060031A4 RID: 12708 RVA: 0x000BFD24 File Offset: 0x000BDF24
		private static bool \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			switch (\u0003)
			{
			case TypeClass.Any:
				return true;
			case TypeClass.AnyBit:
				return \u0002 - TypeClass.Byte <= 3;
			case TypeClass.AnyDate:
				if (\u0002 <= TypeClass.TimeOfDay)
				{
					if (\u0002 != TypeClass.Time && \u0002 - TypeClass.DateAndTime > 1)
					{
						return false;
					}
				}
				else if (\u0002 != TypeClass.LTime && \u0002 - TypeClass.LDateAndTime > 1)
				{
					return false;
				}
				return true;
			case TypeClass.AnyInt:
				return \u0002 - TypeClass.SInt <= 7 || \u0002 == TypeClass.UXInt || \u0002 == TypeClass.XInt;
			case TypeClass.AnyNum:
				return \u0002 - TypeClass.Byte <= 13 || \u0002 == TypeClass.UXInt || \u0002 == TypeClass.XInt;
			case TypeClass.AnyReal:
				return \u0002 - TypeClass.Real <= 1;
			default:
				return false;
			}
		}
	}
}
