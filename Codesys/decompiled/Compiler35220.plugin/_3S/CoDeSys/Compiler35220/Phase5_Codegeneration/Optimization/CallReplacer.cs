using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0018;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000268 RID: 616
	public class CallReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x1700071C RID: 1820
		// (get) Token: 0x0600279D RID: 10141 RVA: 0x00089608 File Offset: 0x00087808
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x1700071D RID: 1821
		// (get) Token: 0x0600279E RID: 10142 RVA: 0x00089610 File Offset: 0x00087810
		private Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] CallHandlers { get; }

		// Token: 0x1700071E RID: 1822
		// (get) Token: 0x0600279F RID: 10143 RVA: 0x00089618 File Offset: 0x00087818
		// (set) Token: 0x060027A0 RID: 10144 RVA: 0x00089620 File Offset: 0x00087820
		private _ICompiledPOU POU { get; set; }

		// Token: 0x060027A1 RID: 10145 RVA: 0x0008962C File Offset: 0x0008782C
		private CallReplacer(global::\u000E.\u0011 context, Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] callHandlers)
		{
			this.Context = context;
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
			this.CallHandlers = callHandlers;
		}

		// Token: 0x060027A2 RID: 10146 RVA: 0x00089650 File Offset: 0x00087850
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.POU = cpou;
			this.\u0001.ReplaceCode(cpou);
		}

		// Token: 0x060027A3 RID: 10147 RVA: 0x00089668 File Offset: 0x00087868
		public override _IExpression ReplaceCallExpression(_ICallExpression callExpression)
		{
			_ISignature arg = this.\u0001(callExpression);
			_IExpression iexpression = callExpression;
			_ICallExpression arg2 = callExpression;
			Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] array = this.CallHandlers;
			for (int i = 0; i < array.Length; i++)
			{
				iexpression = array[i](arg, this.POU, arg2, this.Context);
				_ICallExpression icallExpression = iexpression as _ICallExpression;
				if (icallExpression == null)
				{
					break;
				}
				arg2 = icallExpression;
			}
			return iexpression;
		}

		// Token: 0x060027A4 RID: 10148 RVA: 0x000896C4 File Offset: 0x000878C4
		internal _ISignature \u0001(_ICallExpression \u0002)
		{
			_ISignature isignature = null;
			if (\u0002._Callee.GetVariable(this.Context._Scope) != null)
			{
				_IUserdefType iuserdefType = \u0002._Callee.Type.DeRefType as _IUserdefType;
				if (iuserdefType != null)
				{
					isignature = (iuserdefType.GetSignature(this.Context._Scope) as _ISignature);
				}
			}
			else
			{
				isignature = (\u0002._Callee.GetSignature(this.Context._Scope) as _ISignature);
			}
			if (isignature == null)
			{
				_IUserdefType iuserdefType2 = \u0002._Callee.Type.DeRefType as _IUserdefType;
				if (iuserdefType2 != null)
				{
					isignature = (iuserdefType2.GetSignature(this.Context._Scope) as _ISignature);
				}
			}
			return isignature;
		}

		// Token: 0x060027A5 RID: 10149 RVA: 0x0008977C File Offset: 0x0008797C
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002, Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] \u0003, Func<_ISignature, _ICallExpression, bool>[] \u0004)
		{
			CallReplacer callReplacer = new CallReplacer(\u0002, \u0003);
			return new ReplacerController(callReplacer, new global::\u0018.\u0007(callReplacer, \u0004));
		}

		// Token: 0x04000733 RID: 1843
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000734 RID: 1844
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000735 RID: 1845
		[CompilerGenerated]
		private readonly Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] \u0001;

		// Token: 0x04000736 RID: 1846
		[CompilerGenerated]
		private _ICompiledPOU \u0001;
	}
}
