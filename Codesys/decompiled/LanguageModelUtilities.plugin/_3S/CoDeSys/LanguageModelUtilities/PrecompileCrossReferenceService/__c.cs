using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{115C3A3D-D8E1-43F8-BD58-A107350FAB8F}")]
	public class PrecompileCrossReferenceService : IPrecompileCrossReferenceService3, IPrecompileCrossReferenceService2, IPrecompileCrossReferenceService, IEarlyPreCompileCrossReferenceService
	{
		private PrecompileCheckEnforcer CheckEnforcer { get; } = new PrecompileCheckEnforcer();


		public IList<ICrossReferenceNode> GetVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.FinishPrecompileChecks();
			return GetEarlyVariableCrossReferences(signaturePrecompileId, variablePrecompileId);
		}

		public IList<ICrossReferenceNode> GetSignatureCrossReferences(int signaturePrecompileId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.FinishPrecompileChecks();
			return GetSignatureCrossReferences(signaturePrecompileId, realCallsOnly: false);
		}

		public IList<ICrossReferenceNode> GetSignatureCrossReferences(int signaturePrecompileId, SignatureCrossReferenceFlags flags)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.FinishPrecompileChecks();
			if (flags.HasFlag(SignatureCrossReferenceFlags.IncludeOverridenCalls))
			{
				return GetSignatureCrossReferencesWithCallsToBaseFBs(signaturePrecompileId, flags);
			}
			return GetSignatureCrossReferences(signaturePrecompileId, flags.HasFlag(SignatureCrossReferenceFlags.DirectCallsOnly));
		}

		public IList<ICrossReferenceNode> GetDirectSignatureCrossReferences(int signaturePrecompileId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.FinishPrecompileChecks();
			return GetSignatureCrossReferences(signaturePrecompileId, realCallsOnly: true);
		}

		public IList<int> GetRelatedSignatures(int signaturePrecompileId, SignatureRelationFlags flags)
		{
			throw new NotImplementedException();
		}

		public void ForceVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId)
		{
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signaturePrecompileId);
			if (signatureForPrecompileID != null)
			{
				IVariable4 variableForPrecompileId = signatureForPrecompileID.GetVariableForPrecompileId(variablePrecompileId);
				if (variableForPrecompileId != null)
				{
					CheckEnforcer.ForceSymbolReferences(variableForPrecompileId.OrgName);
				}
			}
		}

		public void ForceSignatureCrossReferences(int signaturePrecompileId)
		{
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signaturePrecompileId);
			if (signatureForPrecompileID != null)
			{
				CheckEnforcer.ForceTypeDeclarationReferences(signatureForPrecompileID);
			}
		}

		public void ForceSymbolCrossReferences(string stSymbol)
		{
			CheckEnforcer.ForceSymbolReferences(stSymbol);
			CheckEnforcer.ForceSymbolTypeReferences(stSymbol);
		}

		public IList<ICrossReferenceNode> GetEarlyVariableCrossReferences(int signaturePrecompileId, int variablePrecompileId)
		{
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signaturePrecompileId);
			if (signatureForPrecompileID == null)
			{
				return (IList<ICrossReferenceNode>)val;
			}
			IVariable4 variableForPrecompileId = signatureForPrecompileID.GetVariableForPrecompileId(variablePrecompileId);
			if (variableForPrecompileId == null)
			{
				return (IList<ICrossReferenceNode>)val;
			}
			foreach (ICrossReference precompileCrossReference in variableForPrecompileId.PrecompileCrossReferences)
			{
				ISignature6 signatureForPrecompileID2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileCrossReference.CodeId);
				if (signatureForPrecompileID2 == null || !(APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signatureForPrecompileID2) is IPreCompileContext11 preCompileContext))
				{
					continue;
				}
				foreach (IPrecompilePositionInfo variableReferencePosition in preCompileContext.GetVariableReferencePositions(signatureForPrecompileID2.PrecompileId, signatureForPrecompileID.PrecompileId, variableForPrecompileId.PrecompileId))
				{
					val.Add((ICrossReferenceNode)new PrecompileCrossRefNode(variableReferencePosition, signaturePrecompileId, variablePrecompileId, Guid.Empty));
				}
			}
			LList<ICrossReferenceNode> val2 = new LList<ICrossReferenceNode>();
			CollectAdditionalCrossReferences(signatureForPrecompileID, variableForPrecompileId, val2);
			foreach (ICrossReferenceNode item in val2)
			{
				ISignature6 signature = item.Signature as ISignature6;
				IVariable4 variable = item.Variable as IVariable4;
				if (signature != null && signature.PrecompileId == signaturePrecompileId && variable != null && variable.PrecompileId == variablePrecompileId)
				{
					val.Add(item);
				}
			}
			return (IList<ICrossReferenceNode>)val;
		}

		private static void CollectAdditionalCrossReferences(ISignature6 signature, IVariable4 variable, LList<ICrossReferenceNode> additionalReferences)
		{
			foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in APEnvironmentFacade.Instance.AdditionalCrossReferenceProviders)
			{
				additionalReferences.AddRange(additionalCrossReferenceProvider.GetAdditionalCrossReferences(variable.Name, signature.ObjectGuid, CrossRefSearchType.Variables, CrossRefOccurence.GeneratedInDownload | CrossRefOccurence.Virtual, CrossReferenceMatchType.Existing));
			}
			foreach (ISimpleFilteredAdditionalCrossReferenceProvider simpleFilteredAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleFilteredAdditionalCrossReferenceProviders)
			{
				additionalReferences.AddRange(CrossRefUtils.RetrieveAdditionalCrossRefs(simpleFilteredAdditionalCrossReferenceProvider, variable.Name));
			}
			foreach (ISimpleAdditionalCrossReferenceProvider simpleAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleAdditionalCrossReferenceProviders)
			{
				try
				{
					additionalReferences.AddRange(simpleAdditionalCrossReferenceProvider.ProvideAdditionalCrossReferences());
				}
				catch
				{
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}
				}
			}
		}

		public IList<ICrossReferenceNode> GetEarlyDirectSignatureCrossReferences(int signaturePrecompileId)
		{
			return GetSignatureCrossReferences(signaturePrecompileId, realCallsOnly: true);
		}

		public IList<ICrossReferenceNode> GetEarlySignatureCrossReferences(int signaturePrecompileId, SignatureCrossReferenceFlags flags)
		{
			if (flags.HasFlag(SignatureCrossReferenceFlags.IncludeOverridenCalls))
			{
				return GetSignatureCrossReferencesWithCallsToBaseFBs(signaturePrecompileId, flags);
			}
			return GetSignatureCrossReferences(signaturePrecompileId, flags.HasFlag(SignatureCrossReferenceFlags.DirectCallsOnly));
		}

		private static bool? IsCallViaReferenceType(IExprement exp)
		{
			if (exp == null)
			{
				return null;
			}
			if (!(exp is ICompoAccessExpression compoAccessExpression))
			{
				return null;
			}
			if (!(compoAccessExpression.Left is IExpression6 expression))
			{
				return null;
			}
			ILMPreCompileService preCompileService = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService;
			ISignature6 signatureForPrecompileID = preCompileService.GetSignatureForPrecompileID(expression.PrecompileSignatureId);
			if (signatureForPrecompileID == null)
			{
				return null;
			}
			IVariable4 variableForPrecompileId = signatureForPrecompileID.GetVariableForPrecompileId(expression.PrecompileVariableId);
			if (variableForPrecompileId == null)
			{
				return null;
			}
			IType type = variableForPrecompileId.Type;
			if (type == null)
			{
				return null;
			}
			if (type.Class == TypeClass.Reference || type.Class == TypeClass.Pointer)
			{
				return true;
			}
			if (type.Class == TypeClass.Userdef)
			{
				IUserdefType userdefType = type as IUserdefType;
				ISignature6 signatureForPrecompileID2 = preCompileService.GetSignatureForPrecompileID(userdefType.SignatureId);
				if (signatureForPrecompileID2 != null)
				{
					return signatureForPrecompileID2.POUType == Operator.Interface;
				}
				return false;
			}
			return false;
		}

		internal IList<ICrossReferenceNode> GetSignatureCrossReferencesWithCallsToBaseFBs(int signaturePrecompileId, SignatureCrossReferenceFlags flags)
		{
			ILanguageModelManager22 languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			IList<ICrossReferenceNode> signatureCrossReferences = GetSignatureCrossReferences(signaturePrecompileId, flags.HasFlag(SignatureCrossReferenceFlags.DirectCallsOnly));
			ISignature6 signatureForPrecompileID = languageModelMgr.GetSignatureForPrecompileID(signaturePrecompileId);
			if (signatureForPrecompileID == null)
			{
				return signatureCrossReferences;
			}
			IEnumerable<ICrossReferenceNode> signatureCallsToBaseFBs = GetSignatureCallsToBaseFBs(flags, signatureForPrecompileID);
			Enumerable.AddRange<ICrossReferenceNode>((ICollection<ICrossReferenceNode>)signatureCrossReferences, signatureCallsToBaseFBs);
			return signatureCrossReferences;
		}

		private IEnumerable<ICrossReferenceNode> GetSignatureCallsToBaseFBs(SignatureCrossReferenceFlags flags, ISignature6 signature)
		{
			if (signature == null)
			{
				return Enumerable.Empty<ICrossReferenceNode>();
			}
			if (signature.POUType != Operator.Method && signature.POUType != Operator.Action)
			{
				return Enumerable.Empty<ICrossReferenceNode>();
			}
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signature.PrecompileParentId);
			ISignature6 subSignature = ((signatureForPrecompileID == null) ? null : signature);
			bool flag;
			IEnumerable<IRelatedSignature> source;
			if (signatureForPrecompileID.POUType == Operator.FunctionBlock)
			{
				flag = signature.GetFlag(SignatureFlag.Abstract);
				SignatureRelationFlags signatureRelationFlags = SignatureRelationFlags.OverriddenBases | SignatureRelationFlags.ImplementedInterfaces;
				if (flag)
				{
					signatureRelationFlags |= SignatureRelationFlags.ImplementingInterface;
				}
				source = new RelatedSignatureFinder(signature, subSignature, signatureRelationFlags).FindResult();
			}
			else if (signatureForPrecompileID.POUType == Operator.Interface)
			{
				flag = true;
				source = new RelatedSignatureFinder(signature, subSignature, SignatureRelationFlags.ImplementingInterface).FindResult();
			}
			else
			{
				flag = false;
				source = Enumerable.Empty<IRelatedSignature>();
			}
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			foreach (ISignature6 item in source.Select((IRelatedSignature r) => r.Signature as ISignature6))
			{
				if (item == null || item.PrecompileId == signature.PrecompileId)
				{
					continue;
				}
				IEnumerable<ICrossReferenceNode> signatureCrossReferences = GetSignatureCrossReferences(item.PrecompileId, flags.HasFlag(SignatureCrossReferenceFlags.DirectCallsOnly));
				signatureCrossReferences = signatureCrossReferences.Where((ICrossReferenceNode r) => r.AccessInfo.Access.HasFlag(AccessFlag.Call));
				if (!flag)
				{
					signatureCrossReferences = signatureCrossReferences.Where((ICrossReferenceNode r) => IsCallViaReferenceType((r as ICrossReferenceNode3)?.ExpressionAtSourcePosition).GetValueOrDefault(true));
				}
				val.AddRange(signatureCrossReferences);
			}
			return (IEnumerable<ICrossReferenceNode>)val;
		}

		internal IList<ICrossReferenceNode> GetSignatureCrossReferences(int signaturePrecompileId, bool realCallsOnly)
		{
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signaturePrecompileId);
			if (signatureForPrecompileID == null)
			{
				return (IList<ICrossReferenceNode>)val;
			}
			LList<int> obj = new LList<int>();
			obj.Add(signatureForPrecompileID.PrecompileId);
			foreach (int item in Enumerable.ToLList<int>(((IEnumerable<int>)obj).Union(signatureForPrecompileID.PrecompileDeclarerIds).Union(signatureForPrecompileID.PrecompileCallerIds).Union((IEnumerable<int>)ReferenceHelper.GetImplicitDeclarers(signatureForPrecompileID))))
			{
				ISignature6 signatureForPrecompileID2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(item);
				if (signatureForPrecompileID2 == null || !(APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signatureForPrecompileID2) is IPreCompileContext13 preCompileContext))
				{
					continue;
				}
				IList<IPrecompilePositionInfo> list = ((!realCallsOnly) ? preCompileContext.GetCrossReferencePositions(item, signaturePrecompileId) : preCompileContext.GetDirectCrossReferencePositions(item, signaturePrecompileId));
				foreach (IPrecompilePositionInfo item2 in list)
				{
					int num = (item2 as IPrecompilePositionInfo3)?.VariableIDAtCodePosition ?? (-1);
					int referencedSignatureId = signaturePrecompileId;
					if (num != -1 && item2 is IPrecompilePositionInfo5)
					{
						referencedSignatureId = ((IPrecompilePositionInfo5)item2).SignatureIDAtCodePosition;
					}
					Guid alternativeMessageGuid = (item2 as IPrecompilePositionInfo4)?.MessageGuid ?? Guid.Empty;
					PrecompileCrossRefNode precompileCrossRefNode = new PrecompileCrossRefNode(item2, referencedSignatureId, num, alternativeMessageGuid);
					val.Add((ICrossReferenceNode)precompileCrossRefNode);
				}
			}
			LList<ICrossReferenceNode> val2 = new LList<ICrossReferenceNode>();
			foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in APEnvironmentFacade.Instance.AdditionalCrossReferenceProviders)
			{
				val2.AddRange(additionalCrossReferenceProvider.GetAdditionalCrossReferences(signatureForPrecompileID.Name, signatureForPrecompileID.ObjectGuid, CrossRefSearchType.Signatures, CrossRefOccurence.GeneratedInDownload | CrossRefOccurence.Virtual, CrossReferenceMatchType.Existing));
			}
			foreach (ISimpleFilteredAdditionalCrossReferenceProvider simpleFilteredAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleFilteredAdditionalCrossReferenceProviders)
			{
				val2.AddRange(CrossRefUtils.RetrieveAdditionalCrossRefs(simpleFilteredAdditionalCrossReferenceProvider, signatureForPrecompileID.Name));
			}
			foreach (ISimpleAdditionalCrossReferenceProvider simpleAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleAdditionalCrossReferenceProviders)
			{
				try
				{
					val2.AddRange(simpleAdditionalCrossReferenceProvider.ProvideAdditionalCrossReferences());
				}
				catch
				{
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}
				}
			}
			foreach (ICrossReferenceNode item3 in val2)
			{
				if (item3.Signature is ISignature6 signature && signature.PrecompileId == signaturePrecompileId)
				{
					val.Add(item3);
				}
			}
			return (IList<ICrossReferenceNode>)val;
		}

		internal IList<ICrossReferenceNode> Perform(ISignature signature, IVariable variable, string stNewName, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid scope)
		{
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			if (searchType.HasFlag(CrossRefSearchType.Variables))
			{
				int variablePrecompileId = -1;
				if (variable is IVariable4 variable2)
				{
					variablePrecompileId = variable2.PrecompileId;
				}
				val.AddRange((IEnumerable<ICrossReferenceNode>)GetVariableCrossReferences(((ISignature6)signature).PrecompileId, variablePrecompileId));
			}
			else if (searchType.HasFlag(CrossRefSearchType.Signatures))
			{
				val.AddRange((IEnumerable<ICrossReferenceNode>)GetSignatureCrossReferences(((ISignature6)signature).PrecompileId));
			}
			if (matchType.HasFlag(CrossReferenceMatchType.Shadowed))
			{
				string stOldName = string.Empty;
				if (searchType.HasFlag(CrossRefSearchType.Variables) && variable != null)
				{
					stOldName = variable.Name;
				}
				else if (searchType.HasFlag(CrossRefSearchType.Signatures))
				{
					stOldName = signature.Name;
				}
				ResolveShadowing(signature, stNewName, val, stOldName);
				IIdentifierInfo[] identifierInfo = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature).GetIdentifierInfo(signature.ObjectGuid, stNewName);
				if (identifierInfo != null && identifierInfo.Length != 0)
				{
					LList<ICrossReferenceNode> val2 = new LList<ICrossReferenceNode>();
					IIdentifierInfo[] array = identifierInfo;
					foreach (IIdentifierInfo identifierInfo2 in array)
					{
						if (identifierInfo2.Variable != null)
						{
							val2.AddRange((IEnumerable<ICrossReferenceNode>)GetVariableCrossReferences(((ISignature6)identifierInfo2.Signature).PrecompileId, ((IVariable4)identifierInfo2.Variable).PrecompileId));
						}
						else if (identifierInfo2.Signature != null)
						{
							val2.AddRange((IEnumerable<ICrossReferenceNode>)GetSignatureCrossReferences(((ISignature6)identifierInfo2.Signature).PrecompileId));
						}
					}
					ResolveShadowing(signature, stNewName, val2, stNewName);
					val.AddRange((IEnumerable<ICrossReferenceNode>)val2);
				}
			}
			return (IList<ICrossReferenceNode>)val;
		}

		private void ResolveShadowing(ISignature signature, string stNewName, LList<ICrossReferenceNode> nodes, string stOldName)
		{
			foreach (ICrossReferenceNode node in nodes)
			{
				CrossReferenceMatchType crossReferenceMatchType = node.MatchType & ~CrossReferenceMatchType.Shadowed;
				IIdentifierInfo identInfo = null;
				bool bShadowResolutionPossible = true;
				if (IsShadowed(stOldName, stNewName, node, signature, out identInfo, out bShadowResolutionPossible))
				{
					crossReferenceMatchType = ((!bShadowResolutionPossible) ? CrossReferenceMatchType.ShadowResolutionNotPossible : CrossReferenceMatchType.Shadowed);
					((_ICrossReferenceNode)node).MatchType = crossReferenceMatchType;
					((_ICrossReferenceNode)node).ShadowedBy((IIdentifierInfo2)identInfo, stNewName);
				}
			}
		}

		private bool DoesVariableAlwaysHaveSameNameAsSignature(ICrossReferenceNode node, _ISignature signature)
		{
			if (node.Variable == null)
			{
				return false;
			}
			if (node.Signature.HasAttribute("property"))
			{
				string text = "__get" + node.Variable.Name;
				string text2 = "__set" + node.Variable.Name;
				if (!text.Equals(node.Signature.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					return text2.Equals(node.Signature.Name, StringComparison.InvariantCultureIgnoreCase);
				}
				return true;
			}
			if ((node.Signature as _ISignature).PrecompileId != signature.PrecompileId)
			{
				return false;
			}
			return node.Name == signature.Name;
		}

		private bool IsShadowed(string stOldName, string stNewName, ICrossReferenceNode node, ISignature renamingSignature, out IIdentifierInfo identInfo, out bool bShadowResolutionPossible)
		{
			identInfo = null;
			bShadowResolutionPossible = true;
			if (node.AccessInfo == null || node.AccessInfo.Position == null)
			{
				return false;
			}
			IAccessInfo accessInfo = node.AccessInfo;
			IExprement expressionAtSourcePosition = ((ICrossReferenceNode3)node).ExpressionAtSourcePosition;
			IPreCompileContext precom = null;
			APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(accessInfo.Position.ObjectGuid, out precom);
			if (precom == null)
			{
				return false;
			}
			if (DoesVariableAlwaysHaveSameNameAsSignature(node, renamingSignature as _ISignature))
			{
				return false;
			}
			IIdentifierInfo[] identifierInfo = precom.GetIdentifierInfo(accessInfo.Position.ObjectGuid, stNewName);
			if (accessInfo.Access != AccessFlag.Declarative && identifierInfo != null && identifierInfo.Length != 0 && identifierInfo[0].Signature != null)
			{
				ISignature signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(renamingSignature.ObjectGuid, out precom);
				if (signature.ObjectGuid == identifierInfo[0].Signature.ObjectGuid)
				{
					return false;
				}
				IIdentifierInfo[] identifierInfo2 = precom.GetIdentifierInfo(renamingSignature.ObjectGuid, stOldName);
				if (identifierInfo2 != null && identifierInfo2.Length != 0 && expressionAtSourcePosition != null && expressionAtSourcePosition.ToString().StartsWith(stOldName, StringComparison.InvariantCultureIgnoreCase))
				{
					ISignature signature2 = signature;
					ISignature[] array = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(accessInfo.Position.ProjectHandle, renamingSignature.ObjectGuid, stNewName);
					if (array != null && array.Length != 0)
					{
						signature2 = array[0];
					}
					if (signature2.POUType == Operator.FunctionBlock || signature2.POUType == Operator.Type || signature2.POUType == Operator.Interface)
					{
						if ((accessInfo.Access & AccessFlag.Type) != 0)
						{
							return false;
						}
						if (identifierInfo[0].Variable == null && identifierInfo2[0].Variable != null)
						{
							return false;
						}
						if (identifierInfo[0].Variable != null && identifierInfo2[0].Variable == null)
						{
							return false;
						}
						if (identifierInfo[0].Variable == null && identifierInfo2[0].Variable == null && renamingSignature.ObjectGuid == accessInfo.Position.ObjectGuid && stNewName.Equals(stOldName))
						{
							return false;
						}
					}
					if (stOldName.Equals(stNewName, StringComparison.InvariantCultureIgnoreCase))
					{
						if (renamingSignature == null && identifierInfo2[0].Signature != null && identifierInfo2[0].Signature.POUType == Operator.Program && identifierInfo2[0].Signature.OrgName.Equals(stNewName, StringComparison.InvariantCultureIgnoreCase) && accessInfo.Access == AccessFlag.Call)
						{
							bShadowResolutionPossible = false;
							identInfo = null;
						}
						else
						{
							identInfo = identifierInfo[0];
						}
					}
					else if (renamingSignature != null && renamingSignature.POUType == Operator.Program && accessInfo.Access == AccessFlag.Call)
					{
						bShadowResolutionPossible = false;
						identInfo = null;
					}
					else
					{
						identInfo = identifierInfo2[0];
					}
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
