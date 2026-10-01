using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Features;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200015A RID: 346
	internal class TypeHelper : ITypeInfo4, ITypeInfo3, ITypeInfo2, ITypeInfo, IVarLenArrayTypeInfo2, IVarLenArrayTypeInfo
	{
		// Token: 0x06001BD0 RID: 7120 RVA: 0x0004E71B File Offset: 0x0004D71B
		public bool IsEquivalent(TypeClass tc1, TypeClass tc2)
		{
			return TypeTable.IsEquivalent(tc1, tc2);
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x0004E724 File Offset: 0x0004D724
		public bool IsBlock(TypeClass tc)
		{
			return TypeTable.IsBlock(tc);
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x0004E72C File Offset: 0x0004D72C
		public bool IsSigned(TypeClass tc)
		{
			return TypeTable.IsSigned(tc);
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x0004E734 File Offset: 0x0004D734
		public bool IsLType(TypeClass tc)
		{
			return TypeTable.IsLType(tc);
		}

		// Token: 0x06001BD4 RID: 7124 RVA: 0x0004E73C File Offset: 0x0004D73C
		public bool IsBoolean(TypeClass tc)
		{
			return TypeTable.IsBoolean(tc);
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x0004E744 File Offset: 0x0004D744
		public bool IsNumber(TypeClass tc)
		{
			return TypeTable.IsNumber(tc);
		}

		// Token: 0x06001BD6 RID: 7126 RVA: 0x0004E74C File Offset: 0x0004D74C
		public bool IsInteger(TypeClass tc)
		{
			return TypeTable.IsInteger(tc);
		}

		// Token: 0x06001BD7 RID: 7127 RVA: 0x0004E754 File Offset: 0x0004D754
		[Obsolete("Use IsLInteger(TypeClass tc, IScope scope) instead.")]
		public bool IsLInteger(TypeClass tc)
		{
			return TypeTable.IsLInteger(tc, null);
		}

		// Token: 0x06001BD8 RID: 7128 RVA: 0x0004E75D File Offset: 0x0004D75D
		public bool IsReal(TypeClass tc)
		{
			return TypeTable.IsReal(tc);
		}

		// Token: 0x06001BD9 RID: 7129 RVA: 0x0004E765 File Offset: 0x0004D765
		public bool IsString(TypeClass tc)
		{
			return TypeTable.IsString(tc);
		}

		// Token: 0x06001BDA RID: 7130 RVA: 0x0004E76D File Offset: 0x0004D76D
		[Obsolete("Use GetSize(TypeClass tc, IScope scope) instead.")]
		public int GetSize(TypeClass tc)
		{
			return TypeTable.GetSize(tc, null);
		}

		// Token: 0x06001BDB RID: 7131 RVA: 0x0004E776 File Offset: 0x0004D776
		public int GetSize(TypeClass tc, IScope scope)
		{
			return TypeTable.GetSize(tc, scope);
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x0004E77F File Offset: 0x0004D77F
		public bool IsLInteger(TypeClass tc, IScope scope)
		{
			return TypeTable.IsLInteger(tc, scope);
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x0004E788 File Offset: 0x0004D788
		public string GetIecName(TypeClass tc)
		{
			return CompilerProxy.GetTextOfOperator(TypeTable.GetOperatorByType(tc));
		}

		// Token: 0x06001BDE RID: 7134 RVA: 0x0004E795 File Offset: 0x0004D795
		public ulong GetTypeRangeHigh(TypeClass tc)
		{
			return TypeTable.GetTypeRangeHigh(tc);
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x0004E79D File Offset: 0x0004D79D
		public long GetTypeRangeLow(TypeClass tc)
		{
			return TypeTable.GetTypeRangeLow(tc);
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x0004E7A5 File Offset: 0x0004D7A5
		public bool IsResolvedXType(IType type)
		{
			return TypeTable.IsResolvedXType(type);
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x0004E7B0 File Offset: 0x0004D7B0
		public static long GetNumericValue(object o, IScope5 scope)
		{
			if (o is ulong)
			{
				return (long)((ulong)o);
			}
			if (o is string && scope != null)
			{
				try
				{
					IExpression expression = CompilerProxy.CreateParser(o as string).ParseExpression();
					IVariable[] array;
					if (expression is ICompoAccessExpression)
					{
						ISignature[] array2;
						IScope scope2;
						scope.FindDeclaration((expression as _ICompoAccessExpression)._Right, out array, out array2, out scope2);
					}
					else
					{
						ISignature[] array2;
						IScope scope2;
						scope.FindDeclaration(expression, out array, out array2, out scope2);
					}
					if (array != null && array.Length == 1 && (array[0].HasFlag(VarFlag.Constant) || array[0].HasFlag(VarFlag.ReplacedConstant)))
					{
						ILiteralValue literalValue = array[0].Initial.Literal(scope);
						if (literalValue.KindOf == KindOfLiteral.SignedInteger)
						{
							return literalValue.SignedLong;
						}
						if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
						{
							return (long)literalValue.UnsignedLong;
						}
					}
				}
				catch
				{
				}
			}
			return long.Parse(o.ToString());
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x0004E8A0 File Offset: 0x0004D8A0
		public static ulong GetUnsignedNumericValue(object o, IScope5 scope)
		{
			if (o is ulong)
			{
				return (ulong)o;
			}
			return (ulong)TypeHelper.GetNumericValue(o, scope);
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x0004E8B8 File Offset: 0x0004D8B8
		public static int GetInt(IExpression exp, IScope scope, out bool bValid)
		{
			return TypeHelper.GetInt(exp, scope, false, out bValid);
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x0004E8C4 File Offset: 0x0004D8C4
		public static int GetInt(IExpression exp, IScope scope, bool bAllocatedOK, IRecursionGuard recursionGuard, out bool bValid)
		{
			bValid = false;
			if (exp == null)
			{
				return -1;
			}
			bool flag = false;
			_IExpression2 iexpression = exp as _IExpression2;
			ILiteralValue literalValue = (iexpression != null) ? iexpression.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out flag) : null;
			bValid = (literalValue != null && !flag);
			if (!bValid)
			{
				return -1;
			}
			return literalValue.GetInt(out bValid);
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x0004E910 File Offset: 0x0004D910
		public static int GetInt(IExpression exp, IScope scope, bool bAllocatedOK, out bool bValid)
		{
			bValid = false;
			_IExpression iexpression = exp as _IExpression;
			ILiteralValue literalValue = (iexpression != null) ? iexpression.Literal(scope, bAllocatedOK) : null;
			if (literalValue == null)
			{
				return -1;
			}
			return literalValue.GetInt(out bValid);
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x0004E944 File Offset: 0x0004D944
		public static int GetInt(IExpression exp, IPrecompileScope scope, out bool bValid)
		{
			bValid = false;
			if (exp == null || !(exp is _IExpression))
			{
				return -1;
			}
			ILiteralValue literalValue = ((_IExpression)exp).Literal(scope);
			if (literalValue == null)
			{
				return -1;
			}
			return literalValue.GetInt(out bValid);
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x0004E97A File Offset: 0x0004D97A
		public bool IsVarLenArray(IVariable var)
		{
			return VarLenArray.IsVarLenArray(var);
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x0004E982 File Offset: 0x0004D982
		public int GetDimensions(IVariable var)
		{
			return VarLenArray.GetDimensions(var);
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x0004E98A File Offset: 0x0004D98A
		public IVariable GetDimensionInfoVariable(ISignature declaringSign, IVariable var)
		{
			return VarLenArray.GetDimensionInfoVariable(declaringSign, var);
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x0004E993 File Offset: 0x0004D993
		public bool IsVarLenArray(IVariable var, out string stOriginalType)
		{
			return VarLenArray.IsVarLenArray(var, out stOriginalType);
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x0004E99C File Offset: 0x0004D99C
		public VarFlag GetOriginalVariableDeclarationScope(IVariable var)
		{
			return VarLenArray.GetOriginalVariableDeclarationScope(var);
		}

		// Token: 0x040005E5 RID: 1509
		public static readonly TypeHelper Singleton = new TypeHelper();
	}
}
