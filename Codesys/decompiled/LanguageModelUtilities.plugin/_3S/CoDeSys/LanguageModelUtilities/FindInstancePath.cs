using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class FindInstancePath
	{
		private readonly ILMCompiledApplicationSet _compiledApplicationSet;

		private readonly int _iSelectedArea;

		private readonly int _iOffset;

		private string _stInstancePath = string.Empty;

		private _ISignature _signFound;

		private _IVariable _varFound;

		private readonly _ISignature _signFB;

		public IVariable VariableFound => _varFound;

		public ISignature SignatureFound => _signFound;

		public bool _Found { get; set; }

		internal FindInstancePath(int iArea, int iOffset, _ISignature signFB, ILMCompiledApplicationSet compiledApplicationSet)
		{
			_iSelectedArea = iArea;
			_iOffset = iOffset;
			_compiledApplicationSet = compiledApplicationSet;
			_signFB = signFB;
		}

		public string GetInstancePath()
		{
			return _stInstancePath;
		}

		public void FindNearestSymbol()
		{
			_signFound = null;
			_varFound = null;
			LookupMethod();
			if (_signFound != null && _varFound != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (!string.IsNullOrEmpty(_signFound.LibraryPath))
				{
					ILMPreCompileSet preCompileSet = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(_compiledApplicationSet.ApplicationGuid);
					string localLibraryNamespaceRecursive = ((ILibraryTable4)preCompileSet.GetLibraryTable(_compiledApplicationSet.ApplicationGuid)).GetLocalLibraryNamespaceRecursive(preCompileSet as IPreCompileContext, _signFound.LibraryPath);
					stringBuilder.Append(localLibraryNamespaceRecursive).Append(Common.GetNamespaceDelimiterConsideringCompilerversion3_5_21_10());
				}
				stringBuilder.Append(_signFound.OrgName + "." + _varFound.OrgName);
				FindLocalInstanceOffset(stringBuilder, _signFound, _varFound, _iOffset - _varFound.DataLocation.Offset);
				_stInstancePath = stringBuilder.ToString();
			}
		}

		private void LookupMethod()
		{
			ILMCompiledApplicationTypification typificator = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetTypificator(_compiledApplicationSet.ApplicationGuid);
			foreach (ISignature4 item in _compiledApplicationSet.AllSignaturesFlat)
			{
				IScope scope = typificator.CreateScope(item.Id);
				IVariable[] all = item.All;
				foreach (IVariable variable in all)
				{
					if (variable.DataLocation != null && variable.DataLocation.Area == _iSelectedArea && variable.DataLocation.Offset <= _iOffset && variable.DataLocation.Offset + variable.CompiledType.Size(scope) > _iOffset)
					{
						_signFound = item as _ISignature;
						_varFound = variable as _IVariable;
						return;
					}
				}
			}
		}

		private void FindLocalInstanceOffset(StringBuilder strbInstance, ISignature sign, IVariable var, int iOffset)
		{
			IScope scope = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetTypificator(_compiledApplicationSet.ApplicationGuid).CreateScope(sign.Id);
			if (var.Type.Class == TypeClass.Userdef)
			{
				HandleUserdefType(var.Type as _IUserdefType, scope as IScope5, iOffset, strbInstance);
			}
			else if (var.Type.Class == TypeClass.Array)
			{
				_IArrayType arrtype = var.Type as _IArrayType;
				ICompiledType basetype = arrtype.BaseType;
				GetArrayIndexAccess(ref iOffset, scope, strbInstance, ref arrtype, ref basetype);
				if (basetype is IUserdefType)
				{
					HandleUserdefType(basetype as _IUserdefType, scope as IScope5, iOffset, strbInstance);
				}
			}
		}

		private void HandleUserdefSignature(_ISignature signUserdef, IScope5 scope, int iOffset, StringBuilder strbInstance)
		{
			_signFound = signUserdef;
			bool flag = false;
			IVariable[] all = signUserdef.All;
			foreach (IVariable variable in all)
			{
				if (variable.DataLocation != null && variable.DataLocation.Offset <= iOffset && variable.DataLocation.Offset + variable.CompiledType.Size(scope) > iOffset)
				{
					strbInstance.Append("." + variable.OrgName);
					FindLocalInstanceOffset(strbInstance, signUserdef, variable, iOffset - variable.DataLocation.Offset);
					flag = true;
					_varFound = (_IVariable)variable;
				}
			}
			if (!flag && signUserdef.BaseSignatureId != -1)
			{
				_ISignature signUserdef2 = scope[signUserdef.BaseSignatureId] as _ISignature;
				HandleUserdefSignature(signUserdef2, scope, iOffset, strbInstance);
			}
		}

		private void HandleUserdefType(_IUserdefType udtype, IScope5 scope, int iOffset, StringBuilder strbInstance)
		{
			if (udtype.GetSignature(scope) is _ISignature iSignature)
			{
				_Found = iSignature.Id == _signFB.Id;
				if (!_Found)
				{
					HandleUserdefSignature(iSignature, scope, iOffset, strbInstance);
				}
			}
		}

		private static void GetArrayIndexAccess(ref int iOffset, IScope scope, StringBuilder strbInstance, ref _IArrayType arrtype, ref ICompiledType basetype)
		{
			int num = basetype.Size(scope);
			int num2 = iOffset / num;
			bool bValid;
			int[] values = arrtype.ToDimensionIndexes(num2, scope as IScope5, out bValid);
			string value = $"[{num2}]";
			if (bValid)
			{
				value = string.Format("[{0}]", string.Join(", ", values));
			}
			strbInstance.Append(value);
			iOffset -= num2 * num;
			if (basetype is IArrayType)
			{
				arrtype = basetype as _IArrayType;
				basetype = arrtype.BaseType;
				GetArrayIndexAccess(ref iOffset, scope, strbInstance, ref arrtype, ref basetype);
			}
		}
	}
}
