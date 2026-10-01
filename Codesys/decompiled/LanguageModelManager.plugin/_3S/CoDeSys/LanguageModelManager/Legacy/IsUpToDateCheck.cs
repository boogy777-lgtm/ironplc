using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000277 RID: 631
	internal static class IsUpToDateCheck
	{
		// Token: 0x06002A59 RID: 10841 RVA: 0x0006C81C File Offset: 0x0006B81C
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		internal static bool IsLmUpToDate(CompileContext comcon, _IPreCompileContext precomp, _IPreCompileContext precompPool, _IIsUpTopDateStrategy strategy)
		{
			if (comcon.SimulationMode != precomp.SimulationMode && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 && strategy.SimulationModeChanged())
			{
				return strategy.IsUpToDate;
			}
			foreach (_IPreCompileContext ipreCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(true, true).OfType<_IPreCompileContext>())
			{
				ipreCompileContext.RemoveTimeStampOnlyObjects();
			}
			if (comcon.DefineChanged(precomp) && strategy.DefinesChanged())
			{
				return strategy.IsUpToDate;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800 && IsUpToDateCheck.CompileOptionsChanged(comcon) && strategy.CompileOptionsChanged())
			{
				return strategy.IsUpToDate;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100)
			{
				Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(comcon.ApplicationGuid);
				Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(comcon.ApplicationGuid);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && parentApplication != Guid.Empty && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) != null && !(APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) as _ICompileContext).IsUpToDate() && strategy.ParentContextChanged())
				{
					return strategy.IsUpToDate;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && parentApplication != Guid.Empty && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) == null)
				{
					if (strategy.ParentContextNull())
					{
						return strategy.IsUpToDate;
					}
				}
				else
				{
					uint num = MemorySettingsHelperX._GetMemorySettings(deviceOfApplication, parentApplication, comcon.ApplicationGuid, comcon.SimulationMode).CalculateChecksum();
					if (comcon.MemorySettingsChecksum != 0U && comcon.MemorySettingsChecksum != num && strategy.MemorySettingsChanged())
					{
						return strategy.IsUpToDate;
					}
				}
			}
			if (!comcon.LibraryParamTablesEqual(precomp) && strategy.LibraryParamTablesChanged())
			{
				return strategy.IsUpToDate;
			}
			if (!comcon.LibraryListsEqual(precomp, precompPool) && strategy.LibraryListChanged())
			{
				return strategy.IsUpToDate;
			}
			if (IsUpToDateCheck.GetGlobalErrors(comcon).Any<_ICompilerMessage>() && strategy.GlobalError())
			{
				return strategy.IsUpToDate;
			}
			if (IsUpToDateCheck.CheckForChangedSignature(precomp, precompPool, comcon, strategy))
			{
				return strategy.IsUpToDate;
			}
			if (IsUpToDateCheck.CheckForChangedPou(precomp, precompPool, comcon, strategy))
			{
				return strategy.IsUpToDate;
			}
			bool flag = true;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				flag = false;
				uint? num2 = CompilerProxy.CalculateOCRelevantPreComNamesHash(new _IPreCompileContext[]
				{
					precomp,
					precompPool
				});
				if (num2 == null)
				{
					flag = true;
				}
				else
				{
					uint? num3 = num2;
					uint precompileContextNamesChecksum = comcon.PrecompileContextNamesChecksum;
					if (!(num3.GetValueOrDefault() == precompileContextNamesChecksum & num3 != null))
					{
						flag = true;
						if (strategy.PrecomNameHashChanged())
						{
							return strategy.IsUpToDate;
						}
					}
				}
			}
			if (flag)
			{
				if (IsUpToDateCheck.CheckForNewSignInPrecompile(precomp, comcon, strategy))
				{
					return strategy.IsUpToDate;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100 && IsUpToDateCheck.CheckForAdditionalPoolSignature(precompPool, comcon, strategy))
				{
					return strategy.IsUpToDate;
				}
			}
			strategy.CheckGlobalOnlineChangePreConditions();
			return true;
		}

		// Token: 0x06002A5A RID: 10842 RVA: 0x0006CB1C File Offset: 0x0006BB1C
		private static IEnumerable<_ICompilerMessage> GetGlobalErrors(_ICompileContext comcon)
		{
			return from msg in CompilerProxy.GetGlobalErrors(comcon)
			where msg.Severity == Severity.Error || msg.Severity == Severity.FatalError
			select msg;
		}

		// Token: 0x06002A5B RID: 10843 RVA: 0x0006CB48 File Offset: 0x0006BB48
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForAdditionalPoolSignature(IPreCompileContext precompPool, CompileContext comcon, _IIsUpTopDateStrategy strategy)
		{
			foreach (_ISignature isignature in precompPool.AllSignatures.OfType<_ISignature>())
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600 || isignature.GetFlag(SignatureFlag.TopLevel))
				{
					_ISignature isignature2 = comcon[isignature.ObjectGuid];
					if (isignature2 == null || isignature2.Name != isignature.Name)
					{
						isignature2 = comcon[isignature.Name];
					}
					if ((isignature2 == null || (isignature2.Checksum != 0U && isignature2.Checksum != isignature.Checksum) || (isignature2.Checksum == 0U && isignature2.TimeStamp != isignature.TimeStamp)) && strategy.AdditionalSignInPool(isignature, isignature2))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002A5C RID: 10844 RVA: 0x0006CC28 File Offset: 0x0006BC28
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForChangedSignature(_IPreCompileContext precomp, _IPreCompileContext precompPool, CompileContext comcon, _IIsUpTopDateStrategy strategy)
		{
			foreach (_ISignature isignature in comcon._AllFlat)
			{
				strategy.CheckNextPOU();
				bool flag = isignature.GetFlag(SignatureFlag.SuperGlobal);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
				{
					flag = (isignature.GetFlag(SignatureFlag.SuperGlobal) && !isignature.GetFlag(SignatureFlag.InhibitOnlineChange));
				}
				if (flag && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
				{
					flag = false;
				}
				if (!(isignature.ObjectGuid == Guid.Empty) && !isignature.GetFlag(SignatureFlag.Generated) && !flag)
				{
					ISignature[] array = null;
					_IPreCompileContext ipreCompileContext = null;
					_ISignature precompileSignature = CompilerProxy.GetPrecompileSignature(comcon, isignature, precomp, precompPool, out array, out ipreCompileContext);
					if (precompileSignature != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700 && !comcon.SimulationMode && precompileSignature.GetFlag(SignatureFlag.External) != isignature.GetFlag(SignatureFlag.External) && strategy.ExternalSignatureFlagChanged())
					{
						return true;
					}
					if (precompileSignature == null || (isignature.Checksum != 0U && precompileSignature.Checksum != isignature.Checksum) || (isignature.Checksum == 0U && precompileSignature.TimeStamp != isignature.TimeStamp))
					{
						if ((precompileSignature != null && isignature.GetFlag(SignatureFlag.InterfaceLibraryObject) && isignature.POUType == Operator.VarGlobal && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200) || (precompileSignature != null && isignature.GetFlag(SignatureFlag.InterfaceLibraryObject) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35370))
						{
							continue;
						}
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400 && isignature.HasAttribute("omit_uptodate_check"))
						{
							if (precompileSignature != null && isignature.HasAttribute("uptodate_check_checksum") && precompileSignature.HasAttribute("uptodate_check_checksum") && isignature.GetAttributeValue("uptodate_check_checksum") != precompileSignature.GetAttributeValue("uptodate_check_checksum") && strategy.SignatureChangedByChecksumAttribute(isignature, precompileSignature))
							{
								return true;
							}
							continue;
						}
						else if (strategy.SignatureChanged(isignature, precompileSignature))
						{
							return true;
						}
					}
					int num = 0;
					foreach (object obj in isignature._SubSignatures)
					{
						ISignature signature = (ISignature)obj;
						if (!(signature.ObjectGuid == Guid.Empty) && !signature.GetFlag(SignatureFlag.Generated))
						{
							num++;
						}
					}
					IEnumerable<ISignature> enumerable;
					if (array == null)
					{
						enumerable = null;
					}
					else
					{
						enumerable = from x in array
						where !x.GetFlag(SignatureFlag.Generated)
						select x;
					}
					IEnumerable<ISignature> enumerable2 = enumerable;
					if (enumerable2 != null && enumerable2.Count<ISignature>() > num && strategy.SubSignaturesChanged(isignature))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002A5D RID: 10845 RVA: 0x0006CF48 File Offset: 0x0006BF48
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForChangedPou(_IPreCompileContext precomp, _IPreCompileContext precompPool, CompileContext comcon, _IIsUpTopDateStrategy strategy)
		{
			foreach (_ICompiledPOU icompiledPOU in comcon.CompiledPOUList)
			{
				if (!(icompiledPOU.ObjectGuid == Guid.Empty) && !icompiledPOU.GetFlag(CompiledPOUFlags.NotForUpToDate))
				{
					_ICompiledPOU precompiledPOU = CompilerProxy.GetPrecompiledPOU(comcon, icompiledPOU, precomp, precompPool);
					if (precompiledPOU == null || (icompiledPOU.Checksum != 0U && precompiledPOU.Checksum != icompiledPOU.Checksum) || (icompiledPOU.Checksum == 0U && precompiledPOU.TimeStamp != icompiledPOU.TimeStamp))
					{
						ISignature signature = comcon[icompiledPOU.SignatureId];
						if (signature == null || !signature.GetFlag(SignatureFlag.SuperGlobal) || signature.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
						{
							if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
							{
								ISignature signature2;
								if (signature == null || !(signature.Name == IdentifierConstants.MainSignatureName))
								{
									signature2 = signature;
								}
								else
								{
									ISignature signature3 = comcon[signature.ParentSignatureId];
									signature2 = signature3;
								}
								ISignature signature4 = signature2;
								if (signature4 != null && signature4.HasAttribute("omit_uptodate_check"))
								{
									continue;
								}
							}
							if (strategy.CompiledPouChanged(signature as _ISignature, icompiledPOU, precompiledPOU))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002A5E RID: 10846 RVA: 0x0006D094 File Offset: 0x0006C094
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForNewSignInPrecompile(_IPreCompileContext precomp, CompileContext comcon, _IIsUpTopDateStrategy strategy)
		{
			foreach (_ISignature isignature in precomp.AllSignatures.OfType<_ISignature>())
			{
				bool flag = isignature.GetFlag(SignatureFlag.SuperGlobal);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
				{
					flag = (isignature.GetFlag(SignatureFlag.SuperGlobal) && !isignature.GetFlag(SignatureFlag.InhibitOnlineChange));
				}
				if (flag && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
				{
					flag = false;
				}
				if (!isignature.GetFlag(SignatureFlag.Generated) && !flag)
				{
					bool flag2 = isignature.GetFlag(SignatureFlag.TopLevel);
					if (flag2 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
					{
						_ISignature isignature2;
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34500 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000))
						{
							isignature2 = comcon[isignature.GetSearchName(comcon)];
						}
						else
						{
							isignature2 = comcon[isignature.ObjectGuid];
						}
						if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
						{
							if (isignature2 == null && !flag2 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
							{
								continue;
							}
						}
						else if (isignature2 == null && isignature.HasFlag(SignatureFlag.TimeStampOnly))
						{
							continue;
						}
						if (isignature2 == null || isignature2.Name != isignature.Name)
						{
							isignature2 = comcon[isignature.Name];
						}
						if (isignature2 != null && isignature2.GetFlag(SignatureFlag.Generated))
						{
							isignature.SetFlag(SignatureFlag.Generated, true);
						}
						else if ((isignature2 == null || (isignature2.Checksum != 0U && isignature2.Checksum != isignature.Checksum) || (isignature2.Checksum == 0U && isignature2.TimeStamp != isignature.TimeStamp)) && strategy.AdditionalSignInPrecompile(isignature, isignature2))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002A5F RID: 10847 RVA: 0x0006D2A0 File Offset: 0x0006C2A0
		private static bool CompileOptionsChanged(CompileContext comcon)
		{
			return comcon.CompileOptionsSavedWith != null && comcon.CompileOptionsSavedWith.CompileOptionsChanged();
		}
	}
}
