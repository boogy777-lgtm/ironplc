using System;
using System.Linq;
using \u0001;
using \u0018;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003B9 RID: 953
	internal static class FbInitSignatureGenerator
	{
		// Token: 0x060036B6 RID: 14006 RVA: 0x000DDF64 File Offset: 0x000DC164
		private static _IPreCompileContext \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(\u0002) as _IPreCompileContext;
			if (ipreCompileContext != null || !\u0018.\u0003.\u0001(\u0002))
			{
				return ipreCompileContext;
			}
			_ISignature isignature = \u0018.\u0003.\u0001(\u0003, \u0002);
			if (isignature != null)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(isignature) as _IPreCompileContext;
			}
			return null;
		}

		// Token: 0x060036B7 RID: 14007 RVA: 0x000DDFB8 File Offset: 0x000DC1B8
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (FbInitSignatureGenerator.\u0001(\u0002))
			{
				return;
			}
			_IPreCompileContext ipreCompileContext = FbInitSignatureGenerator.\u0001(\u0002, \u0004);
			_ISignature isignature = null;
			if (ipreCompileContext != null)
			{
				isignature = (FbInitSignatureGenerator.\u0001(\u0002, ipreCompileContext, \u0004.ApplicationGuid).FindSignatureLocal("FB_Init") as _ISignature);
			}
			_ISignature isignature2 = null;
			if (\u0003 != null)
			{
				isignature2 = (\u0003.GetSubSignature(global::\u0001.\u0013.InitMethod.Name) as _ISignature);
			}
			_ISignature isignature3;
			if (isignature != null)
			{
				isignature3 = isignature.CreateCompiledSignature(isignature2, \u0004.HasByteSupport());
				_IVariable ivariable = isignature3[IdentifierConstants.InstancePointer] as _IVariable;
				if (ivariable != null)
				{
					isignature3.RemoveVariable(ivariable);
				}
				isignature3.ObjectGuid = Guid.Empty;
				foreach (IVariable variable in isignature.Locals)
				{
					_IVariable ivariable2 = isignature3[variable.Name] as _IVariable;
					if (ivariable2 != null)
					{
						isignature3.RemoveVariable(ivariable2);
					}
				}
				isignature3.LibraryPath = \u0002.LibraryPath;
				if (isignature3.LibraryPath != isignature.LibraryPath)
				{
					isignature3.AddAttribute(InternalAttributes.INHERITED_FBINIT_LIBPATH, isignature.LibraryPath);
				}
			}
			else
			{
				isignature3 = global::\u0001.\u0013.InitMethod.CreateCompiledSignature(isignature2, \u0004.HasByteSupport());
			}
			isignature3.ParentObjectGuid = \u0002.ObjectGuid;
			isignature3.ParentSignatureId = \u0002.Id;
			\u0002.AddSubSignature(isignature3);
			\u0004.AddSignature(isignature3, isignature2, \u0005, true);
		}

		// Token: 0x060036B8 RID: 14008 RVA: 0x000DE104 File Offset: 0x000DC304
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (_ISignature u in \u0002.SubSignatures.Cast<_ISignature>())
			{
				FbInitSignatureGenerator.\u0001(\u0002, \u0003, u);
			}
		}

		// Token: 0x060036B9 RID: 14009 RVA: 0x000DE158 File Offset: 0x000DC358
		private static bool \u0001(ISignature \u0002)
		{
			bool result;
			FbInitSignatureGenerator.\u0001(\u0002, out result);
			foreach (_ISignature u in \u0002.SubSignatures.Cast<_ISignature>())
			{
				FbInitSignatureGenerator.\u0001(u);
			}
			return result;
		}

		// Token: 0x060036BA RID: 14010 RVA: 0x000DE1B0 File Offset: 0x000DC3B0
		private static void \u0001(ISignature \u0002, out bool \u0003)
		{
			_ISignature4 isignature = (_ISignature4)\u0002.GetSubSignature(IdentifierConstants.InitMethodName);
			\u0003 = false;
			if (isignature != null && !isignature.GetFlagInternal(SignatureFlagInternal.Overloaded))
			{
				if (FbInitSignatureGenerator.\u0003(isignature))
				{
					isignature.AddMessage(Severity.Error, MessageId.Err_WrongConstructor, Array.Empty<object>());
				}
				\u0003 = true;
				return;
			}
			foreach (_ISignature isignature2 in ((_ISignature4)\u0002).GetOverloadedSignatures(IdentifierConstants.InitMethodName))
			{
				if (FbInitSignatureGenerator.\u0003(isignature2))
				{
					isignature2.AddMessage(Severity.Error, MessageId.Err_WrongConstructor, Array.Empty<object>());
				}
				\u0003 = true;
			}
		}

		// Token: 0x060036BB RID: 14011 RVA: 0x000DE258 File Offset: 0x000DC458
		private static void \u0001(_ISignature \u0002)
		{
			if (\u0002.Name == IdentifierConstants.ExitMethodName)
			{
				bool flag = false;
				if (FbInitSignatureGenerator.\u0002(\u0002))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_WrongDestructor, Array.Empty<object>());
					flag = true;
				}
				if (!flag && !SignatureCheckHelper.\u0002(\u0002))
				{
					\u0002.AddMessage(Severity.Warning, MessageId.Wrn_WrongDestructor, Array.Empty<object>());
					return;
				}
			}
			else if (\u0002.Name == IdentifierConstants.ReInitMethodName && !SignatureCheckHelper.\u0001(\u0002))
			{
				\u0002.AddMessage(Severity.Warning, MessageId.Wrn_WrongReInit, Array.Empty<object>());
			}
		}

		// Token: 0x060036BC RID: 14012 RVA: 0x000DE2DC File Offset: 0x000DC4DC
		private static bool \u0002(ISignature \u0002)
		{
			IVariable[] allInputs = \u0002.AllInputs;
			return allInputs.Count(new Func<IVariable, bool>(FbInitSignatureGenerator.<>c.<>9.\u0001)) != 1 || allInputs[0].Type.Class != TypeClass.Bool || allInputs[0].Name != "BINCOPYCODE";
		}

		// Token: 0x060036BD RID: 14013 RVA: 0x000DE33C File Offset: 0x000DC53C
		private static bool \u0003(ISignature \u0002)
		{
			IVariable[] allInputs = \u0002.AllInputs;
			return allInputs.Length < 2 || allInputs[0].Type.Class != TypeClass.Bool || allInputs[0].Name != "BINITRETAINS" || allInputs[1].Type.Class != TypeClass.Bool || allInputs[1].Name != "BINCOPYCODE";
		}

		// Token: 0x060036BE RID: 14014 RVA: 0x000DE39C File Offset: 0x000DC59C
		private static IPrecompileScope2 \u0001(_ISignature \u0002, _IPreCompileContext \u0003, Guid \u0004)
		{
			if (!string.IsNullOrEmpty(\u0003.LibraryId) || \u0003.ApplicationGuid != Guid.Empty)
			{
				return \u0003.CreatePrecompileScope2(\u0002) as IPrecompileScope2;
			}
			return \u0081.\u0007.\u0001(\u0004, \u0003, \u0002);
		}

		// Token: 0x060036BF RID: 14015 RVA: 0x000DE3D4 File Offset: 0x000DC5D4
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			if (\u0004.POUType != Operator.Action)
			{
				return;
			}
			\u0004.SetFlag(SignatureFlag.Action, true);
			\u0004.POUType = Operator.Method;
			_ISignature isignature = \u0003;
			if (\u0003 != null)
			{
				isignature = (\u0003.GetSubSignature(\u0004.Name) as _ISignature);
			}
			foreach (_IVariable ivariable in \u0002.Temps.Cast<_IVariable>())
			{
				_IVariable ivariable2 = (_IVariable)ivariable.Duplicate();
				ivariable2.SetFlag(VarFlag.Local, true);
				ivariable2.SetFlag(VarFlag.Temp, false);
				if (isignature != null && isignature[ivariable2.VersionedName] != null)
				{
					ivariable2.Id = isignature[ivariable2.VersionedName].Id;
				}
				else
				{
					ivariable2.Id = \u0002.NextId;
				}
				\u0004.AddVariable(ivariable2);
			}
		}
	}
}
