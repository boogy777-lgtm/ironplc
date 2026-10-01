using System;
using System.Collections.Generic;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200025E RID: 606
	public static class BitOffsetHandler
	{
		// Token: 0x06002751 RID: 10065 RVA: 0x0008760C File Offset: 0x0008580C
		internal static _IExpression \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return BitOffsetHandler.\u0002(\u0002, \u0003);
		}

		// Token: 0x06002752 RID: 10066 RVA: 0x00087618 File Offset: 0x00085818
		internal static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IOperatorExpression ioperatorExpression = \u0002._RValue as _IOperatorExpression;
			if (ioperatorExpression != null && ioperatorExpression.Code == Operator.__BitOffset)
			{
				return BitOffsetHandler.\u0001(ioperatorExpression, \u0002._LValue, \u0003);
			}
			return null;
		}

		// Token: 0x06002753 RID: 10067 RVA: 0x00087650 File Offset: 0x00085850
		internal static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IOperatorExpression ioperatorExpression = \u0002._RValue as _IOperatorExpression;
			return ioperatorExpression != null && ioperatorExpression.Code == Operator.__BitOffset;
		}

		// Token: 0x06002754 RID: 10068 RVA: 0x0008767C File Offset: 0x0008587C
		private static _IExpression \u0002(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			bool flag = operandsList.Count == 3;
			long u = -1L;
			if (!flag)
			{
				long num;
				long num2;
				BitOffsetHandler.\u0001(operandsList, \u0003, out u, out num, out num2);
			}
			return \u0003.Generator.\u0001<_ILiteralExpression>(\u0019.\u0003.\u0001(u), \u0003._Scope, \u0003.CompiledPOU);
		}

		// Token: 0x06002755 RID: 10069 RVA: 0x000876CC File Offset: 0x000858CC
		private static _IStatement \u0001(_IOperatorExpression \u0002, _IExprement \u0003, global::\u000E.\u0011 \u0004)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			bool flag = operandsList.Count == 3;
			long num;
			long num2;
			long num3;
			BitOffsetHandler.\u0001(operandsList, \u0004, out num, out num2, out num3);
			if (!flag)
			{
				return null;
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			string text = "FALSE";
			if (num != -1L)
			{
				if (\u0004.CodeGen.MotorolaByteOrder)
				{
					num2 = num3 - 1L - num / 8L;
				}
				else
				{
					num2 = num / 8L;
				}
				num %= 8L;
				text = "TRUE";
			}
			lstringBuilder.AppendFormat("{0} := {1};", new object[]
			{
				\u0003,
				text
			});
			lstringBuilder.AppendFormat("{0} := {1};", new object[]
			{
				operandsList[1],
				num2
			});
			lstringBuilder.AppendFormat("{0} := {1}", new object[]
			{
				operandsList[2],
				num
			});
			return (_ISequenceStatement)\u0004.Generator.\u0001(lstringBuilder.ToString(), \u0004._Scope, \u0004.CompiledPOU);
		}

		// Token: 0x06002756 RID: 10070 RVA: 0x000877C8 File Offset: 0x000859C8
		private static void \u0001(IList<_IExpression> \u0002, global::\u000E.\u0011 \u0003, out long \u0004, out long \u0005, out long \u0006)
		{
			\u0004 = -1L;
			\u0005 = 0L;
			\u0006 = 1L;
			IAddressExpression addressExpression = \u0002[0] as IAddressExpression;
			if (addressExpression != null)
			{
				BitOffsetHandler.\u0001(ref \u0004, addressExpression);
			}
			else
			{
				ICompoAccessExpression compoAccessExpression = \u0002[0] as ICompoAccessExpression;
				if (compoAccessExpression != null)
				{
					BitOffsetHandler.\u0001(\u0003, ref \u0004, ref \u0006, compoAccessExpression);
				}
			}
			BitOffsetHandler.\u0001(\u0002, \u0003, ref \u0004, ref \u0006);
		}

		// Token: 0x06002757 RID: 10071 RVA: 0x00087820 File Offset: 0x00085A20
		private static void \u0001(IList<_IExpression> \u0002, global::\u000E.\u0011 \u0003, ref long \u0004, ref long \u0005)
		{
			if (\u0004 == -1L)
			{
				IDataLocation dataLocation = \u0002[0].DataLocation(\u0003._Scope);
				if (dataLocation != null && dataLocation.IsBitLocation)
				{
					\u0004 = (long)((ulong)dataLocation.BitNr);
					\u0005 = (long)\u0002[0].Type.Size(\u0003._Scope);
				}
			}
		}

		// Token: 0x06002758 RID: 10072 RVA: 0x00087878 File Offset: 0x00085A78
		private static void \u0001(ref long \u0002, IAddressExpression \u0003)
		{
			IDirectVariable directAddress = \u0003.DirectAddress;
			if (directAddress != null && directAddress.Size == DirectVariableSize.X && directAddress.Components.Length != 0)
			{
				\u0002 = (long)directAddress.Components[1] % 8L;
			}
		}

		// Token: 0x06002759 RID: 10073 RVA: 0x000878B0 File Offset: 0x00085AB0
		private static void \u0001(global::\u000E.\u0011 \u0002, ref long \u0003, ref long \u0004, ICompoAccessExpression \u0005)
		{
			ILiteralValue literalValue = \u0005.Right.Literal(\u0002._Scope);
			if (literalValue != null && \u0005.Left.Type.DeRefType.IsInteger)
			{
				\u0004 = (long)\u0005.Left.Type.Size(\u0002._Scope);
				bool flag;
				\u0003 = literalValue.GetSignedLong(out flag);
				if (!flag)
				{
					try
					{
						\u0003 = (long)literalValue.GetUnsignedLong(out flag);
					}
					catch
					{
						\u0003 = -1L;
					}
				}
				if (!flag)
				{
					\u0003 = -1L;
				}
			}
		}
	}
}
