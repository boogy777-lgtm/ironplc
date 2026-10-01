using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u001E;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace \u000F
{
	// Token: 0x020001F0 RID: 496
	internal sealed class \u000F
	{
		// Token: 0x1700065D RID: 1629
		// (get) Token: 0x060021B4 RID: 8628 RVA: 0x00073E48 File Offset: 0x00072048
		private ICompileContext CompileContext { get; }

		// Token: 0x1700065E RID: 1630
		// (get) Token: 0x060021B5 RID: 8629 RVA: 0x00073E50 File Offset: 0x00072050
		// (set) Token: 0x060021B6 RID: 8630 RVA: 0x00073E58 File Offset: 0x00072058
		private \u001E.\u0008 PersistenceInformation { get; set; }

		// Token: 0x060021B7 RID: 8631 RVA: 0x00073E64 File Offset: 0x00072064
		internal \u000F(ICompileContext \u0001\u0002)
		{
			this.CompileContext = \u0001\u0002;
		}

		// Token: 0x060021B8 RID: 8632 RVA: 0x00073E74 File Offset: 0x00072074
		internal void \u0002(\u001E.\u0008 \u0002)
		{
			this.PersistenceInformation = \u0002;
			bool u = ((ICompileContext18)this.CompileContext).IsDefined("CheckAllPoolObjects");
			ITaskInfo[] allTasks = this.CompileContext.AllTasks;
			\u0080.\u0005.\u0002 u2 = \u0080.\u0005.\u0001();
			this.\u0001(u, allTasks, u2);
			this.PersistenceInformation.\u0001();
		}

		// Token: 0x060021B9 RID: 8633 RVA: 0x00073EC4 File Offset: 0x000720C4
		private void \u0001(bool \u0002, _ISignature \u0003, LList<string> \u0004, HashSet<string> \u0005, _IVariable \u0006, int \u0007, byte[] \u0008)
		{
			string u0003_u = \u0004[\u0007] + "." + \u0006.VersionedName;
			bool u0004_u = !\u0005.Contains(\u0004[\u0007]);
			global::\u0003.\u000E u = new global::\u0003.\u000E(u0003_u, \u0006, \u0003, u0004_u);
			if (\u0002)
			{
				this.PersistenceInformation.PersistentInstancesForCheckAllPoolObjects.\u0001(u);
				return;
			}
			foreach (byte u2 in \u0008)
			{
				this.PersistenceInformation.\u0001(u2).\u0001(u);
			}
		}

		// Token: 0x060021BA RID: 8634 RVA: 0x00073F48 File Offset: 0x00072148
		private void \u0001(bool \u0002, ITaskInfo[] \u0003, \u0080.\u0005.\u0002 \u0004)
		{
			foreach (_ISignature isignature in this.CompileContext.AllSignatures.OfType<_ISignature>())
			{
				if (isignature.GetFlag(SignatureFlag.Persistent))
				{
					this.PersistenceInformation.Signature = isignature;
				}
				else if (isignature.GetFlag(SignatureFlag.ContainsPersistent))
				{
					IEnumerable allVariables = isignature.AllVariables;
					LList<_IVariable> llist = new LList<_IVariable>();
					LList<_ISignature> llist2 = new LList<_ISignature>();
					\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
					{
						\u0003 = true,
						\u0004 = true,
						\u0005 = false,
						\u0001 = true,
						\u0002 = true,
						\u0006 = true
					};
					LList<string> llist3 = InstancePathService.\u0001(this.CompileContext as _ICompileContext, isignature, Array.Empty<int>(), \u0004, llist, llist2, u);
					u.\u0002 = false;
					LList<string> llist4 = InstancePathService.\u0001(this.CompileContext as _ICompileContext, isignature, Array.Empty<int>(), \u0004, llist, llist2, u);
					HashSet<string> hashSet = new HashSet<string>();
					Enumerable.AddRange<string>(hashSet, llist4);
					foreach (_IVariable ivariable in allVariables.OfType<_IVariable>())
					{
						if (ivariable.GetFlag(VarFlag.LocalPersistent))
						{
							for (int i = 0; i < llist3.Count; i++)
							{
								byte[] u2 = this.\u0001(\u0003, isignature, llist, llist2, ivariable, i);
								this.\u0001(\u0002, isignature, llist3, hashSet, ivariable, i, u2);
							}
						}
					}
				}
			}
		}

		// Token: 0x060021BB RID: 8635 RVA: 0x00074108 File Offset: 0x00072308
		private byte[] \u0001(ITaskInfo[] \u0002, _ISignature \u0003, LList<_IVariable> \u0004, LList<_ISignature> \u0005, _IVariable \u0006, int \u0007)
		{
			byte[] array;
			if (\u0004.Count == 0 && \u0005.Count == 0)
			{
				array = StaticSignatureTaskReferenceDetector.\u0001(\u0006, \u0003, true, this.CompileContext);
			}
			else
			{
				array = StaticSignatureTaskReferenceDetector.\u0001(\u0004[\u0007], \u0005[\u0007], true, this.CompileContext);
			}
			if (array.Length == 0 && this.CompileContext.AllTasks.Length != 0)
			{
				array = new byte[1];
				for (int i = 0; i < \u0002.Length; i++)
				{
					TaskInfoValues taskInfoValues = TaskInfoValueCollector.\u0001(\u0002[i], this.CompileContext);
					if (taskInfoValues.dwVersion >= 0 && taskInfoValues.KindOf == "_implicit_cyclic")
					{
						array[0] = (byte)i;
					}
				}
			}
			return array;
		}

		// Token: 0x040005C8 RID: 1480
		[CompilerGenerated]
		private readonly ICompileContext \u0001;

		// Token: 0x040005C9 RID: 1481
		[CompilerGenerated]
		private \u001E.\u0008 \u0001;
	}
}
