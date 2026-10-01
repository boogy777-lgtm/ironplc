using System;
using \u0007;
using \u000E;
using \u000F;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x020002D4 RID: 724
	internal static class \u0019
	{
		// Token: 0x06002BC5 RID: 11205 RVA: 0x00099384 File Offset: 0x00097584
		internal static _IExpression \u0001(_ISignature \u0002, _ICompiledPOU \u0003, _ICallExpression \u0004, global::\u000E.\u0011 \u0005)
		{
			if ((\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method || \u0002.POUType == Operator.FunctionBlock) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_HASANYTYPE))
			{
				LList<IVariable> llist = new LList<IVariable>();
				\u0084.\u0019.\u0001(\u0002, llist, \u0005._Scope);
				LList<AssignmentInfo> llist2 = new LList<AssignmentInfo>();
				foreach (IVariable variable in llist)
				{
					int num;
					AssignmentInfo u = \u0084.\u0019.\u0001(\u0004, variable, out num);
					if (num >= 0 && !TypeTable.IsFunctionalAnyType(u.RValue._CompiledType))
					{
						AssignmentInfo assignmentInfo = \u0084.\u0019.\u0003(variable, u);
						llist2.Add(assignmentInfo);
						AssignmentInfo assignmentInfo2 = \u0084.\u0019.\u0002(variable, u);
						llist2.Add(assignmentInfo2);
						AssignmentInfo assignmentInfo3 = \u0084.\u0019.\u0001(variable, u);
						llist2.Add(assignmentInfo3);
						\u0004.RemoveInputAt(num);
					}
				}
				IScope5 u2 = global::\u0007.\u0005.\u0001(\u0005.Comcon, \u0002.Id);
				IScope5 u3 = \u0005._Scope;
				foreach (AssignmentInfo assignmentInfo4 in llist2)
				{
					_IExpression expVariable = \u0005.Generator.\u0001<_IExpression>(assignmentInfo4.LValue, u2, \u0003);
					_IExpression exp = \u0005.Generator.\u0001<_IExpression>(assignmentInfo4.RValue, u3, \u0003);
					\u0004.AddParam(exp, expVariable);
				}
			}
			return \u0004;
		}

		// Token: 0x06002BC6 RID: 11206 RVA: 0x00099508 File Offset: 0x00097708
		private static AssignmentInfo \u0001(_ICallExpression \u0002, IVariable \u0003, out int \u0004)
		{
			\u0004 = -1;
			int num = 0;
			foreach (AssignmentInfo result in \u0002.\u0001())
			{
				if (string.Compare(result.LValue.ToString(), \u0003.Name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					\u0004 = num;
					return result;
				}
				num++;
			}
			return new AssignmentInfo(null, null, Operator.None);
		}

		// Token: 0x06002BC7 RID: 11207 RVA: 0x00099568 File Offset: 0x00097768
		private static AssignmentInfo \u0001(IVariable \u0002, AssignmentInfo \u0003)
		{
			_IExpression lvalue = global::\u0019.\u0003.\u0001(\u0002.Name + "__sizeOf");
			_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.SizeOf);
			ioperatorExpression.AddOperand(\u0003.RValue.Duplicate() as _IExpression);
			_IExpression rvalue = global::\u0019.\u0003.\u0001(TypeClass.Any, TypeClass.DInt, ioperatorExpression);
			return new AssignmentInfo(lvalue, rvalue, Operator.Assign);
		}

		// Token: 0x06002BC8 RID: 11208 RVA: 0x000995C0 File Offset: 0x000977C0
		private static AssignmentInfo \u0002(IVariable \u0002, AssignmentInfo \u0003)
		{
			_IExpression lvalue = global::\u0019.\u0003.\u0001(\u0002.Name + "__typeClass");
			_IExpression rvalue = global::\u0019.\u0003.\u0001((long)\u0003.RValue._CompiledType.Class, TypeClass.DWord);
			return new AssignmentInfo(lvalue, rvalue, Operator.Assign);
		}

		// Token: 0x06002BC9 RID: 11209 RVA: 0x00099608 File Offset: 0x00097808
		private static AssignmentInfo \u0003(IVariable \u0002, AssignmentInfo \u0003)
		{
			_IExpression lvalue = global::\u0019.\u0003.\u0001(\u0002.Name + "__pValue");
			_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr);
			ioperatorExpression.AddOperand(\u0003.RValue.Duplicate() as _IExpression);
			_IExpression rvalue = ioperatorExpression;
			return new AssignmentInfo(lvalue, rvalue, Operator.Assign);
		}

		// Token: 0x06002BCA RID: 11210 RVA: 0x00099654 File Offset: 0x00097854
		private static void \u0001(ISignature \u0002, LList<IVariable> \u0003, IScope5 \u0004)
		{
			ISignature signature = \u0004[\u0002.BaseSignatureId];
			if (signature != null)
			{
				\u0084.\u0019.\u0001(signature, \u0003, \u0004);
			}
			foreach (IVariable variable in \u0002.Inputs)
			{
				if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_ANYTYPECLASS))
				{
					\u0003.Add(variable);
				}
			}
		}

		// Token: 0x06002BCB RID: 11211 RVA: 0x000996A8 File Offset: 0x000978A8
		public static bool \u0001(_ISignature \u0002, _ICallExpression \u0003)
		{
			return (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method || \u0002.POUType == Operator.FunctionBlock) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_HASANYTYPE);
		}
	}
}
