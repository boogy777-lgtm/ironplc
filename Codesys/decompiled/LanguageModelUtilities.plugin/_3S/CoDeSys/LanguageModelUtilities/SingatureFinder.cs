using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class SingatureFinder
	{
		private readonly Guid _guidApplication;

		private readonly IPreCompileContext11 _precom;

		private readonly string _stAccessPathOrType;

		private readonly FindSubelementsFlags _flags;

		private readonly ICompiledType _ctype;

		private IPrecompileScope2 _searchScope2;

		internal SingatureFinder(Guid guidApplication, IPreCompileContext11 precom, string stAccessPathOrType, FindSubelementsFlags flags, ICompiledType ctype, IPrecompileScope searchScope)
		{
			_guidApplication = guidApplication;
			_precom = precom;
			_stAccessPathOrType = stAccessPathOrType;
			_flags = flags;
			_ctype = ctype;
			_searchScope2 = searchScope as IPrecompileScope2;
		}

		private ISignature FindSignature(ref IPrecompileScope6 pscope, ISignature derivedSignature, IVariable derivedVariable)
		{
			ISignature signature;
			if (derivedSignature != null && derivedVariable == null)
			{
				signature = derivedSignature;
			}
			else
			{
				pscope = _precom.CreatePrecompileScope(Guid.Empty) as IPrecompileScope6;
				pscope.ApplicationGuid = _guidApplication;
				signature = pscope.FindSignatureGlobal(_stAccessPathOrType);
				if (signature != null && (_flags & FindSubelementsFlags.IncludeTypeSubElements) == 0 && (signature.POUType == Operator.FunctionBlock || signature.POUType == Operator.Type || signature.POUType == Operator.Interface))
				{
					signature = null;
				}
			}
			return signature;
		}

		private ISignature FindSignature(ref IPrecompileScope6 pscope, ISignature derivedSignature)
		{
			IUserdefType2 userdefType = (IUserdefType2)_ctype.DeRefType;
			ISignature signatureForPrecompileID;
			if (userdefType is _IUserdefType iUserdefType)
			{
				signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(iUserdefType.SignatureId);
				if (signatureForPrecompileID != null)
				{
					return signatureForPrecompileID;
				}
			}
			signatureForPrecompileID = _searchScope2.FindSignatureLocal(userdefType.NameExpression.ToString());
			if (signatureForPrecompileID == null)
			{
				signatureForPrecompileID = _searchScope2.FindSignatureGlobal(userdefType.NameExpression);
			}
			if (signatureForPrecompileID == null && !string.IsNullOrEmpty(derivedSignature?.LibraryPath))
			{
				_searchScope2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(derivedSignature)?.CreatePrecompileScope(derivedSignature.ObjectGuid) as IPrecompileScope2;
				signatureForPrecompileID = _searchScope2?.FindSignatureLocal(userdefType.NameExpression.ToString());
				if (signatureForPrecompileID == null)
				{
					signatureForPrecompileID = _searchScope2?.FindSignatureGlobal(userdefType.NameExpression);
				}
			}
			if (signatureForPrecompileID == null)
			{
				pscope = (IPrecompileScope6)APEnvironmentFacade.Instance.LanguageModelMgr.SystemContext.CreatePrecompileScope(Guid.Empty);
				signatureForPrecompileID = pscope.FindSignatureGlobal(userdefType.NameExpression);
			}
			return signatureForPrecompileID;
		}

		internal IIdentifierInfo[] FindSignature(ref IPrecompileScope6 pscope, ref ISignature signHelp, ISignature derivedSignature, IVariable derivedVariable)
		{
			if (_ctype == null || _ctype.DeRefType.Class != TypeClass.Userdef)
			{
				signHelp = FindSignature(ref pscope, derivedSignature, derivedVariable);
			}
			else if (_searchScope2 != null)
			{
				signHelp = FindSignature(ref pscope, derivedSignature);
				if (signHelp != null && signHelp.GetFlag(SignatureFlag.Enum) && derivedVariable != null)
				{
					return Array.Empty<IIdentifierInfo>();
				}
			}
			return null;
		}
	}
}
