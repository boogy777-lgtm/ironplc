using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000021 RID: 33
	internal class AllFlowPosVisitor : EmptyVisitor, IFlowPosVisitor351300, IFlowPosVisitor, IExprementVisitorNoTraversion, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590, IFlowPosVisitor3590
	{
		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000128 RID: 296 RVA: 0x0000344C File Offset: 0x0000244C
		// (set) Token: 0x06000129 RID: 297 RVA: 0x00003454 File Offset: 0x00002454
		private PropertyFlowpositionCreator PropertyFlowpositionCreator { get; set; }

		// Token: 0x0600012A RID: 298 RVA: 0x0000345D File Offset: 0x0000245D
		protected AllFlowPosVisitor()
		{
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00003468 File Offset: 0x00002468
		internal static LList<Flowposition> FindExpressionBySourceposition(long[] sourceposIn, _IStatement state, _IBreakpointList bplist, int nSignatureId, Guid guidApplication, CompiledPOU cpou, string stInstancePath, IVarRef varrefInstance, IScope scope, CompileContext comcon)
		{
			if (state == null)
			{
				return null;
			}
			state = (state.Duplicate() as _IStatement);
			CompilerProxy.TypifyExprement(state, scope, comcon, null, true, false, cpou);
			AllFlowPosVisitor allFlowPosVisitor = new AllFlowPosVisitor();
			allFlowPosVisitor._sourcepos = sourceposIn;
			allFlowPosVisitor._nSignatureId = nSignatureId;
			allFlowPosVisitor._guidApplication = guidApplication;
			allFlowPosVisitor._cpou = cpou;
			allFlowPosVisitor._stInstancePath = stInstancePath;
			allFlowPosVisitor._varRefInstance = (varrefInstance as VarRef);
			allFlowPosVisitor._flowFound = new LList<Flowposition>();
			allFlowPosVisitor._scope = (scope as IScope5);
			allFlowPosVisitor.PropertyFlowpositionCreator = new PropertyFlowpositionCreator(guidApplication, cpou);
			IExprementVisitor ivisit = CompilerProxy.CreateFlowTraverser(allFlowPosVisitor, bplist);
			state.Accept(ivisit);
			IBreakpoint breakpoint = null;
			foreach (object obj in bplist)
			{
				IBreakpoint breakpoint2 = (IBreakpoint)obj;
				if (breakpoint2.Position.Position == cpou.ImplicitReturnPositionPos)
				{
					breakpoint = breakpoint2;
					break;
				}
			}
			for (int i = allFlowPosVisitor._flowFound.Count - 1; i >= 0; i--)
			{
				if (allFlowPosVisitor._flowFound[i].Breakpoint == null)
				{
					if (breakpoint != null)
					{
						allFlowPosVisitor._flowFound[i].Breakpoint = breakpoint;
					}
					else
					{
						allFlowPosVisitor._flowFound.RemoveAt(i);
					}
				}
			}
			return allFlowPosVisitor._flowFound;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000035C0 File Offset: 0x000025C0
		private Flowposition TestPosition(_IExpression exp, bool bReadAccess)
		{
			return this.TestPosition(exp, bReadAccess, true);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x000035CC File Offset: 0x000025CC
		private bool TestPosition(_IExpression exp)
		{
			if (exp == null)
			{
				return false;
			}
			if (exp.Position == null)
			{
				return false;
			}
			if (this._sourcepos == null)
			{
				return true;
			}
			foreach (long num in this._sourcepos)
			{
				if (exp.Position.Position == num)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000361C File Offset: 0x0000261C
		private Flowposition TestPosition(_IExpression exp, bool bReadAccess, bool bGenerateVarRef)
		{
			if (exp == null)
			{
				return null;
			}
			if (exp.Position == null)
			{
				return null;
			}
			bool flag = false;
			if (this._sourcepos == null)
			{
				flag = true;
			}
			else
			{
				foreach (long num in this._sourcepos)
				{
					if (exp.Position.Position == num)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return this.InsertFlowposition(exp, bReadAccess, bGenerateVarRef);
			}
			return null;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00003680 File Offset: 0x00002680
		private Flowposition InsertFlowposition(_IExpression exp, bool bReadAccess, bool bGenerateVarRef)
		{
			Flowposition flowposition = new Flowposition();
			flowposition.Position = exp.Position;
			if (bGenerateVarRef)
			{
				flowposition.VarReference = (VarReferenceCreator.GetVarReference(exp, this._nSignatureId, this._guidApplication, this._varRefInstance) as IVarRef2);
				if (flowposition.VarReference == null)
				{
					return null;
				}
				if (flowposition.WatchExpression.Type != null && flowposition.WatchExpression.Type.Class == TypeClass.Reference)
				{
					(flowposition.WatchExpression as _IExpression)._CompiledType = flowposition.WatchExpression.Type.DeRefType;
				}
				if (flowposition.VarReference.GetFlag(VarRefFlag.Extensible) && flowposition.WatchExpression.Type.Class != TypeClass.Pointer)
				{
					return null;
				}
			}
			flowposition.CompiledPOU = this._cpou;
			flowposition.InstancePath = this._stInstancePath;
			flowposition.Breakpoint = this._bpCurrent;
			flowposition.LastListedBreakpoint = this._bpLastListed;
			flowposition.ReadAccess = bReadAccess;
			if (!this.PropertyFlowpositionCreator.ChangeFlowpositionForProperty(this._scope, this._bpCurrent, flowposition, exp, bReadAccess))
			{
				return null;
			}
			this._flowFound.Add(flowposition);
			return flowposition;
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0000379C File Offset: 0x0000279C
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(variable, bReadAccess);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x000037BC File Offset: 0x000027BC
		public override void visit(_ICallExpression call)
		{
			ISignature[] array = this._scope.FindSignature(call.Callee);
			ISignature signature = null;
			if (array != null && array.Length == 1)
			{
				signature = array[0];
			}
			if (signature == null && call.Callee.Type is UserdefType)
			{
				signature = (call.Callee.Type as UserdefType).GetSignature(this._scope);
			}
			if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method) && signature.Outputs.Length != 0)
			{
				Flowposition flowposition = this.TestPosition(call, true, false);
				this.CreateCallFlowposition(call, flowposition, signature);
				foreach (_IExpression iexpression in call.EmptyAssigns)
				{
					Flowposition flowposition2 = this.TestPosition(iexpression, true, false);
					if (iexpression is IVariableExpression && flowposition != null && this._bpCurrent != null)
					{
						flowposition2.Breakpoint = this._bpCurrent;
						flowposition2.LastListedBreakpoint = this._bpLastListed;
						IVariable variable = iexpression.GetVariable(this._scope);
						_IExpression iexpression2 = iexpression.Duplicate() as _IExpression;
						iexpression2._CompiledType = iexpression._CompiledType;
						iexpression2._Position = iexpression._Position;
						iexpression2.LengthIntern = iexpression.LengthIntern;
						VarRef varRef = new VarRef(iexpression2, this._guidApplication, this._scope);
						varRef.SetFlag(VarRefFlag.Invalid, false);
						varRef.AddressInfo = new StackRelativeAddressInfo(variable.DataLocation.Offset, byte.MaxValue, variable.CompiledType.Size(this._scope), (int)this._cpou.CompiledCode.Location.Area, this._cpou.CompiledCode.Location.Offset, this._cpou.CompiledCode.CodeSize, variable.CompiledType);
						(varRef.AddressInfo as StackRelativeAddressInfo).StackRelative = true;
						flowposition2.VarReference = varRef;
					}
				}
			}
		}

		// Token: 0x06000132 RID: 306 RVA: 0x000039D0 File Offset: 0x000029D0
		private void CreateCallFlowposition(_ICallExpression call, Flowposition flowpos, ISignature signHelp)
		{
			if (flowpos != null && this._bpCurrent != null)
			{
				flowpos.Breakpoint = this._bpCurrent;
				flowpos.LastListedBreakpoint = this._bpLastListed;
				IVariable variable = signHelp.Outputs[0];
				_IExpression iexpression = call.Duplicate() as _IExpression;
				CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
				iexpression._Position = call._Position;
				VarRef varRef = new VarRef(iexpression, this._guidApplication, this._scope);
				varRef.SetFlag(VarRefFlag.Invalid, false);
				varRef.AddressInfo = new StackRelativeAddressInfo(variable.DataLocation.Offset, byte.MaxValue, variable.CompiledType.Size(this._scope), (int)this._cpou.CompiledCode.Location.Area, this._cpou.CompiledCode.Location.Offset, this._cpou.CompiledCode.CodeSize, variable.CompiledType);
				(varRef.AddressInfo as StackRelativeAddressInfo).StackRelative = true;
				flowpos.VarReference = varRef;
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void visit(_IAddressExpression address)
		{
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void visit(_IConversionExpression conv)
		{
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00003AEC File Offset: 0x00002AEC
		public override void visit(_IOperatorExpression op)
		{
			if (CompilerProxy.IsPrefixOperator(op.Code) && op.Code != Operator.Adr && op.Code != Operator.__Reloc && op.Code != Operator.__SystemScope && op.Code != Operator.Ini && op.Code != Operator.BitAdr && op.Code != Operator.TestAndSet && op.Code != Operator.Time && op.Code != Operator.LTime && op.Code != Operator.__AdrInst && op.Code != Operator.__RefAdr && op.Code != Operator.__MemorySet && op.Code != Operator.__MaxOffset && op.Code != Operator.__CRC && op.Code != Operator.__QueryPointer && op.Code != Operator.__FCall && op.Code != Operator.__PropertyInfo && op.Code != Operator.__QueryInterface && op.Code != Operator.__Delete && this.TestPosition(op))
			{
				Flowposition flowposition = new Flowposition();
				flowposition.Position = op.Position;
				flowposition.CompiledPOU = this._cpou;
				flowposition.InstancePath = this._stInstancePath;
				flowposition.Breakpoint = this._bpCurrent;
				flowposition.LastListedBreakpoint = this._bpLastListed;
				flowposition.ReadAccess = true;
				_IOperatorExpression ioperatorExpression = op.Duplicate() as _IOperatorExpression;
				CompilerProxy.TypifyExprement(ioperatorExpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
				VarRef varRef = new VarRef(ioperatorExpression, this._guidApplication, this._scope);
				varRef.AddressInfo = new ComplexAddressInfo();
				IOnlineExpressionInterpreter3 onlineExpressionInterpreter = APEnvironmentFacade.Instance.CreateOnlineExpressionInterpreter();
				OnlineExpressionException ex = null;
				IOnlineVarRefProvider2 onlineVarRefProvider = new FlowVarRefProvider(this._flowFound);
				IOnlineExpression2 flowOnlineExpression = onlineExpressionInterpreter.GetFlowOnlineExpression(this._guidApplication, ioperatorExpression, onlineVarRefProvider, out ex);
				if (ex == null)
				{
					(varRef.AddressInfo as ComplexAddressInfo).SetOnlineExpression(flowOnlineExpression);
					flowposition.VarReference = varRef;
					this._flowFound.Add(flowposition);
				}
			}
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00003D10 File Offset: 0x00002D10
		public void visitCompoAccess(_ICompoAccessExpression compo, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(compo, bReadAccess);
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00003D30 File Offset: 0x00002D30
		public void visitIndexAccess(_IIndexAccessExpression index, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(index, bReadAccess);
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00003D50 File Offset: 0x00002D50
		public void visitDeRefAccess(_IDeRefAccessExpression deref, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(deref, bReadAccess);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00003D6D File Offset: 0x00002D6D
		public IBreakpoint GetCurrentBreakpoint()
		{
			return this._bpCurrent;
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00003D78 File Offset: 0x00002D78
		public virtual void GenerateDummyFlow(IBreakpoint bp)
		{
			if (bp == null || bp.AssemblySuccessors == null)
			{
				return;
			}
			bool flag = true;
			using (IEnumerator<Flowposition> enumerator = this._flowFound.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Breakpoint == bp)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				Flowposition flowposition = new Flowposition();
				flowposition.Position = bp.Position;
				_IVariableExpression ivariableExpression = LanguageModelBuilder.Singleton.CreateVariableExpression("___XXX???");
				new StringType().Length = (LanguageModelBuilder.Singleton.CreateLiteralExpression(null, 1L) as _IExpression);
				ivariableExpression._CompiledType = TypeTable.Byte;
				VarRef varRef = new VarRef(ivariableExpression, this._guidApplication, this._scope);
				flowposition.VarReference = varRef;
				IVariable[] array;
				ISignature[] array2;
				IScope scope;
				if (!this._scope.FindDeclaration("__ApplicationName", out array, out array2, out scope))
				{
					return;
				}
				IDataLocation dataLocation = array2[0].All[0].DataLocation;
				varRef.AddressInfo = new AbsoluteAddressInfo((int)dataLocation.Area, dataLocation.Offset, byte.MaxValue, TypeTable.ByteSize, TypeTable.Byte);
				varRef.SetFlag(VarRefFlag.Invalid, false);
				varRef.SetFlag(VarRefFlag.ReachedOnly, true);
				flowposition.CompiledPOU = this._cpou;
				flowposition.InstancePath = this._stInstancePath;
				flowposition.Breakpoint = bp;
				flowposition.LastListedBreakpoint = this._bpLastListed;
				flowposition.ReadAccess = true;
				this._flowFound.Add(flowposition);
			}
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00003EE4 File Offset: 0x00002EE4
		public virtual void SetCurrentBreakpoint(IBreakpoint bp)
		{
			if (bp != null)
			{
				this._bpCurrent = bp;
			}
			foreach (Flowposition flowposition in this._flowFound)
			{
				if (flowposition.Breakpoint == null)
				{
					flowposition.Breakpoint = bp;
				}
			}
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPoolScopeExpression poolscope)
		{
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICurrentTaskExpression currentTaskExp)
		{
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600013E RID: 318 RVA: 0x00003F44 File Offset: 0x00002F44
		// (set) Token: 0x0600013F RID: 319 RVA: 0x00003F4C File Offset: 0x00002F4C
		public virtual IBreakpoint LastListedBreakpoint
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

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000140 RID: 320 RVA: 0x00003F55 File Offset: 0x00002F55
		public IScope IScope
		{
			get
			{
				return this._scope;
			}
		}

		// Token: 0x04000011 RID: 17
		private long[] _sourcepos;

		// Token: 0x04000012 RID: 18
		internal LList<Flowposition> _flowFound;

		// Token: 0x04000013 RID: 19
		internal IBreakpoint _bpCurrent;

		// Token: 0x04000014 RID: 20
		internal IBreakpoint _bpLastListed;

		// Token: 0x04000015 RID: 21
		internal IBreakpoint _bpFound;

		// Token: 0x04000017 RID: 23
		private int _nSignatureId;

		// Token: 0x04000018 RID: 24
		private Guid _guidApplication;

		// Token: 0x04000019 RID: 25
		private CompiledPOU _cpou;

		// Token: 0x0400001A RID: 26
		private string _stInstancePath;

		// Token: 0x0400001B RID: 27
		private VarRef _varRefInstance;

		// Token: 0x0400001C RID: 28
		private IScope5 _scope;
	}
}
