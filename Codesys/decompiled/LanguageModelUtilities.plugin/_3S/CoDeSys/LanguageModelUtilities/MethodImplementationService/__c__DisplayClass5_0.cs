using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelUtilities.Legacy;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{C387EC7A-F129-4B62-8C8B-C431482EE763}")]
	[SystemInterface("_3S.CoDeSys.LanguageModelUtilities.IMethodImplementationService")]
	public class MethodImplementationService : IMethodImplementationService
	{
		public IEnumerable<IAbstractMethodDescription> GetAbstractMethodsToImplement(ISignature2 sign, IPreCompileContext comcon)
		{
			Dictionary<ISignature, object> htRecursionCheckBaseSignature = new Dictionary<ISignature, object>();
			IEnumerable<AbstractMethodDescription> allAbstractMethods = CollectFBAllMethods(htRecursionCheckBaseSignature, sign, comcon, bAbstract: true);
			IEnumerable<AbstractMethodDescription> allNonAbstractMethods = CollectFBAllMethods(htRecursionCheckBaseSignature, sign, comcon, bAbstract: false);
			return DetermineMethodsToImplement(allAbstractMethods, allNonAbstractMethods);
		}

		public IEnumerable<IAbstractMethodDescription> GetInterfaceMethodsToImplement(ISignature2 sign, IPreCompileContext comcon)
		{
			return InterfaceHelper.CollectFBInterfaceMethods(sign, comcon);
		}

		public string GetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest)
		{
			return LegacySwitch.GetQualifiedTypeText(type, precomSource, precomDest);
		}

		public string CreateTextualInterface(ISignature2 signFB, ISignature2 signMethod, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, IAdditionalAttributeProvider additionalAttributeProvider)
		{
			return TextualInterfaceCreator.CreateTextualInterface(signFB, signMethod, precomSource, precomDest, additionalAttributeProvider);
		}

		private IEnumerable<AbstractMethodDescription> DetermineMethodsToImplement(IEnumerable<AbstractMethodDescription> allAbstractMethods, IEnumerable<AbstractMethodDescription> allNonAbstractMethods)
		{
			List<AbstractMethodDescription> list = new List<AbstractMethodDescription>();
			Dictionary<string, ISignature> dictionary = new Dictionary<string, ISignature>();
			foreach (AbstractMethodDescription allNonAbstractMethod in allNonAbstractMethods)
			{
				dictionary[allNonAbstractMethod.Signature.Name] = allNonAbstractMethod.Signature;
			}
			foreach (AbstractMethodDescription allAbstractMethod in allAbstractMethods)
			{
				if (!dictionary.ContainsKey(allAbstractMethod.Signature.Name))
				{
					list.Add(allAbstractMethod);
				}
			}
			return list;
		}

		private IEnumerable<AbstractMethodDescription> CollectFBAllMethods(Dictionary<ISignature, object> htRecursionCheckBaseSignature, ISignature2 sign, IPreCompileContext comcon, bool bAbstract)
		{
			List<AbstractMethodDescription> list = new List<AbstractMethodDescription>();
			list.AddRange(from s in comcon.GetSubSignatures(sign.ObjectGuid)
				where (bAbstract && s.GetFlag(SignatureFlag.Abstract)) || (!bAbstract && !s.GetFlag(SignatureFlag.Abstract))
				select s into signSub
				select new AbstractMethodDescription(signSub, sign, comcon));
			IExpression baseExpression = sign.BaseExpression;
			if (baseExpression != null && (comcon.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2)?.FindSignatureGlobal(baseExpression) is ISignature2 signature)
			{
				if (htRecursionCheckBaseSignature.ContainsKey(signature) || sign == signature)
				{
					return list;
				}
				IPreCompileContext2 precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature);
				list.AddRange(CollectFBAllMethods(htRecursionCheckBaseSignature, signature, precompileContextOfSignature, bAbstract));
			}
			return list;
		}
	}
}
