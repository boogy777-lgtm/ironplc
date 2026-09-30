using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{8789160F-A30B-47C0-B017-3F5F9FDBB937}")]
	public sealed class InterfaceOnlineVarRef : ReferencingOnlineVarRef, IInterfaceOnlineVarRef, IReferencingOnlineVarRef, IOnlineVarRef
	{
		private IOnlineVarRef _onlineVarRefItfDerefDeref;

		private string _stStaticInstancePath = string.Empty;

		private EInterfaceOnlineVarRefState _eState;

		public IInterfaceMonitoringHelper InterfaceMonitoringHelper => _interfaceMonitoringHelper;

		public EInterfaceOnlineVarRefState InterfaceOnlineVarRefState => _eState;

		public string StaticInstancePath => _stStaticInstancePath;

		protected override bool CanVFTableProvideOffset
		{
			get
			{
				if (_onlineVarRefItfDerefDeref != null)
				{
					return _onlineVarRefItfDerefDeref.State == VarRefState.Good;
				}
				return false;
			}
		}

		protected override ulong OffsetWithinInstance
		{
			get
			{
				if (CanVFTableProvideOffset)
				{
					return Convert.ToUInt64(_onlineVarRefItfDerefDeref.Value);
				}
				return base.OffsetWithinInstance;
			}
		}

		public override void Initialize(IInterfaceMonitoringHelper interfaceMonitoringHelper, IVarRef owningVarRef, Guid gdApplication)
		{
			base.Initialize(interfaceMonitoringHelper, owningVarRef, gdApplication);
			CreateItfDerefDerefOnlineVarRef();
		}

		public override void Release()
		{
			base.Release();
			_onlineVarRefItfDerefDeref?.Release();
		}

		public override void ResumeMonitoring()
		{
			base.ResumeMonitoring();
			_onlineVarRefItfDerefDeref?.ResumeMonitoring();
		}

		public override void SuspendMonitoring()
		{
			base.SuspendMonitoring();
			_onlineVarRefItfDerefDeref?.SuspendMonitoring();
		}

		protected override void OnOnlineVarRefReferenceChanged(IOnlineVarRef onlineVarRef)
		{
			base.OnOnlineVarRefReferenceChanged(onlineVarRef);
			if (onlineVarRef == _onlineVarRefReference && onlineVarRef.State == VarRefState.Good)
			{
				_stStaticInstancePath = _interfaceMonitoringHelper.GetInterfaceInstancePath(_gdApplication, onlineVarRef.Value, out var bHidden);
				if (bHidden)
				{
					_eState = EInterfaceOnlineVarRefState.HiddenInstance;
				}
				else if (string.IsNullOrEmpty(_stStaticInstancePath))
				{
					_eState = EInterfaceOnlineVarRefState.DynamicInstance;
				}
				else
				{
					_eState = EInterfaceOnlineVarRefState.StaticInstancePathAvailable;
				}
			}
		}

		private void CreateItfDerefDerefOnlineVarRef()
		{
			int pointerSize = _3S.CoDeSys.LanguageModelUtilities.InterfaceMonitoringHelper.GetPointerSize(_gdApplication);
			string stType = ((8 == pointerSize) ? "POINTER TO POINTER TO LWORD" : "POINTER TO POINTER TO DWORD");
			string stType2 = ((8 == pointerSize) ? "POINTER TO LWORD" : "POINTER TO DWORD");
			string stType3 = ((8 == pointerSize) ? "LWORD" : "DWORD");
			ICompiledType pointerType = _3S.CoDeSys.LanguageModelUtilities.InterfaceMonitoringHelper.CreateType(stType);
			ICompiledType compiledType = _3S.CoDeSys.LanguageModelUtilities.InterfaceMonitoringHelper.CreateType(stType2);
			ICompiledType baseType = _3S.CoDeSys.LanguageModelUtilities.InterfaceMonitoringHelper.CreateType(stType3);
			DeRefAccessVarRef deRefAccessVarRef = new DeRefAccessVarRef(_gdApplication, _owningVarRef.AddressInfo, _owningVarRef?.WatchExpression?.ToString(), pointerSize, pointerType, compiledType);
			DeRefAccessVarRef varRef = new DeRefAccessVarRef(_gdApplication, deRefAccessVarRef.AddressInfo, _owningVarRef?.WatchExpression?.ToString() + "^", pointerSize, compiledType, baseType);
			_onlineVarRefItfDerefDeref = APEnvironmentFacade.Instance.CreateWatch(varRef);
			_onlineVarRefItfDerefDeref.Changed += OnOnlineVarRefItfDerefDerefChanged;
			if (!_bMonitoringSuspended)
			{
				_onlineVarRefItfDerefDeref.ResumeMonitoring();
			}
		}

		private void OnOnlineVarRefItfDerefDerefChanged(IOnlineVarRef onlineVarRef)
		{
			if (onlineVarRef == _onlineVarRefItfDerefDeref && onlineVarRef.State == VarRefState.Good)
			{
				DetermineInstanceSignatureIfPossible();
				RaiseValueChanged();
				OnChanged(onlineVarRef);
			}
		}
	}
}
