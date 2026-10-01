using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000024 RID: 36
	internal class Flowposition : IFlowVarRef, IVarRef2, IVarRef
	{
		// Token: 0x0600017B RID: 379 RVA: 0x00002476 File Offset: 0x00001476
		internal Flowposition()
		{
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600017C RID: 380 RVA: 0x00004EAF File Offset: 0x00003EAF
		// (set) Token: 0x0600017D RID: 381 RVA: 0x00004EB7 File Offset: 0x00003EB7
		public IBreakpoint Breakpoint
		{
			get
			{
				return this._bp;
			}
			set
			{
				this._bp = value;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x0600017E RID: 382 RVA: 0x00004EC0 File Offset: 0x00003EC0
		// (set) Token: 0x0600017F RID: 383 RVA: 0x00004EC8 File Offset: 0x00003EC8
		public IBreakpoint LastListedBreakpoint
		{
			get
			{
				return this._bpLastListed;
			}
			set
			{
				this._bpLastListed = value;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000180 RID: 384 RVA: 0x00004ED1 File Offset: 0x00003ED1
		public IBreakpoint[] BreakpointsReached
		{
			get
			{
				return new IBreakpoint[]
				{
					this.Breakpoint
				};
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000181 RID: 385 RVA: 0x00004EE4 File Offset: 0x00003EE4
		public IBreakpoint[] BreakpointsValue
		{
			get
			{
				if (this.ReadAccess)
				{
					return new IBreakpoint[]
					{
						this.Breakpoint
					};
				}
				IBreakpoint breakpoint = this._bp;
				if (this._cpou == null || this._bp == null || this._cpou.BreakpointList == null)
				{
					return null;
				}
				if (this._bp.Successors == null || this._bp.Successors.Length == 0)
				{
					if (this._bpLastListed == null || this._bpLastListed.Successors == null || this._bpLastListed.Successors.Length == 0)
					{
						return null;
					}
					breakpoint = this._bpLastListed;
				}
				List<IBreakpoint> list = new List<IBreakpoint>();
				foreach (int num in breakpoint.Successors)
				{
					if (num >= this._cpou.BreakpointList.Count)
					{
						return null;
					}
					list.Add(this._cpou.BreakpointList[num]);
				}
				return list.ToArray();
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000182 RID: 386 RVA: 0x00004FCA File Offset: 0x00003FCA
		// (set) Token: 0x06000183 RID: 387 RVA: 0x00004FF5 File Offset: 0x00003FF5
		public bool Reached
		{
			get
			{
				if (this.AddressInfo is ComplexAddressInfo)
				{
					return (this.AddressInfo as ComplexAddressInfo).VarRefProxy.Reached;
				}
				return this._bReached;
			}
			set
			{
				this._bReached = value;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000184 RID: 388 RVA: 0x00004FFE File Offset: 0x00003FFE
		// (set) Token: 0x06000185 RID: 389 RVA: 0x00005006 File Offset: 0x00004006
		public bool ReadAccess
		{
			get
			{
				return this._bReadAccess;
			}
			set
			{
				this._bReadAccess = value;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000186 RID: 390 RVA: 0x0000500F File Offset: 0x0000400F
		// (set) Token: 0x06000187 RID: 391 RVA: 0x00005017 File Offset: 0x00004017
		public ISourcePosition Position
		{
			get
			{
				return this._sp;
			}
			set
			{
				this._sp = value;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000188 RID: 392 RVA: 0x00005020 File Offset: 0x00004020
		// (set) Token: 0x06000189 RID: 393 RVA: 0x00005028 File Offset: 0x00004028
		public IVarRef2 VarReference
		{
			get
			{
				return this._varref;
			}
			set
			{
				this._varref = value;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600018A RID: 394 RVA: 0x00005031 File Offset: 0x00004031
		// (set) Token: 0x0600018B RID: 395 RVA: 0x00005039 File Offset: 0x00004039
		public string InstancePath
		{
			get
			{
				return this._stInstancePath;
			}
			set
			{
				this._stInstancePath = value;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600018C RID: 396 RVA: 0x00005042 File Offset: 0x00004042
		// (set) Token: 0x0600018D RID: 397 RVA: 0x0000504A File Offset: 0x0000404A
		public ICompiledPOU CompiledPOU
		{
			get
			{
				return this._cpou;
			}
			set
			{
				this._cpou = value;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600018E RID: 398 RVA: 0x00005053 File Offset: 0x00004053
		public int SignatureId
		{
			get
			{
				return this._varref.SignatureId;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600018F RID: 399 RVA: 0x00005060 File Offset: 0x00004060
		public Guid ApplicationGuid
		{
			get
			{
				return this._varref.ApplicationGuid;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000190 RID: 400 RVA: 0x0000506D File Offset: 0x0000406D
		public IExpression WatchExpression
		{
			get
			{
				return this._varref.WatchExpression;
			}
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000507A File Offset: 0x0000407A
		public bool GetFlag(VarRefFlag vrflag)
		{
			return this._varref.GetFlag(vrflag);
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000192 RID: 402 RVA: 0x00005088 File Offset: 0x00004088
		public IAddressInfo AddressInfo
		{
			get
			{
				return this._varref.AddressInfo;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000193 RID: 403 RVA: 0x00005095 File Offset: 0x00004095
		public object ConstantValue
		{
			get
			{
				return this._varref.ConstantValue;
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x000050A4 File Offset: 0x000040A4
		public override int GetHashCode()
		{
			if (this.ConstantValue != null)
			{
				return this.ConstantValue.GetHashCode() ^ base.GetType().GetHashCode();
			}
			int num = this.ApplicationGuid.GetHashCode() ^ this.SignatureId.GetHashCode();
			if (this.AddressInfo != null)
			{
				num ^= this.AddressInfo.GetHashCode();
			}
			if (this.WatchExpression != null && this.WatchExpression.Type != null)
			{
				num ^= this.WatchExpression.Type.ToString().GetHashCode();
			}
			if (this.Breakpoint != null)
			{
				num ^= this.Breakpoint.Offset.GetHashCode();
			}
			if (this.Position != null)
			{
				num ^= this.Position.Position.GetHashCode();
				num ^= this.Position.PositionOffset.GetHashCode();
			}
			if (this.InstancePath != null)
			{
				num ^= this.InstancePath.GetHashCode();
			}
			if (this.CompiledPOU != null && this.CompiledPOU.CompiledCode != null && this.CompiledPOU.CompiledCode.Location != null)
			{
				num ^= this.CompiledPOU.CompiledCode.Location.Area.GetHashCode();
				num ^= this.CompiledPOU.CompiledCode.Location.Offset.GetHashCode();
			}
			return num ^ base.GetType().GetHashCode();
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00005218 File Offset: 0x00004218
		public override bool Equals(object obj)
		{
			if (obj != null && base.GetType() != obj.GetType())
			{
				return false;
			}
			Flowposition flowposition = obj as Flowposition;
			if (flowposition == null)
			{
				return false;
			}
			if (this.ConstantValue != null)
			{
				return object.Equals(this.ConstantValue, flowposition.ConstantValue);
			}
			return flowposition.ConstantValue == null && flowposition.AddressInfo != null && flowposition.AddressInfo.Equals(this.AddressInfo) && flowposition.WatchExpression != null && flowposition.WatchExpression.Type != null && !(flowposition.ApplicationGuid != this.ApplicationGuid) && flowposition.Breakpoint.Offset == this.Breakpoint.Offset && !(flowposition.InstancePath != this.InstancePath) && flowposition.Position.PositionCombination == this.Position.PositionCombination && (this.CompiledPOU == null || this.CompiledPOU.CompiledCode == null || this.CompiledPOU.CompiledCode.Location == null || flowposition.CompiledPOU == null || flowposition.CompiledPOU.CompiledCode == null || flowposition.CompiledPOU.CompiledCode.Location == null || flowposition.CompiledPOU.CompiledCode.Location.Area == this.CompiledPOU.CompiledCode.Location.Area) && flowposition.WatchExpression.Type.IsEqual(this.WatchExpression.Type) && this.SignatureId == flowposition.SignatureId;
		}

		// Token: 0x0400002A RID: 42
		private IBreakpoint _bp;

		// Token: 0x0400002B RID: 43
		private IBreakpoint _bpLastListed;

		// Token: 0x0400002C RID: 44
		private ISourcePosition _sp;

		// Token: 0x0400002D RID: 45
		private IVarRef2 _varref;

		// Token: 0x0400002E RID: 46
		private string _stInstancePath;

		// Token: 0x0400002F RID: 47
		private ICompiledPOU _cpou;

		// Token: 0x04000030 RID: 48
		private bool _bReached;

		// Token: 0x04000031 RID: 49
		private bool _bReadAccess;
	}
}
