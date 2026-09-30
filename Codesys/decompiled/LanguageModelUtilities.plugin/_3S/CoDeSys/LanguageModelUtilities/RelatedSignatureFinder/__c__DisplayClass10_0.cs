using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class RelatedSignatureFinder
	{
		private static Dictionary<ISignature2, Set<ISignature2>> s_derivedSignatureCache;

		private static bool s_bCacheHasLibraries;

		private readonly Set<ISignature> _checked = new Set<ISignature>();

		private readonly List<IRelatedSignature> _relatedSignatures = new List<IRelatedSignature>();

		private SignatureRelationFlags _initialFlags;

		private readonly ISignature2 _initialSignature;

		private readonly Predicate<ISignature> _isRelevant;

		private readonly Func<IList<IRelatedSignature>> _action;

		private readonly IPreCompileContext2 _scope;

		private readonly ISignature _subSignature;

		public RelatedSignatureFinder(ISignature signature, ISignature subSignature, SignatureRelationFlags flags)
		{
			RelatedSignatureFinder relatedSignatureFinder = this;
			_initialSignature = (ISignature2)signature;
			_subSignature = subSignature;
			if (flags.HasFlag(SignatureRelationFlags.ScopeProject))
			{
				flags |= SignatureRelationFlags.ScopeApplication;
			}
			else
			{
				_scope = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(_initialSignature);
			}
			_initialFlags = flags;
			switch (signature.POUType)
			{
			case Operator.Action:
			case Operator.Method:
				_action = () => relatedSignatureFinder.GetRelatedMethods(signature.POUType);
				_isRelevant = (ISignature sign) => relatedSignatureFinder.CheckForImplementedMethod(signature.POUType, sign);
				break;
			case Operator.Function:
			case Operator.FunctionBlock:
			case Operator.Program:
			case Operator.VarGlobal:
			case Operator.Interface:
				_action = GetRelatedPous;
				if (_subSignature == null)
				{
					_isRelevant = MatchAll;
				}
				else
				{
					_isRelevant = MatchConsideringSubSignatureName;
				}
				break;
			default:
				throw new InvalidOperationException($"Signature type {signature.POUType} not supported");
			}
		}

		private bool ContainsSubSignature(ISignature sig)
		{
			IEnumerable<ISignature> subSignatureSet = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(sig).GetSubSignatureSet(sig.ObjectGuid);
			if (subSignatureSet != null)
			{
				string orgName = _subSignature.OrgName;
				foreach (ISignature item in subSignatureSet)
				{
					if (orgName.Equals(item.OrgName, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool MatchConsideringSubSignatureName(ISignature sig)
		{
			if (ContainsSubSignature(sig))
			{
				return true;
			}
			ISignature2 signature = sig as ISignature2;
			if (signature?.BaseExpression != null)
			{
				for (ISignature2 signature2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(signature); signature2 != null; signature2 = ((signature2.BaseExpression != null) ? APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(signature2) : null))
				{
					if (ContainsSubSignature(signature2))
					{
						return true;
					}
				}
			}
			if ((_initialFlags & SignatureRelationFlags.OverridingSubclasses) != 0)
			{
				_initialFlags ^= SignatureRelationFlags.OverridingSubclasses;
			}
			return false;
		}

		private static bool MatchAll(ISignature sig)
		{
			return true;
		}

		private static string QualifiedName(ISignature signature)
		{
			if (signature == null)
			{
				return "";
			}
			return CrossReferenceService.GetQualifiedNameOfSignatureInternal(signature);
		}

		private bool CheckForImplementedMethod(Operator subPouType, ISignature relatedSignature)
		{
			ISignature[] subSignatures = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(relatedSignature).GetSubSignatures(relatedSignature.ObjectGuid);
			foreach (ISignature signature in subSignatures)
			{
				if (signature.POUType == subPouType && signature.Name == _initialSignature.Name)
				{
					return true;
				}
			}
			return false;
		}

		private static void LMMOnAfterClearAll(object sender, EventArgs eventArgs)
		{
			s_derivedSignatureCache.Clear();
			s_bCacheHasLibraries = false;
		}

		private static void LMMOnSignatureChanged(object sender, SignatureChangedEventArgs signatureChangedEventArgs)
		{
			s_derivedSignatureCache.Clear();
			s_bCacheHasLibraries = false;
		}

		private static void CheckRebuildCache(bool bNeedLibraries)
		{
			if (s_derivedSignatureCache == null)
			{
				s_derivedSignatureCache = new Dictionary<ISignature2, Set<ISignature2>>();
				APEnvironmentFacade.Instance.LanguageModelMgr.SignatureChanged += LMMOnSignatureChanged;
				APEnvironmentFacade.Instance.LanguageModelMgr.SignatureDeleted += LMMOnSignatureChanged;
				APEnvironmentFacade.Instance.LanguageModelMgr.SignatureInserted += LMMOnSignatureChanged;
				APEnvironmentFacade.Instance.LanguageModelMgr.AfterClearAll += LMMOnAfterClearAll;
			}
			if ((bNeedLibraries && s_bCacheHasLibraries) || (!bNeedLibraries && s_derivedSignatureCache.Count > 0))
			{
				return;
			}
			IEnumerable<IPreCompileContext> enumerable;
			if (s_derivedSignatureCache.Count == 0)
			{
				enumerable = APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileContexts.Concat(new IPreCompileContext[1] { APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty) });
				if (bNeedLibraries)
				{
					enumerable = enumerable.Concat(APEnvironmentFacade.Instance.LanguageModelMgr.LibraryContexts);
				}
			}
			else
			{
				enumerable = APEnvironmentFacade.Instance.LanguageModelMgr.LibraryContexts;
			}
			foreach (IPreCompileContext item in enumerable)
			{
				ISignature2[] array = item.AllSignatures.OfType<ISignature2>().ToArray();
				foreach (ISignature2 signature in array)
				{
					ISignature2 baseSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(signature);
					if (baseSignature != null)
					{
						AddDerivedSignature(baseSignature, signature);
					}
					ISignature2[] interfaceSignatures = APEnvironmentFacade.Instance.LanguageModelMgr.GetInterfaceSignatures(signature);
					for (int j = 0; j < interfaceSignatures.Length; j++)
					{
						AddDerivedSignature(interfaceSignatures[j], signature);
					}
				}
			}
			s_bCacheHasLibraries = bNeedLibraries;
		}

		private static void AddDerivedSignature(ISignature2 baseSignature, ISignature2 signature)
		{
			if (!s_derivedSignatureCache.TryGetValue(baseSignature, out var value))
			{
				value = new Set<ISignature2>();
				s_derivedSignatureCache.Add(baseSignature, value);
			}
			value.Add(signature);
		}

		private IList<IRelatedSignature> GetRelatedPous()
		{
			string stReason = string.Format(Strings.SignatureX0WasOriginallySearchedFor, QualifiedName(_initialSignature));
			AddSignature(_initialSignature, stReason, ESignatureRelationship.InitialSignature);
			return _relatedSignatures;
		}

		private void AddSignature(ISignature2 signature, string stReason, ESignatureRelationship eSignatureRelationship)
		{
			if (signature == null || _checked.Contains((ISignature)signature) || (signature.POUType == Operator.Interface && !_initialFlags.HasFlag(SignatureRelationFlags.ImplementedInterfaces) && !_initialFlags.HasFlag(SignatureRelationFlags.ImplementingInterface)) || (_scope != null && APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature) != _scope && string.IsNullOrEmpty(signature.LibraryPath)) || (!_initialFlags.HasFlag(SignatureRelationFlags.ScopeLibraries) && !string.IsNullOrEmpty(signature.LibraryPath)))
			{
				return;
			}
			_checked.Add((ISignature)signature);
			if (_isRelevant(signature))
			{
				_relatedSignatures.Add(new RelatedSignature(signature, stReason, eSignatureRelationship));
			}
			string arg = QualifiedName(signature);
			if (_initialFlags.HasFlag(SignatureRelationFlags.ImplementedInterfaces))
			{
				ISignature2[] interfaceSignatures = APEnvironmentFacade.Instance.LanguageModelMgr.GetInterfaceSignatures(signature);
				foreach (ISignature2 signature2 in interfaceSignatures)
				{
					string stReason2 = string.Format(Strings.ImplementationOfInterfaceX0, QualifiedName(signature2));
					AddSignature(signature2, stReason2, ESignatureRelationship.ImplementationOfInterface);
				}
			}
			if (_initialFlags.HasFlag(SignatureRelationFlags.OverridingSubclasses) || _initialFlags.HasFlag(SignatureRelationFlags.ImplementingInterface))
			{
				foreach (ISignature2 derivedSignature in GetDerivedSignatures(signature, _initialFlags.HasFlag(SignatureRelationFlags.ScopeLibraries)))
				{
					string stReason3 = string.Format(Strings.InheritedFromX0, arg);
					AddSignature(derivedSignature, stReason3, ESignatureRelationship.Inheritance);
				}
			}
			if (_initialFlags.HasFlag(SignatureRelationFlags.OverriddenBases))
			{
				ISignature2 baseSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetBaseSignature(signature);
				if (baseSignature != null)
				{
					string stReason4 = string.Format(Strings.IsBaseImplementationOfX0, arg);
					AddSignature(baseSignature, stReason4, ESignatureRelationship.BaseImplementation);
				}
			}
		}

		private IEnumerable<ISignature2> GetDerivedSignatures(ISignature2 signature, bool bNeedLibraries)
		{
			CheckRebuildCache(bNeedLibraries);
			if (s_derivedSignatureCache.TryGetValue(signature, out var value))
			{
				return (IEnumerable<ISignature2>)value;
			}
			return new ISignature2[0];
		}

		private IList<IRelatedSignature> GetRelatedMethods(Operator subPouType)
		{
			IPreCompileContext2 precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(_initialSignature);
			Guid parentObjectGuid = _initialSignature.ParentObjectGuid;
			IList<IRelatedSignature> list = new RelatedSignatureFinder((ISignature2)precompileContextOfSignature.GetSignature(parentObjectGuid), _initialSignature, _initialFlags).FindResult();
			List<IRelatedSignature> list2 = new List<IRelatedSignature>(list.Count);
			foreach (IRelatedSignature item in list)
			{
				ESignatureRelationship eSignatureRelationship = ESignatureRelationship.Unknown;
				if (item is IRelatedSignature2 relatedSignature)
				{
					eSignatureRelationship = relatedSignature.SignatureRelationship;
				}
				ISignature[] subSignatures = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(item.Signature).GetSubSignatures(item.Signature.ObjectGuid);
				foreach (ISignature signature in subSignatures)
				{
					if (signature.POUType == subPouType && signature.Name == _initialSignature.Name)
					{
						list2.Add(new RelatedSignature((ISignature2)signature, item.Reason, eSignatureRelationship));
					}
				}
			}
			return list2;
		}

		public IList<IRelatedSignature> FindResult()
		{
			return _action();
		}
	}
}
