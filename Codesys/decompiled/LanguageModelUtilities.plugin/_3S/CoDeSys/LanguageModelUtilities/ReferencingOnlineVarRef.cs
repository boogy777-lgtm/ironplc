using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public abstract class ReferencingOnlineVarRef : IOnlineVarRef2, IOnlineVarRef, IReferencingOnlineVarRef
	{
		protected IOnlineVarRef _onlineVarRefReference;

		protected ulong _ulAddressItf;

		private IOnlineVarRef _onlineVarRefReferenceDeref;

		protected IVarRef _owningVarRef;

		protected IInterfaceMonitoringHelper _interfaceMonitoringHelper;

		protected Guid _gdApplication;

		protected bool _bMonitoringSuspended;

		private object _Monat;

		public DateTime Timestamp
		{
			get
			{
				if (_onlineVarRefReference == null)
				{
					return DateTime.Now;
				}
				return _onlineVarRefReference.Timestamp;
			}
		}

		public VarRefState State
		{
			get
			{
				if (_onlineVarRefReference == null)
				{
					return VarRefState.Bad;
				}
				return _onlineVarRefReference.State;
			}
		}

		public bool Forced
		{
			get
			{
				if (_onlineVarRefReference == null)
				{
					return false;
				}
				return _onlineVarRefReference.Forced;
			}
		}

		public object PreparedValue
		{
			get
			{
				if (_onlineVarRefReference == null)
				{
					return null;
				}
				return _onlineVarRefReference.PreparedValue;
			}
			set
			{
				if (_onlineVarRefReference != null)
				{
					_onlineVarRefReference.PreparedValue = value;
				}
			}
		}

		public object Value => _onlineVarRefReference?.Value;

		public byte[] RawValue => _onlineVarRefReference?.RawValue;

		public byte[] PreparedRawValue => _onlineVarRefReference?.PreparedRawValue;

		public IExpression Expression => _onlineVarRefReference?.Expression;

		protected virtual bool CanVFTableProvideOffset => true;

		protected virtual ulong OffsetWithinInstance => 0uL;

		public object Tag
		{
			get
			{
				return _Monat;
			}
			set
			{
				_Monat = value;
			}
		}

		public event OnlineVarRefEventHandler ValueChanged;

		public event OnlineVarRefEventHandler ReferencedInstanceAvailable;

		public event OnlineVarRefEventHandler Changed;

		public virtual void Initialize(IInterfaceMonitoringHelper interfaceMonitoringHelper, IVarRef owningVarRef, Guid gdApplication)
		{
			_owningVarRef = owningVarRef;
			_interfaceMonitoringHelper = interfaceMonitoringHelper;
			_gdApplication = gdApplication;
			CreateReferenceOnlineVarRef();
			CreateReferenceDerefOnlineVarRef();
		}

		public bool DetermineInstanceSignatureIfPossible(bool bCheckAddressChange, out bool bAddressChanged, out ulong ulAddressInstance, out ISignature signInstance)
		{
			signInstance = null;
			ulAddressInstance = 0uL;
			bAddressChanged = false;
			bool flag = false;
			if (_onlineVarRefReference != null && _onlineVarRefReference.State == VarRefState.Good && _onlineVarRefReferenceDeref != null && _onlineVarRefReferenceDeref.State == VarRefState.Good && CanVFTableProvideOffset)
			{
				if (bCheckAddressChange)
				{
					ulong num = Convert.ToUInt64(_onlineVarRefReference.Value);
					bAddressChanged = num != _ulAddressItf;
					_ulAddressItf = num;
				}
				flag = true;
				uint uiArea = uint.MaxValue;
				uint uiOffset = uint.MaxValue;
				if (flag)
				{
					ulong ulAddress = Convert.ToUInt64(_onlineVarRefReferenceDeref.Value);
					flag = _interfaceMonitoringHelper.GetAreaOffsetForAbsoluteAddess(_gdApplication, ulAddress, out uiArea, out uiOffset);
				}
				ILMCompiledApplicationSet iLMCompiledApplicationSet = null;
				ISignature signature = null;
				if (flag)
				{
					iLMCompiledApplicationSet = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetCompiledApplicationSet(_gdApplication);
					signature = iLMCompiledApplicationSet?.GetSignature("CODE__INIT__VFTABLES");
					flag = signature != null;
				}
				if (flag && (Expression?.Type as IPointerType)?.Base is IUserdefType userdefType)
				{
					ISignature signatureById = iLMCompiledApplicationSet.GetSignatureById(userdefType.SignatureId);
					if (signatureById == null)
					{
						flag = false;
					}
					else if (Operator.Type == signatureById.POUType)
					{
						flag = false;
					}
				}
				IScope scope = null;
				if (flag)
				{
					scope = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetTypificator(_gdApplication).CreateScope(signature.Id);
					flag = scope != null;
				}
				IVariable variable = null;
				if (flag)
				{
					IVariable[] all = signature.All;
					foreach (IVariable variable2 in all)
					{
						if (uiArea == variable2.DataLocation.Area && uiOffset >= variable2.DataLocation.Offset && uiOffset < variable2.DataLocation.Offset + variable2.CompiledType.Size(scope))
						{
							variable = variable2;
							break;
						}
					}
					flag = variable != null;
				}
				int result = -1;
				if (flag)
				{
					flag = false;
					if (variable.OrgName.StartsWith("VFTABLE__"))
					{
						flag = int.TryParse(variable.OrgName.Substring("VFTABLE__".Length), out result);
					}
				}
				if (flag)
				{
					signInstance = iLMCompiledApplicationSet.GetSignatureById(result);
					flag = signInstance != null;
				}
				if (flag)
				{
					ulAddressInstance = Convert.ToUInt64(_onlineVarRefReference.Value);
					ulong offsetWithinInstance = OffsetWithinInstance;
					ulAddressInstance -= offsetWithinInstance;
				}
			}
			return flag;
		}

		public virtual void Release()
		{
			_onlineVarRefReference?.Release();
			_onlineVarRefReferenceDeref?.Release();
		}

		public virtual void ResumeMonitoring()
		{
			try
			{
				_onlineVarRefReference?.ResumeMonitoring();
				_onlineVarRefReferenceDeref?.ResumeMonitoring();
			}
			finally
			{
				_bMonitoringSuspended = false;
			}
		}

		public virtual void SuspendMonitoring()
		{
			try
			{
				_onlineVarRefReference?.SuspendMonitoring();
				_onlineVarRefReferenceDeref?.SuspendMonitoring();
			}
			finally
			{
				_bMonitoringSuspended = true;
			}
		}

		public string GetStateMessage()
		{
			return _onlineVarRefReference?.GetStateMessage();
		}

		protected void RaiseValueChanged()
		{
			if (this.ValueChanged != null)
			{
				this.ValueChanged(this);
			}
		}

		private void CreateReferenceOnlineVarRef()
		{
			_onlineVarRefReference = APEnvironmentFacade.Instance.CreateWatch(_owningVarRef);
			_onlineVarRefReference.Changed += OnOnlineVarRefReferenceChanged;
			if (_onlineVarRefReference.State == VarRefState.Good)
			{
				OnOnlineVarRefReferenceChanged(_onlineVarRefReference);
			}
		}

		private void CreateReferenceDerefOnlineVarRef()
		{
			int pointerSize = InterfaceMonitoringHelper.GetPointerSize(_gdApplication);
			string stType = ((8 == pointerSize) ? "POINTER TO LWORD" : "POINTER TO DWORD");
			string stType2 = ((8 == pointerSize) ? "LWORD" : "DWORD");
			ICompiledType pointerType = InterfaceMonitoringHelper.CreateType(stType);
			ICompiledType baseType = InterfaceMonitoringHelper.CreateType(stType2);
			DeRefAccessVarRef varRef = new DeRefAccessVarRef(_gdApplication, _owningVarRef.AddressInfo, _owningVarRef?.WatchExpression?.ToString(), pointerSize, pointerType, baseType);
			_onlineVarRefReferenceDeref = APEnvironmentFacade.Instance.CreateWatch(varRef);
			_onlineVarRefReferenceDeref.Changed += OnOnlineVarRefReferenceDerefChanged;
			if (!_bMonitoringSuspended)
			{
				_onlineVarRefReferenceDeref.ResumeMonitoring();
			}
		}

		protected void DetermineInstanceSignatureIfPossible()
		{
			ulong ulAddressInstance = 0uL;
			ISignature signInstance = null;
			bool bAddressChanged = false;
			if (DetermineInstanceSignatureIfPossible(bCheckAddressChange: false, out bAddressChanged, out ulAddressInstance, out signInstance) && this.ReferencedInstanceAvailable != null)
			{
				this.ReferencedInstanceAvailable(this);
			}
		}

		protected virtual void OnOnlineVarRefReferenceChanged(IOnlineVarRef onlineVarRef)
		{
			if (this.Changed != null)
			{
				this.Changed(onlineVarRef);
			}
			if (onlineVarRef == _onlineVarRefReference && onlineVarRef.State == VarRefState.Good)
			{
				if (_ulAddressItf == 0L)
				{
					_ulAddressItf = Convert.ToUInt64(_onlineVarRefReference.Value);
				}
				DetermineInstanceSignatureIfPossible();
				RaiseValueChanged();
				OnChanged(onlineVarRef);
			}
		}

		private void OnOnlineVarRefReferenceDerefChanged(IOnlineVarRef onlineVarRef)
		{
			if (onlineVarRef == _onlineVarRefReferenceDeref && onlineVarRef.State == VarRefState.Good)
			{
				DetermineInstanceSignatureIfPossible();
				RaiseValueChanged();
				OnChanged(onlineVarRef);
			}
		}

		protected void OnChanged(IOnlineVarRef varRef)
		{
			if (this.Changed != null)
			{
				this.Changed(varRef);
			}
		}
	}
}
