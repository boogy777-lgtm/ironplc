using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0002;
using \u0007;
using \u0016;
using \u001A;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u000E
{
	// Token: 0x020003EA RID: 1002
	internal sealed class \u001A
	{
		// Token: 0x17000917 RID: 2327
		// (get) Token: 0x060037A4 RID: 14244 RVA: 0x000E4948 File Offset: 0x000E2B48
		// (set) Token: 0x060037A5 RID: 14245 RVA: 0x000E4950 File Offset: 0x000E2B50
		private _ICompileContext CompileContext { get; set; }

		// Token: 0x17000918 RID: 2328
		// (get) Token: 0x060037A6 RID: 14246 RVA: 0x000E495C File Offset: 0x000E2B5C
		// (set) Token: 0x060037A7 RID: 14247 RVA: 0x000E4964 File Offset: 0x000E2B64
		private bool OnlineChange { get; set; }

		// Token: 0x17000919 RID: 2329
		// (get) Token: 0x060037A8 RID: 14248 RVA: 0x000E4970 File Offset: 0x000E2B70
		// (set) Token: 0x060037A9 RID: 14249 RVA: 0x000E4978 File Offset: 0x000E2B78
		private \u0018 RelocationStrategy { get; set; }

		// Token: 0x060037AA RID: 14250 RVA: 0x000E4984 File Offset: 0x000E2B84
		private \u001A(_ICompileContext \u008E\u0005, bool \u008A\u0004)
		{
			this.CompileContext = \u008E\u0005;
			this.OnlineChange = \u008A\u0004;
		}

		// Token: 0x060037AB RID: 14251 RVA: 0x000E499C File Offset: 0x000E2B9C
		public static \u001A \u0001(_ICompileContext \u0002, bool \u0003, int[] \u0004)
		{
			\u001A u001A = new \u001A(\u0002, \u0003);
			\u001A.\u0016 u = new \u001A.\u0016(\u0002, \u0004);
			u001A.RelocationStrategy = u;
			return u001A;
		}

		// Token: 0x060037AC RID: 14252 RVA: 0x000E49C0 File Offset: 0x000E2BC0
		public static global::\u0016.\u0016 \u0001(_ICompileContext \u0002, bool \u0003, bool \u0004, out global::\u0016.\u0016 \u0005)
		{
			\u001A u001A = new \u001A(\u0002, \u0003);
			_IMemorySettings2 imemorySettings = \u0002.DataManager._MemorySettings as _IMemorySettings2;
			global::\u0002.\u0013 u;
			if (imemorySettings != null && imemorySettings.LateRelocationForFixedAreas)
			{
				u = new global::\u0007.\u0014(\u0004, \u0002);
			}
			else
			{
				u = new global::\u0002.\u0013(\u0004, \u0002);
			}
			u001A.RelocationStrategy = u;
			u001A.\u0001();
			\u0005 = u.BootSorter;
			u.\u0001();
			return u.Sorter;
		}

		// Token: 0x060037AD RID: 14253 RVA: 0x000E4A24 File Offset: 0x000E2C24
		private void \u0001()
		{
			if (this.CompileContext.DataManager._MemorySettings.OnlineChangeInOwnSegment)
			{
				this.OnlineChange = false;
			}
			foreach (_ICompiledPOU icompiledPOU in this.CompileContext.GetCompiledPOUsToCompileEx().OfType<_ICompiledPOU>())
			{
				ICompiledCode2 compiledCode;
				if (\u001A.\u0001(this.CompileContext, this.OnlineChange, icompiledPOU, out compiledCode))
				{
					this.\u0001(icompiledPOU, compiledCode, compiledCode.GetCode());
					this.RelocationStrategy.\u0001(compiledCode as ICompiledCode4);
				}
			}
		}

		// Token: 0x060037AE RID: 14254 RVA: 0x000E4AC8 File Offset: 0x000E2CC8
		public void \u0001(_ICompiledPOU \u0002, ICompiledCode2 \u0003, Stream \u0004)
		{
			IRelocationList relocationList = \u0003.RelocationList;
			if (relocationList == null)
			{
				return;
			}
			IEnumerable<IRelocationAreaList> enumerable = \u001A.\u0001(relocationList);
			bool u = \u0002.GetFlag(CompiledPOUFlags.Blob) || \u0002.GetFlag(CompiledPOUFlags.ConstBlob);
			foreach (IRelocationAreaList relocationAreaList in enumerable)
			{
				int area = relocationAreaList.Area;
				IList<IRelocation> list;
				if (relocationAreaList is IRelocationAreaList2)
				{
					list = (relocationAreaList as IRelocationAreaList2).RelocationsEx;
				}
				else
				{
					LList<IRelocation> llist = new LList<IRelocation>();
					llist.AddRange(relocationAreaList.Relocations);
					list = llist;
				}
				foreach (IRelocation u2 in list)
				{
					this.RelocationStrategy.\u0001(\u0002, \u0004, u, area, u2);
				}
			}
		}

		// Token: 0x060037AF RID: 14255 RVA: 0x000E4BB4 File Offset: 0x000E2DB4
		private static IEnumerable<IRelocationAreaList> \u0001(IRelocationList \u0002)
		{
			IEnumerable<IRelocationAreaList> result;
			if (\u0002 is IRelocationList2)
			{
				result = (\u0002 as IRelocationList2).RelocationAreaListsEx;
			}
			else
			{
				result = \u0002.RelocationAreaLists;
			}
			return result;
		}

		// Token: 0x060037B0 RID: 14256 RVA: 0x000E4BE4 File Offset: 0x000E2DE4
		private static bool \u0001(_ICompileContext \u0002, bool \u0003, _ICompiledPOU \u0004, out ICompiledCode2 \u0005)
		{
			\u0005 = null;
			if (\u0003 && !\u0004.GetFlag(CompiledPOUFlags.ToCompile))
			{
				return false;
			}
			if (\u0004.GetFlag(CompiledPOUFlags.ContainsNoCode))
			{
				return false;
			}
			if (\u0004.CompiledCode.Location == null)
			{
				return false;
			}
			\u0005 = (\u0004.CompiledCode as ICompiledCode2);
			if (\u0005 is ICompiledCode5)
			{
				((ICompiledCode5)\u0005).FinishRelocations(\u0002);
			}
			return \u0005.RelocationList != null;
		}

		// Token: 0x04000AF1 RID: 2801
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000AF2 RID: 2802
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000AF3 RID: 2803
		[CompilerGenerated]
		private \u0018 \u0001;
	}
}
