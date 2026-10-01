using System;
using System.Runtime.CompilerServices;
using \u0007;
using \u000E;
using \u000F;
using \u0011;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0018
{
	// Token: 0x02000260 RID: 608
	internal sealed class \u0006 : global::\u0011.\u000F
	{
		// Token: 0x17000710 RID: 1808
		// (get) Token: 0x06002760 RID: 10080 RVA: 0x00087D6C File Offset: 0x00085F6C
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x17000711 RID: 1809
		// (get) Token: 0x06002761 RID: 10081 RVA: 0x00087D74 File Offset: 0x00085F74
		private IScope5 _Scope
		{
			get
			{
				return this.Context._Scope;
			}
		}

		// Token: 0x06002762 RID: 10082 RVA: 0x00087D90 File Offset: 0x00085F90
		internal \u0006(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
		}

		// Token: 0x06002763 RID: 10083 RVA: 0x00087DA0 File Offset: 0x00085FA0
		public void \u0001(_IAssignmentExpression \u0002, _ICompiledPOU \u0003)
		{
			_IVariable u0016_u = \u0002._LValue.GetVariable(this.Context._Scope) as _IVariable;
			AssignmentInfo assignmentInfo = new global::\u000F.\u0012(this.Context, \u0003, new AssignmentInfo(\u0002), u0016_u).\u0002();
			\u0002._RValue = assignmentInfo.RValue;
			\u0002._LValue = assignmentInfo.LValue;
		}

		// Token: 0x06002764 RID: 10084 RVA: 0x00087E00 File Offset: 0x00086000
		public void \u0002(_IAssignmentExpression \u0002, _ICompiledPOU \u0003)
		{
			this.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002765 RID: 10085 RVA: 0x00087E0C File Offset: 0x0008600C
		public void \u0001(_ICallExpression \u0002, _ICompiledPOU \u0003)
		{
			_ISignature isignature = this.\u0001(\u0002);
			IScope5 scope = global::\u0007.\u0005.\u0001(this.Context.Comcon, isignature.Id);
			CallParameterEnumerable callParameterEnumerable = \u0002.\u0001();
			int num = 0;
			foreach (AssignmentInfo u0015_u in callParameterEnumerable)
			{
				_IVariable u0016_u = u0015_u.LValue.GetVariable(scope) as _IVariable;
				AssignmentInfo assignmentInfo = new global::\u000F.\u0012(this.Context, \u0003, u0015_u, u0016_u, scope).\u0002();
				\u0002.SetActualParam(assignmentInfo.RValue, num);
				\u0002.SetFormalParam(assignmentInfo.LValue, num);
				num++;
			}
			CallOutputParameterEnumerable callOutputParameterEnumerable = \u0002.\u0001();
			num = 0;
			foreach (AssignmentInfo u0015_u2 in callOutputParameterEnumerable)
			{
				_IVariable u0016_u2 = u0015_u2.LValue.GetVariable(this._Scope) as _IVariable;
				\u0002.SetActualOutput(new global::\u000F.\u0012(this.Context, \u0003, u0015_u2, u0016_u2, scope).\u0002().LValue, num);
				num++;
			}
		}

		// Token: 0x06002766 RID: 10086 RVA: 0x00087F18 File Offset: 0x00086118
		private _ISignature \u0001(_ICallExpression \u0002)
		{
			_ISignature isignature = null;
			if (\u0002._Callee.GetVariable(this._Scope) == null)
			{
				isignature = (\u0002._Callee.GetSignature(this._Scope) as _ISignature);
			}
			if (isignature == null)
			{
				_IUserdefType iuserdefType = \u0002._Callee.Type.DeRefType as _IUserdefType;
				if (iuserdefType != null)
				{
					isignature = (iuserdefType.GetSignature(this._Scope) as _ISignature);
				}
			}
			return isignature;
		}

		// Token: 0x0400072B RID: 1835
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
