using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{46B78F02-D871-41DF-A993-382AFF8FC20C}")]
	public class CrossReferenceService : ICrossReferenceService6, ICrossReferenceService5, ICrossReferenceService4, ICrossReferenceService3, ICrossReferenceService2, ICrossReferenceService
	{
		private class ExtendedAccessInfo : IAccessInfo2, IAccessInfo
		{
			private CrossRefSearchType _searchType;

			private IAccessInfo2 _accInfo;

			public Guid ApplicationGuid => _accInfo.ApplicationGuid;

			public Guid MessageGuid => _accInfo.MessageGuid;

			public ISourcePosition Position => _accInfo.Position;

			public AccessFlag Access => _accInfo.Access;

			public CrossRefSearchType CrossRefSearchType => _searchType;

			public ExtendedAccessInfo(CrossRefSearchType searchType, IAccessInfo2 accInfo)
			{
				if (accInfo == null)
				{
					throw new NullReferenceException("accessinfo");
				}
				_searchType = searchType;
				_accInfo = accInfo;
			}

			public static IList<ExtendedAccessInfo> Create(IEnumerable<IAccessInfo2> accInfos, CrossRefSearchType type)
			{
				IList<ExtendedAccessInfo> list = (IList<ExtendedAccessInfo>)new LList<ExtendedAccessInfo>();
				if (accInfos != null)
				{
					foreach (IAccessInfo2 accInfo in accInfos)
					{
						list.Add(new ExtendedAccessInfo(type, accInfo));
					}
					return list;
				}
				return list;
			}
		}

		private static readonly IScanner s_scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);

		private static readonly IParser s_parser = APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(s_scanner);

		private bool _bCheckShadowing;

		private static CrossReferenceScope _scope;

		private IVariable _sourceVar;

		private ISignature _renamingSignature;

		private bool _bExplicitShadowing;

		private LList<CrossRefNode> CreateCrossReferenceNodes(string stOldName, string stNewName, IList<ExtendedAccessInfo> alCrossRefs, Guid guidObject, Guid guidScope, CrossRefSearchType searchType, CrossReferenceMatchType matchType)
		{
			LList<CrossRefNode> val = new LList<CrossRefNode>();
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return val;
			}
			LDictionary<string, object> val2 = new LDictionary<string, object>();
			foreach (ExtendedAccessInfo alCrossRef in alCrossRefs)
			{
				CrossReferenceMatchType matchType2 = matchType & ~CrossReferenceMatchType.Shadowed;
				IIdentifierInfo identInfo = null;
				bool bShadowResolutionPossible = true;
				if (_bCheckShadowing && IsShadowed(stOldName, stNewName, alCrossRef, guidObject, out identInfo, out bShadowResolutionPossible))
				{
					matchType2 = ((!bShadowResolutionPossible) ? CrossReferenceMatchType.ShadowResolutionNotPossible : CrossReferenceMatchType.Shadowed);
				}
				CrossRefNode crossRefNode = CreateNode(alCrossRef, stOldName, alCrossRef.CrossRefSearchType, matchType2, identInfo, null, stOldName, stNewName, guidScope);
				if (crossRefNode == null)
				{
					continue;
				}
				if (string.Compare(crossRefNode.POUName, 0, "__EVENT__FUNCTION__", 0, "__EVENT__FUNCTION__".Length) == 0)
				{
					if (val2.ContainsKey(crossRefNode.POUName))
					{
						continue;
					}
					val2.set_Item(crossRefNode.POUName, (object)null);
				}
				if (string.Compare(crossRefNode.POUName, 0, "CALLTASK__", 0, "CALLTASK__".Length) == 0)
				{
					if (val2.ContainsKey(crossRefNode.POUName) || alCrossRef.Access != AccessFlag.Call)
					{
						continue;
					}
					val2.set_Item(crossRefNode.POUName, (object)null);
				}
				if (!((IEnumerable<ICrossReferenceNode>)val).Contains(crossRefNode, (IEqualityComparer<ICrossReferenceNode>)IdentityComparer<ICrossReferenceNode>.Instance) && NodeIsInSearchScope(crossRefNode, guidScope, guidObject))
				{
					val.Add(crossRefNode);
				}
			}
			FilterNodes((IList<CrossRefNode>)val, guidObject, stOldName, guidScope, searchType);
			return val;
		}

		private bool IsLocalShadowed(IAccessInfo2 accinfo)
		{
			if (accinfo == null || accinfo.Position == null)
			{
				return false;
			}
			IPreCompileContext precom = null;
			IExprement exprement = APEnvironmentFacade.Instance.LanguageModelMgr.FindExpressionAtSourcePosition(accinfo.Position, WhatToFind.WholeInstancePath, out precom);
			IIdentifierInfo[] identifierInfo = precom.GetIdentifierInfo(accinfo.Position.ObjectGuid, exprement.ToString());
			if (identifierInfo != null && identifierInfo.Length == 1 && identifierInfo[0].Signature != null && identifierInfo[0].Signature.ObjectGuid == accinfo.Position.ObjectGuid)
			{
				return false;
			}
			if (accinfo.Access == AccessFlag.Declarative || (exprement != null && exprement.ToString().Contains(".")))
			{
				return false;
			}
			return true;
		}

		private bool IsShadowed(string stOldName, string stNewName, IAccessInfo2 accinfo, Guid objectGuid, out IIdentifierInfo identInfo, out bool bShadowResolutionPossible)
		{
			identInfo = null;
			bShadowResolutionPossible = true;
			if (accinfo == null || accinfo.Position == null)
			{
				return false;
			}
			IPreCompileContext precom = null;
			IExprement exprement = APEnvironmentFacade.Instance.LanguageModelMgr.FindExpressionAtSourcePosition(accinfo.Position, WhatToFind.WholeInstancePath, out precom);
			if (exprement != null && (exprement.ToString().EndsWith("NULL") || !exprement.ToString().Contains(stOldName, StringComparison.InvariantCultureIgnoreCase)))
			{
				exprement = APEnvironmentFacade.Instance.LanguageModelMgr.FindExpressionAtSourcePosition(accinfo.Position, WhatToFind.PartialInstancePath, out precom);
			}
			if (precom == null)
			{
				return false;
			}
			IIdentifierInfo[] identifierInfo = precom.GetIdentifierInfo(accinfo.Position.ObjectGuid, stNewName);
			if (accinfo.Access != AccessFlag.Declarative && identifierInfo != null && identifierInfo.Length != 0 && identifierInfo[0].Signature != null)
			{
				ISignature signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(objectGuid, out precom);
				if (signature.ObjectGuid == identifierInfo[0].Signature.ObjectGuid)
				{
					return false;
				}
				IIdentifierInfo[] identifierInfo2 = precom.GetIdentifierInfo(objectGuid, stOldName);
				if (identifierInfo2 != null && identifierInfo2.Length != 0 && exprement != null && exprement.ToString().StartsWith(stOldName))
				{
					ISignature signature2 = signature;
					ISignature[] array = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(accinfo.Position.ProjectHandle, objectGuid, stNewName);
					if (array != null && array.Length != 0)
					{
						signature2 = array[0];
					}
					if (signature2.POUType == Operator.FunctionBlock || signature2.POUType == Operator.Type || signature2.POUType == Operator.Interface)
					{
						if ((accinfo.Access & AccessFlag.Type) != 0)
						{
							return false;
						}
						if (identifierInfo[0].Variable == null && identifierInfo2[0].Variable != null)
						{
							return false;
						}
						if (identifierInfo[0].Variable == null && identifierInfo2[0].Variable == null && objectGuid == accinfo.Position.ObjectGuid && stNewName.Equals(stOldName))
						{
							return false;
						}
					}
					if (stOldName.Equals(stNewName, StringComparison.InvariantCultureIgnoreCase))
					{
						if (_renamingSignature == null && identifierInfo2[0].Signature != null && identifierInfo2[0].Signature.POUType == Operator.Program && identifierInfo2[0].Signature.OrgName.Equals(stNewName, StringComparison.InvariantCultureIgnoreCase) && accinfo.Access == AccessFlag.Call)
						{
							bShadowResolutionPossible = false;
							identInfo = null;
						}
						else
						{
							identInfo = identifierInfo[0];
						}
					}
					else if (_renamingSignature != null && _renamingSignature.POUType == Operator.Program && accinfo.Access == AccessFlag.Call)
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

		private IList<ICrossReferenceNode> PerformRegExCollection(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid scope)
		{
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return (IList<ICrossReferenceNode>)val;
			}
			if (occurence.HasFlag(CrossRefOccurence.Virtual) || occurence.HasFlag(CrossRefOccurence.GeneratedInDownload))
			{
				foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in APEnvironmentFacade.Instance.AdditionalCrossReferenceProviders)
				{
					val.AddRange(additionalCrossReferenceProvider.GetAdditionalCrossReferences(regex, guidObject, searchType, occurence, matchType) ?? new ICrossReferenceNode[0]);
				}
				LList<ICrossReferenceNode> val2 = new LList<ICrossReferenceNode>();
				foreach (ISimpleAdditionalCrossReferenceProvider simpleAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleAdditionalCrossReferenceProviders)
				{
					simpleAdditionalCrossReferenceProvider.ProvideAdditionalCrossReferences();
					foreach (ICrossReferenceNode item in val2)
					{
						if (regex.IsMatch(item.Name))
						{
							val2.Add(item);
						}
					}
				}
				foreach (ISimpleFilteredAdditionalCrossReferenceProvider simpleFilteredAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleFilteredAdditionalCrossReferenceProviders)
				{
					simpleFilteredAdditionalCrossReferenceProvider.ProvideAdditionalCrossReferences((string s) => true);
					foreach (ICrossReferenceNode item2 in val2)
					{
						if (regex.IsMatch(item2.Name))
						{
							val2.Add(item2);
						}
					}
				}
				if (val2.get_Count() > 0)
				{
					FilterNodes((IList<CrossRefNode>)Enumerable.ToLList<CrossRefNode>(((IEnumerable)val2).Cast<CrossRefNode>()), guidObject, ((IEnumerable<ICrossReferenceNode>)val2).First().Name, scope, searchType);
					val.AddRange((IEnumerable<ICrossReferenceNode>)val2);
				}
			}
			_bCheckShadowing = matchType.HasFlag(CrossReferenceMatchType.Shadowed);
			if (occurence.HasFlag(CrossRefOccurence.LanguageModel))
			{
				IDictionary<string, IList<IAccessInfo>> precompiledCrossReferences = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompiledCrossReferences(regex);
				foreach (string key in precompiledCrossReferences.Keys)
				{
					IList<IAccessInfo> source = precompiledCrossReferences[key];
					LList<CrossRefNode> val3 = CreateCrossReferenceNodes(key, string.Empty, ExtendedAccessInfo.Create(source.Cast<IAccessInfo2>(), CrossRefSearchType.None), guidObject, scope, searchType, matchType);
					val.AddRange((IEnumerable<ICrossReferenceNode>)val3);
				}
				if (regex.ToString().StartsWith("%") && searchType.HasFlag(CrossRefSearchType.Variables))
				{
					AddVarsOnDirectAddresses(regex, guidObject, scope, val, searchType, matchType);
				}
			}
			if (!matchType.HasFlag(CrossReferenceMatchType.Existing))
			{
				val.RemoveAll((Predicate<ICrossReferenceNode>)((ICrossReferenceNode p) => !p.MatchType.HasFlag(CrossReferenceMatchType.Shadowed)));
			}
			if (!matchType.HasFlag(CrossReferenceMatchType.Shadowed))
			{
				val.RemoveAll((Predicate<ICrossReferenceNode>)((ICrossReferenceNode p) => p.MatchType.HasFlag(CrossReferenceMatchType.Shadowed)));
			}
			return (IList<ICrossReferenceNode>)val;
		}

		private void FilteredAdd(LList<ICrossReferenceNode> destination, IEnumerable<ICrossReferenceNode> src, string filterString)
		{
			destination.AddRange(src.Where((ICrossReferenceNode p) => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(filterString, StringComparison.InvariantCultureIgnoreCase)));
		}

		private IList<ICrossReferenceNode> PerformCollection(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid scope)
		{
			LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>();
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject || stOldName == string.Empty)
			{
				return (IList<ICrossReferenceNode>)val;
			}
			string empty = string.Empty;
			string empty2 = string.Empty;
			empty = AdaptInputString(stOldName);
			empty2 = AdaptInputString(stNewName);
			_bCheckShadowing = matchType.HasFlag(CrossReferenceMatchType.Shadowed);
			if (occurence.HasFlag(CrossRefOccurence.Virtual) || occurence.HasFlag(CrossRefOccurence.GeneratedInDownload))
			{
				foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in APEnvironmentFacade.Instance.AdditionalCrossReferenceProviders)
				{
					try
					{
						val.AddRange(additionalCrossReferenceProvider.GetAdditionalCrossReferences(empty, guidObject, searchType, occurence, matchType) ?? new ICrossReferenceNode[0]);
					}
					catch (Exception ex)
					{
						val.Add((ICrossReferenceNode)new CrossRefErrorNode(ex));
					}
				}
				LList<ICrossReferenceNode> val2 = new LList<ICrossReferenceNode>();
				foreach (ISimpleFilteredAdditionalCrossReferenceProvider simpleFilteredAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleFilteredAdditionalCrossReferenceProviders)
				{
					try
					{
						FilteredAdd(val2, CrossRefUtils.RetrieveAdditionalCrossRefs(simpleFilteredAdditionalCrossReferenceProvider, empty), empty);
					}
					catch (Exception ex2)
					{
						val.Add((ICrossReferenceNode)new CrossRefErrorNode(ex2));
					}
				}
				foreach (ISimpleAdditionalCrossReferenceProvider simpleAdditionalCrossReferenceProvider in APEnvironmentFacade.Instance.SimpleAdditionalCrossReferenceProviders)
				{
					try
					{
						FilteredAdd(val2, simpleAdditionalCrossReferenceProvider.ProvideAdditionalCrossReferences(), empty);
					}
					catch (Exception ex3)
					{
						val.Add((ICrossReferenceNode)new CrossRefErrorNode(ex3));
					}
				}
				FilterNodes((IList<CrossRefNode>)Enumerable.ToLList<CrossRefNode>(((IEnumerable)val2).Cast<CrossRefNode>()), guidObject, empty, scope, searchType);
				val.AddRange(((IEnumerable<ICrossReferenceNode>)val2).Where((ICrossReferenceNode p) => p.Visible));
			}
			if (occurence.HasFlag(CrossRefOccurence.LanguageModel))
			{
				IDirectVariable directVariable = ParseDirectAddress(empty);
				if (directVariable != null && searchType.HasFlag(CrossRefSearchType.Variables))
				{
					IAccessInfo[] directVariableAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetDirectVariableAccess(directVariable, bCompiled: false);
					LList<ExtendedAccessInfo> val3 = new LList<ExtendedAccessInfo>();
					val3.AddRange((IEnumerable<ExtendedAccessInfo>)ExtendedAccessInfo.Create(directVariableAccess.Cast<IAccessInfo2>(), CrossRefSearchType.Variables));
					IList<CrossRefNode> list = (IList<CrossRefNode>)CreateCrossReferenceNodes(empty, empty2, (IList<ExtendedAccessInfo>)val3, guidObject, scope, searchType, matchType);
					val.AddRange((IEnumerable<ICrossReferenceNode>)list);
					AddVarsOnDirectAddresses(directVariable, guidObject, scope, val, searchType, occurence, matchType);
				}
				else
				{
					LList<ExtendedAccessInfo> val4 = new LList<ExtendedAccessInfo>();
					LList<string> val5 = ParseInputString(empty);
					if (val5 == null || val5.get_Count() == 0)
					{
						return (IList<ICrossReferenceNode>)val;
					}
					AddVarsToDeclaration(empty, stNewName, val, scope, guidObject);
					if (searchType.HasFlag(CrossRefSearchType.Variables))
					{
						IAccessInfo[] variableAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetVariableAccess(empty, bCompiled: false);
						val4.AddRange((IEnumerable<ExtendedAccessInfo>)ExtendedAccessInfo.Create(variableAccess.Cast<IAccessInfo2>(), CrossRefSearchType.Variables));
					}
					if (searchType.HasFlag(CrossRefSearchType.Signatures))
					{
						IAccessInfo[] pOUAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetPOUAccess(empty, bCompiled: false);
						val4.AddRange((IEnumerable<ExtendedAccessInfo>)ExtendedAccessInfo.Create(pOUAccess.Cast<IAccessInfo2>(), CrossRefSearchType.Signatures));
					}
					IList<CrossRefNode> list2 = (IList<CrossRefNode>)CreateCrossReferenceNodes(empty, empty2, (IList<ExtendedAccessInfo>)val4, guidObject, scope, searchType, matchType);
					val.AddRange((IEnumerable<ICrossReferenceNode>)list2);
				}
			}
			if (!matchType.HasFlag(CrossReferenceMatchType.Existing))
			{
				val.RemoveAll((Predicate<ICrossReferenceNode>)((ICrossReferenceNode p) => !p.MatchType.HasFlag(CrossReferenceMatchType.Shadowed)));
			}
			if (!matchType.HasFlag(CrossReferenceMatchType.Shadowed))
			{
				val.RemoveAll((Predicate<ICrossReferenceNode>)((ICrossReferenceNode p) => p.MatchType.HasFlag(CrossReferenceMatchType.Shadowed)));
			}
			if (matchType.HasFlag(CrossReferenceMatchType.Existing) && val.get_Count() == 0)
			{
				CrossRefNode crossRefNode = null;
				if (APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(guidObject, out var _) is ISignature2 signature && signature.Name.Equals(empty, StringComparison.InvariantCultureIgnoreCase))
				{
					crossRefNode = CreateNodeBySignature(signature, empty, guidObject, scope, AccessFlag.Declarative);
				}
				if (crossRefNode != null)
				{
					val.Add((ICrossReferenceNode)crossRefNode);
				}
			}
			return (IList<ICrossReferenceNode>)Enumerable.ToLList<ICrossReferenceNode>(((IEnumerable<ICrossReferenceNode>)val).Distinct());
		}

		internal CrossRefNode CreateNodeBySignature(ISignature2 signature, string stName, Guid guidObject, Guid applicationGuid, AccessFlag access)
		{
			if (signature != null && !string.IsNullOrEmpty(stName))
			{
				IAccessInfo2 accinfo;
				if (signature.NameExpression.Position != null && signature.NameExpression.Position.ObjectGuid != Guid.Empty)
				{
					accinfo = new CrossReferenceAccessInfo(signature.NameExpression.Position, access, applicationGuid, Guid.Empty);
				}
				else
				{
					if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
					{
						return null;
					}
					accinfo = new CrossReferenceAccessInfo(new CrossReferenceSourcePosition(APEnvironmentFacade.Instance.PrimaryProjectHandle, guidObject, 0L, 0, 0), access, applicationGuid, Guid.Empty);
				}
				return CreateNode(accinfo, stName, CrossRefSearchType.Signatures, CrossReferenceMatchType.Existing, null, null, null, null, applicationGuid);
			}
			return null;
		}

		internal static CrossRefNode CreateAdditionalCrossRefNode(ISourcePosition sourcePosition, string stName, Guid applicationGuid, AccessFlag access, IIdentifierInfo externalIdentifierInfo)
		{
			if (sourcePosition != null && !string.IsNullOrEmpty(stName))
			{
				return CreateNode(new CrossReferenceAccessInfo(sourcePosition, access, applicationGuid, Guid.Empty), stName, CrossRefSearchType.Signatures, CrossReferenceMatchType.Existing, null, null, null, null, applicationGuid, externalIdentifierInfo);
			}
			return null;
		}

		internal static IVariable GetVariable(string stName, Guid guidObject, CrossRefSearchType searchType, Guid guidApplication)
		{
			if (_scope == CrossReferenceScope.Project)
			{
				return null;
			}
			if (searchType.HasFlag(CrossRefSearchType.Signatures) && searchType.HasFlag(CrossRefSearchType.Variables))
			{
				return null;
			}
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject && searchType.HasFlag(CrossRefSearchType.Variables))
			{
				IPreCompileContext preCompileContext = null;
				preCompileContext = ((!(guidObject == Guid.Empty) || _scope != CrossReferenceScope.Application) ? Common.GetPreCompileContext(guidObject, APEnvironmentFacade.Instance.PrimaryProjectHandle) : Common.GetPreCompileContextByApplicationGuid(guidApplication, APEnvironmentFacade.Instance.PrimaryProjectHandle));
				IIdentifierInfo[] identifierInfo = preCompileContext.GetIdentifierInfo(guidObject, stName);
				if (identifierInfo != null && identifierInfo.Count() == 1)
				{
					if (identifierInfo[0].Variable != null && identifierInfo[0].Variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
					{
						return null;
					}
					if (identifierInfo[0].Signature != null && identifierInfo[0].Signature.POUType == Operator.Method)
					{
						return null;
					}
					if (identifierInfo[0].Variable != null && identifierInfo[0].Signature != null && identifierInfo[0].Variable.OrgName == identifierInfo[0].Signature.OrgName && searchType.HasFlag(CrossRefSearchType.Signatures))
					{
						return null;
					}
					return identifierInfo[0].Variable;
				}
				return null;
			}
			return null;
		}

		private IList<ICrossReferenceNode> Perform(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid scope)
		{
			_sourceVar = GetVariable(stOldName, guidObject, searchType, scope);
			if (searchType.HasFlag(CrossRefSearchType.Signatures) && !searchType.HasFlag(CrossRefSearchType.Variables) && guidObject != Guid.Empty)
			{
				_renamingSignature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(guidObject, out var _);
			}
			else
			{
				_renamingSignature = null;
			}
			searchType |= CrossRefSearchType.Objects;
			IList<ICrossReferenceNode> list = PerformCollection(stOldName, stNewName, guidObject, searchType, occurence, matchType, scope);
			_sourceVar = null;
			if (matchType.HasFlag(CrossReferenceMatchType.Shadowed))
			{
				_bExplicitShadowing = true;
				LList<ICrossReferenceNode> val = new LList<ICrossReferenceNode>((IEnumerable<ICrossReferenceNode>)PerformCollection(stNewName, stNewName, guidObject, searchType, occurence, CrossReferenceMatchType.Shadowed, scope));
				((LList<ICrossReferenceNode>)list).AddRange((IEnumerable<ICrossReferenceNode>)val);
			}
			_bExplicitShadowing = false;
			return list;
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProjectAndSourceLibraries(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			_scope = CrossReferenceScope.ProjectAndSourceLibs;
			return Perform(stOldName, stNewName, guidObject, searchType, occurence, matchType, Guid.Empty);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProject(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			_scope = CrossReferenceScope.Project;
			return Perform(stOldName, stNewName, guidObject, searchType, occurence, matchType, Guid.Empty);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication)
		{
			_scope = CrossReferenceScope.Application;
			return Perform(stOldName, stNewName, guidObject, searchType, occurence, matchType, guidApplication);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinSignature(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature)
		{
			_scope = CrossReferenceScope.Signature;
			return Perform(stOldName, stNewName, guidObject, searchType, occurence, matchType, guidSignature);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceForVariable(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature)
		{
			_scope = CrossReferenceScope.Variable;
			return Perform(stOldName, stNewName, guidObject, searchType, occurence, matchType, guidSignature);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(ISignature signature, IVariable variable, string stNewName, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication)
		{
			_scope = CrossReferenceScope.Application;
			return ((PrecompileCrossReferenceService)APEnvironmentFacade.Instance.PrecompileCrossReferenceService).Perform(signature, variable, stNewName, searchType, occurence, matchType, guidApplication);
		}

		private void FilterNodes(IList<CrossRefNode> alNodes, Guid guidObject, string stSymbol, Guid guidScope, CrossRefSearchType searchType)
		{
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return;
			}
			bool flag = _scope == CrossReferenceScope.Project || _scope == CrossReferenceScope.ProjectAndSourceLibs;
			IPreCompileContext precom = null;
			if (!flag)
			{
				precom = Common.GetPreCompileContext(guidObject, APEnvironmentFacade.Instance.PrimaryProjectHandle);
			}
			IIdentifierInfo[] array = null;
			if (!flag && !string.IsNullOrEmpty(stSymbol))
			{
				array = precom.GetIdentifierInfo((guidScope == Guid.Empty) ? guidObject : guidScope, stSymbol);
			}
			Guid guid = Guid.Empty;
			if (!flag)
			{
				guid = APEnvironmentFacade.Instance.GetDeviceObjectGuidTopLevel(APEnvironmentFacade.Instance.PrimaryProjectHandle, guidObject);
			}
			foreach (CrossRefNode alNode in alNodes)
			{
				alNode.Visible = true;
				bool flag2 = false;
				if (searchType.HasFlag(CrossRefSearchType.Variables) && _sourceVar != null && alNode.Variable != null && alNode.Variable != _sourceVar && !alNode.Variable.OrgName.ToString().Contains("ImpVar_", StringComparison.OrdinalIgnoreCase))
				{
					flag2 = true;
				}
				if (!_bExplicitShadowing)
				{
					ISignature signature = ((_renamingSignature != null && _renamingSignature.ParentObjectGuid != Guid.Empty) ? APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(_renamingSignature.ParentObjectGuid, out precom) : null);
					ISignature signature2 = ((alNode.IdentifierInfo != null && alNode.IdentifierInfo.Signature != null && alNode.IdentifierInfo.Signature.ParentObjectGuid != Guid.Empty) ? APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(alNode.IdentifierInfo.Signature.ParentObjectGuid, out precom) : null);
					if (_renamingSignature != null && alNode.IdentifierInfo != null && alNode.IdentifierInfo.Signature != null)
					{
						if (signature != null && signature2 == null)
						{
							if (signature != alNode.IdentifierInfo.Signature)
							{
								flag2 = true;
							}
						}
						else if (signature2 != null && signature == null)
						{
							if (signature2 != _renamingSignature)
							{
								flag2 = true;
							}
						}
						else if (_renamingSignature != alNode.IdentifierInfo.Signature)
						{
							flag2 = true;
						}
					}
					if (alNode.AccessInfo.Access == AccessFlag.Declarative && _renamingSignature != null && !alNode.Variable.Type.ToString().Equals(_renamingSignature.OrgName, StringComparison.InvariantCultureIgnoreCase))
					{
						flag2 = true;
					}
					if (_renamingSignature == null && _sourceVar != null && alNode.Variable == null && alNode.IdentifierInfo != null && alNode.IdentifierInfo.Signature != null && alNode.IdentifierInfo.Signature.OrgName.Equals(stSymbol, StringComparison.OrdinalIgnoreCase))
					{
						flag2 = true;
					}
				}
				if (guid != Guid.Empty && !flag2)
				{
					IAccessInfo accessInfo = alNode.AccessInfo;
					if (accessInfo != null && accessInfo is IAccessInfo2)
					{
						IAccessInfo2 accessInfo2 = accessInfo as IAccessInfo2;
						if (accessInfo2.ApplicationGuid != Guid.Empty)
						{
							Guid deviceObjectGuidTopLevel = APEnvironmentFacade.Instance.GetDeviceObjectGuidTopLevel(APEnvironmentFacade.Instance.PrimaryProjectHandle, accessInfo2.ApplicationGuid);
							if (deviceObjectGuidTopLevel != Guid.Empty && deviceObjectGuidTopLevel != guid)
							{
								flag2 = true;
							}
						}
					}
				}
				if (!flag && !flag2)
				{
					if (alNode.AccessInfo == null || array == null || array.Length == 0 || array[0].Signature == null)
					{
						string nameText = alNode.NameText;
						if (string.IsNullOrEmpty(nameText) || !nameText.StartsWith("%"))
						{
							continue;
						}
						if (_scope == CrossReferenceScope.Application)
						{
							if (alNode.ApplicationGuid != guidScope)
							{
								flag2 = true;
							}
						}
						else if (alNode.ObjectGuid != Guid.Empty && alNode.ObjectGuid != guidObject)
						{
							flag2 = true;
						}
					}
					else if (alNode.AccessInfo.Access == AccessFlag.Declarative)
					{
						if (guidScope != Guid.Empty)
						{
							if (_scope == CrossReferenceScope.Signature && alNode.AccessInfo.Position.ObjectGuid != guidScope)
							{
								if (alNode.PositionGuid == Guid.Empty || alNode.PositionGuid != guidScope)
								{
									flag2 = true;
								}
							}
							else if (_scope == CrossReferenceScope.Application && alNode.ApplicationGuid != guidScope)
							{
								flag2 = true;
							}
						}
						else if (guidObject != Guid.Empty && alNode.ObjectGuid != guidObject)
						{
							flag2 = true;
						}
					}
					else if (guidScope != Guid.Empty)
					{
						if (_scope == CrossReferenceScope.Signature && guidScope != alNode.ObjectGuid)
						{
							flag2 = true;
						}
						else if (_scope == CrossReferenceScope.Application)
						{
							if (alNode.AccessInfo is IAccessInfo2)
							{
								if (guidScope != (alNode.AccessInfo as IAccessInfo2).ApplicationGuid && !APEnvironmentFacade.Instance.AccessInChildApplication(guidScope, (alNode.AccessInfo as IAccessInfo2).ApplicationGuid))
								{
									flag2 = true;
								}
							}
							else if (guidScope != alNode.MetaObject.ParentObjectGuid)
							{
								flag2 = true;
							}
						}
					}
					else
					{
						IIdentifierInfo identifierInfoAtPosition = Common.GetIdentifierInfoAtPosition(alNode.SourcePosition, stSymbol);
						if (identifierInfoAtPosition != null)
						{
							if (identifierInfoAtPosition.Signature == null)
							{
								if (guidObject != Guid.Empty && alNode.ObjectGuid != guidObject && (_scope == CrossReferenceScope.Application || _scope == CrossReferenceScope.Signature))
								{
									flag2 = true;
								}
							}
							else if (identifierInfoAtPosition.Signature == null || (identifierInfoAtPosition.Signature.ObjectGuid != guidObject && !_bCheckShadowing && (_scope == CrossReferenceScope.Application || _scope == CrossReferenceScope.Signature)))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					alNode.Visible = false;
					alNode.MatchType &= ~(CrossReferenceMatchType.Existing | CrossReferenceMatchType.Shadowed);
					alNode.MatchType |= CrossReferenceMatchType.Unrelated;
				}
			}
		}

		internal IDirectVariable ParseDirectAddress(string st)
		{
			s_scanner.Initialize(st);
			IExpression expression = s_parser.ParseOperand();
			if (expression != null && expression is IAddressExpression)
			{
				return (expression as IAddressExpression).DirectAddress;
			}
			return null;
		}

		private bool CheckInput(string st)
		{
			if (st.Length > 300)
			{
				return false;
			}
			st = st.Trim();
			s_scanner.Initialize(st);
			int num = 0;
			IToken token;
			while (s_scanner.GetNext(out token) != TokenType.End)
			{
				if (token.Type == TokenType.Operator)
				{
					Operator @operator = s_scanner.GetOperator(token);
					switch (@operator)
					{
					case Operator.LeftBracket:
						num++;
						continue;
					case Operator.RightBracket:
						num--;
						continue;
					}
					if (num == 0 && @operator != Operator.Period && @operator != Operator.DeRef)
					{
						return false;
					}
				}
				else if (num == 0 && token.Type != TokenType.Identifier)
				{
					return false;
				}
			}
			return true;
		}

		private LList<string> ParseInputString(string st)
		{
			LList<string> val = new LList<string>();
			s_scanner.AllowMultipleUnderlines = true;
			s_scanner.Initialize(st);
			int num = 0;
			IToken token;
			while (s_scanner.GetNext(out token) != TokenType.End)
			{
				if (token.Type == TokenType.Operator)
				{
					switch (s_scanner.GetOperator(token))
					{
					case Operator.LeftBracket:
						num++;
						continue;
					case Operator.RightBracket:
						num--;
						continue;
					}
				}
				if (num == 0 && token.Type == TokenType.Identifier)
				{
					val.Add(s_scanner.GetIdentifier(token));
				}
			}
			return val;
		}

		private string AdaptInputString(string st)
		{
			int num = st.LastIndexOf('.');
			if (num <= 0 || st.StartsWith("%"))
			{
				return st;
			}
			if (uint.TryParse(st.Substring(num).TrimStart('.'), out var _))
			{
				st = st.Remove(num);
			}
			return st;
		}

		private static CrossRefNode CreateNode(IAccessInfo accinfo, string stName, CrossRefSearchType searchType, CrossReferenceMatchType matchType, IIdentifierInfo shadowingIdentifierInfo, ISourcePosition sourcePos, string stOldName, string stNewName, Guid guidScope, IIdentifierInfo externalIdentifierInfo = null)
		{
			if (accinfo == null || accinfo.Position == null)
			{
				return null;
			}
			IPreCompileContext precom;
			ISignature signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(accinfo.Position.ObjectGuid, out precom);
			if (signature != null && VisibilityUtil.IsHiddenSignature(signature, accinfo, GUIHidingFlags.SignatureGenerated | GUIHidingFlags.EvaluateFlags | GUIHidingFlags.EvaluateAttributes) && !signature.HasAttribute(CompileAttributes.ATTRIBUTE_IOCONFIG_POU) && !signature.OrgName.StartsWith("__get", StringComparison.OrdinalIgnoreCase) && !signature.OrgName.StartsWith("__set", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			if ((_scope == CrossReferenceScope.ProjectAndSourceLibs || _scope == CrossReferenceScope.Signature || accinfo.Position.ProjectHandle == APEnvironmentFacade.Instance.PrimaryProjectHandle) && accinfo.Position.ObjectGuid != Guid.Empty && accinfo.Access != AccessFlag.Unknown && accinfo.Access != AccessFlag.Implicit)
			{
				if (accinfo.Access == AccessFlag.Declarative && signature != null)
				{
					IVariable[] all = signature.All;
					foreach (IVariable variable in all)
					{
						if (variable.Name == stName.ToUpperInvariant() && (variable.GetFlag(VarFlag.Implicit) || variable.HasAttribute("hide")))
						{
							return null;
						}
					}
				}
				else if (accinfo.Access == AccessFlag.Type && signature != null)
				{
					IVariable[] all = signature.All;
					foreach (IVariable variable2 in all)
					{
						if (variable2.Type.ToString() == stName.ToUpperInvariant() && (variable2.GetFlag(VarFlag.Implicit) || variable2.HasAttribute("hide")))
						{
							return null;
						}
					}
				}
				if (APEnvironmentFacade.Instance.ExistsObject(accinfo.Position.ProjectHandle, accinfo.Position.ObjectGuid) || (accinfo is IAccessInfo2 && (accinfo as IAccessInfo2).MessageGuid != Guid.Empty && APEnvironmentFacade.Instance.ExistsObject(accinfo.Position.ProjectHandle, (accinfo as IAccessInfo2).MessageGuid)))
				{
					return new CrossRefNode(stName, accinfo, searchType, CrossRefOccurence.LanguageModel, matchType, shadowingIdentifierInfo, sourcePos, stNewName, null, null, null, externalIdentifierInfo);
				}
			}
			return null;
		}

		private void AddVarsToDeclaration(string stName, string stNewName, LList<ICrossReferenceNode> alNodes, Guid guidScope, Guid guidObject)
		{
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			IList<ISignature4> list = null;
			if (_scope == CrossReferenceScope.Application && guidScope != Guid.Empty)
			{
				list = ((IPreCompileContext10)APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidScope)).GetAllSignaturesFlat();
			}
			else
			{
				LList<ISignature4> val = new LList<ISignature4>();
				IPreCompileContext[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: true, bWithLibraries: true);
				foreach (IPreCompileContext preCompileContext in array)
				{
					val.AddRange((IEnumerable<ISignature4>)((IPreCompileContext10)preCompileContext).GetAllSignaturesFlat());
				}
				list = (IList<ISignature4>)val;
			}
			if (list == null)
			{
				return;
			}
			LDictionary<string, IVariable> val2 = new LDictionary<string, IVariable>();
			for (int j = 0; j < list.Count; j++)
			{
				ISignature4 signature = list[j];
				if (signature != null && _sourceVar == null)
				{
					if (signature.BaseExpression != null && signature.BaseExpression.ToString().Equals(stName, StringComparison.OrdinalIgnoreCase))
					{
						CreateAndAddNodeFromExpression(signature.BaseExpression, stName, alNodes, guidScope, signature);
					}
					if (signature.InterfaceExpressions != null)
					{
						IExpression[] interfaceExpressions = signature.InterfaceExpressions;
						foreach (IExpression expression in interfaceExpressions)
						{
							if (expression.ToString().Equals(stName, StringComparison.OrdinalIgnoreCase))
							{
								CreateAndAddNodeFromExpression(expression, stName, alNodes, guidScope, signature);
							}
						}
					}
					if (signature.GetFlag(SignatureFlag.Alias) && signature.All.Length != 0 && signature.All[0].Type is IUserdefType2 && signature.All[0].Type.ToString().Equals(stName, StringComparison.OrdinalIgnoreCase))
					{
						IExpression nameExpression = (signature.All[0].Type as IUserdefType2).NameExpression;
						CreateAndAddNodeFromExpression(nameExpression, stName, alNodes, guidScope, signature);
					}
					if ((signature.POUType == Operator.Function || signature.POUType == Operator.Method) && signature.Outputs.Length != 0 && signature.Outputs[0].Type is IUserdefType2 && signature.Outputs[0].Type.ToString().Equals(stName, StringComparison.OrdinalIgnoreCase))
					{
						IExpression nameExpression2 = (signature.Outputs[0].Type as IUserdefType2).NameExpression;
						CreateAndAddNodeFromExpression(nameExpression2, stName, alNodes, guidScope, signature);
					}
				}
				IVariable[] all = signature.All;
				foreach (IVariable variable in all)
				{
					if (!variable.GetFlag(VarFlag.Implicit))
					{
						IType type = variable.Type;
						if (type != null && IsFromUserDefType(type, stName, 0) && !val2.ContainsKey(variable.Name))
						{
							val2.Add(variable.Name, variable);
						}
					}
				}
			}
			LDictionary<Guid, LHashSet<long>> processedPositions = new LDictionary<Guid, LHashSet<long>>();
			Enumerator<string, IVariable> enumerator = val2.get_Values().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IVariable current = enumerator.get_Current();
					IAccessInfo[] variableAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetVariableAccess(current.OrgName, bCompiled: false);
					for (int l = 0; l < variableAccess.Length; l++)
					{
						ISourcePosition sourcePos = null;
						if (current.Type is IUserdefType2 && variableAccess[l].Access == AccessFlag.Declarative && guidScope == Guid.Empty && _scope == CrossReferenceScope.Application)
						{
							sourcePos = ((IUserdefType2)current.Type).NameExpression.Position;
						}
						else if (current.Type is IEnumType && variableAccess[l].Access == AccessFlag.Read && variableAccess[l].Position.PositionOffset - current.Type.ToString().Length + 1 > 0)
						{
							sourcePos = CreateSourcePosition(APEnvironmentFacade.Instance.PrimaryProjectHandle, variableAccess[l].Position.ObjectGuid, variableAccess[l].Position.Position, (short)(variableAccess[l].Position.PositionOffset - (current.Type.ToString().Length + 1)), (short)current.Type.ToString().Length);
						}
						CrossRefNode crossRefNode = CreateNode(variableAccess[l], current.OrgName, CrossRefSearchType.Variables, CrossReferenceMatchType.Existing, null, sourcePos, stName, stNewName, guidScope);
						if (crossRefNode != null && crossRefNode != null && NodeIsInSearchScope(crossRefNode, guidScope, guidObject) && !IsAlreadyProcessed(processedPositions, crossRefNode.ObjectGuid, crossRefNode.AccessInfo.Position.PositionCombination))
						{
							alNodes.Add((ICrossReferenceNode)crossRefNode);
							AddPosition(processedPositions, crossRefNode.ObjectGuid, crossRefNode.AccessInfo.Position.PositionCombination);
						}
					}
					IAccessInfo[] pOUAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetPOUAccess(current.OrgName, bCompiled: false);
					for (int m = 0; m < pOUAccess.Length; m++)
					{
						ISourcePosition sourcePos2 = null;
						if (current.Type is IUserdefType2 && pOUAccess[m].Access == AccessFlag.Declarative && guidScope == Guid.Empty && _scope == CrossReferenceScope.Application)
						{
							sourcePos2 = ((IUserdefType2)current.Type).NameExpression.Position;
						}
						CrossRefNode crossRefNode2 = CreateNode(pOUAccess[m], current.OrgName, CrossRefSearchType.Signatures, CrossReferenceMatchType.Existing, null, sourcePos2, stName, stNewName, guidScope);
						if (crossRefNode2 != null && crossRefNode2 != null && NodeIsInSearchScope(crossRefNode2, guidScope, guidObject) && !IsAlreadyProcessed(processedPositions, crossRefNode2.ObjectGuid, crossRefNode2.AccessInfo.Position.PositionCombination))
						{
							alNodes.Add((ICrossReferenceNode)crossRefNode2);
							AddPosition(processedPositions, crossRefNode2.ObjectGuid, crossRefNode2.AccessInfo.Position.PositionCombination);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}

		private bool IsAlreadyProcessed(LDictionary<Guid, LHashSet<long>> processedPositions, Guid guidObject, long nPositionCombination)
		{
			LHashSet<long> val = default(LHashSet<long>);
			if (processedPositions.TryGetValue(guidObject, ref val))
			{
				return val.Contains(nPositionCombination);
			}
			return false;
		}

		private void AddPosition(LDictionary<Guid, LHashSet<long>> processedPositions, Guid guidObject, long nPositionCombination)
		{
			LHashSet<long> val = default(LHashSet<long>);
			if (processedPositions.TryGetValue(guidObject, ref val))
			{
				val.Add(nPositionCombination);
				return;
			}
			LHashSet<long> obj = new LHashSet<long>();
			obj.Add(nPositionCombination);
			processedPositions.Add(guidObject, obj);
		}

		private void CreateAndAddNodeFromExpression(IExpression exp, string stName, LList<ICrossReferenceNode> alNodes, Guid guidScope, ISignature2 signature)
		{
			ISourcePosition sourcePos = CreateSourcePosition(APEnvironmentFacade.Instance.PrimaryProjectHandle, signature.ObjectGuid, exp.Position.Position, exp.Position.PositionOffset, exp.Position.Length);
			CrossRefNode crossRefNode = CreateNode(new MyAccessInfo((_scope == CrossReferenceScope.Application) ? guidScope : Guid.Empty, signature.MessageGuid, sourcePos), stName, CrossRefSearchType.Variables, CrossReferenceMatchType.Existing, null, null, null, null, guidScope);
			if (crossRefNode != null)
			{
				alNodes.Add((ICrossReferenceNode)crossRefNode);
			}
		}

		private bool NodeIsInSearchScope(CrossRefNode node, Guid guidScope, Guid guidObject)
		{
			if (guidScope != Guid.Empty)
			{
				if (_scope == CrossReferenceScope.Signature && node.AccessInfo.Position.ObjectGuid != guidScope)
				{
					if (node.PositionGuid == Guid.Empty || node.PositionGuid != guidScope)
					{
						return false;
					}
				}
				else if (_scope == CrossReferenceScope.Application && node.ApplicationGuid != guidScope && !APEnvironmentFacade.Instance.AccessInChildApplication(guidScope, node.ApplicationGuid))
				{
					return false;
				}
			}
			else
			{
				if (guidScope == Guid.Empty && _scope == CrossReferenceScope.Application && node.AccessInfo is IAccessInfo2 && ((IAccessInfo2)node.AccessInfo).ApplicationGuid == Guid.Empty)
				{
					return true;
				}
				if (!(guidScope == Guid.Empty) || _scope != CrossReferenceScope.Application || !(node.AccessInfo is IAccessInfo2) || node.Variable == null)
				{
					if (guidObject != Guid.Empty && node.ObjectGuid != guidObject && _sourceVar != null)
					{
						return false;
					}
					if (guidObject != Guid.Empty && node.ObjectGuid != guidObject)
					{
						_ = _renamingSignature;
						return true;
					}
					return true;
				}
				if (SignatureBelongsToDevice(node.Signature))
				{
					return true;
				}
				ISignature[] array = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(((IAccessInfo2)node.AccessInfo).ApplicationGuid).FindSignature(node.Variable.Type.ToString());
				if (array != null && array.Length != 0)
				{
					ISignature[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].ObjectGuid != guidObject)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool SignatureBelongsToDevice(ISignature signature)
		{
			if (signature == null)
			{
				return false;
			}
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.ExistsObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, signature.MessageGuid))
			{
				return SignatureBelongsToDevice(signature.MessageGuid, APEnvironmentFacade.Instance.PrimaryProjectHandle);
			}
			if (APEnvironmentFacade.Instance.ExistsObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, signature.ObjectGuid))
			{
				return SignatureBelongsToDevice(signature.ObjectGuid, APEnvironmentFacade.Instance.PrimaryProjectHandle);
			}
			return false;
		}

		private bool SignatureBelongsToDevice(Guid guidWalk, int iProjectHandle)
		{
			return Guid.Empty != APEnvironmentFacade.Instance.GetDeviceObjectGuid(iProjectHandle, guidWalk);
		}

		private bool IsFromUserDefType(IType type, string stType, int iDepth)
		{
			if (iDepth > 5 || type == null)
			{
				return false;
			}
			iDepth++;
			if (type.Class == TypeClass.Array)
			{
				IArrayType arrayType = type as IArrayType;
				return IsFromUserDefType(arrayType.Base, stType, iDepth);
			}
			if (type.Class == TypeClass.Pointer)
			{
				IPointerType pointerType = type as IPointerType;
				return IsFromUserDefType(pointerType.Base, stType, iDepth);
			}
			if (type.Class == TypeClass.Reference)
			{
				IReferenceType referenceType = type as IReferenceType;
				return IsFromUserDefType(referenceType.Base, stType, iDepth);
			}
			if (type.Class == TypeClass.Userdef || type.Class == TypeClass.Enum)
			{
				if (type.ToString().ToUpperInvariant() == stType.ToUpperInvariant())
				{
					return true;
				}
				return false;
			}
			return false;
		}

		private void AddVarsOnDirectAddresses(IDirectVariable dirVarPrm, Guid guidObject, Guid guidApplication, LList<ICrossReferenceNode> alNodes, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			ISignature[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPrecompiledSignatures(bWithLibraries: true, bWithResources: true);
			if (array == null)
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < array.Length; i++)
			{
				IVariable[] all = array[i].All;
				foreach (IVariable variable in all)
				{
					if (!variable.GetFlag(VarFlag.Implicit))
					{
						IDirectVariable address = variable.Address;
						if (address != null && address.IsEqual(dirVarPrm) && !hashSet.Contains(variable.Name))
						{
							hashSet.Add(variable.Name);
							IAccessInfo[] variableAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetVariableAccess(variable.OrgName, bCompiled: false);
							LList<ExtendedAccessInfo> val = new LList<ExtendedAccessInfo>();
							val.AddRange((IEnumerable<ExtendedAccessInfo>)ExtendedAccessInfo.Create(variableAccess.Cast<IAccessInfo2>(), CrossRefSearchType.Variables));
							alNodes.AddRange((IEnumerable<ICrossReferenceNode>)CreateCrossReferenceNodes(variable.OrgName, string.Empty, (IList<ExtendedAccessInfo>)val, guidObject, guidApplication, searchType, matchType));
						}
					}
				}
			}
		}

		private void AddVarsOnDirectAddresses(Regex regex, Guid guidObject, Guid guidApplication, LList<ICrossReferenceNode> alNodes, CrossRefSearchType searchType, CrossReferenceMatchType matchType)
		{
			ISignature[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPrecompiledSignatures(bWithLibraries: true, bWithResources: true);
			if (array == null)
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < array.Length; i++)
			{
				IVariable[] all = array[i].All;
				foreach (IVariable variable in all)
				{
					if (!variable.GetFlag(VarFlag.Implicit))
					{
						IDirectVariable address = variable.Address;
						if (address != null && regex.Match(address.ToString()).Success && !hashSet.Contains(variable.Name))
						{
							hashSet.Add(variable.Name);
							IAccessInfo[] variableAccess = APEnvironmentFacade.Instance.LanguageModelMgr.GetVariableAccess(variable.OrgName, bCompiled: false);
							LList<ExtendedAccessInfo> val = new LList<ExtendedAccessInfo>();
							val.AddRange((IEnumerable<ExtendedAccessInfo>)ExtendedAccessInfo.Create(variableAccess.Cast<IAccessInfo2>(), CrossRefSearchType.Variables));
							alNodes.AddRange((IEnumerable<ICrossReferenceNode>)CreateCrossReferenceNodes(variable.OrgName, string.Empty, (IList<ExtendedAccessInfo>)val, guidObject, guidApplication, searchType, matchType));
						}
					}
				}
			}
		}

		private ICrossReferenceNode CreateNode(string stName, IAccessInfo accessInfo, CrossRefSearchType searchType, CrossReferenceMatchType matchType, IIdentifierInfo identifierInfo = null, ISourcePosition sourcePosition = null, string stNewName = null)
		{
			return new CrossRefNode(stName, accessInfo, searchType, CrossRefOccurence.LanguageModel, matchType, identifierInfo, sourcePosition, stNewName, null, null, null);
		}

		ICrossReferenceNode ICrossReferenceService.CreateNode(string stName, IAccessInfo accessInfo, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			if (occurence.HasFlag(CrossRefOccurence.LanguageModel))
			{
				throw new ArgumentException("LanguageModel flag not allowed", "occurence");
			}
			if (occurence == CrossRefOccurence.None || !Enum.IsDefined(typeof(CrossRefOccurence), occurence))
			{
				throw new ArgumentException("Exactly one CrossRefOccurence flag must be given.", "occurence");
			}
			if (searchType == CrossRefSearchType.None || !Enum.IsDefined(typeof(CrossRefSearchType), searchType))
			{
				throw new ArgumentException("Exactly one CrossRefSearchType flag must be given", "searchType");
			}
			if (matchType == CrossReferenceMatchType.None || !Enum.IsDefined(typeof(CrossReferenceMatchType), matchType))
			{
				throw new ArgumentException("Exactly one CrossReferenceMatchType flag must be given", "matchType");
			}
			return new CrossRefNode(stName, accessInfo, searchType, occurence, matchType, null, null, null, null, null, null);
		}

		ICrossReferenceNode ICrossReferenceService2.CreateNode(string stName, IAccessInfo accessInfo, IVariable variable, ISignature signature, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			if (occurence.HasFlag(CrossRefOccurence.LanguageModel))
			{
				throw new ArgumentException("LanguageModel flag not allowed", "occurence");
			}
			if (occurence == CrossRefOccurence.None || !Enum.IsDefined(typeof(CrossRefOccurence), occurence))
			{
				throw new ArgumentException("Exactly one CrossRefOccurence flag must be given.", "occurence");
			}
			if (searchType == CrossRefSearchType.None || !Enum.IsDefined(typeof(CrossRefSearchType), searchType))
			{
				throw new ArgumentException("Exactly one CrossRefSearchType flag must be given", "searchType");
			}
			if (matchType == CrossReferenceMatchType.None || !Enum.IsDefined(typeof(CrossReferenceMatchType), matchType))
			{
				throw new ArgumentException("Exactly one CrossReferenceMatchType flag must be given", "matchType");
			}
			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}
			return new CrossRefNode(stName, accessInfo, searchType, occurence, matchType, null, null, null, variable, signature, null);
		}

		ICrossReferenceNode ICrossReferenceService5.CreateNode(string stName, IAccessInfo accessInfo, IVariable variable, ISignature signature, IExprement expressionAtSourcePosition, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			if (occurence.HasFlag(CrossRefOccurence.LanguageModel))
			{
				throw new ArgumentException("LanguageModel flag not allowed", "occurence");
			}
			if (occurence == CrossRefOccurence.None || !Enum.IsDefined(typeof(CrossRefOccurence), occurence))
			{
				throw new ArgumentException("Exactly one CrossRefOccurence flag must be given.", "occurence");
			}
			if (searchType == CrossRefSearchType.None || !Enum.IsDefined(typeof(CrossRefSearchType), searchType))
			{
				throw new ArgumentException("Exactly one CrossRefSearchType flag must be given", "searchType");
			}
			if (matchType == CrossReferenceMatchType.None || !Enum.IsDefined(typeof(CrossReferenceMatchType), matchType))
			{
				throw new ArgumentException("Exactly one CrossReferenceMatchType flag must be given", "matchType");
			}
			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}
			return new CrossRefNode(stName, accessInfo, searchType, occurence, matchType, null, null, null, variable, signature, expressionAtSourcePosition);
		}

		public IAccessInfo2 CreateAccessInfo(ISourcePosition position, AccessFlag access, Guid applicationGuid, Guid messageGuid)
		{
			return new CrossReferenceAccessInfo(position, access, applicationGuid, messageGuid);
		}

		public ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long position, short? offset, short length)
		{
			return new CrossReferenceSourcePosition(nProjectHandle, objectGuid, position, offset, length);
		}

		public IList<IRelatedSignature> GetRelatedSignatures(ISignature signature, SignatureRelationFlags flags)
		{
			return new RelatedSignatureFinder(signature, null, flags).FindResult();
		}

		public IList<IRelatedSignature> GetRelatedSignatures(ISignature signature, ISignature subSignature, SignatureRelationFlags flags)
		{
			return new RelatedSignatureFinder(signature, subSignature, flags).FindResult();
		}

		IIdentifierInfo2 ICrossReferenceService2.CreateIdentifierInfo(ISignature containingSignature, string stName, string stComment, IdentifierInfoFlag flags, IType type, IVariable variable, ISignature signature, IScope scope)
		{
			return new CrossReferenceIdentifierInfo(containingSignature, stName, stComment, flags, type, variable, signature, scope);
		}

		internal static IIdentifierInfo2 CreateIdentifierInfo(ISignature containingSignature, string stName, string stComment, IdentifierInfoFlag flags, IType type, IVariable variable, ISignature signature, IScope scope)
		{
			return new CrossReferenceIdentifierInfo(containingSignature, stName, stComment, flags, type, variable, signature, scope);
		}

		private IList<ICrossReferenceNode> PerformRegEx(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid scope)
		{
			IList<ICrossReferenceNode> list = PerformRegExCollection(regex, guidObject, searchType, occurence, matchType & ~CrossReferenceMatchType.Shadowed, scope);
			if (matchType.HasFlag(CrossReferenceMatchType.Shadowed))
			{
				((LList<ICrossReferenceNode>)list).AddRange((IEnumerable<ICrossReferenceNode>)PerformRegExCollection(regex, guidObject, searchType, occurence, CrossReferenceMatchType.Shadowed, scope));
			}
			return list;
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProjectAndSourceLibraries(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			_scope = CrossReferenceScope.ProjectAndSourceLibs;
			return PerformRegEx(regex, guidObject, searchType, occurence, matchType, Guid.Empty);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProject(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType)
		{
			_scope = CrossReferenceScope.Project;
			return PerformRegEx(regex, guidObject, searchType, occurence, matchType, Guid.Empty);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication)
		{
			_scope = CrossReferenceScope.Application;
			return PerformRegEx(regex, guidObject, searchType, occurence, matchType, guidApplication);
		}

		public IList<ICrossReferenceNode> GetCrossReferenceNodesWithinSignature(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature)
		{
			_scope = CrossReferenceScope.Signature;
			return PerformRegEx(regex, guidObject, searchType, occurence, matchType, guidSignature);
		}

		public string GetQualifiedNameOfSignature(ISignature signature)
		{
			return GetQualifiedNameOfSignatureInternal(signature);
		}

		internal static string GetQualifiedNameOfSignatureInternal(ISignature signature)
		{
			StringBuilder stringBuilder = new StringBuilder();
			IPreCompileContext2 precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature);
			if (precompileContextOfSignature != null && !string.IsNullOrWhiteSpace(precompileContextOfSignature.Namespace))
			{
				stringBuilder.Append(precompileContextOfSignature.Namespace);
				stringBuilder.Append('.');
			}
			AddParentsRecursively(stringBuilder, precompileContextOfSignature, signature);
			stringBuilder.Append(signature.OrgName);
			return stringBuilder.ToString();
		}

		private static void AddParentsRecursively(StringBuilder sb, IPreCompileContext2 cc, ISignature signature)
		{
			Guid parentObjectGuid = signature.ParentObjectGuid;
			if (!(parentObjectGuid == Guid.Empty))
			{
				ISignature signature2 = cc.GetSignature(parentObjectGuid);
				if (signature2 != null)
				{
					AddParentsRecursively(sb, cc, signature2);
					sb.Append(signature2.OrgName);
					sb.Append('.');
				}
			}
		}
	}
}
