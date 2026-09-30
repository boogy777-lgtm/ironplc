using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001B
{
	// Token: 0x02000092 RID: 146
	internal sealed class \u0001
	{
		// Token: 0x06000C47 RID: 3143 RVA: 0x0001D864 File Offset: 0x0001BA64
		public static int \u0001(BinaryReader \u0002)
		{
			return \u0002.ReadInt32();
		}

		// Token: 0x06000C48 RID: 3144 RVA: 0x0001D86C File Offset: 0x0001BA6C
		public static uint \u0001(BinaryReader \u0002)
		{
			return \u0002.ReadUInt32();
		}

		// Token: 0x06000C49 RID: 3145 RVA: 0x0001D874 File Offset: 0x0001BA74
		public static string \u0001(BinaryReader \u0002)
		{
			return \u0002.ReadString();
		}

		// Token: 0x06000C4A RID: 3146 RVA: 0x0001D87C File Offset: 0x0001BA7C
		public Guid \u0001(BinaryReader \u0002)
		{
			if (\u0002.Read(this.\u0001, 0, this.\u0001.Length) != this.\u0001.Length)
			{
				throw new EndOfStreamException();
			}
			return new Guid(this.\u0001);
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x0001D8B0 File Offset: 0x0001BAB0
		public static bool \u0001(BinaryReader \u0002)
		{
			return \u0002.ReadByte() == 0;
		}

		// Token: 0x06000C4C RID: 3148 RVA: 0x0001D8C0 File Offset: 0x0001BAC0
		public static IList<KeyValuePair<\u0001, \u0002>> \u0001<\u0001, \u0002>(BinaryReader \u0002, Func<BinaryReader, \u0001> \u0003, Func<BinaryReader, \u0002> \u0004)
		{
			int num = \u0002.ReadInt32();
			int num2 = \u0002.ReadInt32();
			int num3 = num2 + num;
			if (num3 <= 0)
			{
				return Array.Empty<KeyValuePair<\u0001, \u0002>>();
			}
			List<KeyValuePair<\u0001, \u0002>> list = new List<KeyValuePair<\u0001, \u0002>>(num3);
			for (int i = 0; i < num; i++)
			{
				list.Add(new KeyValuePair<\u0001, \u0002>(\u0003(\u0002), default(\u0002)));
			}
			for (int j = 0; j < num2; j++)
			{
				list.Add(new KeyValuePair<\u0001, \u0002>(\u0003(\u0002), \u0004(\u0002)));
			}
			return list;
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x0001D948 File Offset: 0x0001BB48
		public static IList<KeyValuePair<\u0001, \u0002>> \u0001<\u0001, \u0002>(BinaryReader \u0002, ILMSerializableTypeFactory \u0003, Func<BinaryReader, ILMSerializableTypeFactory, \u0001> \u0004, Func<BinaryReader, ILMSerializableTypeFactory, \u0002> \u0005)
		{
			int num = \u0002.ReadInt32();
			int num2 = \u0002.ReadInt32();
			int num3 = num2 + num;
			if (num3 <= 0)
			{
				return Array.Empty<KeyValuePair<\u0001, \u0002>>();
			}
			List<KeyValuePair<\u0001, \u0002>> list = new List<KeyValuePair<\u0001, \u0002>>(num3);
			for (int i = 0; i < num; i++)
			{
				list.Add(new KeyValuePair<\u0001, \u0002>(\u0004(\u0002, \u0003), default(\u0002)));
			}
			for (int j = 0; j < num2; j++)
			{
				list.Add(new KeyValuePair<\u0001, \u0002>(\u0004(\u0002, \u0003), \u0005(\u0002, \u0003)));
			}
			return list;
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x0001D9D4 File Offset: 0x0001BBD4
		public static IList<\u0001> \u0001<\u0001>(BinaryReader \u0002, Func<BinaryReader, \u0001> \u0003)
		{
			int num = \u0002.ReadInt32();
			if (num == 0)
			{
				return Array.Empty<\u0001>();
			}
			List<\u0001> list = new List<\u0001>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(\u0003(\u0002));
			}
			return list;
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x0001DA14 File Offset: 0x0001BC14
		public static IList<\u0001> \u0001<\u0001>(BinaryReader \u0002, _ILanguageModelBuilder2 \u0003, Func<BinaryReader, _ILanguageModelBuilder2, \u0001> \u0004)
		{
			int num = \u0002.ReadInt32();
			if (num == 0)
			{
				return Array.Empty<\u0001>();
			}
			List<\u0001> list = new List<\u0001>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(\u0004(\u0002, \u0003));
			}
			return list;
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x0001DA54 File Offset: 0x0001BC54
		public static IList<\u0001> \u0002<\u0001>(BinaryReader \u0002, Func<BinaryReader, \u0001> \u0003)
		{
			int num = \u0002.ReadInt32();
			if (num == 0)
			{
				return Array.Empty<\u0001>();
			}
			List<\u0001> list = new List<\u0001>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(\u0003(\u0002));
			}
			return list;
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x0001DA94 File Offset: 0x0001BC94
		public static IDirectVariable \u0001(BinaryReader \u0002, _ILanguageModelBuilder2 \u0003)
		{
			byte b = \u0002.ReadByte();
			if (b == 0)
			{
				return null;
			}
			if (b == 1)
			{
				DirectVariableLocation dirvarlocation = (DirectVariableLocation)\u0002.ReadUInt32();
				return \u0003.CreateIncompleteDirectVariable(dirvarlocation);
			}
			if (b == 2)
			{
				DirectVariableLocation location = (DirectVariableLocation)\u0002.ReadUInt32();
				DirectVariableSize size = (DirectVariableSize)\u0002.ReadUInt32();
				int num = \u0002.ReadInt32();
				int[] array = new int[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = \u0002.ReadInt32();
				}
				return \u0003.CreateDirectVariable(location, size, array);
			}
			throw new FormatException(string.Format("Unknown kind of direct variable {0}.", b));
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0001DB20 File Offset: 0x0001BD20
		public _ICompilerMessage \u0001(BinaryReader \u0002, _ILanguageModelBuilder2 \u0003)
		{
			MessageId number = (MessageId)\u0002.ReadInt32();
			Severity severity = (Severity)\u0002.ReadInt32();
			string stError = \u0002.ReadString();
			int nProjectHandle = \u0002.ReadInt32();
			Guid objectGuid = this.\u0001(\u0002);
			this.\u0001(\u0002);
			long num = \u0002.ReadInt64();
			short nLength = \u0002.ReadInt16();
			ShowAttribute showAttribute = (ShowAttribute)\u0002.ReadInt32();
			long nPosition;
			short sPositionOffset;
			PositionHelper.SplitPosition(num, ref nPosition, ref sPositionOffset);
			_ICompilerMessage icompilerMessage = \u0003.CreateCompilerMessage(\u0003.CreateSourcePosition(nProjectHandle, objectGuid, nPosition, sPositionOffset, nLength), stError, severity, number);
			icompilerMessage.ShowAttribute = showAttribute;
			return icompilerMessage;
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x0001DB9C File Offset: 0x0001BD9C
		public \u001B.\u0001.\u0001 \u0001(BinaryReader \u0002)
		{
			\u001B.\u0001.\u0001 u = new \u001B.\u0001.\u0001();
			u.\u0001 = \u0002.ReadString();
			u.\u0001 = this.\u0001(\u0002);
			u.\u0002 = this.\u0001(\u0002);
			u.\u0002 = \u0002.ReadString();
			if (u.\u0002.Length == 0)
			{
				u.\u0002 = null;
			}
			return u;
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x0001DBF8 File Offset: 0x0001BDF8
		public IStaticMemorySegment \u0001(BinaryReader \u0002, _ILanguageModelBuilder2 \u0003)
		{
			Guid guidSubApplication = this.\u0001(\u0002);
			int offset = \u0002.ReadInt32();
			int size = \u0002.ReadInt32();
			return \u0003.CreateStaticMemorySegment(guidSubApplication, offset, size);
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x0001DC24 File Offset: 0x0001BE24
		public static _IMemoryManager \u0001(BinaryReader \u0002, ILMSerializableTypeFactory \u0003)
		{
			IMemoryManagerSerializable memoryManagerSerializable = \u0003.CreateMemMan();
			memoryManagerSerializable.Size = \u0002.ReadInt32();
			memoryManagerSerializable.Base = \u0002.ReadInt32();
			int num = \u0002.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int nAddress = \u0002.ReadInt32();
				int nSize = \u0002.ReadInt32();
				memoryManagerSerializable.AddGap(nAddress, nSize);
			}
			return (_IMemoryManager)memoryManagerSerializable;
		}

		// Token: 0x06000C56 RID: 3158 RVA: 0x0001DC84 File Offset: 0x0001BE84
		public static DataSegmentFlags \u0001(BinaryReader \u0002, ILMSerializableTypeFactory \u0003)
		{
			return (DataSegmentFlags)\u0002.ReadUInt16();
		}

		// Token: 0x06000C57 RID: 3159 RVA: 0x0001DC8C File Offset: 0x0001BE8C
		public static IList<KeyValuePair<string, string>> \u0001(BinaryReader \u0002)
		{
			return \u001B.\u0001.\u0001<string, string>(\u0002, new Func<BinaryReader, string>(\u001B.\u0001.\u0001), new Func<BinaryReader, string>(\u001B.\u0001.\u0001));
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x0001DCAC File Offset: 0x0001BEAC
		public void \u0001(BinaryReader \u0002, _ISlotPOUList \u0003)
		{
			int num = \u0002.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Guid guidTask = this.\u0001(\u0002);
				int num2 = \u0002.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					int nSlot = \u0002.ReadInt32();
					int num3 = \u0002.ReadInt32();
					for (int k = 0; k < num3; k++)
					{
						Guid guidObject = this.\u0001(\u0002);
						\u0003.Add(guidTask, nSlot, guidObject);
					}
				}
			}
			int num4 = \u0002.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				int nSlot2 = \u0002.ReadInt32();
				Guid guidObject2 = this.\u0001(\u0002);
				\u0003.AddDownloadSlot(nSlot2, guidObject2);
			}
			int num5 = \u0002.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				int nSlot3 = \u0002.ReadInt32();
				Guid guidObject3 = this.\u0001(\u0002);
				\u0003.AddOnlineChangeSlot(nSlot3, guidObject3);
			}
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x0001DD88 File Offset: 0x0001BF88
		public static \u0001 \u0001<\u0001>(BinaryReader \u0002)
		{
			IArchiveReader archiveReader = APEnvironmentFacade.Instance.CreateNewBinaryArchiveReader();
			archiveReader.Initialize(\u0002.BaseStream);
			return (\u0001)((object)archiveReader.Load());
		}

		// Token: 0x04000211 RID: 529
		private readonly byte[] \u0001 = new byte[16];

		// Token: 0x02000093 RID: 147
		public sealed class \u0001
		{
			// Token: 0x04000212 RID: 530
			public string \u0001;

			// Token: 0x04000213 RID: 531
			public Guid \u0001;

			// Token: 0x04000214 RID: 532
			public Guid \u0002;

			// Token: 0x04000215 RID: 533
			public string \u0002;
		}
	}
}
