using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001F7 RID: 503
	public static class StaticSignatureTaskReferenceDetector
	{
		// Token: 0x060021EE RID: 8686 RVA: 0x00075980 File Offset: 0x00073B80
		internal static byte[] \u0001(IVariable \u0002, ISignature \u0003, bool \u0004, ICompileContext \u0005)
		{
			LList<ITaskCrossref> llist = StaticSignatureTaskReferenceDetector.\u0001(\u0002, \u0003, \u0004, true, \u0005);
			byte[] array = new byte[llist.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = llist[i].TaskId;
			}
			return array;
		}

		// Token: 0x060021EF RID: 8687 RVA: 0x000759C4 File Offset: 0x00073BC4
		internal static LList<ITaskCrossref> \u0001(IVariable \u0002, ISignature \u0003, bool \u0004, bool \u0005, ICompileContext \u0006)
		{
			LSortedList<byte, int> lsortedList = new LSortedList<byte, int>();
			IEnumerable<int> enumerable;
			if (\u0004)
			{
				IVariableWithModifyingAccesses variableWithModifyingAccesses = \u0002 as IVariableWithModifyingAccesses;
				if (variableWithModifyingAccesses != null)
				{
					enumerable = variableWithModifyingAccesses.GetModifyingCrossReferences();
					goto IL_47;
				}
			}
			enumerable = \u0002.CrossReferences.Select(new Func<ICrossReference, int>(StaticSignatureTaskReferenceDetector.<>c.<>9.\u0001));
			IL_47:
			if (\u0005)
			{
				foreach (byte b in \u0003.TaskReferenceList)
				{
					lsortedList[b] = -1;
				}
			}
			foreach (int num in enumerable)
			{
				ISignature signature = \u0006.CreateGlobalIScope()[num];
				if (signature != null)
				{
					foreach (byte b2 in signature.TaskReferenceList)
					{
						lsortedList[b2] = num;
					}
				}
			}
			LList<ITaskCrossref> llist = new LList<ITaskCrossref>(lsortedList.Keys.Count);
			foreach (byte b3 in lsortedList.Keys)
			{
				llist.Add(new StaticSignatureTaskReferenceDetector.\u0001(b3, new StaticSignatureTaskReferenceDetector.\u0002(lsortedList[b3]), \u0003.Id));
			}
			return llist;
		}

		// Token: 0x020001F8 RID: 504
		internal sealed class \u0001 : ITaskCrossref
		{
			// Token: 0x060021F0 RID: 8688 RVA: 0x00075B34 File Offset: 0x00073D34
			internal \u0001(byte \u009F\u0004, ICrossReference \u0001\u0005, int \u0011\u0002)
			{
				this.TaskId = \u009F\u0004;
				this.CrossRef = \u0001\u0005;
				this.SignatureId = \u0011\u0002;
			}

			// Token: 0x1700066C RID: 1644
			// (get) Token: 0x060021F1 RID: 8689 RVA: 0x00075B54 File Offset: 0x00073D54
			public byte TaskId { get; }

			// Token: 0x1700066D RID: 1645
			// (get) Token: 0x060021F2 RID: 8690 RVA: 0x00075B5C File Offset: 0x00073D5C
			public ICrossReference CrossRef { get; }

			// Token: 0x1700066E RID: 1646
			// (get) Token: 0x060021F3 RID: 8691 RVA: 0x00075B64 File Offset: 0x00073D64
			public int SignatureId { get; }

			// Token: 0x040005E5 RID: 1509
			[CompilerGenerated]
			private readonly byte \u0001;

			// Token: 0x040005E6 RID: 1510
			[CompilerGenerated]
			private readonly ICrossReference \u0001;

			// Token: 0x040005E7 RID: 1511
			[CompilerGenerated]
			private readonly int \u0001;
		}

		// Token: 0x020001F9 RID: 505
		private sealed class \u0002 : ICrossReference
		{
			// Token: 0x060021F4 RID: 8692 RVA: 0x00075B6C File Offset: 0x00073D6C
			internal \u0002(int \u007F\u0008)
			{
				this.CodeId = \u007F\u0008;
			}

			// Token: 0x1700066F RID: 1647
			// (get) Token: 0x060021F5 RID: 8693 RVA: 0x00075B7C File Offset: 0x00073D7C
			public ICodePosition[] Positions
			{
				get
				{
					throw new NotSupportedException("code positions are not supported");
				}
			}

			// Token: 0x17000670 RID: 1648
			// (get) Token: 0x060021F6 RID: 8694 RVA: 0x00075B88 File Offset: 0x00073D88
			public int CodeId { get; }

			// Token: 0x040005E8 RID: 1512
			[CompilerGenerated]
			private readonly int \u0001;
		}
	}
}
