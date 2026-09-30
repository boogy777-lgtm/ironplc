using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200034F RID: 847
	public class VarStatInitValueChecker : StandardVisitor
	{
		// Token: 0x06003313 RID: 13075 RVA: 0x000C5B38 File Offset: 0x000C3D38
		private VarStatInitValueChecker(IScope5 scope) : base(AccessFlag.None)
		{
			this.Scope = scope;
		}

		// Token: 0x17000866 RID: 2150
		// (get) Token: 0x06003314 RID: 13076 RVA: 0x000C5B50 File Offset: 0x000C3D50
		private IScope5 Scope { get; }

		// Token: 0x06003315 RID: 13077 RVA: 0x000C5B58 File Offset: 0x000C3D58
		public static bool IsVarStatInit(_IExpression expr, IScope5 scope)
		{
			return new VarStatInitValueChecker(scope).\u0001(expr);
		}

		// Token: 0x06003316 RID: 13078 RVA: 0x000C5B68 File Offset: 0x000C3D68
		private bool \u0001(_IExprement \u0002)
		{
			this.\u0001 = true;
			\u0002.Accept(this);
			return this.\u0001;
		}

		// Token: 0x06003317 RID: 13079 RVA: 0x000C5B80 File Offset: 0x000C3D80
		public override void visit(_IAssignmentExpression assign)
		{
			base.Push(AccessFlag.Read);
			assign._RValue.Accept(this);
			base.Pop();
		}

		// Token: 0x06003318 RID: 13080 RVA: 0x000C5B9C File Offset: 0x000C3D9C
		public override void visit(_ICallExpression call)
		{
			_IUserdefType iuserdefType = call.Callee.Type as _IUserdefType;
			if (iuserdefType != null)
			{
				ISignature signature = iuserdefType.GetSignature(this.Scope);
				if (signature.POUType == Operator.Method || signature.POUType == Operator.Program)
				{
					this.\u0001 = false;
					return;
				}
			}
			base.visit(call);
		}

		// Token: 0x06003319 RID: 13081 RVA: 0x000C5BF0 File Offset: 0x000C3DF0
		public override void visit(_IOperatorExpression op)
		{
			if (op.Code != Operator.SizeOf && op.Code != Operator.XSizeOf)
			{
				base.visit(op);
			}
		}

		// Token: 0x0600331A RID: 13082 RVA: 0x000C5C10 File Offset: 0x000C3E10
		public override void visit(_IVariableExpression varExpr)
		{
			if (base.TopOfStack.Access == AccessFlag.Write)
			{
				return;
			}
			IVariable variable = varExpr.GetVariable(this.Scope);
			if (variable == null || variable.GetFlag(VarFlag.ReplacedConstant))
			{
				return;
			}
			if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				this.\u0001 = false;
			}
			if (!variable.GetFlag(VarFlag.Static) && !variable.GetFlag(VarFlag.Global))
			{
				this.\u0001 = false;
			}
		}

		// Token: 0x040009AD RID: 2477
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040009AE RID: 2478
		private bool \u0001 = true;
	}
}
