using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class AnalyzationService
	{
		internal static void AddInstrumentationForAnalyzation(CompileEventArgs e)
		{
			try
			{
				if (!(APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(e.ApplicationGuid) is ICompileContext10 compileContext))
				{
					return;
				}
				foreach (ICompiledPOU4 item in compileContext.GetAllCompiledPOUsEx())
				{
					if (compileContext.GetSignatureById(item.SignatureId) is ISignature6 signature && !IsLibraryObjectToIgnore(signature) && (!(compileContext.GetSignatureById(signature.ParentSignatureId) is ISignature6 sign) || !IsLibraryObjectToIgnore(sign)) && CheckAnalyzationCallee(signature, compileContext))
					{
						Analyzation.AnalyzeStatement(item, compileContext);
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 0))
						{
							(signature as _ISignature).AddAttribute("prevent-fastonlinechange", "");
							(signature as _ISignature).AddAttribute("prevent-fastonlinechange-oncodechanges", "");
						}
					}
				}
			}
			catch
			{
			}
		}

		private static bool IsLibraryObjectToIgnore(ISignature6 sign)
		{
			if (string.IsNullOrEmpty(sign.LibraryPath))
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 18, 30))
			{
				return sign.IsCompiledLibraryObject;
			}
			return true;
		}

		private static bool CheckAnalyzationCallee(ISignature sign, ICompileContext comcon)
		{
			int[] calleeIds = sign.CalleeIds;
			IScope scope = comcon.CreateGlobalIScope();
			int[] array = calleeIds;
			foreach (int nId in array)
			{
				ISignature signature = scope[nId];
				if (signature != null && signature.HasAttribute("analyzation"))
				{
					return true;
				}
			}
			return false;
		}
	}
}
