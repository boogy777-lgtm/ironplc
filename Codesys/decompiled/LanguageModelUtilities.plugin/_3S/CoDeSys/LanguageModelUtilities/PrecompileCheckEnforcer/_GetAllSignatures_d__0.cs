using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class PrecompileCheckEnforcer
	{
		internal IEnumerable<Tuple<_IPreCompileContext, _ISignature>> GetAllSignatures(Guid objguid)
		{
			foreach (_IPreCompileContext item2 in APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileContexts.OfType<_IPreCompileContext>())
			{
				if (item2.GetSignature(objguid) is _ISignature item)
				{
					yield return new Tuple<_IPreCompileContext, _ISignature>(item2, item);
				}
			}
		}

		internal IEnumerable<Tuple<_IPreCompileContext, _ICompiledPOU>> GetAllCompiledPOUs(Guid objguid)
		{
			foreach (_IPreCompileContext item2 in APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileContexts.OfType<_IPreCompileContext>())
			{
				if (item2.GetCompiledPOU(objguid) is _ICompiledPOU item)
				{
					yield return new Tuple<_IPreCompileContext, _ICompiledPOU>(item2, item);
				}
			}
		}

		internal IEnumerable<Tuple<_IPreCompileContext, _ISignature>> GetAllSignatures(string stName)
		{
			foreach (_IPreCompileContext item2 in APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileContexts.OfType<_IPreCompileContext>())
			{
				if (item2.GetSignature(stName) is _ISignature item)
				{
					yield return new Tuple<_IPreCompileContext, _ISignature>(item2, item);
				}
			}
		}

		internal void ForceSymbolOnlyReferences(string stSymbol)
		{
			IEnumerable<IAccessInfo> test = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService.GetVariableAccess(stSymbol).Union(APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService.GetPOUAccess(stSymbol));
			ForceAccesses(test);
		}

		private void ForceAccesses(IEnumerable<IAccessInfo> test)
		{
			foreach (IAccessInfo item in test.Where((IAccessInfo x) => x.Position != null))
			{
				foreach (Tuple<_IPreCompileContext, _ISignature> allSignature in GetAllSignatures(item.Position.ObjectGuid))
				{
					if (!allSignature.Item2.GetFlagInternal(SignatureFlagInternal.Checked))
					{
						allSignature.Item1.CheckSignature(allSignature.Item2);
						allSignature.Item1.SetSignatureChecked(allSignature.Item2);
					}
				}
				foreach (Tuple<_IPreCompileContext, _ICompiledPOU> allCompiledPOU in GetAllCompiledPOUs(item.Position.ObjectGuid))
				{
					List<IMessage4> compilermessages = new List<IMessage4>();
					if (allCompiledPOU.Item2 != null && !allCompiledPOU.Item2.GetFlagInternal(InternalCompiledPOUFlags.Checked))
					{
						allCompiledPOU.Item1.CheckPOUCode(item.Position.ObjectGuid, compilermessages);
						allCompiledPOU.Item1.SetPouChecked(allCompiledPOU.Item2);
					}
				}
			}
		}

		internal void ForceSymbolReferences(string stSymbol)
		{
			bool num = stSymbol.StartsWith("%");
			bool flag = stSymbol.Contains('*') || stSymbol.Contains('?');
			if (num || flag)
			{
				IDictionary<string, IList<IAccessInfo>> precompiledCrossReferences = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompiledCrossReferences(new Regex(stSymbol));
				{
					foreach (string key in precompiledCrossReferences.Keys)
					{
						ForceAccesses(precompiledCrossReferences[key]);
					}
					return;
				}
			}
			ForceSymbolOnlyReferences(stSymbol);
		}

		internal void ForceSymbolTypeReferences(string stSymbol)
		{
			foreach (Tuple<_IPreCompileContext, _ISignature> allSignature in GetAllSignatures(stSymbol))
			{
				ForceTypeDeclarationReferences(allSignature.Item2);
			}
		}

		internal void ForceTypeDeclarationReferences(ISignature6 signature)
		{
			foreach (int item in Enumerable.ToLList<int>(((IEnumerable<int>)new LList<int>()).Union(signature.PrecompileDeclarerIds).Union((IEnumerable<int>)ReferenceHelper.GetImplicitDeclarers(signature))))
			{
				ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(item);
				if (signatureForPrecompileID != null)
				{
					IVariable[] all = signatureForPrecompileID.All;
					foreach (IVariable variable in all)
					{
						ForceTypeReference(variable.OrgName, variable.Type, signature.PrecompileId);
					}
				}
			}
		}

		internal void ForceTypeReference(string stSymbolToForce, IType ctype, int id)
		{
			if (ctype == null)
			{
				return;
			}
			switch (ctype.Class)
			{
			case TypeClass.Userdef:
				if ((ctype as IUserdefType).SignatureId == id)
				{
					ForceSymbolReferences(stSymbolToForce);
				}
				break;
			case TypeClass.Pointer:
			{
				IPointerType pointerType = ctype as IPointerType;
				ForceTypeReference(stSymbolToForce, pointerType.Base, id);
				break;
			}
			case TypeClass.Reference:
			{
				IReferenceType referenceType = ctype as IReferenceType;
				ForceTypeReference(stSymbolToForce, referenceType.Base, id);
				break;
			}
			case TypeClass.Array:
			{
				IArrayType arrayType = ctype as IArrayType;
				ForceTypeReference(stSymbolToForce, arrayType.Base as ICompiledType, id);
				break;
			}
			case TypeClass.Subrange:
			case TypeClass.Enum:
			case TypeClass.Params:
				break;
			}
		}
	}
}
