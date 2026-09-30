using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000EA RID: 234
	internal static class MemoryChecker
	{
		// Token: 0x0600103B RID: 4155 RVA: 0x0002DBA4 File Offset: 0x0002BDA4
		internal static bool \u0001(_ICompileContext \u0002, IList<ICompiledPOU4> \u0003, bool \u0004)
		{
			bool flag = false;
			_IDataManager u = \u0019.\u0003.\u0001();
			MemoryCompiler.\u0001(u, \u0002.DataManager._MemorySettings, Array.ConvertAll<IArea, _IArea>(\u0002.DataManager.Areas, new Converter<IArea, _IArea>(MemoryChecker.<>c.<>9.\u0001)), \u0002.DataManager.FirstArea);
			for (int i = 0; i < \u0003.Count; i++)
			{
				_ICompiledPOU icompiledPOU = \u0003[i] as _ICompiledPOU;
				if ((!\u0004 || icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile) || \u0002.DataManager._MemorySettings.OnlineChangeInOwnSegment) && !icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode))
				{
					if (icompiledPOU.CompiledCode == null)
					{
						flag = true;
					}
					else if (!MemoryCompiler.\u0002(u, icompiledPOU.CompiledCode.Location.Area, icompiledPOU.CompiledCode.Location.Offset, icompiledPOU.CompiledCode.CodeSize, DataSegmentFlags.None))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				string u2 = string.Format(\u0081.\u0002.Err_InternalErrorProhibitingOnlineChange, 3);
				IMessage message = \u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_InternalErrorProhibitingOnlineChange);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
				return false;
			}
			return true;
		}
	}
}
