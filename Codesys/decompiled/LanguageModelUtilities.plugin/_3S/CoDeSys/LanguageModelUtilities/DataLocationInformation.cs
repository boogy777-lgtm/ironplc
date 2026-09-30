using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class DataLocationInformation : IDataLocationInformation3, IDataLocationInformation2, IDataLocationInformation
	{
		private ICompiledPOU _cpou;

		private IBreakpoint _bp;

		private string _instancePath;

		private string _editorInstancePath;

		private IVarRef _varRef;

		public ICompiledPOU CompiledPOU
		{
			get
			{
				return _cpou;
			}
			set
			{
				_cpou = value;
			}
		}

		public IBreakpoint Breakpoint
		{
			get
			{
				return _bp;
			}
			set
			{
				_bp = value;
			}
		}

		public string InstancePath
		{
			get
			{
				if (_instancePath == null)
				{
					FindInstancePath();
				}
				return _instancePath;
			}
			internal set
			{
				_instancePath = value;
			}
		}

		public bool InstancePathAvailableImmediate => _instancePath != null;

		public string EditorInstancePath
		{
			get
			{
				if (_editorInstancePath == null)
				{
					CreateEditorInstancePath();
				}
				return _editorInstancePath;
			}
		}

		public ISourcePosition SourcePosition
		{
			get
			{
				if (_bp != null)
				{
					return _cpou.GetSourcePositionOfBreakpoint(_bp);
				}
				return null;
			}
		}

		public IVarRef VarRef => _varRef;

		internal IDataLocation InstanceLocation { get; set; }

		public Guid ApplicationGuid { get; internal set; }

		public DataLocationInformation()
		{
		}

		internal DataLocationInformation(ICompiledPOU cpou)
		{
			_cpou = cpou;
		}

		internal void FindInstancePath()
		{
			ICompileContext referenceContextIfAvailable = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextIfAvailable(ApplicationGuid);
			if (referenceContextIfAvailable == null)
			{
				_instancePath = string.Empty;
				return;
			}
			ISignature signatureById = referenceContextIfAvailable.GetSignatureById(_cpou.SignatureId);
			if (signatureById == null || signatureById.POUType != Operator.Method)
			{
				_instancePath = string.Empty;
				return;
			}
			if (signatureById["__INSTANCEPOINTER"] == null)
			{
				_instancePath = string.Empty;
				return;
			}
			ISignature signatureById2 = referenceContextIfAvailable.GetSignatureById(signatureById.ParentSignatureId);
			if (signatureById2 == null)
			{
				_instancePath = string.Empty;
				return;
			}
			_instancePath = DetermineInstancePathDirect(referenceContextIfAvailable, signatureById2 as _ISignature, signatureById as _ISignature);
			if (_instancePath == null)
			{
				DetermineViaAllInstancePaths(referenceContextIfAvailable, signatureById2, signatureById);
			}
		}

		private void DetermineViaAllInstancePaths(ICompileContext comcon, ISignature signFunctionBlock, ISignature signHelp)
		{
			IVariable[] varInstances;
			ISignature[] declaringSignatures;
			string[] stInstancePaths = ((!(comcon is ICompileContext8 compileContext)) ? comcon.InstancePaths(signFunctionBlock, out varInstances, out declaringSignatures) : compileContext.InstancePaths(signFunctionBlock, out varInstances, out declaringSignatures, bWithNamespace: true, bWithStackVariables: true, bWithDerivedClasses: true));
			DetermineInstancePath(comcon, stInstancePaths, signHelp, signFunctionBlock);
			if (!string.IsNullOrEmpty(_instancePath))
			{
				return;
			}
			_instancePath = signFunctionBlock.OrgName;
			if (signHelp.Name != "__MAIN")
			{
				_instancePath = _instancePath + "." + signHelp.OrgName;
			}
			if (!string.IsNullOrEmpty(signFunctionBlock.LibraryPath))
			{
				string libraryNamespace = (comcon as ICompileContext8).GetLibraryNamespace(signFunctionBlock.LibraryPath);
				if (!string.IsNullOrEmpty(libraryNamespace))
				{
					string text = (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 19, 0) ? "#" : ".");
					_instancePath = libraryNamespace + text + _instancePath;
				}
			}
		}

		private string DetermineInstancePathDirect(ICompileContext comcon, _ISignature signFunctionBlock, _ISignature signHelp)
		{
			FindInstancePath findInstancePath = new FindInstancePath(InstanceLocation.Area, InstanceLocation.Offset, signFunctionBlock, comcon as ILMCompiledApplicationSet);
			findInstancePath.FindNearestSymbol();
			string text = findInstancePath.GetInstancePath();
			if (!findInstancePath._Found)
			{
				return null;
			}
			if (signHelp.Name != "__MAIN")
			{
				text = text + "." + signHelp.OrgName;
			}
			return text;
		}

		private void DetermineInstancePath(ICompileContext comcon, string[] stInstancePaths, ISignature signHelp, ISignature signFunctionBlock)
		{
			Guid applicationGuid = comcon.ApplicationGuid;
			foreach (string text in stInstancePaths)
			{
				IVarRef varReference = APEnvironmentFacade.Instance.LanguageModelMgr.GetVarReference(applicationGuid, text, bAllowShortExpressions: true);
				if (varReference.AddressInfo is IAbsoluteAddressInfo && (varReference.AddressInfo as IAbsoluteAddressInfo).Area == InstanceLocation.Area && (varReference.AddressInfo as IAbsoluteAddressInfo).Offset == InstanceLocation.Offset)
				{
					_varRef = varReference;
					if (signHelp.Name != "__MAIN")
					{
						_instancePath = text + "." + signHelp.OrgName;
					}
					else
					{
						_instancePath = text;
					}
					break;
				}
				if (varReference.AddressInfo is IStackRelativeAddressInfo && InstanceLocation.Area == ushort.MaxValue && InstanceLocation.Offset == -1)
				{
					_varRef = varReference;
					_instancePath = signFunctionBlock.OrgName;
					AddNamespaceToInstancePath(comcon, signFunctionBlock);
					if (signHelp.Name != "__MAIN" && signHelp.POUType == Operator.Method)
					{
						_instancePath = _instancePath + "." + signHelp.OrgName;
					}
				}
			}
		}

		private void AddNamespaceToInstancePath(ICompileContext comcon, ISignature signFunctionBlock)
		{
			if (!string.IsNullOrEmpty(signFunctionBlock.LibraryPath) && comcon is ICompileContext16 comcon2)
			{
				string libraryNamespace = GetLibraryNamespace(comcon2, signFunctionBlock, comcon.ApplicationGuid);
				if (string.IsNullOrEmpty(libraryNamespace))
				{
					libraryNamespace = GetLibraryNamespace(comcon2, signFunctionBlock, Guid.Empty);
				}
				if (!string.IsNullOrEmpty(libraryNamespace))
				{
					_instancePath = libraryNamespace + Common.GetNamespaceDelimiterConsideringCompilerversion3_5_21_10() + _instancePath;
				}
			}
		}

		private static string GetLibraryNamespace(ICompileContext16 comcon16, ISignature signFunctionBlock, Guid gdPrecompileSet)
		{
			string result = null;
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(gdPrecompileSet);
			if (comcon16.LibraryTable is ILibraryTable4 libraryTable)
			{
				result = libraryTable.GetLocalLibraryNamespaceRecursive(precompileContext, signFunctionBlock.LibraryPath);
			}
			return result;
		}

		internal void CreateEditorInstancePath()
		{
			ICompileContext referenceContextIfAvailable = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextIfAvailable(ApplicationGuid);
			if (referenceContextIfAvailable == null)
			{
				_editorInstancePath = string.Empty;
				return;
			}
			ISignature signatureById = referenceContextIfAvailable.GetSignatureById(_cpou.SignatureId);
			if (signatureById == null)
			{
				_editorInstancePath = string.Empty;
				return;
			}
			string applicationName = APEnvironmentFacade.Instance.GetApplicationName(referenceContextIfAvailable.ApplicationGuid);
			if (!string.IsNullOrEmpty(InstancePath))
			{
				_editorInstancePath = applicationName + "." + InstancePath;
				return;
			}
			_editorInstancePath = applicationName;
			if (signatureById.POUType == Operator.Function || signatureById.POUType == Operator.Program)
			{
				_editorInstancePath += ".";
				if (referenceContextIfAvailable is ICompileContext8 compileContext && !string.IsNullOrEmpty(signatureById.LibraryPath))
				{
					_editorInstancePath = _editorInstancePath + compileContext.GetLibraryNamespace(signatureById.LibraryPath) + Common.GetNamespaceDelimiterConsideringCompilerversion3_5_21_10();
				}
				_editorInstancePath += signatureById.OrgName;
			}
			else
			{
				if (signatureById.POUType != Operator.Action && signatureById.POUType != Operator.Method)
				{
					return;
				}
				ISignature signatureById2 = referenceContextIfAvailable.GetSignatureById(signatureById.ParentSignatureId);
				if (signatureById2 == null)
				{
					_editorInstancePath = string.Empty;
					return;
				}
				_editorInstancePath = _editorInstancePath + "." + signatureById2.OrgName;
				if (signatureById2.POUType != Operator.FunctionBlock)
				{
					_editorInstancePath = _editorInstancePath + "." + signatureById.OrgName;
				}
			}
		}
	}
}
