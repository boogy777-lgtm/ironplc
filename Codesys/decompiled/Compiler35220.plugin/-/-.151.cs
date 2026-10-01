using System;
using System.Collections.Generic;
using \u0006;
using \u001E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001D
{
	// Token: 0x020001A4 RID: 420
	internal static class \u0005
	{
		// Token: 0x06001DC5 RID: 7621 RVA: 0x00060834 File Offset: 0x0005EA34
		public static _IType \u0001(Operator \u0002, bool \u0003, int \u0004, IList<_IExpression> \u0005, ICompiledType \u0006, IScope5 \u0007, _ICompileContext \u0008)
		{
			return \u001D.\u0005.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007 as ICommonScope, \u0008);
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x0006084C File Offset: 0x0005EA4C
		public static _IType \u0001(Operator \u0002, bool \u0003, int \u0004, IList<_IExpression> \u0005, ICompiledType \u0006, ICommonScope \u0007, _ICompileContext \u0008)
		{
			_IType itype = \u001D.\u0005.\u0001(\u0002, \u0005, \u0008);
			if (itype != null)
			{
				return itype;
			}
			if (\u001D.\u0005.\u0001(\u0007, \u0005, \u0004))
			{
				return \u0005[\u0004].Type as _IType;
			}
			global::\u0006.\u0004 u = new global::\u0006.\u0004();
			for (int i = \u0004; i < \u0005.Count; i++)
			{
				_IExpression iexpression = \u0005[i];
				Debug.\u0002(((iexpression != null) ? iexpression.Type : null) != null);
				ICompiledType deRefType = iexpression.Type.DeRefType;
				if (TypeTable.IsXType(deRefType))
				{
					return TypeTable.Get(deRefType.Class);
				}
				\u001D.\u0005.\u0001(\u0003, \u0007, u, iexpression, deRefType);
			}
			\u001D.\u0005.\u0001(\u0002, \u0006, u);
			_IType result;
			if (\u001D.\u0005.\u0001(\u0002, u, out result))
			{
				return result;
			}
			if (u.RealConstant)
			{
				return \u001D.\u0005.\u0001(\u0006, \u0008, u.OnlyLiterals);
			}
			if (u.AtLeastOneSigned && u.AtLeastOneUnsigned && u.SizeBiggest < 4 && \u001D.\u0005.\u0001(\u0002))
			{
				u.SizeBiggest = 4;
			}
			return \u001D.\u0005.\u0001(u.Signed, u.SizeBiggest);
		}

		// Token: 0x06001DC7 RID: 7623 RVA: 0x00060954 File Offset: 0x0005EB54
		private static void \u0001(Operator \u0002, ICompiledType \u0003, global::\u0006.\u0004 \u0004)
		{
			\u0004.Signed = (\u0004.Signed || (\u0004.OnlyLiterals && \u0004.LiteralSigned));
			if (!\u0004.Signed && \u0004.OnlyLiterals && !\u0004.ConcreteTypes && (\u0002 == Operator.Sub || \u0002 == Operator.Minus))
			{
				\u0004.Signed = true;
				if (\u0004.LiteralSignedSizeBiggest > \u0004.SizeBiggest)
				{
					\u0004.SizeBiggest = \u0004.LiteralSignedSizeBiggest;
				}
			}
			if (!\u0004.ConcreteTypes && \u0004.OnlyLiterals && !\u0004.Signed && \u0003 != null && TypeTable.IsConcreteType(\u0003.Class) && TypeTable.IsSigned(\u0003.Class))
			{
				\u0004.Signed = true;
			}
			if (\u0004.Signed && \u0004.LiteralSignedSizeBiggest > \u0004.SizeBiggest)
			{
				\u0004.SizeBiggest = \u0004.LiteralSignedSizeBiggest;
			}
		}

		// Token: 0x06001DC8 RID: 7624 RVA: 0x00060A28 File Offset: 0x0005EC28
		private static bool \u0001(Operator \u0002, global::\u0006.\u0004 \u0003, out _IType \u0004)
		{
			\u0004 = null;
			bool flag = \u001E.\u000E.\u0001(\u0002) || ((\u0002 == Operator.Mux || \u0002 == Operator.Sel) && \u0003.Typebiggest != TypeClass.BitConst) || \u0002 == Operator.And || \u0002 == Operator.Or;
			if ((\u0003.Typebiggest == TypeClass.Bit || \u0003.Typebiggest == TypeClass.BitConst || \u0003.Typebiggest == TypeClass.Bool) && flag)
			{
				\u0004 = TypeTable.Get(TypeClass.Bit);
				return true;
			}
			if (\u0003.LRealType)
			{
				\u0004 = TypeTable.Get(TypeClass.LReal);
				return true;
			}
			if (\u0003.RealType)
			{
				\u0004 = TypeTable.Get(TypeClass.Real);
				return true;
			}
			return false;
		}

		// Token: 0x06001DC9 RID: 7625 RVA: 0x00060ACC File Offset: 0x0005ECCC
		private static void \u0001(bool \u0002, ICommonScope \u0003, global::\u0006.\u0004 \u0004, _IExpression \u0005, ICompiledType \u0006)
		{
			int num = \u0003.GetSize(\u0006);
			if (\u0006 is ISpecialSizeType)
			{
				num = (\u0006 as ISpecialSizeType).CompatibilitySize;
			}
			if (num > \u0004.SizeBiggest || \u0004.Typebiggest == TypeClass.Bit || \u0004.Typebiggest == TypeClass.BitConst)
			{
				\u0004.SizeBiggest = num;
				\u0004.Typebiggest = \u0006.Class;
			}
			if (TypeTable.IsSigned(\u0006.Class))
			{
				\u0004.AtLeastOneSigned = true;
			}
			else
			{
				\u0004.AtLeastOneUnsigned = true;
			}
			\u0004.Signed = (\u0004.Signed || (!\u0002 && !(\u0005 is _ILiteralExpression) && TypeTable.IsSigned(\u0006.Class)));
			\u0004.RealType = (\u0004.RealType || \u0006.Class == TypeClass.Real);
			\u001D.\u0005.\u0001(\u0005, \u0004);
			if (\u0005.Type.DeRefType.Class != TypeClass.LReal || !(\u0005 is _ILiteralExpression))
			{
				\u0004.LRealType = (\u0004.LRealType || \u0005.Type.DeRefType.Class == TypeClass.LReal);
				return;
			}
			if ((\u0005 as _ILiteralExpression).ConstantType != TypeClass.LReal)
			{
				\u0004.RealConstant = true;
				return;
			}
			\u0004.LRealType = true;
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x00060BF4 File Offset: 0x0005EDF4
		private static _IType \u0001(Operator \u0002, IList<_IExpression> \u0003, _ICompileContext \u0004)
		{
			if (\u0004 == null || !(\u0004.ApplicationGuid == Guid.Empty) || !\u001E.\u000E.\u0001(\u0002) || \u0003.Count != 2 || (!TypeTable.IsResolvedXType(\u0003[0]._CompiledType) && !TypeTable.IsResolvedXType(\u0003[1]._CompiledType)) || (\u0003[0]._CompiledType.Class != TypeClass.Pointer && \u0003[1]._CompiledType.Class != TypeClass.Pointer))
			{
				return null;
			}
			if (TypeTable.IsResolvedXType(\u0003[0]._CompiledType))
			{
				return \u0003[0]._CompiledType as _IType;
			}
			return \u0003[1]._CompiledType as _IType;
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x00060CBC File Offset: 0x0005EEBC
		private static _IEnumType \u0001(IType \u0002)
		{
			_IEnumType ienumType = \u0002 as _IEnumType;
			if (ienumType != null)
			{
				return ienumType;
			}
			IReferenceType2 referenceType = \u0002 as IReferenceType2;
			if (referenceType != null)
			{
				return \u001D.\u0005.\u0001(referenceType.OriginalBase);
			}
			return null;
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x00060CEC File Offset: 0x0005EEEC
		public static bool \u0001(IScope5 \u0002, IList<_IExpression> \u0003, int \u0004, out _IEnumType \u0005)
		{
			\u0005 = null;
			if (\u001D.\u0005.\u0001(\u0002 as ICommonScope, \u0003, \u0004))
			{
				\u0005 = \u001D.\u0005.\u0001(\u0003[\u0004].Type);
				return true;
			}
			return false;
		}

		// Token: 0x06001DCD RID: 7629 RVA: 0x00060D18 File Offset: 0x0005EF18
		private static bool \u0001(ICommonScope \u0002, IList<_IExpression> \u0003, int \u0004)
		{
			if (\u0003 == null)
			{
				return false;
			}
			if (\u0003.Count <= \u0004 + 1)
			{
				return false;
			}
			if (\u0003[\u0004] == null || \u0003[\u0004].Type == null)
			{
				return false;
			}
			_IEnumType ienumType = \u001D.\u0005.\u0001(\u0003[\u0004].Type);
			if (ienumType == null)
			{
				return false;
			}
			ISignature signature = \u0002.FindSignature(ienumType);
			if (signature == null || !signature.HasAttribute("strict"))
			{
				return false;
			}
			for (int i = \u0004 + 1; i < \u0003.Count; i++)
			{
				if (!\u001D.\u0005.\u0001(ienumType, \u0003[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001DCE RID: 7630 RVA: 0x00060DA8 File Offset: 0x0005EFA8
		private static bool \u0001(_IEnumType \u0002, _IExpression \u0003)
		{
			if (\u0003 == null)
			{
				return false;
			}
			_IEnumType ienumType = \u001D.\u0005.\u0001(\u0003.Type);
			return ienumType != null && ienumType.SignatureId >= 0 && ienumType.SignatureId == \u0002.SignatureId;
		}

		// Token: 0x06001DCF RID: 7631 RVA: 0x00060DE8 File Offset: 0x0005EFE8
		private static void \u0001(_IExpression \u0002, global::\u0006.\u0004 \u0003)
		{
			_ILiteralExpression iliteralExpression = \u0002 as _ILiteralExpression;
			if (iliteralExpression == null)
			{
				\u0003.OnlyLiterals = false;
				return;
			}
			\u0003.LiteralSigned = (\u0003.LiteralSigned || iliteralExpression.Negative);
			if (TypeTable.IsConcreteType(iliteralExpression.ConstantType))
			{
				\u0003.Signed = (\u0003.Signed || TypeTable.IsSigned(\u0002.Type.DeRefType.Class));
				\u0003.ConcreteTypes = true;
				return;
			}
			bool flag;
			ulong unsignedLong = iliteralExpression.LiteralValue.GetUnsignedLong(out flag);
			if (flag)
			{
				int num;
				if (unsignedLong > 2147483647UL)
				{
					num = 8;
				}
				else if (unsignedLong > 32767UL)
				{
					num = 4;
				}
				else if (unsignedLong > 127UL)
				{
					num = 2;
				}
				else
				{
					num = 1;
				}
				if (num > \u0003.LiteralSignedSizeBiggest)
				{
					\u0003.LiteralSignedSizeBiggest = num;
				}
			}
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x00060EA0 File Offset: 0x0005F0A0
		private static _IType \u0001(ICompiledType \u0002, _ICompileContext \u0003, bool \u0004)
		{
			if (\u0002 != null && \u0002.Class == TypeClass.LReal && \u0004)
			{
				return TypeTable.Get(TypeClass.LReal);
			}
			if (\u0002 != null && \u0002.Class == TypeClass.LReal)
			{
				return TypeTable.Get(TypeClass.LReal);
			}
			if (\u0002 != null && \u0002.Class == TypeClass.Real)
			{
				return TypeTable.Get(TypeClass.Real);
			}
			if (\u0003 != null && \u0003.TypeIsSupported(TypeClass.LReal))
			{
				return TypeTable.Get(TypeClass.LReal);
			}
			return TypeTable.Get(TypeClass.Real);
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x00060F10 File Offset: 0x0005F110
		private static _IType \u0001(bool \u0002, int \u0003)
		{
			if (\u0002)
			{
				switch (\u0003)
				{
				case 1:
					return TypeTable.Get(TypeClass.SInt);
				case 2:
					return TypeTable.Get(TypeClass.Int);
				case 3:
					break;
				case 4:
					return TypeTable.Get(TypeClass.DInt);
				default:
					if (\u0003 == 8)
					{
						return TypeTable.Get(TypeClass.LInt);
					}
					break;
				}
			}
			else
			{
				switch (\u0003)
				{
				case 1:
					return TypeTable.Get(TypeClass.USInt);
				case 2:
					return TypeTable.Get(TypeClass.UInt);
				case 3:
					break;
				case 4:
					return TypeTable.Get(TypeClass.UDInt);
				default:
					if (\u0003 == 8)
					{
						return TypeTable.Get(TypeClass.ULInt);
					}
					break;
				}
			}
			return null;
		}

		// Token: 0x06001DD2 RID: 7634 RVA: 0x00060F9C File Offset: 0x0005F19C
		private static bool \u0001(Operator \u0002)
		{
			return \u0002 - Operator.Eq <= 5 || \u0002 - Operator.Less <= 5;
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x00060FB8 File Offset: 0x0005F1B8
		public static bool \u0001(_IOperatorExpression \u0002)
		{
			bool result = false;
			Operator code = \u0002.Code;
			if (code <= Operator.Not)
			{
				if (code - Operator.And > 5)
				{
					if (code != Operator.Not)
					{
						return result;
					}
					if (\u0002.Type != null && !TypeTable.IsBoolean(\u0002.Type.Class))
					{
						return true;
					}
					return result;
				}
			}
			else if (code - Operator.Ampersand > 1 && code - Operator.And_Then > 1)
			{
				return result;
			}
			result = true;
			return result;
		}
	}
}
