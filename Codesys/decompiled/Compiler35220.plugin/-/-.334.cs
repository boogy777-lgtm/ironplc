using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using \u0018;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;
using \u0082;
using \u0084;

namespace \u001F
{
	// Token: 0x02000379 RID: 889
	internal sealed class \u0014 : \u001E
	{
		// Token: 0x0600344E RID: 13390 RVA: 0x000CDB90 File Offset: 0x000CBD90
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			ushort maxValue = ushort.MaxValue;
			int u = -1;
			\u0082.\u0004 u000F_u = new \u0082.\u0004();
			\u0002.codegen = CompilerServicesInternal.\u0001(\u0002.DeviceGuid, \u0002.ApplicationGuid, \u0002.ComconNew.SimulationMode, \u0002.KeepCompileInformation);
			\u0002.codegeneration = new Codegeneration(\u0002.ComconNew, \u0002.ComconNew, true, true, \u0002.codegen);
			new \u001E.\u0005(new ConcurrentQueue<_ICompiledPOU>(\u0002.compiledpous), \u0002.codegeneration, u000F_u).\u0001();
			List<ICompiledPOU4> cpous = new List<ICompiledPOU4>();
			IMemoryAllocationCallback dsfcallback = \u0002.ComconNew.DSFCallback;
			if (dsfcallback != null)
			{
				dsfcallback.OnBeforeCodeAllocation(cpous, \u0002.ComconNew);
			}
			bool flag = true;
			ILMCompileOptions3 ilmcompileOptions = APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.CompileOptions as ILMCompileOptions3;
			if (ilmcompileOptions != null)
			{
				flag = ilmcompileOptions.ReportCompiledPousDuringIncrementalCompile;
			}
			foreach (_ICompiledPOU icompiledPOU in \u0002.compiledpous)
			{
				DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Code;
				_ISignature sign = \u0002.ComconNew[icompiledPOU.SignatureId];
				if (\u0002.ComconNew.DSFCallback != null)
				{
					dataSegmentFlags = \u0002.ComconNew.DSFCallback.GetDataSegmentFlagForCode(icompiledPOU, sign, \u0002.ComconNew, dataSegmentFlags);
				}
				if (flag)
				{
					string u2 = string.Format(\u0081.\u0001.GenerateCode, icompiledPOU.GetFullName(\u0002.ComconNew));
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u2, Severity.Information, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
				}
				if (!MemoryCompiler.\u0003(\u0002.ComconNew.DataManager, ref maxValue, ref u, \u0002.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, \u0002.ComconNew.DataManager._MemorySettings.CodeSegmentSize, dataSegmentFlags))
				{
					return false;
				}
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u);
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			}
			return true;
		}
	}
}
