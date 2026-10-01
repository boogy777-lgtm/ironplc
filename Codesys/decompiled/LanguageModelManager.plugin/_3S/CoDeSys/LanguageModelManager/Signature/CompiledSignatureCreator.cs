using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000253 RID: 595
	public static class CompiledSignatureCreator
	{
		// Token: 0x060027DF RID: 10207 RVA: 0x00063D38 File Offset: 0x00062D38
		internal static Signature _CreateCompiledSignature(Signature signPre, Signature signOld, CompileContext comcon, CompileContext comconOld, bool bByteSupport)
		{
			Signature signature = new Signature();
			if (signPre._NameExpression != null)
			{
				signature._NameExpression = (signPre._NameExpression.Duplicate() as _IExpression);
			}
			signature.Flags = signPre.Flags;
			signature.InternalFlags = signPre.InternalFlags;
			signature.SetFlag(SignatureFlag.Compiled, true);
			signature.ObjectGuid = signPre.ObjectGuid;
			signature.MessageGuid = signPre.MessageGuid;
			signature.ParentObjectGuid = signPre.ParentObjectGuid;
			signature.POUType = signPre.POUType;
			if (signPre._BaseSignature != null)
			{
				signature._BaseSignature = (signPre._BaseSignature.Duplicate() as _IExpression);
			}
			signature.TimeStamp = signPre.TimeStamp;
			signature.Checksum = signPre.Checksum;
			signature.ChecksumNoInit = signPre.ChecksumNoInit;
			signature.ChecksumOptionalInputs = signPre.ChecksumOptionalInputs;
			CompiledSignatureCreator.CopyInterfaces(signPre, signature);
			bool bDoCrossReferences = CompiledSignatureCreator.DoCrossReferences(signOld, comcon, comconOld);
			CompiledSignatureCreator._CreateCompiledVariables(signPre, signOld, comcon, comconOld, bByteSupport, bDoCrossReferences, signature);
			CompiledSignatureCreator.CopyCrossReferences(signOld, comconOld, bDoCrossReferences, signature);
			CompiledSignatureCreator.CopyAttributes(signPre, signature);
			signPre.CloneAttributesTo(signature);
			CompiledSignatureCreator.SetSomeSpecialFlags(signPre, comcon, signature);
			signature.LibraryPath = signPre.LibraryPath;
			return signature;
		}

		// Token: 0x060027E0 RID: 10208 RVA: 0x00063E58 File Offset: 0x00062E58
		private static void SetSomeSpecialFlags(Signature signPre, CompileContext comcon, Signature signRet)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				if (signRet.HasAttribute(CompileAttributes.ATTRIBUTE_EXPLICIT_INIT_EXIT_HANDLING) && CompilerProxy.RuntimeVersion((comcon != null) ? comcon.GetTargetSettings() : null) >= new Version(3, 5, 5, 0))
				{
					signRet.SetFlagInternal(SignatureFlagInternal.ExplicitInitExitHandling, true);
				}
				if (signRet.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_WITHIN_GLOBAL_INIT_EXIT_SLOT))
				{
					signRet.SetFlagInternal(SignatureFlagInternal.CallWithinGlobalInitExit, true);
				}
				if (!string.IsNullOrEmpty(signPre.LibraryPath) && comcon != null && comcon.LibraryIsUnique(signPre.LibraryPath))
				{
					signRet.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, true);
				}
			}
		}

		// Token: 0x060027E1 RID: 10209 RVA: 0x00063EF0 File Offset: 0x00062EF0
		private static void CopyAttributes(Signature signPre, Signature signRet)
		{
			if (signPre.GetAllMessages() != null)
			{
				signPre.RemoveMessageDuplicates();
				foreach (_ICompilerMessage cm in signPre.GetAllMessages())
				{
					signRet.AddError(cm);
				}
			}
		}

		// Token: 0x060027E2 RID: 10210 RVA: 0x00063F4C File Offset: 0x00062F4C
		private static void CopyCrossReferences(Signature signOld, CompileContext comconOld, bool bDoCrossReferences, Signature signRet)
		{
			if (bDoCrossReferences)
			{
				foreach (int nId in signOld.ReferencerIds)
				{
					ICompiledPOU compiledPOUById = comconOld.GetCompiledPOUById(nId);
					if (compiledPOUById != null && !compiledPOUById.GetFlag(CompiledPOUFlags.ToTypify))
					{
						signRet.AddReferencer(nId);
					}
				}
			}
		}

		// Token: 0x060027E3 RID: 10211 RVA: 0x00063F94 File Offset: 0x00062F94
		private static bool DoCrossReferences(Signature signOld, CompileContext comcon, CompileContext comconOld)
		{
			return signOld != null && comconOld != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000 || (comcon != null && comcon.TypificationDone));
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x00063FD0 File Offset: 0x00062FD0
		private static void CopyInterfaces(Signature signPre, Signature signRet)
		{
			if (signPre.InterfacesList != null)
			{
				foreach (_IExpression iexpression in signPre.InterfacesList)
				{
					signRet.AddInterface(iexpression.Duplicate() as _IExpression);
				}
			}
		}

		// Token: 0x060027E5 RID: 10213 RVA: 0x00064030 File Offset: 0x00063030
		private static void _CreateCompiledVariables(Signature signPre, Signature signOld, CompileContext comcon, CompileContext comconOld, bool bByteSupport, bool bDoCrossReferences, Signature signRet)
		{
			CompiledSignatureCreator.CopyIDManager(signOld, signRet);
			object varlock = signPre._varlock;
			lock (varlock)
			{
				foreach (_IVariable ivariable in signPre.AllVariables)
				{
					IVariable varRef = null;
					if (signOld != null)
					{
						varRef = signOld[ivariable.VersionedName];
					}
					_IVariable ivariable2 = CompiledSignatureCreator.CreateVariable(ivariable);
					CompiledSignatureCreator.SetVariableID(signRet, varRef, ivariable2);
					if (ivariable2 != null)
					{
						ivariable2.SetFlag(VarFlag.IsCompiled, true);
					}
					signRet.AddVariable(ivariable2);
					CompiledSignatureCreator.HandleNoByteInRetains(comcon, ivariable, ivariable2);
					if (!bByteSupport && ivariable2 != null)
					{
						ivariable2._Type = TypeReplacer.ReplaceTypes(ivariable2._Type, SpecialFeatures.NoByteSupport);
					}
					CompiledSignatureCreator.CopyCrossReferences(comconOld, bDoCrossReferences, varRef, ivariable2);
					CompiledSignatureCreator.CopyCallAttribute(varRef, ivariable2);
				}
			}
		}

		// Token: 0x060027E6 RID: 10214 RVA: 0x00064128 File Offset: 0x00063128
		private static void SetVariableID(Signature signRet, IVariable varRef, _IVariable varNew)
		{
			if (varRef == null)
			{
				if (varNew != null)
				{
					varNew.Id = signRet.NextId;
					return;
				}
			}
			else if (varNew != null)
			{
				varNew.Id = varRef.Id;
			}
		}

		// Token: 0x060027E7 RID: 10215 RVA: 0x0006414C File Offset: 0x0006314C
		private static _IVariable CreateVariable(_IVariable var)
		{
			_IVariable result;
			if (var is AbstractGreenVariable)
			{
				result = GreenVariableFactory.CreateRedVariable(var as _IVariable2);
			}
			else
			{
				result = (var.Duplicate() as _IVariable);
			}
			return result;
		}

		// Token: 0x060027E8 RID: 10216 RVA: 0x0006417C File Offset: 0x0006317C
		private static void CopyIDManager(Signature signOld, Signature signRet)
		{
			if (signOld != null)
			{
				signRet.IdMan = signOld.IdMan;
			}
		}

		// Token: 0x060027E9 RID: 10217 RVA: 0x0006418D File Offset: 0x0006318D
		private static void CopyCallAttribute(IVariable varRef, _IVariable varNew)
		{
			if (varRef != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700 && varRef.HasAttribute("@callattribute"))
			{
				varNew.SetAttributeValue("@callattribute", varRef.GetAttributeValue("@callattribute"));
			}
		}

		// Token: 0x060027EA RID: 10218 RVA: 0x000641C8 File Offset: 0x000631C8
		private static void CopyCrossReferences(CompileContext comconOld, bool bDoCrossReferences, IVariable varRef, _IVariable varNew)
		{
			if (bDoCrossReferences && varRef != null)
			{
				foreach (ICrossReference crossReference in varRef.CrossReferences)
				{
					ICompiledPOU compiledPOUById = comconOld.GetCompiledPOUById(crossReference.CodeId);
					if (compiledPOUById != null && !compiledPOUById.GetFlag(CompiledPOUFlags.ToTypify))
					{
						varNew.AddCrossReference(crossReference.CodeId, null);
					}
				}
			}
		}

		// Token: 0x060027EB RID: 10219 RVA: 0x00064220 File Offset: 0x00063220
		private static void HandleNoByteInRetains(CompileContext comcon, _IVariable var, _IVariable varNew)
		{
			if (var.GetFlag(VarFlag.Retain) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33010 && comcon != null && LocalTargetSettings.NoBytesInRetain.GetBoolValue(comcon.GetTargetSettings()))
			{
				if (varNew.Type.Class == TypeClass.Byte)
				{
					varNew._Type = new RetainByteType();
					return;
				}
				if (varNew.Type.Class == TypeClass.Bool)
				{
					varNew._Type = new RetainBoolType();
					return;
				}
				if (varNew.Type.Class == TypeClass.SInt)
				{
					varNew._Type = new RetainSIntType();
					return;
				}
				if (varNew.Type.Class == TypeClass.USInt)
				{
					varNew._Type = new RetainUSIntType();
				}
			}
		}
	}
}
