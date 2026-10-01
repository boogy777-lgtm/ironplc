using System;
using System.Collections.Generic;
using \u0004;
using \u0005;
using \u0006;
using \u0008;
using \u0017;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0082;

namespace \u0002
{
	// Token: 0x020000A4 RID: 164
	internal sealed class \u0002 : IConstantFolder3, IConstantFolder2, IConstantFolder
	{
		// Token: 0x06000D52 RID: 3410 RVA: 0x00022BF0 File Offset: 0x00020DF0
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, bool \u0004)
		{
			EConstantFoldingResult econstantFoldingResult;
			return this.\u0001(\u0002, \u0003, \u0004, out econstantFoldingResult);
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x00022C08 File Offset: 0x00020E08
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, bool \u0004, out EConstantFoldingResult \u0005)
		{
			\u0005 = EConstantFoldingResult.None;
			ICompiledType type = \u0002.Type;
			Operator code = \u0002.Code;
			global::\u0005.\u0001 u = ConstantFoldingHelper.\u0001(\u0002, \u0003, \u0004, type, code);
			if (u == global::\u0005.\u0001.\u0008)
			{
				return null;
			}
			ILiteralValue literalValue = this.\u0001(\u0003, code);
			if (literalValue != null)
			{
				return literalValue;
			}
			global::\u0008.\u0004 u2 = this.\u0001(u);
			return (u2 != null) ? u2.\u0001(\u0002, \u0003, out \u0005) : null;
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x00022C60 File Offset: 0x00020E60
		private ILiteralValue \u0001(ILiteralValue[] \u0002, Operator \u0003)
		{
			if (\u0003 != Operator.Mux)
			{
				if (\u0003 != Operator.Sel)
				{
					if (\u0003 != Operator.Move)
					{
						return null;
					}
					return \u0002[0];
				}
				else
				{
					bool flag;
					bool boolV = \u0002[0].GetBoolV(out flag);
					if (!flag || \u0002.Length != 3)
					{
						return null;
					}
					if (boolV)
					{
						return \u0002[2];
					}
					return \u0002[1];
				}
			}
			else
			{
				bool flag2;
				int num = \u0002[0].GetInt(out flag2);
				num++;
				if (!flag2 || num < 0 || num >= \u0002.Length)
				{
					return null;
				}
				return \u0002[num];
			}
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x00022CCC File Offset: 0x00020ECC
		private global::\u0008.\u0004 \u0001(global::\u0005.\u0001 \u0002)
		{
			switch (\u0002)
			{
			case global::\u0005.\u0001.\u0001:
				return new \u0082.\u0002();
			case global::\u0005.\u0001.\u0002:
				return new global::\u0004.\u0001();
			case global::\u0005.\u0001.\u0003:
				return new global::\u0017.\u0002();
			case global::\u0005.\u0001.\u0004:
				return new \u001C.\u0002();
			case global::\u0005.\u0001.\u0006:
				return new \u0080.\u0002();
			case global::\u0005.\u0001.\u0007:
				return new DateOperationConstantFolder();
			}
			return null;
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00022D24 File Offset: 0x00020F24
		public ILiteralValue \u0001(_IOperatorExpression \u0002, IScope \u0003)
		{
			ILiteralValue result;
			if (global::\u0002.\u0002.\u0001(\u0002, \u0003, out result))
			{
				return result;
			}
			if (!\u0002.IsConstant(\u0003, true) || \u0002.Type == null || \u0002._OperandsList.Count == 0)
			{
				return null;
			}
			ILiteralValue[] array = new ILiteralValue[\u0002._OperandsList.Count];
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				_IExpression iexpression = \u0002._OperandsList[i];
				array[i] = iexpression.LiteralUnchecked(\u0003);
				if (array[i] == null)
				{
					return null;
				}
			}
			return this.\u0001(\u0002, array, false);
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x00022DB0 File Offset: 0x00020FB0
		public ILiteralValue \u0001(_IExpression \u0002, ICommonScope \u0003)
		{
			return \u0080.\u0001.\u0001((_IExpression2)\u0002, this, \u0003, new RecursionGuard());
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x00022DC4 File Offset: 0x00020FC4
		public ILiteralValue \u0001(_IExpression \u0002, ICommonScope \u0003, IRecursionGuard \u0004)
		{
			return \u0080.\u0001.\u0001((_IExpression2)\u0002, this, \u0003, \u0004);
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x00022DD4 File Offset: 0x00020FD4
		public bool \u0001(_IExpression \u0002, ICommonScope \u0003, IRecursionGuard \u0004, out int \u0005)
		{
			\u0005 = 0;
			bool result;
			try
			{
				ILiteralValue literalValue = this.\u0001(\u0002, \u0003, \u0004);
				if (literalValue == null)
				{
					result = false;
				}
				else
				{
					bool flag;
					\u0005 = literalValue.GetInt(out flag);
					result = flag;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x00022E1C File Offset: 0x0002101C
		private static bool \u0001(_IOperatorExpression \u0002, IScope \u0003, out ILiteralValue \u0004)
		{
			\u0004 = null;
			if (\u0002.Code != Operator.SizeOf && Operator.XSizeOf != \u0002.Code)
			{
				return false;
			}
			if (\u0002._OperandsList.Count != 1)
			{
				return true;
			}
			_IExpression iexpression = \u0002._OperandsList[0];
			if (iexpression.Type == null)
			{
				return true;
			}
			int num = ((_IType)iexpression.Type.DeRefType).Size(\u0003);
			if (num > 1)
			{
				IScope5 scope = \u0003 as IScope5;
				if (scope != null && scope.Codegenerator != null)
				{
					ICodegenerator3 codegenerator = scope.Codegenerator as ICodegenerator3;
					if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.WordAddressing))
					{
						num /= 2;
					}
				}
			}
			\u0004 = global::\u0019.\u0003.\u0001((long)num);
			return true;
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x00022EC0 File Offset: 0x000210C0
		internal ILiteralValue \u0001(_IOperatorExpression \u0002, ICommonScope \u0003, IRecursionGuard \u0004, bool \u0005, out bool \u0006)
		{
			IScope scope = \u0003 as IScope;
			if (scope != null)
			{
				return this.\u0001(\u0002, scope, \u0004, \u0005, out \u0006);
			}
			IPrecompileScope precompileScope = \u0003 as IPrecompileScope;
			if (precompileScope != null)
			{
				return this.\u0001(\u0002, precompileScope, \u0004, \u0005, out \u0006);
			}
			\u0006 = false;
			return null;
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x00022F04 File Offset: 0x00021104
		public ILiteralValue \u0001(_IOperatorExpression \u0002, IScope \u0003, IRecursionGuard \u0004, bool \u0005, out bool \u0006)
		{
			if (\u0004 == null)
			{
				\u0004 = new RecursionGuard();
			}
			EConstantFoldingResult econstantFoldingResult;
			ILiteralValue result = this.\u0001(\u0002, \u0003, \u0004, \u0005, out econstantFoldingResult);
			\u0006 = ((EConstantFoldingResult.RecursionError & econstantFoldingResult) > EConstantFoldingResult.None);
			return result;
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x00022F34 File Offset: 0x00021134
		public ILiteralValue \u0001(_IOperatorExpression \u0002, IScope \u0003, IRecursionGuard \u0004, bool \u0005, out EConstantFoldingResult \u0006)
		{
			\u0006 = EConstantFoldingResult.None;
			Operator code = \u0002.Code;
			if (code != Operator.SizeOf)
			{
				switch (code)
				{
				case Operator.__LocalOffset:
					return global::\u0004.\u0003.\u0001(\u0002._OperandsList, \u0003);
				case Operator.__VarInfo:
				case Operator.__SystemScope:
					break;
				case Operator.__TypeOf:
					return global::\u0002.\u0002.\u0001(\u0002._OperandsList);
				case Operator.__CRC:
					return global::\u0002.\u0002.\u0001(\u0002._OperandsList, \u0003);
				case Operator.__MaxOffset:
					return global::\u0002.\u0002.\u0002(\u0002._OperandsList, \u0003);
				default:
					if (code == Operator.XSizeOf)
					{
						goto IL_3F;
					}
					break;
				}
				if (!\u0002.IsConstant(\u0003, \u0005) || \u0002.Type == null || \u0002._OperandsList.Count == 0)
				{
					return null;
				}
				ILiteralValue[] array = new ILiteralValue[\u0002.Operands.Length];
				for (int i = 0; i < \u0002.Operands.Length; i++)
				{
					_IExpression2 iexpression = (_IExpression2)\u0002.Operands[i];
					bool flag;
					array[i] = iexpression.LiteralWithRecursionCheck(\u0003, \u0004, \u0005, out flag);
					if (flag)
					{
						\u0006 |= EConstantFoldingResult.RecursionError;
						return null;
					}
					if (array[i] == null)
					{
						return null;
					}
				}
				return this.\u0001(\u0002, array, false, out \u0006);
			}
			IL_3F:
			return global::\u0002.\u0002.\u0001(\u0002, (IScope5)\u0003, \u0004);
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x00023044 File Offset: 0x00021244
		public ILiteralValue \u0001(_IOperatorExpression \u0002, IPrecompileScope \u0003, IRecursionGuard \u0004, bool \u0005, out bool \u0006)
		{
			\u0006 = false;
			if (\u0002._OperandsList.Count == 0)
			{
				return null;
			}
			ILiteralValue result;
			if (global::\u0002.\u0002.\u0001(\u0002, \u0003, \u0004, out result))
			{
				return result;
			}
			ILiteralValue[] array = new ILiteralValue[\u0002._OperandsList.Count];
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				_IExpression2 u = (_IExpression2)\u0002._OperandsList[i];
				try
				{
					array[i] = this.\u0001(u, (global::\u0017.\u0006)\u0003, \u0004);
				}
				catch (RecursiveConstantException)
				{
					\u0006 = true;
				}
				if (array[i] == null)
				{
					return null;
				}
			}
			return this.\u0001(\u0002, array, true);
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x000230E8 File Offset: 0x000212E8
		private static bool \u0001(_IOperatorExpression \u0002, IPrecompileScope \u0003, IRecursionGuard \u0004, out ILiteralValue \u0005)
		{
			\u0005 = null;
			if (\u0002.Code == Operator.SizeOf || Operator.XSizeOf == \u0002.Code)
			{
				ICommonScope commonScope = \u0003 as ICommonScope;
				if (commonScope != null)
				{
					if (\u0002._OperandsList.Count != 1)
					{
						return true;
					}
					_IExpression iexpression = \u0002._OperandsList[0];
					ICompiledType type = iexpression.Type;
					if (iexpression.Type == null && global::\u0002.\u0002.\u0001(\u0003, ref \u0005, commonScope, iexpression, ref type))
					{
						return true;
					}
					ICompiledType type2 = global::\u0006.\u0011.\u0001(type.DeRefType, commonScope);
					int size;
					if (\u0004 != null)
					{
						ICommonScope2 commonScope2 = commonScope as ICommonScope2;
						if (commonScope2 != null)
						{
							size = commonScope2.GetSize(type2, \u0004);
							goto IL_94;
						}
					}
					size = commonScope.GetSize(type2);
					IL_94:
					\u0005 = global::\u0019.\u0003.\u0001((long)size);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x00023198 File Offset: 0x00021398
		private static bool \u0001(IPrecompileScope \u0002, ref ILiteralValue \u0003, ICommonScope \u0004, _IExpression \u0005, ref ICompiledType \u0006)
		{
			ISignature[] array = \u0004.FindSignature(\u0005);
			if (array != null && array.Length == 1)
			{
				\u0006 = global::\u0019.\u0003.\u0001(\u0005);
			}
			else if (array == null || array.Length == 0)
			{
				IVariable variable;
				ISignature signature;
				IPrecompileScope precompileScope;
				\u0002.FindDeclaration(\u0005.ToString(), out variable, out signature, out precompileScope);
				if (variable != null)
				{
					\u0006 = (variable.Type as ICompiledType);
				}
			}
			if (\u0006 == null)
			{
				_IIndexAccessExpression iindexAccessExpression = \u0005 as _IIndexAccessExpression;
				if (iindexAccessExpression != null)
				{
					ISignature signature;
					IPrecompileScope precompileScope;
					IVariable variable2;
					\u0002.FindDeclaration(iindexAccessExpression.Var.ToString(), out variable2, out signature, out precompileScope);
					if (variable2 != null && variable2.Type != null && variable2.Type.Class == TypeClass.Array)
					{
						_IArrayType iarrayType = (_IArrayType)variable2.Type;
						\u0006 = iarrayType.BaseType;
					}
				}
			}
			if (\u0006 == null)
			{
				\u0003 = null;
				return true;
			}
			return false;
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x00023254 File Offset: 0x00021454
		internal static int? \u0001(ICompiledType \u0002, IScope \u0003, IRecursionGuard \u0004)
		{
			\u0002 = global::\u0006.\u0011.\u0001(\u0002.DeRefType, \u0003 as ICommonScope);
			ITypeWithRecursiveTypeCheck typeWithRecursiveTypeCheck = \u0002 as ITypeWithRecursiveTypeCheck;
			bool flag;
			int num;
			if (typeWithRecursiveTypeCheck != null)
			{
				num = typeWithRecursiveTypeCheck.SizeWithRecursionCheck(\u0003, \u0004, out flag);
			}
			else
			{
				num = ((_IType)\u0002).SizeChecked(\u0003, out flag);
			}
			if (!flag)
			{
				return null;
			}
			if (num > 1)
			{
				IScope5 scope = \u0003 as IScope5;
				if (scope != null && scope.Codegenerator != null)
				{
					ICodegenerator3 codegenerator = scope.Codegenerator as ICodegenerator3;
					if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.WordAddressing))
					{
						num /= 2;
					}
				}
			}
			return new int?(num);
		}

		// Token: 0x06000D62 RID: 3426 RVA: 0x000232E8 File Offset: 0x000214E8
		private static ILiteralValue \u0001(_IOperatorExpression \u0002, IScope5 \u0003, IRecursionGuard \u0004)
		{
			if (!OperationOnTypeChecker.CheckForValidOperationOnType(\u0002, \u0003, null))
			{
				return null;
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != 1)
			{
				return null;
			}
			_IExpression iexpression = operandsList[0];
			if (iexpression.Type == null)
			{
				return null;
			}
			int? num = global::\u0002.\u0002.\u0001(iexpression.Type, \u0003, \u0004);
			if (num == null)
			{
				return null;
			}
			return global::\u0019.\u0003.\u0001((long)num.Value);
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x0002334C File Offset: 0x0002154C
		private static ILiteralValue \u0001(IList<_IExpression> \u0002, IScope \u0003)
		{
			if (\u0002.Count != 1)
			{
				return null;
			}
			ISignature signature = \u0002[0].GetSignature(\u0003);
			if (signature == null || !signature.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				return null;
			}
			string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			uint num;
			try
			{
				num = uint.Parse(attributeValue);
			}
			catch
			{
				return null;
			}
			return global::\u0019.\u0003.\u0001((ulong)num);
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x000233B8 File Offset: 0x000215B8
		private static ILiteralValue \u0002(IList<_IExpression> \u0002, IScope \u0003)
		{
			if (\u0002.Count != 1)
			{
				return null;
			}
			ISignature signature = \u0002[0].GetSignature(\u0003);
			if (signature == null)
			{
				return null;
			}
			if (!signature.GetFlag(SignatureFlag.Located))
			{
				return null;
			}
			int num = 0;
			foreach (IVariable variable in signature.All)
			{
				if (variable != null)
				{
					IDataLocation dataLocation = variable.DataLocation;
					if (dataLocation != null)
					{
						int num2 = variable.CompiledType.Size(\u0003);
						if (dataLocation.Offset + num2 > num)
						{
							num = dataLocation.Offset + num2;
						}
					}
				}
			}
			return global::\u0019.\u0003.\u0001((ulong)((long)num));
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x00023450 File Offset: 0x00021650
		private static ILiteralValue \u0001(IList<_IExpression> \u0002)
		{
			if (\u0002.Count != 1)
			{
				return global::\u0019.\u0003.\u0001(29L);
			}
			_IExpression iexpression = \u0002[0];
			if (iexpression.Type == null)
			{
				return global::\u0019.\u0003.\u0001(29L);
			}
			return global::\u0019.\u0003.\u0001((long)iexpression.Type.DeRefType.Class);
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x000234A0 File Offset: 0x000216A0
		private ILiteralValue \u0001(_IExpression \u0002, ICommonScope \u0003, IRecursionGuard \u0004, out bool \u0005)
		{
			if (\u0004 == null)
			{
				\u0004 = new RecursionGuard();
			}
			ILiteralValue result = null;
			\u0005 = false;
			try
			{
				result = this.\u0001(\u0002, \u0003, \u0004);
			}
			catch (RecursiveConstantException)
			{
				\u0005 = true;
			}
			return result;
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x000234E4 File Offset: 0x000216E4
		public ILiteralValue \u0001(_IExpression \u0002, IPrecompileScope \u0003, out bool \u0004)
		{
			return this.\u0001(\u0002, (ICommonScope)\u0003, new RecursionGuard(), out \u0004);
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x000234FC File Offset: 0x000216FC
		public ILiteralValue \u0001(_IExpression \u0002, IScope \u0003, out bool \u0004)
		{
			return this.\u0001(\u0002, (ICommonScope)\u0003, new RecursionGuard(), out \u0004);
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x00023514 File Offset: 0x00021714
		public ILiteralValue \u0001(_IExpression \u0002, IPrecompileScope \u0003, IRecursionGuard \u0004, out bool \u0005)
		{
			return this.\u0001(\u0002, (ICommonScope)\u0003, \u0004, out \u0005);
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x00023528 File Offset: 0x00021728
		public ILiteralValue \u0001(_IExpression \u0002, IScope \u0003, IRecursionGuard \u0004, out bool \u0005)
		{
			return this.\u0001(\u0002, (ICommonScope)\u0003, \u0004, out \u0005);
		}
	}
}
