using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000104 RID: 260
	internal class TaskLocalVariableOnlineExpression : IOnlineExpression
	{
		// Token: 0x0600132B RID: 4907 RVA: 0x00034F08 File Offset: 0x00033F08
		internal TaskLocalVariableOnlineExpression(IVarRef varref, Guid guidApplication)
		{
			IEnumerable<ITaskInfo> taskSet = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetCompiledApplicationSet(guidApplication).TaskSet;
			this._guidApplication = guidApplication;
			this._vfOrg = varref;
			this._taskvarref.Add(this._vfOrg);
			int num = 0;
			foreach (ITaskInfo taskInfo in taskSet)
			{
				IVarRef taskSpecificVarRef = this.GetTaskSpecificVarRef(num++);
				this._taskvarref.Add(taskSpecificVarRef);
			}
			this.RefillOnlineTaskVarList();
		}

		// Token: 0x0600132C RID: 4908 RVA: 0x00034FC8 File Offset: 0x00033FC8
		internal void RefillOnlineTaskVarList()
		{
			this._taskvarlist.Clear();
			foreach (IVarRef varRef in this._taskvarref)
			{
				IOnlineVarRef5 onlineVarRef = APEnvironmentFacade.Instance.CreateWatch(varRef) as IOnlineVarRef5;
				this._taskvarlist.Add(onlineVarRef);
				onlineVarRef.Changed += this.VarRefChanged;
			}
		}

		// Token: 0x0600132D RID: 4909 RVA: 0x00035048 File Offset: 0x00034048
		internal void ReleaseOnlineTaskVarList()
		{
			foreach (IOnlineVarRef5 onlineVarRef in this._taskvarlist)
			{
				onlineVarRef.Changed -= this.VarRefChanged;
			}
			this._taskvarlist.Clear();
		}

		// Token: 0x0600132E RID: 4910 RVA: 0x000350AC File Offset: 0x000340AC
		private IVarRef GetTaskSpecificVarRef(int iTaskIndex)
		{
			IVarRef varRef = null;
			try
			{
				_IScope scope = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetTypificator(this._guidApplication).CreateGlobalScope() as _IScope;
				_IExpression exp = this._vfOrg.WatchExpression as _IExpression;
				if ((this._vfOrg as VarRef)._OrgExpression != null)
				{
					exp = (this._vfOrg as VarRef)._OrgExpression;
				}
				string text = TaskLocalVariableGenerator.GenerateTaskLocalVariable(scope, exp, iTaskIndex);
				if (text == null)
				{
					varRef = this._vfOrg;
				}
				else if ((this._vfOrg as VarRef).InstancePathExpression != null)
				{
					string stPOUName = (this._vfOrg as VarRef).InstancePathExpression.ToString();
					varRef = APEnvironmentFacade.Instance.LMServiceProvider.MonitoringService.GetVarReference(this._guidApplication, stPOUName, text);
				}
				else
				{
					varRef = APEnvironmentFacade.Instance.LMServiceProvider.MonitoringService.GetVarReference(this._guidApplication, text);
				}
			}
			catch
			{
			}
			if (varRef == null || varRef.AddressInfo == null)
			{
				varRef = this._vfOrg;
			}
			return varRef;
		}

		// Token: 0x0600132F RID: 4911 RVA: 0x000351B4 File Offset: 0x000341B4
		private void VarRefChanged(IOnlineVarRef varRef)
		{
			if (this.Changed != null)
			{
				this.Changed(this);
			}
		}

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x06001330 RID: 4912 RVA: 0x000351CC File Offset: 0x000341CC
		internal int CurrentVariableIndex
		{
			get
			{
				IOnlineApplication onlineApplication = APEnvironmentFacade.Instance.GetOnlineApplication(this._guidApplication);
				if (onlineApplication.ApplicationState == ApplicationState.halt_on_bp)
				{
					IDebuggableOnlineApplication2 debuggableOnlineApplication = onlineApplication as IDebuggableOnlineApplication2;
					if (debuggableOnlineApplication != null && debuggableOnlineApplication.DebugTaskIndex >= 0)
					{
						return debuggableOnlineApplication.DebugTaskIndex + 1;
					}
				}
				return 0;
			}
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x06001331 RID: 4913 RVA: 0x00035210 File Offset: 0x00034210
		public IExpression Expression
		{
			get
			{
				return this._vfOrg.WatchExpression;
			}
		}

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x06001332 RID: 4914 RVA: 0x0003521D File Offset: 0x0003421D
		public ICompiledType ResultingCompiledType
		{
			get
			{
				return this._vfOrg.WatchExpression.Type;
			}
		}

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x06001333 RID: 4915 RVA: 0x0003522F File Offset: 0x0003422F
		public TypeClass ResultingType
		{
			get
			{
				return this._vfOrg.WatchExpression.Type.Class;
			}
		}

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x06001334 RID: 4916 RVA: 0x00035246 File Offset: 0x00034246
		public VarRefState State
		{
			get
			{
				if (this._taskvarlist.Count == 0)
				{
					return VarRefState.MonitoringSuspended;
				}
				return this._taskvarlist[this.CurrentVariableIndex].State;
			}
		}

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x06001335 RID: 4917 RVA: 0x0003526D File Offset: 0x0003426D
		public object Value
		{
			get
			{
				if (this._taskvarlist.Count == 0)
				{
					return 0;
				}
				return this._taskvarlist[this.CurrentVariableIndex].Value;
			}
		}

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06001336 RID: 4918 RVA: 0x0003529C File Offset: 0x0003429C
		// (remove) Token: 0x06001337 RID: 4919 RVA: 0x000352D4 File Offset: 0x000342D4
		public event OnlineExpressionEventHandler Changed;

		// Token: 0x06001338 RID: 4920 RVA: 0x0003530C File Offset: 0x0003430C
		public void Release()
		{
			foreach (IOnlineVarRef5 onlineVarRef in this._taskvarlist)
			{
				onlineVarRef.Release();
			}
			this.ReleaseOnlineTaskVarList();
		}

		// Token: 0x06001339 RID: 4921 RVA: 0x0003535C File Offset: 0x0003435C
		public void ResumeMonitoring()
		{
			if (this._taskvarlist.Count == 0 && this._taskvarref.Count != 0)
			{
				this.RefillOnlineTaskVarList();
			}
			foreach (IOnlineVarRef5 onlineVarRef in this._taskvarlist)
			{
				onlineVarRef.ResumeMonitoring();
			}
		}

		// Token: 0x0600133A RID: 4922 RVA: 0x000353C8 File Offset: 0x000343C8
		public void SuspendMonitoring()
		{
			foreach (IOnlineVarRef5 onlineVarRef in this._taskvarlist)
			{
				onlineVarRef.SuspendMonitoring();
			}
		}

		// Token: 0x04000467 RID: 1127
		private readonly IVarRef _vfOrg;

		// Token: 0x04000468 RID: 1128
		private readonly LList<IOnlineVarRef5> _taskvarlist = new LList<IOnlineVarRef5>();

		// Token: 0x04000469 RID: 1129
		private readonly LList<IVarRef> _taskvarref = new LList<IVarRef>();

		// Token: 0x0400046A RID: 1130
		private Guid _guidApplication = Guid.Empty;
	}
}
