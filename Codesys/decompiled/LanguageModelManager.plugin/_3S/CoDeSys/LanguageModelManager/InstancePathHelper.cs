using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FB RID: 251
	internal static class InstancePathHelper
	{
		// Token: 0x0600124E RID: 4686 RVA: 0x00034194 File Offset: 0x00033194
		public static ISignature FindSignatureForInstancePath(ICompileContext comcon, int nProjectHandle, Guid guidObject, string stInstancePath)
		{
			ISignature signature = InstancePathHelper.FindSignatureForObjectGuid(comcon, nProjectHandle, guidObject);
			if (signature == null)
			{
				return null;
			}
			stInstancePath = InstancePathHelper.StripResourceFromInstancePathString(stInstancePath);
			IVariable[] array2;
			ISignature[] array3;
			string[] array = ((ICompileContext21)comcon).InstancePaths(signature, out array2, out array3, true, true, true);
			if (array.Length != array2.Length)
			{
				return signature;
			}
			InstancePathHelper.FindMatchingSignatureForInstancePath(comcon, array, stInstancePath, array2, ref signature);
			return signature;
		}

		// Token: 0x0600124F RID: 4687 RVA: 0x000341E4 File Offset: 0x000331E4
		private static string StripResourceFromInstancePathString(string instancePath)
		{
			if (!string.IsNullOrWhiteSpace(instancePath))
			{
				IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(instancePath, false, false, false, false);
				scanner.AllowMultipleUnderlines = true;
				IToken token = null;
				IToken token2;
				if (scanner.GetNext(out token2) != TokenType.Identifier || scanner.GetNext(out token) != TokenType.Operator || scanner.GetNext(out token) != TokenType.Identifier || scanner.GetNext(out token) != TokenType.Operator)
				{
					scanner.SetPosition(token2);
				}
				if (token == null)
				{
					token = token2;
				}
				return instancePath.Substring((int)token.PositionOffset + token.Length);
			}
			return null;
		}

		// Token: 0x06001250 RID: 4688 RVA: 0x00034268 File Offset: 0x00033268
		private static ISignature GetSignature(ICompileContext comcon, int nProjectHandle, Guid guidObject)
		{
			IProject projectByHandle = APEnvironmentFacade.Instance.GetProjectByHandle(nProjectHandle);
			if (projectByHandle == null)
			{
				return null;
			}
			string projectId = string.Empty;
			if (projectByHandle.Library)
			{
				projectId = projectByHandle.Id;
			}
			foreach (ISignature signature in comcon.AllSignatures)
			{
				if (signature.ObjectGuid == guidObject && InstancePathHelper.IsLibraryPathMatch(signature.LibraryPath, projectId))
				{
					return signature;
				}
				foreach (ISignature signature2 in signature.SubSignatures)
				{
					if (signature2.ObjectGuid == guidObject && InstancePathHelper.IsLibraryPathMatch(signature2.LibraryPath, projectId))
					{
						return signature2;
					}
				}
			}
			return null;
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x0003431C File Offset: 0x0003331C
		private static bool IsLibraryPathMatch(string libraryPath, string projectId)
		{
			bool flag = string.IsNullOrEmpty(libraryPath);
			bool flag2 = string.IsNullOrEmpty(projectId);
			if (flag)
			{
				return flag2;
			}
			return !flag2 && string.Equals(libraryPath, projectId, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x00034348 File Offset: 0x00033348
		private static ISignature FindSignatureForObjectGuid(ICompileContext comcon, int nProjectHandle, Guid objectGuidBP)
		{
			if (APEnvironmentFacade.Instance.ExistsObject(nProjectHandle, objectGuidBP))
			{
				ISignature signature = InstancePathHelper.GetSignature(comcon, nProjectHandle, objectGuidBP);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x06001253 RID: 4691 RVA: 0x00034374 File Offset: 0x00033374
		private static void FindMatchingSignatureForInstancePath(ICompileContext comcon, string[] stInstancePaths, string stInstancePath, IVariable[] vardecls, ref ISignature sign)
		{
			for (int i = 0; i < stInstancePaths.Length; i++)
			{
				if (stInstancePaths[i] == stInstancePath)
				{
					InstancePathHelper.GetSignatureForVariableType(vardecls[i].CompiledType, comcon, ref sign);
				}
			}
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x000343AC File Offset: 0x000333AC
		private static void GetSignatureForVariableType(ICompiledType compiledType, ICompileContext compileContext, ref ISignature sign)
		{
			IUserdefType userdefType = compiledType as IUserdefType;
			if (userdefType == null)
			{
				if (compiledType.Class == TypeClass.Array)
				{
					InstancePathHelper.GetSignatureForVariableType(compiledType.BaseType, compileContext, ref sign);
				}
				return;
			}
			ISignature signatureById = compileContext.GetSignatureById(userdefType.SignatureId);
			if (signatureById == null)
			{
				return;
			}
			if (sign.POUType == Operator.Method)
			{
				sign = InstancePathHelper.GetMethodSignatureForVariableType(compileContext, signatureById, sign);
				return;
			}
			sign = signatureById;
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x00034408 File Offset: 0x00033408
		private static ISignature GetMethodSignatureForVariableType(ICompileContext compileContext, ISignature signVarType, ISignature signEditorMethod)
		{
			ISignature signatureById = compileContext.GetSignatureById(signEditorMethod.ParentSignatureId);
			signVarType = InstancePathHelper.GetMatchingParentType(compileContext, signVarType, signatureById);
			return signVarType.GetSubSignature(signEditorMethod.Name);
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x00034438 File Offset: 0x00033438
		private static ISignature GetMatchingParentType(ICompileContext compileContext, ISignature signVarType, ISignature signEditorType)
		{
			while (!(InstancePathHelper.GetGenericFreeName(signVarType.Name) == InstancePathHelper.GetGenericFreeName(signEditorType.Name)))
			{
				signVarType = compileContext.GetSignatureById(signVarType.BaseSignatureId);
				if (signVarType == null)
				{
					return null;
				}
			}
			return signVarType;
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x0003446C File Offset: 0x0003346C
		private static string GetGenericFreeName(string stSignatureName)
		{
			string text = stSignatureName;
			int num = text.IndexOf('<');
			if (num >= 0)
			{
				text = text.Substring(0, num);
			}
			return text;
		}
	}
}
