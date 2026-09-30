using System;
using \u0003;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;
using \u0084;

namespace \u0004
{
	// Token: 0x02000383 RID: 899
	internal sealed class \u0015 : \u001E
	{
		// Token: 0x06003494 RID: 13460 RVA: 0x000CF5A8 File Offset: 0x000CD7A8
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			string u = \u0081.\u0001.GenerateCodeInit;
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(\u0002.cmc, message);
			ushort maxValue = ushort.MaxValue;
			int u2 = -1;
			_ISignature isignature = null;
			if (\u0002.ComconNew.ConcurrentOnlineChange)
			{
				isignature = CodeInitGenerator.\u0001(\u0002.ComconNew, true);
			}
			_ICompiledPOU icompiledPOU = CodeInitGenerator.\u0001(\u0002.ComconNew, true, isignature != null, \u0002.ComconOld, out isignature, true);
			\u0002.codegeneration.\u0003(icompiledPOU);
			if (!MemoryCompiler.\u0003(\u0002.ComconNew.DataManager, ref maxValue, ref u2, \u0002.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, \u0002.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				u = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(\u0002.cmc, message);
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u2);
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
			\u0002.ComconNew.AddCompiledPOU(icompiledPOU, isignature, true, null);
			return true;
		}
	}
}
