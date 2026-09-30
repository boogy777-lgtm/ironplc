using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class InterfaceHelper
	{
		private class InterfaceCollectionResultContainer
		{
			private readonly List<ISignature2> _lstInterfaces = new List<ISignature2>();

			private readonly HashSet<string> _hsAlreadyAddedInterfaces = new HashSet<string>();

			internal IEnumerable<ISignature2> Interfaces => _lstInterfaces;

			internal void AddInterface(ISignature2 signItf)
			{
				if (!_hsAlreadyAddedInterfaces.Contains(signItf.Name))
				{
					_hsAlreadyAddedInterfaces.Add(signItf.Name);
					_lstInterfaces.Add(signItf);
				}
			}
		}

		internal static IEnumerable<ISignature2> CollectFBInterfaces(ISignature2 sign, ILMPreCompileSet precom)
		{
			InterfaceCollectionResultContainer interfaceCollectionResultContainer = new InterfaceCollectionResultContainer();
			CollectFBInterfaces(new Dictionary<ISignature, object>(), sign, precom, sign, precom, interfaceCollectionResultContainer);
			return interfaceCollectionResultContainer.Interfaces;
		}

		private static void CollectFBInterfaces(Dictionary<ISignature, object> htRecursionCheckBaseSignature, ISignature2 sign, ILMPreCompileSet precom, ISignature signOrg, ILMPreCompileSet precomOrg, InterfaceCollectionResultContainer resultContainer)
		{
			Dictionary<ISignature, object> htRecursionCheck = new Dictionary<ISignature, object>();
			IExpression[] interfaceExpressions = sign.InterfaceExpressions;
			htRecursionCheckBaseSignature.Add(sign, sign);
			IExpression[] array = interfaceExpressions;
			for (int i = 0; i < array.Length; i++)
			{
				CollectInterfaces(array[i], sign, precom, signOrg, precomOrg, htRecursionCheck, resultContainer);
			}
		}

		private static void CollectInterfaces(IExpression exp, ISignature sign, ILMPreCompileSet precom, ISignature signOrg, ILMPreCompileSet precomOrg, Dictionary<ISignature, object> htRecursionCheck, InterfaceCollectionResultContainer resultContainer)
		{
			if (!(precom is IPreCompileContext comconMy) || !(FindInterfaceSignature(exp, comconMy) is ISignature2 signature))
			{
				return;
			}
			resultContainer.AddInterface(signature);
			try
			{
				CollectInterfaces(signature, sign, signOrg, precomOrg, htRecursionCheck, resultContainer);
			}
			catch
			{
			}
		}

		private static void CollectInterfaces(ISignature signInterface, ISignature signMy, ISignature signOrg, ILMPreCompileSet precomOrg, Dictionary<ISignature, object> htRecursionCheck, InterfaceCollectionResultContainer resultContainer)
		{
			if (htRecursionCheck.ContainsKey(signInterface))
			{
				return;
			}
			htRecursionCheck.Add(signInterface, null);
			if (signInterface == null || signInterface.POUType != Operator.Interface)
			{
				return;
			}
			ILMPreCompileSet precompileSetOfSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(signInterface);
			ISignature2 signature = signInterface as ISignature2;
			if (signature != null && signature.BaseExpression != null)
			{
				CollectInterfaces(signature.BaseExpression, signMy, precompileSetOfSignature, signOrg, precomOrg, htRecursionCheck, resultContainer);
			}
			if (signature != null && signature.InterfaceExpressions != null)
			{
				IExpression[] interfaceExpressions = signature.InterfaceExpressions;
				for (int i = 0; i < interfaceExpressions.Length; i++)
				{
					CollectInterfaces(interfaceExpressions[i], signMy, precompileSetOfSignature, signOrg, precomOrg, htRecursionCheck, resultContainer);
				}
			}
		}

		internal static IEnumerable<AbstractMethodDescription> CollectFBInterfaceMethods(ISignature2 sign, IPreCompileContext comcon)
		{
			List<AbstractMethodDescription> list = new List<AbstractMethodDescription>();
			List<AbstractMethodDescription> list2 = new List<AbstractMethodDescription>();
			CollectImplementedMethods(new Dictionary<ISignature, object>(), sign, comcon, sign, comcon, list2);
			if (CollectFBInterfaceMethods(new Dictionary<ISignature, object>(), sign, comcon, sign, comcon, list))
			{
				return FilterImplementedMethods(list, list2);
			}
			return null;
		}

		private static bool CollectFBInterfaceMethods(Dictionary<ISignature, object> htRecursionCheckBaseSignature, ISignature2 sign, IPreCompileContext comcon, ISignature signOrg, IPreCompileContext comconOrg, List<AbstractMethodDescription> lstResult)
		{
			bool flag = false;
			Dictionary<ISignature, object> htRecursionCheck = new Dictionary<ISignature, object>();
			IExpression[] interfaceExpressions = sign.InterfaceExpressions;
			htRecursionCheckBaseSignature.Add(sign, sign);
			IExpression[] array = interfaceExpressions;
			for (int i = 0; i < array.Length; i++)
			{
				flag = CollectInterfaceMethods(array[i], sign, comcon, signOrg, comconOrg, htRecursionCheck, lstResult) || flag;
			}
			IExpression baseExpression = sign.BaseExpression;
			if (baseExpression != null && (comcon.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2)?.FindSignatureGlobal(baseExpression) is ISignature2 signature)
			{
				if (htRecursionCheckBaseSignature.ContainsKey(signature))
				{
					return flag;
				}
				IPreCompileContext precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature);
				flag = CollectFBInterfaceMethods(htRecursionCheckBaseSignature, signature, precompileContextOfSignature, signOrg, comconOrg, lstResult) || flag;
			}
			return flag;
		}

		private static bool CollectInterfaceMethods(IExpression exp, ISignature sign, IPreCompileContext comcon, ISignature signOrg, IPreCompileContext comconOrg, Dictionary<ISignature, object> htRecursionCheck, List<AbstractMethodDescription> lstResult)
		{
			ISignature signature = FindInterfaceSignature(exp, comcon);
			if (signature != null)
			{
				try
				{
					return CollectInterfaceMethods(signature, sign, signOrg, comconOrg, htRecursionCheck, lstResult);
				}
				catch
				{
				}
			}
			return false;
		}

		private static bool CollectInterfaceMethods(ISignature signInterface, ISignature signMy, ISignature signOrg, IPreCompileContext precomOrg, Dictionary<ISignature, object> htRecursionCheck, List<AbstractMethodDescription> lstResult)
		{
			if (htRecursionCheck.ContainsKey(signInterface))
			{
				return false;
			}
			htRecursionCheck.Add(signInterface, null);
			bool flag = false;
			if (signInterface == null)
			{
				return flag;
			}
			if (signInterface.POUType != Operator.Interface)
			{
				return flag;
			}
			IPreCompileContext precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signInterface);
			ISignature2 signature = signInterface as ISignature2;
			if (signature != null && signature.BaseExpression != null)
			{
				flag = CollectInterfaceMethods(signature.BaseExpression, signMy, precompileContextOfSignature, signOrg, precomOrg, htRecursionCheck, lstResult);
			}
			if (signature != null && signature.InterfaceExpressions != null)
			{
				IExpression[] interfaceExpressions = signature.InterfaceExpressions;
				for (int i = 0; i < interfaceExpressions.Length; i++)
				{
					flag = CollectInterfaceMethods(interfaceExpressions[i], signMy, precompileContextOfSignature, signOrg, precomOrg, htRecursionCheck, lstResult) || flag;
				}
			}
			ISignature[] subSignatures = precompileContextOfSignature.GetSubSignatures(signInterface.ObjectGuid);
			ISignature[] subsignaturesOrg = null;
			if (signOrg != signMy)
			{
				subsignaturesOrg = precomOrg.GetSubSignatures(signOrg.ObjectGuid);
			}
			flag = CollectInterfaceProperties(signInterface, lstResult, flag, precompileContextOfSignature, subSignatures);
			return CollectInterfaceMethods(signInterface, lstResult, flag, precompileContextOfSignature, subSignatures, subsignaturesOrg);
		}

		private static bool CollectInterfaceMethods(ISignature signInterface, List<AbstractMethodDescription> lstResult, bool bres, IPreCompileContext precomInterface, ISignature[] subsignaturesInterface, ISignature[] subsignaturesOrg)
		{
			foreach (ISignature signature in subsignaturesInterface)
			{
				bool flag = true;
				if (subsignaturesOrg != null)
				{
					foreach (ISignature signature2 in subsignaturesOrg)
					{
						if (signature.Name == signature2.Name)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag && !signature.Name.StartsWith("__"))
				{
					bres = true;
					AddFoundSignature(signInterface, signature, precomInterface, lstResult);
				}
			}
			return bres;
		}

		private static bool CollectInterfaceProperties(ISignature signInterface, List<AbstractMethodDescription> lstResult, bool bres, IPreCompileContext precomInterface, ISignature[] subsignaturesInterface)
		{
			IVariable[] all = signInterface.All;
			foreach (IVariable variable in all)
			{
				if (!variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
				{
					continue;
				}
				string[] array = new string[2]
				{
					"__Get" + variable.Name,
					"__Set" + variable.Name
				};
				foreach (ISignature signature in subsignaturesInterface)
				{
					string[] array2 = array;
					foreach (string strB in array2)
					{
						if (string.Compare(signature.Name, strB, StringComparison.OrdinalIgnoreCase) == 0)
						{
							AddFoundSignature(signInterface, signature, precomInterface, lstResult);
						}
					}
				}
				bres = true;
			}
			return bres;
		}

		private static void AddFoundSignature(ISignature signInterface, ISignature signMethod, IPreCompileContext precom, List<AbstractMethodDescription> lstResult)
		{
			AbstractMethodDescription item = new AbstractMethodDescription(signMethod, signInterface, precom);
			if (!lstResult.Contains(item))
			{
				lstResult.Add(item);
			}
		}

		private static ISignature FindInterfaceSignature(IExpression exp, IPreCompileContext comconMy)
		{
			ISignature result = null;
			if (!(comconMy.CreatePrecompileScope(Guid.Empty) is IPrecompileScope2 precompileScope))
			{
				return null;
			}
			ISignature itfCandidateSign = precompileScope.FindSignatureGlobal(exp);
			itfCandidateSign = GetNonAliasedSignature(precompileScope, itfCandidateSign);
			if (itfCandidateSign != null && itfCandidateSign.POUType == Operator.Interface)
			{
				result = itfCandidateSign;
			}
			return result;
		}

		private static ISignature GetNonAliasedSignature(IPrecompileScope2 scope, ISignature itfCandidateSign)
		{
			IPrecompileScope7 foundScope;
			if (scope is IPrecompileScopeWithAliasService)
			{
				return (scope as IPrecompileScopeWithAliasService)?.DetermineAliasBaseSignature(itfCandidateSign, out foundScope);
			}
			return itfCandidateSign;
		}

		private static void CollectImplementedMethods(Dictionary<ISignature, object> htRecursionCheckBaseSignature, ISignature2 sign, IPreCompileContext comcon, ISignature signOrg, IPreCompileContext comconOrg, List<AbstractMethodDescription> lstResult)
		{
			htRecursionCheckBaseSignature.Add(sign, sign);
			foreach (ISignature item in from s in comcon.GetSubSignatures(sign.ObjectGuid)
				where !s.GetFlag(SignatureFlag.Abstract)
				select s)
			{
				AddFoundSignature(sign, item, comcon, lstResult);
			}
			IExpression baseExpression = sign.BaseExpression;
			if (baseExpression != null && (comcon.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2)?.FindSignatureGlobal(baseExpression) is ISignature2 signature && !htRecursionCheckBaseSignature.ContainsKey(signature))
			{
				IPreCompileContext precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature);
				CollectImplementedMethods(htRecursionCheckBaseSignature, signature, precompileContextOfSignature, signOrg, comconOrg, lstResult);
			}
		}

		private static IEnumerable<AbstractMethodDescription> FilterImplementedMethods(List<AbstractMethodDescription> lstItfMethodsToImplement, List<AbstractMethodDescription> lstAlreadyImplementedFinals)
		{
			HashSet<string> hsFinalMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (AbstractMethodDescription lstAlreadyImplementedFinal in lstAlreadyImplementedFinals)
			{
				hsFinalMethods.Add(lstAlreadyImplementedFinal.Signature.Name);
			}
			foreach (AbstractMethodDescription item in lstItfMethodsToImplement)
			{
				if (!hsFinalMethods.Contains(item.Signature.Name))
				{
					yield return item;
				}
			}
		}
	}
}
