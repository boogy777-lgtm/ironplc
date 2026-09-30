using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u000F;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x02000264 RID: 612
	internal sealed class \u0010 : global::\u0011.\u000F
	{
		// Token: 0x17000719 RID: 1817
		// (get) Token: 0x06002786 RID: 10118 RVA: 0x00088B38 File Offset: 0x00086D38
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x1700071A RID: 1818
		// (get) Token: 0x06002787 RID: 10119 RVA: 0x00088B40 File Offset: 0x00086D40
		private IScope5 _Scope
		{
			get
			{
				return this.Context._Scope;
			}
		}

		// Token: 0x1700071B RID: 1819
		// (get) Token: 0x06002788 RID: 10120 RVA: 0x00088B5C File Offset: 0x00086D5C
		// (set) Token: 0x06002789 RID: 10121 RVA: 0x00088B64 File Offset: 0x00086D64
		private bool CallAssign { get; set; }

		// Token: 0x0600278A RID: 10122 RVA: 0x00088B70 File Offset: 0x00086D70
		public \u0010(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
			this.CallAssign = false;
		}

		// Token: 0x0600278B RID: 10123 RVA: 0x00088B88 File Offset: 0x00086D88
		public void \u0001(_IAssignmentExpression \u0002, _ICompiledPOU \u0003)
		{
			this.CallAssign = false;
			AssignmentInfo assignmentInfo;
			this.\u0001(new AssignmentInfo(\u0002), out assignmentInfo);
			\u0002._LValue = assignmentInfo.LValue;
			\u0002._RValue = assignmentInfo.RValue;
		}

		// Token: 0x0600278C RID: 10124 RVA: 0x00088BC4 File Offset: 0x00086DC4
		public void \u0002(_IAssignmentExpression \u0002, _ICompiledPOU \u0003)
		{
			this.\u0001(\u0002, \u0003);
		}

		// Token: 0x0600278D RID: 10125 RVA: 0x00088BD0 File Offset: 0x00086DD0
		public void \u0001(_ICallExpression \u0002, _ICompiledPOU \u0003)
		{
			this.CallAssign = true;
			CallParameterEnumerable callParameterEnumerable = \u0002.\u0001();
			int num = 0;
			foreach (AssignmentInfo u in callParameterEnumerable)
			{
				AssignmentInfo assignmentInfo;
				this.\u0001(u, out assignmentInfo);
				\u0002.SetActualParam(assignmentInfo.RValue, num);
				\u0002.SetFormalParam(assignmentInfo.LValue, num);
				num++;
			}
		}

		// Token: 0x0600278E RID: 10126 RVA: 0x00088C34 File Offset: 0x00086E34
		private bool \u0001(_IExpression \u0002)
		{
			ILiteralValue literalValue = \u0002.Literal(this._Scope);
			int num;
			return literalValue != null && literalValue.GetInt(out num) && num == 0;
		}

		// Token: 0x0600278F RID: 10127 RVA: 0x00088C64 File Offset: 0x00086E64
		internal static bool \u0001(AssignmentInfo \u0002, bool \u0003, IScope5 \u0004)
		{
			if (\u0002.KindOf == Operator.AssignOut)
			{
				return false;
			}
			if (\u0002.KindOf == Operator.RefAssign)
			{
				return true;
			}
			if (!\u0003)
			{
				return false;
			}
			IVariable variable = \u0002.LValue.GetVariable(\u0004);
			return variable != null && variable.Type.Class == TypeClass.Reference;
		}

		// Token: 0x06002790 RID: 10128 RVA: 0x00088CB8 File Offset: 0x00086EB8
		private bool \u0001(_IExpression \u0002, out _IExpression \u0003)
		{
			bool result = false;
			_IDeRefAccessExpression ideRefAccessExpression = \u0002 as _IDeRefAccessExpression;
			if (ideRefAccessExpression == null)
			{
				_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
				if (ipoolScopeExpression == null)
				{
					_ICopyScopeExpression icopyScopeExpression = \u0002 as _ICopyScopeExpression;
					if (icopyScopeExpression == null)
					{
						_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
						if (inamespaceAccessExpression == null)
						{
							_IGlobalScopeExpression iglobalScopeExpression = \u0002 as _IGlobalScopeExpression;
							if (iglobalScopeExpression != null)
							{
								result = this.\u0001(iglobalScopeExpression._Base, out \u0003);
								iglobalScopeExpression._Base = \u0003;
							}
						}
						else
						{
							result = this.\u0001(inamespaceAccessExpression._Access, out \u0003);
							inamespaceAccessExpression._Access = \u0003;
						}
					}
					else
					{
						result = this.\u0001(icopyScopeExpression._Base, out \u0003);
						icopyScopeExpression._Base = \u0003;
					}
				}
				else
				{
					result = this.\u0001(ipoolScopeExpression._Base, out \u0003);
					ipoolScopeExpression._Base = \u0003;
				}
				\u0003 = \u0002;
				return result;
			}
			\u0003 = ideRefAccessExpression._Base;
			return true;
		}

		// Token: 0x06002791 RID: 10129 RVA: 0x00088D70 File Offset: 0x00086F70
		private void \u0001(AssignmentInfo \u0002, out AssignmentInfo \u0003)
		{
			\u0003 = \u0002;
			if (!global::\u0014.\u0010.\u0001(\u0002, this.CallAssign, this._Scope))
			{
				return;
			}
			_IExpression iexpression = \u0002.RValue;
			_IExpression lvalue = \u0002.LValue;
			if (!this.\u0001(iexpression) && iexpression.Type.Class != TypeClass.Reference && !this.\u0001(iexpression, out iexpression))
			{
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr, Token.Empty);
				ioperatorExpression.AddOperand(\u0002.RValue.Duplicate() as _IExpression);
				iexpression = this.Context.Generator.\u0001<_IOperatorExpression>(ioperatorExpression, this.Context._Scope, this.Context.CompiledPOU);
			}
			if (lvalue.Type.Class != TypeClass.Reference)
			{
				this.\u0001(lvalue, out lvalue);
			}
			\u0003 = new AssignmentInfo(lvalue, iexpression, \u0002.KindOf);
		}

		// Token: 0x04000731 RID: 1841
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000732 RID: 1842
		[CompilerGenerated]
		private bool \u0001;
	}
}
