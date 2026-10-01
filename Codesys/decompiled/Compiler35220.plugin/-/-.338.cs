using System;
using \u0003;
using \u0004;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.OnlineChange;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0002
{
	// Token: 0x02000381 RID: 897
	internal sealed class \u0010 : \u001E
	{
		// Token: 0x06003490 RID: 13456 RVA: 0x000CF3EC File Offset: 0x000CD5EC
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			if (!\u0002.\u0001)
			{
				return true;
			}
			ushort maxValue = ushort.MaxValue;
			int u = -1;
			_ICompiledPOU icompiledPOU = global::\u0004.\u0014.\u0001(\u0002.ComconNew, \u0002.ComconOld, \u0002.codegeneration, false, \u0002.\u0001 as OnlineChangeDetails);
			_ISignature sign = \u0002.ComconNew[icompiledPOU.SignatureId];
			if (!MemoryCompiler.\u0003(\u0002.ComconNew.DataManager, ref maxValue, ref u, \u0002.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, \u0002.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(\u0002.cmc, message);
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u);
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
			\u0002.ComconNew.AddCompiledPOU(icompiledPOU, sign, true, null);
			return true;
		}
	}
}
