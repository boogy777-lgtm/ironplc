using System;
using System.Collections.Generic;
using System.Linq;
using \u0006;
using \u0013;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000249 RID: 585
	internal sealed class \u0015 : global::\u0006.\u0002
	{
		// Token: 0x0600265E RID: 9822 RVA: 0x00085EF4 File Offset: 0x000840F4
		private \u0015(_ISignature \u001C\u0002, _ICompiledPOU \u0012\u0002, _ICompiledPOU \u0019\u0004, _ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u001C\u0002;
			this.\u0001 = \u0012\u0002;
			this.\u0001 = \u0001\u0002;
			if (\u0019\u0004 != null)
			{
				this.\u0001 = \u0019\u0004.TryCatchFPAddresses;
			}
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x00085F24 File Offset: 0x00084124
		public override void \u0001(_ITryCatchStatement \u0002)
		{
			base.\u0001(\u0002);
			int num = 0;
			if (this.\u0001.TryCatchFPAddresses != null)
			{
				num = this.\u0001.TryCatchFPAddresses.Count<IDataLocation>();
			}
			IDataLocation dataLocation;
			if (this.\u0001 != null && this.\u0001.Count<IDataLocation>() > num)
			{
				dataLocation = this.\u0001[num];
				Debug.\u0001(MemoryCompiler.\u0002(this.\u0001.DataManager, dataLocation.Area, dataLocation.Offset, this.\u0001.PointerSize, DataSegmentFlags.None));
			}
			else
			{
				dataLocation = \u0015.\u0001(this.\u0001, this.\u0001);
			}
			\u0002.Index = this.\u0001.AddTryCatchFPAddress(dataLocation);
		}

		// Token: 0x06002660 RID: 9824 RVA: 0x00085FD0 File Offset: 0x000841D0
		private static IDataLocation2 \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			ushort u = 0;
			int u2 = 0;
			if (!MemoryCompiler.\u0003(\u0002.DataManager, ref u, ref u2, \u0002.PointerSize, \u0002.PointerSize, CompilerServicesInternal.NoSegmentation, DataSegmentFlags.Data))
			{
				\u0003.AddMessage(Severity.Error, MessageId.Err_OutOfMemoryFunctionPointer, new object[]
				{
					\u0003.OrgName,
					\u0002.PointerSize
				});
			}
			return \u0019.\u0003.\u0001(u, u2);
		}

		// Token: 0x06002661 RID: 9825 RVA: 0x00086034 File Offset: 0x00084234
		public static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			foreach (_ICompiledPOU icompiledPOU in \u0002.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
			{
				if (icompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
				{
					_ICompiledPOU icompiledPOU2 = null;
					if (\u0003 != null)
					{
						icompiledPOU2 = (\u0003.GetCompiledPOUById(icompiledPOU.SignatureId) as _ICompiledPOU);
					}
					if (icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoParseTree) && icompiledPOU2 != null)
					{
						using (IEnumerator<_IDataLocation> enumerator2 = icompiledPOU2.TryCatchFPAddresses.OfType<_IDataLocation>().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								_IDataLocation idataLocation = enumerator2.Current;
								icompiledPOU.AddTryCatchFPAddress(idataLocation);
								Debug.\u0001(MemoryCompiler.\u0002(\u0002.DataManager, idataLocation.Area, idataLocation.Offset, \u0002.PointerSize, DataSegmentFlags.None));
							}
							continue;
						}
					}
					IStatementTraverser ivisit = new \u0013.\u0001(new \u0015(\u0002[icompiledPOU.SignatureId], icompiledPOU, icompiledPOU2, \u0002));
					((_IStatement)icompiledPOU.ParseTree).Accept(ivisit);
				}
			}
		}

		// Token: 0x04000704 RID: 1796
		private new readonly _ISignature \u0001;

		// Token: 0x04000705 RID: 1797
		private new readonly _ICompiledPOU \u0001;

		// Token: 0x04000706 RID: 1798
		private new readonly _ICompileContext \u0001;

		// Token: 0x04000707 RID: 1799
		private new readonly IList<IDataLocation> \u0001;
	}
}
