using System;
using \u0003;
using \u0004;
using \u0007;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0010
{
	// Token: 0x0200038A RID: 906
	internal sealed class \u000F : \u001C.\u0012
	{
		// Token: 0x060034AD RID: 13485 RVA: 0x000CFA40 File Offset: 0x000CDC40
		public bool \u0001(global::\u0003.\u0016 \u0002)
		{
			if (!\u0002.signChanges.RegenerateFbInit)
			{
				return true;
			}
			_ISignature isignature = \u0002.sign.GetSubSignature(IdentifierConstants.InitMethodName) as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			_ISignature isignature2 = \u0002.signRef.GetSubSignature(IdentifierConstants.InitMethodName) as _ISignature;
			if (isignature2 == null)
			{
				return false;
			}
			_ICompiledPOU icompiledPOU = \u0002.focContext.ComconNew._GetCompiledPOUById(isignature.Id);
			if (icompiledPOU == null)
			{
				return false;
			}
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002.focContext.ComconNew, \u0002.sign.Id);
			_IStatement istatement = \u0018.\u0001(\u0002.signCompiled, \u0002.focContext.ComconNew, \u0002.focContext.ComconOld, isignature, u);
			Locator.\u0001(\u0002.focContext.ComconNew.DataManager, \u0002.focContext.ComconNew, \u0002.focContext.ComconOld, isignature, isignature2);
			_ICompiledPOU icompiledPOU2;
			if (isignature.ObjectGuid != Guid.Empty)
			{
				icompiledPOU2 = Helper.\u0001(\u0002.focContext.ComconNew, icompiledPOU, \u0002.focContext.Precomp, \u0002.focContext.PrecompPool).CreateCompiledPOU();
				_ISequenceStatement isequenceStatement = icompiledPOU2.GetParseTree() as _ISequenceStatement;
				if (isequenceStatement == null)
				{
					icompiledPOU2.SetParseTree(istatement);
				}
				else
				{
					_ISequenceStatement isequenceStatement2 = \u0019.\u0003.\u0001();
					isequenceStatement2.Add(istatement);
					isequenceStatement2.Add(isequenceStatement);
					icompiledPOU2.SetParseTree(isequenceStatement2);
				}
			}
			else
			{
				icompiledPOU2 = \u0019.\u0003.\u0001(IdentifierConstants.InitMethodName);
				icompiledPOU2.SetParseTree(istatement);
			}
			icompiledPOU2.SignatureId = isignature.Id;
			icompiledPOU2.MessageGuid = \u0002.sign.ObjectGuid;
			\u0002.focContext.ComconNew.RemoveCompiledPOU(icompiledPOU);
			\u0002.focContext.ComconNew.AddCompiledPOUSimple(icompiledPOU2);
			\u0002.focContext.changedpous[icompiledPOU2.SignatureId] = icompiledPOU;
			return true;
		}
	}
}
