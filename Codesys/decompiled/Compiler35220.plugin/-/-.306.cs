using System;
using \u0003;
using \u001E;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0002
{
	// Token: 0x0200033F RID: 831
	internal sealed class \u000E : \u001E.\u0014, ITypeVisitor2, ITypeVisitor
	{
		// Token: 0x06003238 RID: 12856 RVA: 0x000C1E70 File Offset: 0x000C0070
		private \u000E(IScope \u009B\u0002, ErrorVisitor \u0083\u0004)
		{
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u0083\u0004;
			this.\u0001 = true;
		}

		// Token: 0x06003239 RID: 12857 RVA: 0x000C1E90 File Offset: 0x000C0090
		public static bool \u0001(_IType \u0002, IScope \u0003, ErrorVisitor \u0004)
		{
			global::\u0002.\u000E u000E = new global::\u0002.\u000E(\u0003, \u0004);
			\u0002.Accept(u000E);
			return u000E.\u0001;
		}

		// Token: 0x0600323A RID: 12858 RVA: 0x000C1EB4 File Offset: 0x000C00B4
		private static bool \u0001(ILiteralValue \u0002, ILiteralValue \u0003, out bool \u0004)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (\u0002.KindOf == KindOfLiteral.SignedInteger && \u0003.KindOf == KindOfLiteral.SignedInteger)
			{
				flag = (\u0002.GetSignedLong(out flag2) < \u0003.GetSignedLong(out flag3));
			}
			else if (\u0002.KindOf == KindOfLiteral.UnsignedInteger && \u0003.KindOf == KindOfLiteral.UnsignedInteger)
			{
				flag = (\u0002.GetUnsignedLong(out flag2) < \u0003.GetUnsignedLong(out flag3));
			}
			else if (\u0002.KindOf == KindOfLiteral.SignedInteger && \u0003.KindOf == KindOfLiteral.UnsignedInteger)
			{
				long signedLong = \u0002.GetSignedLong(out flag2);
				ulong unsignedLong = \u0003.GetUnsignedLong(out flag3);
				flag = (signedLong < 0L || signedLong < (long)unsignedLong);
			}
			else if (\u0002.KindOf == KindOfLiteral.UnsignedInteger && \u0003.KindOf == KindOfLiteral.SignedInteger)
			{
				ulong unsignedLong2 = \u0002.GetUnsignedLong(out flag2);
				long signedLong2 = \u0003.GetSignedLong(out flag3);
				flag = (signedLong2 >= 0L && unsignedLong2 < (ulong)signedLong2);
			}
			else if (\u0002.KindOf == KindOfLiteral.Float && \u0003.KindOf == KindOfLiteral.Float)
			{
				flag = (\u0002.GetFloat(out flag2) < \u0003.GetFloat(out flag3));
			}
			\u0004 = (flag2 && flag3);
			return \u0004 && flag;
		}

		// Token: 0x0600323B RID: 12859 RVA: 0x000C1FB4 File Offset: 0x000C01B4
		private void \u0001(_IArrayDimension \u0002)
		{
			bool flag;
			int num = \u0002.LowerBorderInt(out flag, this.\u0001);
			bool flag2;
			int num2 = \u0002.UpperBorderInt(out flag2, this.\u0001);
			if (num2 == 2147483647)
			{
				if (num < 1)
				{
					\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_MaxArraySizeExceeded, Array.Empty<object>()), MessageId.Err_MaxArraySizeExceeded);
					this.\u0001 = false;
				}
			}
			else if (flag && flag2 && num > num2 + 1)
			{
				\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_LowerGreaterUpperBorder, Array.Empty<object>()), MessageId.Err_LowerGreaterUpperBorder);
				this.\u0001 = false;
			}
			if (!flag)
			{
				\u0002._LowerBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderNoValidSignedInteger, new object[]
				{
					\u0002._LowerBorder
				}), MessageId.Err_ArrayBorderNoValidSignedInteger);
				this.\u0001 = false;
			}
			if (!flag2)
			{
				\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderNoValidSignedInteger, new object[]
				{
					\u0002._UpperBorder
				}), MessageId.Err_ArrayBorderNoValidSignedInteger);
				this.\u0001 = false;
			}
			\u0002._LowerBorder.Accept(this.\u0001);
			\u0002._UpperBorder.Accept(this.\u0001);
		}

		// Token: 0x0600323C RID: 12860 RVA: 0x000C20CC File Offset: 0x000C02CC
		public void \u0001(_ISubrangeType \u0002)
		{
			ILiteralValue literalValue = \u0002._LowerBorder.Literal(this.\u0001, true);
			ILiteralValue literalValue2 = \u0002._UpperBorder.Literal(this.\u0001, true);
			if (literalValue != null && literalValue2 != null)
			{
				bool flag2;
				bool flag = global::\u0002.\u000E.\u0001(literalValue2, literalValue, out flag2);
				if (flag2 && flag)
				{
					\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_LowerGreaterUpperBorder, Array.Empty<object>()), MessageId.Err_LowerGreaterUpperBorder);
					this.\u0001 = false;
				}
			}
			\u0002._LowerBorder.Accept(this.\u0001);
			\u0002._UpperBorder.Accept(this.\u0001);
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x0600323D RID: 12861 RVA: 0x000C2184 File Offset: 0x000C0384
		public void \u0001(_IArrayType \u0002)
		{
			foreach (_IArrayDimension u in \u0002._Dimensions)
			{
				this.\u0001(u);
			}
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x0600323E RID: 12862 RVA: 0x000C21FC File Offset: 0x000C03FC
		public void \u0001(_IVectorType \u0002)
		{
			bool flag;
			int num = \u0002.DimensionInt(this.\u0001, out flag);
			if (!flag || num <= 0 || num > 8)
			{
				\u0002._Dimension.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_VectorSizeNotValid, Array.Empty<object>()), MessageId.Err_VectorSizeNotValid);
				this.\u0001 = false;
			}
			\u0002._Dimension.Accept(this.\u0001);
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x0600323F RID: 12863 RVA: 0x000C2284 File Offset: 0x000C0484
		public void \u0001(_IStringType \u0002)
		{
			if (\u0002.Length != null && \u0002.Length.IsConstant(this.\u0001, true))
			{
				bool flag;
				int num = \u0084.\u0004.\u0001(\u0002.Length, this.\u0001 as IScope5, true, out flag);
				if (num < 0)
				{
					\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_InvalidStringSize, new object[]
					{
						num
					}), MessageId.Err_InvalidStringSize);
					this.\u0001 = false;
					\u0002.Length.Accept(this.\u0001);
				}
			}
		}

		// Token: 0x06003240 RID: 12864 RVA: 0x000C230C File Offset: 0x000C050C
		public void \u0001(_IWStringType \u0002)
		{
			if (\u0002.Length != null && \u0002.Length.IsConstant(this.\u0001, true))
			{
				bool flag;
				int num = \u0084.\u0004.\u0001(\u0002.Length, this.\u0001 as IScope5, true, out flag);
				if (num < 0)
				{
					\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_InvalidStringSize, new object[]
					{
						num
					}), MessageId.Err_InvalidStringSize);
					this.\u0001 = false;
					\u0002.Length.Accept(this.\u0001);
				}
			}
		}

		// Token: 0x06003241 RID: 12865 RVA: 0x000C2394 File Offset: 0x000C0594
		public void \u0001(_IXStringType \u0002)
		{
		}

		// Token: 0x06003242 RID: 12866 RVA: 0x000C2398 File Offset: 0x000C0598
		public void \u0001(_IPointerType \u0002)
		{
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x06003243 RID: 12867 RVA: 0x000C23C4 File Offset: 0x000C05C4
		public void \u0001(_IReferenceType \u0002)
		{
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x06003244 RID: 12868 RVA: 0x000C23F0 File Offset: 0x000C05F0
		public void \u0001(_IParamsType \u0002)
		{
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x06003245 RID: 12869 RVA: 0x000C241C File Offset: 0x000C061C
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			this.\u0001 = (global::\u0002.\u000E.\u0001(\u0002._Base, this.\u0001, this.\u0001) && this.\u0001);
		}

		// Token: 0x04000972 RID: 2418
		private readonly IScope \u0001;

		// Token: 0x04000973 RID: 2419
		private readonly ErrorVisitor \u0001;

		// Token: 0x04000974 RID: 2420
		private bool \u0001;
	}
}
