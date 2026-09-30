using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{77EAFEC6-6B42-4CDC-9054-3BCBF954627C}")]
	public sealed class TemporaryWatchVarOnlineVarRef : ITemporaryWatchVarOnlineVarRef, IOnlineVarRef
	{
		private Guid _gdApplication;

		private ulong _ulAddressInstance;

		private IOnlineVarRef _ovrPointer;

		private bool _bValueWritten;

		public DateTime Timestamp
		{
			get
			{
				if (_ovrPointer == null)
				{
					return DateTime.Now;
				}
				return _ovrPointer.Timestamp;
			}
		}

		public VarRefState State
		{
			get
			{
				if (_ovrPointer == null)
				{
					return VarRefState.Bad;
				}
				return _ovrPointer.State;
			}
		}

		public bool Forced
		{
			get
			{
				if (_ovrPointer == null)
				{
					return false;
				}
				return _ovrPointer.Forced;
			}
		}

		public object PreparedValue
		{
			get
			{
				if (_ovrPointer == null)
				{
					return null;
				}
				return _ovrPointer.PreparedValue;
			}
			set
			{
				if (_ovrPointer != null)
				{
					_ovrPointer.PreparedValue = value;
				}
			}
		}

		public object Value => _ovrPointer?.Value;

		public byte[] RawValue => _ovrPointer?.RawValue;

		public byte[] PreparedRawValue => _ovrPointer?.PreparedRawValue;

		public IExpression Expression => _ovrPointer?.Expression;

		public event OnlineVarRefEventHandler Changed;

		public void Initialize(Guid gdApplication, IReferencedInstanceWatchVarDescription referencedInstanceWatchVarDescription)
		{
			_ulAddressInstance = referencedInstanceWatchVarDescription.AddressInstance;
			_gdApplication = gdApplication;
			IVarRef varReference = APEnvironmentFacade.Instance.LMServiceProvider.MonitoringService.GetVarReference(gdApplication, referencedInstanceWatchVarDescription.PointerTempVar);
			_ovrPointer = APEnvironmentFacade.Instance.CreateWatch(varReference);
			_ovrPointer.Changed += OnOnlineVarRefPointerChanged;
			_ovrPointer.ResumeMonitoring();
		}

		public void WriteAddress(ulong ulAddress)
		{
			_ovrPointer.PreparedValue = ulAddress;
			APEnvironmentFacade.Instance.WriteVariable(_gdApplication, _ovrPointer);
		}

		public void Release()
		{
			_ovrPointer?.Release();
		}

		public void ResumeMonitoring()
		{
			_ovrPointer?.ResumeMonitoring();
		}

		public void SuspendMonitoring()
		{
			_ovrPointer?.SuspendMonitoring();
		}

		public string GetStateMessage()
		{
			return _ovrPointer?.GetStateMessage();
		}

		private void OnOnlineVarRefPointerChanged(IOnlineVarRef onlineVarRef)
		{
			if (onlineVarRef == _ovrPointer)
			{
				if (_ovrPointer.State == VarRefState.Good && !_bValueWritten)
				{
					_bValueWritten = true;
					WriteAddress(_ulAddressInstance);
				}
				if (this.Changed != null)
				{
					this.Changed(onlineVarRef);
				}
			}
		}
	}
}
