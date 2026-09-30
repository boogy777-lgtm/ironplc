using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class SubelementItemsCollector
	{
		private readonly IPreCompileContext11 _precom;

		private readonly SubelementItemCollection _collInfoRet = new SubelementItemCollection();

		private readonly LDictionary<string, string> _htHiddenMethods = new LDictionary<string, string>();

		private readonly LDictionary<ISignature, ISignature> _recCheck = new LDictionary<ISignature, ISignature>();

		internal SubelementItemsCollector(IPreCompileContext11 precom)
		{
			_precom = precom;
		}

		private void CollectVariables(ISignature sign, IVariable[] variables, IdentifierInfoFlag eWhichKindOfVariable)
		{
			if (variables == null)
			{
				return;
			}
			foreach (IVariable variable in variables)
			{
				if (!_collInfoRet.Contains(variable.OrgName) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(variable, GUIHidingFlags.AllEvaluation))
				{
					_collInfoRet.Add(new SubelementItem(variable.OrgName, new IdentifierInfo(variable, sign, variable.OrgName, variable.Comment, IdentifierInfoFlag.Variable | eWhichKindOfVariable, variable.Type)));
				}
			}
		}

		internal void FillSubelements(ISignature sign, ISignature signAccessing, IPrecompileScope6 pscope, FindSubelementsFlags flags, bool bIsBaseSignature)
		{
			if (sign == null || _recCheck.ContainsKey(sign))
			{
				return;
			}
			_recCheck.Add(sign, sign);
			Guid applicationGuidForPrecompileLookup = GetApplicationGuidForPrecompileLookup();
			if (sign.POUType == Operator.VarGlobal || sign.POUType == Operator.Type)
			{
				if (!FillSubelementsVarGlobalOrType(ref sign, pscope))
				{
					return;
				}
			}
			else
			{
				FillSubelementsFunctionBlock(sign, signAccessing, flags, bIsBaseSignature, applicationGuidForPrecompileLookup);
			}
			CollectVariablesOfBaseTypesAndInterfaces(sign, signAccessing, pscope, flags);
		}

		internal IIdentifierInfo[] GetCollectedSubElements()
		{
			IdentifierInfo[] array = new IdentifierInfo[_collInfoRet.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _collInfoRet[i].Info;
			}
			return array;
		}

		private bool FillSubelementsVarGlobalOrType(ref ISignature sign, IPrecompileScope6 pscope)
		{
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(sign, GUIHidingFlags.EvaluateAttributes))
			{
				if (sign.GetFlag(SignatureFlag.Alias))
				{
					ISignature signature = PreCompileUtilities._ResolveAlias(sign, pscope, 0);
					if (signature == null)
					{
						return false;
					}
					sign = signature;
				}
				IVariable[] all = sign.All;
				foreach (IVariable variable in all)
				{
					if (!_collInfoRet.Contains(variable.OrgName) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(variable, GUIHidingFlags.AllEvaluation))
					{
						_collInfoRet.Add(new SubelementItem(variable.OrgName, new IdentifierInfo(variable, sign, variable.OrgName, variable.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Global, variable.Type)));
					}
				}
			}
			return true;
		}

		private void FillSubelementsFunctionBlock(ISignature sign, ISignature signAccessing, FindSubelementsFlags flags, bool bIsBaseSignature, Guid gdApplication)
		{
			bool flag = (flags & FindSubelementsFlags.ExcludeSubSignatures) == FindSubelementsFlags.ExcludeSubSignatures;
			bool flag2 = (flags & FindSubelementsFlags.IncludeLocalVars) == FindSubelementsFlags.IncludeLocalVars;
			if (sign.POUType == Operator.FunctionBlock && SmartCodingOptionsHelper.ShowAllInstanceVars)
			{
				flag2 = true;
			}
			IPreCompileContext preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(sign);
			if (preCompileContext == null)
			{
				preCompileContext = _precom;
			}
			IVariable[] varTemps = null;
			IVariable[] variables = null;
			IVariable[] variables2 = null;
			IVariable[] variables3 = null;
			IVariable[] variables4 = null;
			IVariable[] variables5 = null;
			IVariable[] varLocals = null;
			ISignature[] array = null;
			if ((flags & FindSubelementsFlags.InputsOnly) != 0)
			{
				variables3 = sign.Inputs;
				variables5 = sign.InOuts;
			}
			if ((flags & FindSubelementsFlags.OutputsOnly) != 0)
			{
				variables4 = sign.Outputs.Where((IVariable var) => var.Name != sign.Name).ToArray();
			}
			bool flag3 = (flags & FindSubelementsFlags.ExcludeProperties) == FindSubelementsFlags.ExcludeProperties;
			if ((flags & FindSubelementsFlags.InputsOnly) == 0 && (flags & FindSubelementsFlags.OutputsOnly) == 0)
			{
				variables3 = sign.Inputs;
				variables5 = sign.InOuts;
				variables4 = sign.Outputs;
				varTemps = sign.Temps;
				variables = sign.Statics;
				variables2 = sign.Externals;
				varLocals = sign.Locals;
				if (!flag || !flag3)
				{
					array = preCompileContext.GetSubSignatures(sign.ObjectGuid);
				}
			}
			CollectVariables(sign, variables3, IdentifierInfoFlag.Input);
			CollectVariables(sign, variables4, IdentifierInfoFlag.Output);
			CollectVariables(sign, variables5, IdentifierInfoFlag.Inout);
			FindSubelementsPropertiesHelper findSubelementsPropertiesHelper = new FindSubelementsPropertiesHelper();
			findSubelementsPropertiesHelper.Flags = flags;
			findSubelementsPropertiesHelper.Signature = sign;
			findSubelementsPropertiesHelper.SubSignatures = array;
			findSubelementsPropertiesHelper.SignatureAccessing = signAccessing;
			findSubelementsPropertiesHelper.HiddenMethods = _htHiddenMethods;
			findSubelementsPropertiesHelper.Result = _collInfoRet;
			findSubelementsPropertiesHelper.IncludeLocalVars = flag2;
			findSubelementsPropertiesHelper.IsBaseSignature = bIsBaseSignature;
			findSubelementsPropertiesHelper.ApplicationGuid = gdApplication;
			findSubelementsPropertiesHelper.CollectProperties(varLocals);
			CollectTempVariables(sign, signAccessing, flags, flag2, varTemps, bIsBaseSignature);
			if (flag2)
			{
				CollectVariables(sign, variables, IdentifierInfoFlag.Static);
			}
			CollectVariables(sign, variables2, IdentifierInfoFlag.External);
			if (array != null && !flag)
			{
				CollectSubSignatures(sign, signAccessing, flags, bIsBaseSignature, array);
			}
			AddVarInst(sign, signAccessing);
		}

		private void AddVarInst(ISignature sign, ISignature signAccessing)
		{
			if (signAccessing != null && sign.ObjectGuid != signAccessing.ObjectGuid)
			{
				return;
			}
			foreach (IVariable item in sign.All.Where((IVariable x) => x.GetFlag(VarFlag.AllocateInInstance)))
			{
				if (!_collInfoRet.Contains(item.OrgName) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(item, GUIHidingFlags.AllEvaluation))
				{
					_collInfoRet.Add(new SubelementItem(item.OrgName, new IdentifierInfo(item, sign, item.OrgName, item.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Method, item.Type)));
				}
			}
		}

		private static bool IsAccessible(ISignature signHavingSubsignature, ISignature signAccessing, ISignature signSub, FindSubelementsFlags flags, bool bIsBaseSignature)
		{
			bool flag = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signSub, GUIHidingFlags.SignaturePrivate | GUIHidingFlags.EvaluateFlags);
			bool flag2 = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signSub, GUIHidingFlags.SignatureProtected | GUIHidingFlags.EvaluateFlags);
			bool flag3 = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signSub, GUIHidingFlags.EvaluateAttributes);
			if (flag && signAccessing != null && signHavingSubsignature.ObjectGuid != signAccessing.ObjectGuid)
			{
				return false;
			}
			bool result = !(flag2 || flag || flag3);
			if ((flags & FindSubelementsFlags.IncludeLocalVars) != 0 && !flag3 && ((bIsBaseSignature && flag2) || !bIsBaseSignature))
			{
				result = true;
			}
			return result;
		}

		private void CollectSubSignatures(ISignature sign, ISignature signAccessing, FindSubelementsFlags flags, bool bIsBaseSignature, ISignature[] signSubs)
		{
			foreach (ISignature signature in signSubs)
			{
				if (!IsAccessible(sign, signAccessing, signature, flags, bIsBaseSignature) || (signature.GetFlag(SignatureFlag.Internal) && !string.IsNullOrEmpty(signature.LibraryPath)))
				{
					AddToHiddenMethods(signature);
				}
				else if (!_htHiddenMethods.ContainsKey(signature.OrgName) && !_collInfoRet.Contains(signature.Name) && !signature.Name.Contains("__"))
				{
					IdentifierInfoFlag identifierInfoFlag = ((Operator.Method == signature.POUType) ? IdentifierInfoFlag.Method : IdentifierInfoFlag.Action);
					SubelementItem item = new SubelementItem(signature.Name, new IdentifierInfo(signature, signature.OrgName, (signature as ISignature3).Comment, IdentifierInfoFlag.Signature | identifierInfoFlag, null));
					_collInfoRet.Add(item);
				}
			}
		}

		private void AddToHiddenMethods(ISignature signSub)
		{
			if (!_htHiddenMethods.ContainsKey(signSub.OrgName))
			{
				_htHiddenMethods.Add(signSub.OrgName, "");
			}
		}

		private void CollectTempVariables(ISignature sign, ISignature signAccessing, FindSubelementsFlags flags, bool bIncludeLocalVars, IVariable[] varTemps, bool bIsBaseSignature)
		{
			if (!bIncludeLocalVars || bIsBaseSignature || signAccessing == null || sign.ObjectGuid != signAccessing.ObjectGuid || varTemps == null || FindSubelementsFlags.ExcludeVarTemp == (flags & FindSubelementsFlags.ExcludeVarTemp))
			{
				return;
			}
			foreach (IVariable variable in varTemps)
			{
				if (!_collInfoRet.Contains(variable.OrgName) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(variable, GUIHidingFlags.AllEvaluation) && !_htHiddenMethods.ContainsKey(variable.OrgName))
				{
					_collInfoRet.Add(new SubelementItem(variable.OrgName, new IdentifierInfo(variable, sign, variable.OrgName, variable.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Temporary, variable.Type)));
				}
				else if (!_htHiddenMethods.ContainsKey(variable.OrgName))
				{
					_htHiddenMethods.Add(variable.OrgName, "");
				}
			}
		}

		private Guid GetApplicationGuidForPrecompileLookup()
		{
			Guid guid = _precom.ApplicationGuid;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 20, 0) && guid == Guid.Empty)
			{
				guid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
			}
			return guid;
		}

		private void CollectVariablesOfBaseTypesAndInterfaces(ISignature sign, ISignature signAccessing, IPrecompileScope6 pscope, FindSubelementsFlags flags)
		{
			if (!(sign is ISignature2 signature))
			{
				return;
			}
			Guid applicationGuidForPrecompileLookup = GetApplicationGuidForPrecompileLookup();
			if (signature.BaseExpression == null && signature.InterfaceExpressions == null)
			{
				return;
			}
			ISignature2 baseSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetBaseSignature(signature, applicationGuidForPrecompileLookup);
			if (baseSignature != null)
			{
				FillSubelements(baseSignature, signAccessing, MaybeCreateLibraryScope(baseSignature, pscope), flags, bIsBaseSignature: true);
			}
			foreach (ISignature2 interfaceSignature in APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetInterfaceSignatures(signature, applicationGuidForPrecompileLookup))
			{
				FillSubelements(interfaceSignature, signAccessing, MaybeCreateLibraryScope(interfaceSignature, pscope), flags, bIsBaseSignature: true);
			}
		}

		private IPrecompileScope6 MaybeCreateLibraryScope(ISignature sign, IPrecompileScope6 pscope)
		{
			IPrecompileScope6 result = pscope;
			if (!string.IsNullOrEmpty(sign.LibraryPath) && APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryPrecompileContext(sign.LibraryPath) is _IPreCompileContext iPreCompileContext)
			{
				result = iPreCompileContext.CreatePrecompileScope(Guid.Empty) as IPrecompileScope6;
			}
			return result;
		}
	}
}
