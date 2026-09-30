using System;
using \u0003;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001C
{
	// Token: 0x020000DF RID: 223
	internal static class \u0005
	{
		// Token: 0x06000FBC RID: 4028 RVA: 0x0002B9B4 File Offset: 0x00029BB4
		public static void \u0001(_ICompileContext \u0002)
		{
			try
			{
				\u001C.\u0005.\u0001 = \u0002;
				foreach (_ICompiledPOU icompiledPOU in \u001C.\u0005.\u0001.CompiledPOUList)
				{
					if (icompiledPOU.BitWriteAccesses != null)
					{
						foreach (IBitWriteAccess u in icompiledPOU.BitWriteAccesses)
						{
							\u001C.\u0005.\u0001(u);
						}
					}
				}
				foreach (int num in \u001C.\u0005.\u0001.Keys)
				{
					ushort num2 = (ushort)num;
					LDictionary<int, LDictionary<byte, LList<IBitWriteAccess>>> ldictionary = \u001C.\u0005.\u0001[(int)num2];
					foreach (int num3 in ldictionary.Keys)
					{
						LDictionary<byte, LList<IBitWriteAccess>> ldictionary2 = ldictionary[num3];
						if (ldictionary2.Values.Count > 1 && \u001C.\u0005.\u0001(ldictionary2))
						{
							\u001C.\u0005.\u0001(ldictionary2);
						}
					}
				}
			}
			finally
			{
				\u001C.\u0005.\u0001 = null;
				\u001C.\u0005.\u0001.Clear();
				\u001C.\u0005.\u0001.Clear();
			}
		}

		// Token: 0x06000FBD RID: 4029 RVA: 0x0002BB64 File Offset: 0x00029D64
		private static void \u0001(IBitWriteAccess \u0002)
		{
			LDictionary<int, LDictionary<byte, LList<IBitWriteAccess>>> ldictionary = null;
			if (!\u001C.\u0005.\u0001.TryGetValue(\u0002.Area, ref ldictionary))
			{
				ldictionary = new LDictionary<int, LDictionary<byte, LList<IBitWriteAccess>>>();
				\u001C.\u0005.\u0001[\u0002.Area] = ldictionary;
			}
			LDictionary<byte, LList<IBitWriteAccess>> ldictionary2 = null;
			if (!ldictionary.TryGetValue(\u0002.Offset, ref ldictionary2))
			{
				ldictionary2 = new LDictionary<byte, LList<IBitWriteAccess>>();
				ldictionary[\u0002.Offset] = ldictionary2;
			}
			LList<IBitWriteAccess> llist = null;
			if (!ldictionary2.TryGetValue(\u0002.BitNr, ref llist))
			{
				llist = new LList<IBitWriteAccess>();
				ldictionary2[\u0002.BitNr] = llist;
			}
			llist.Add(\u0002);
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x0002BBF0 File Offset: 0x00029DF0
		private static void \u0001(LDictionary<byte, LList<IBitWriteAccess>> \u0002)
		{
			foreach (byte b in \u0002.Keys)
			{
				foreach (IBitWriteAccess bitWriteAccess in \u0002[b])
				{
					_ISignature isignature = \u001C.\u0005.\u0001[bitWriteAccess.SignatureId];
					_ISourcePosition u = null;
					if (bitWriteAccess.Position != null)
					{
						u = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid, bitWriteAccess.Position.EditorPosition, bitWriteAccess.Position.PositionOffset, (short)bitWriteAccess.Symbol.Length);
					}
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Wrn_ConcurrentAccessOfBitInSameByte, new object[]
					{
						bitWriteAccess.Symbol
					});
					isignature.AddError(\u0019.\u0003.\u0001(u, u2, Severity.Warning, MessageId.Wrn_ConcurrentAccessOfBitInSameByte));
				}
			}
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x0002BD18 File Offset: 0x00029F18
		private static bool \u0001(LDictionary<byte, LList<IBitWriteAccess>> \u0002)
		{
			\u001C.\u0005.\u0001.Clear();
			int num = 0;
			foreach (byte b in \u0002.Keys)
			{
				foreach (IBitWriteAccess bitWriteAccess in \u0002[b])
				{
					foreach (byte b2 in \u001C.\u0005.\u0001[bitWriteAccess.SignatureId].TaskReferenceList)
					{
						if (num == 0)
						{
							if (!\u001C.\u0005.\u0001.ContainsKey(b2))
							{
								\u001C.\u0005.\u0001.Add(b2, b2);
							}
						}
						else if (!\u001C.\u0005.\u0001.ContainsKey(b2) || \u001C.\u0005.\u0001.Count > 1)
						{
							return true;
						}
					}
				}
				num++;
			}
			return false;
		}

		// Token: 0x040002B6 RID: 694
		private static LDictionary<int, LDictionary<int, LDictionary<byte, LList<IBitWriteAccess>>>> \u0001 = new LDictionary<int, LDictionary<int, LDictionary<byte, LList<IBitWriteAccess>>>>();

		// Token: 0x040002B7 RID: 695
		private static _ICompileContext \u0001 = null;

		// Token: 0x040002B8 RID: 696
		private static LDictionary<byte, byte> \u0001 = new LDictionary<byte, byte>();
	}
}
