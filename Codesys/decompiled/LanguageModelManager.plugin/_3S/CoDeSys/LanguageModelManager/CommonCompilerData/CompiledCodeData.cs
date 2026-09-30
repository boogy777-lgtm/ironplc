using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C6 RID: 454
	[TypeGuid("{2f0e740d-b75f-401a-a7f2-ecd76eb6ee2d}")]
	[StorageVersion("3.3.0.0")]
	public class CompiledCodeData : GenericObject2, _ICompiledCodeStub, _ICompiledCodeData, ICompiledCode4, ICompiledCode3, ICompiledCode2, ICompiledCode, ICloneable, ICompiledCodeDataSerializable, ICompiledCodeSerializable, ICompiledCodeSerializable2
	{
		// Token: 0x1700085D RID: 2141
		// (get) Token: 0x0600200A RID: 8202 RVA: 0x00058E17 File Offset: 0x00057E17
		// (set) Token: 0x0600200B RID: 8203 RVA: 0x00058E1F File Offset: 0x00057E1F
		[DefaultSerialization("CocoFlags")]
		[StorageVersion("3.5.1.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public CompiledCodeFlags Flags { get; set; }

		// Token: 0x1700085E RID: 2142
		// (get) Token: 0x0600200C RID: 8204 RVA: 0x00058E28 File Offset: 0x00057E28
		// (set) Token: 0x0600200D RID: 8205 RVA: 0x00058EA0 File Offset: 0x00057EA0
		[DefaultSerialization("Code")]
		[StorageVersion("3.3.0.0-3.5.0.99")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		protected byte[] OldCode
		{
			get
			{
				if (this.m_msCodeStream == null)
				{
					return Array.Empty<byte>();
				}
				object locker = this._locker;
				byte[] result;
				lock (locker)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						this.m_msCodeStream.CopyTo(memoryStream);
						result = memoryStream.ToArray();
					}
				}
				return result;
			}
			set
			{
				object locker = this._locker;
				lock (locker)
				{
					if (value != null)
					{
						this.m_msCodeStream = new ChunkedMemoryStream(value);
					}
				}
			}
		}

		// Token: 0x1700085F RID: 2143
		// (get) Token: 0x0600200E RID: 8206 RVA: 0x00058EEC File Offset: 0x00057EEC
		// (set) Token: 0x0600200F RID: 8207 RVA: 0x00058F44 File Offset: 0x00057F44
		[DefaultSerialization("CodeBytesX")]
		[StorageVersion("3.5.1.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		protected LList<byte[]> CodeBytesX
		{
			get
			{
				if (this.m_msCodeStream == null)
				{
					return null;
				}
				LList<byte[]> llist = new LList<byte[]>();
				object locker = this._locker;
				lock (locker)
				{
					llist.AddRange(this.GetAllCodeChunks());
				}
				return llist;
			}
			set
			{
				if (value == null)
				{
					this.m_msCodeStream = null;
					return;
				}
				this.m_msCodeStream = new ChunkedMemoryStream();
				object locker = this._locker;
				lock (locker)
				{
					foreach (byte[] array in value)
					{
						this.m_msCodeStream.Write(array, 0, array.Length);
					}
				}
			}
		}

		// Token: 0x17000860 RID: 2144
		// (get) Token: 0x06002010 RID: 8208 RVA: 0x00058FD4 File Offset: 0x00057FD4
		// (set) Token: 0x06002011 RID: 8209 RVA: 0x00058FDC File Offset: 0x00057FDC
		public Stream CodeBytes
		{
			get
			{
				return this.m_msCodeStream;
			}
			set
			{
				this.m_msCodeStream = value;
			}
		}

		// Token: 0x17000861 RID: 2145
		// (get) Token: 0x06002012 RID: 8210 RVA: 0x00058FE5 File Offset: 0x00057FE5
		// (set) Token: 0x06002013 RID: 8211 RVA: 0x00058FED File Offset: 0x00057FED
		[DefaultSerialization("related_id")]
		[StorageVersion("3.3.1.0")]
		[StorageIgnorable]
		public int RelatedId { get; set; } = -1;

		// Token: 0x06002014 RID: 8212 RVA: 0x00058FF6 File Offset: 0x00057FF6
		public CompiledCodeData()
		{
		}

		// Token: 0x06002015 RID: 8213 RVA: 0x00059010 File Offset: 0x00058010
		[Obsolete("Nix Use")]
		public CompiledCodeData(byte[] abyCode, int nRelatedId)
		{
			this.m_msCodeStream = new ChunkedMemoryStream();
			this.m_msCodeStream.Write(abyCode, 0, abyCode.Length);
			this.RelatedId = nRelatedId;
		}

		// Token: 0x06002016 RID: 8214 RVA: 0x0005904C File Offset: 0x0005804C
		internal CompiledCodeData(Stream stream, int nRelatedId)
		{
			this.m_msCodeStream = stream;
			this.RelatedId = nRelatedId;
		}

		// Token: 0x17000862 RID: 2146
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x00004E6B File Offset: 0x00003E6B
		public int Errors
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x17000863 RID: 2147
		// (get) Token: 0x06002018 RID: 8216 RVA: 0x00004E6B File Offset: 0x00003E6B
		public int Warnings
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06002019 RID: 8217 RVA: 0x00059074 File Offset: 0x00058074
		private bool EqualBytes(byte[] bytes1, byte[] bytes2)
		{
			for (int i = 0; i < bytes1.Length; i++)
			{
				if (bytes1[i] != bytes2[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600201A RID: 8218 RVA: 0x0005909C File Offset: 0x0005809C
		public override int GetHashCode()
		{
			int num = 0;
			object locker = this._locker;
			lock (locker)
			{
				foreach (byte[] array in this.GetAllCodeChunks())
				{
					foreach (byte b in array)
					{
						num ^= (int)b;
					}
				}
			}
			return num;
		}

		// Token: 0x0600201B RID: 8219 RVA: 0x0005912C File Offset: 0x0005812C
		public override bool Equals(object obj)
		{
			return obj is CompiledCodeData && this.EqualCode(obj as CompiledCodeData);
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x00059144 File Offset: 0x00058144
		internal bool EqualCode(CompiledCodeData ccd)
		{
			if (ccd.CodeSize != this.CodeSize)
			{
				return false;
			}
			Stream msCodeStream = ccd.m_msCodeStream;
			if (this.m_msCodeStream == null && msCodeStream == null)
			{
				return true;
			}
			if (this.m_msCodeStream == null || msCodeStream == null)
			{
				return false;
			}
			object locker = this._locker;
			lock (locker)
			{
				IEnumerator enumerator = ccd.GetAllCodeChunksArray().GetEnumerator();
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35340 || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100)
				{
					enumerator.MoveNext();
				}
				foreach (byte[] bytes in this.GetAllCodeChunks())
				{
					byte[] array = enumerator.Current as byte[];
					if (array == null)
					{
						return false;
					}
					if (!this.EqualBytes(bytes, array))
					{
						return false;
					}
					enumerator.MoveNext();
				}
			}
			return true;
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x00059250 File Offset: 0x00058250
		public void GetCode(BinaryWriter binwriter)
		{
			if (this.m_msCodeStream == null)
			{
				return;
			}
			object locker = this._locker;
			lock (locker)
			{
				foreach (byte[] buffer in this.GetAllCodeChunks())
				{
					binwriter.Write(buffer);
				}
			}
		}

		// Token: 0x0600201E RID: 8222 RVA: 0x00058FD4 File Offset: 0x00057FD4
		[Obsolete("use GetCode with binary writer instead")]
		public Stream GetCode()
		{
			return this.m_msCodeStream;
		}

		// Token: 0x17000864 RID: 2148
		// (get) Token: 0x0600201F RID: 8223 RVA: 0x000592D0 File Offset: 0x000582D0
		public int CodeSize
		{
			get
			{
				if (this.m_msCodeStream == null)
				{
					return 0;
				}
				return (int)this.m_msCodeStream.Length;
			}
		}

		// Token: 0x06002020 RID: 8224 RVA: 0x000592E8 File Offset: 0x000582E8
		public void WriteCodeBytesToWriter(BinaryWriter bw)
		{
			this.GetCode(bw);
		}

		// Token: 0x06002021 RID: 8225 RVA: 0x00058FDC File Offset: 0x00057FDC
		public void SetCode(Stream stream)
		{
			this.m_msCodeStream = stream;
		}

		// Token: 0x17000865 RID: 2149
		// (get) Token: 0x06002022 RID: 8226 RVA: 0x000592F1 File Offset: 0x000582F1
		public int NumCodeBytes
		{
			get
			{
				return this.CodeSize;
			}
		}

		// Token: 0x06002023 RID: 8227 RVA: 0x000592FC File Offset: 0x000582FC
		internal virtual bool GenerateDisassembly(TextWriter writer, byte[] bytes)
		{
			string format = "{0:X4}: {1:X2} {2:X2} {3:X2} {4:X2}";
			int i;
			for (i = 0; i < bytes.Length - 4; i += 4)
			{
				writer.WriteLine(format, new object[]
				{
					i,
					bytes[i],
					bytes[i + 1],
					bytes[i + 2],
					bytes[i + 3]
				});
			}
			if (bytes.Length % 4 == 0)
			{
				return true;
			}
			byte[] array = new byte[4];
			for (int j = i; j < bytes.Length; j++)
			{
				array[j - i] = bytes[j];
			}
			writer.WriteLine(format, new object[]
			{
				i,
				array[0],
				array[1],
				array[2],
				array[3]
			});
			return true;
		}

		// Token: 0x06002024 RID: 8228 RVA: 0x000593D0 File Offset: 0x000583D0
		public virtual bool GenerateDisassembly(TextWriter writer)
		{
			if (this.m_msCodeStream == null)
			{
				return true;
			}
			object locker = this._locker;
			lock (locker)
			{
				foreach (byte[] bytes in this.GetAllCodeChunks())
				{
					this.GenerateDisassembly(writer, bytes);
				}
			}
			return true;
		}

		// Token: 0x06002025 RID: 8229 RVA: 0x00059454 File Offset: 0x00058454
		private byte[][] GetAllCodeChunksArray()
		{
			object locker = this._locker;
			byte[][] result;
			lock (locker)
			{
				result = this.GetAllCodeChunks().ToArray<byte[]>();
			}
			return result;
		}

		// Token: 0x06002026 RID: 8230 RVA: 0x0005949C File Offset: 0x0005849C
		private IEnumerable<byte[]> GetAllCodeChunks()
		{
			if (this.m_msCodeStream == null)
			{
				yield break;
			}
			int num = (int)this.m_msCodeStream.Length;
			int nCountCodePieces = num / 65536;
			this.m_msCodeStream.Position = 0L;
			int num2;
			for (int ii = 0; ii < nCountCodePieces; ii = num2 + 1)
			{
				byte[] array = new byte[65536];
				this.m_msCodeStream.Read(array, 0, 65536);
				yield return array;
				num2 = ii;
			}
			int num3 = (int)(this.m_msCodeStream.Length - this.m_msCodeStream.Position);
			if (num3 > 0)
			{
				byte[] array2 = new byte[num3];
				this.m_msCodeStream.Read(array2, 0, num3);
				yield return array2;
			}
			yield break;
		}

		// Token: 0x17000866 RID: 2150
		// (get) Token: 0x06002027 RID: 8231 RVA: 0x000594AC File Offset: 0x000584AC
		// (set) Token: 0x06002028 RID: 8232 RVA: 0x000594B4 File Offset: 0x000584B4
		public IDataLocation Location
		{
			get
			{
				return this.m_datalocation;
			}
			set
			{
				this.m_datalocation = value;
			}
		}

		// Token: 0x17000867 RID: 2151
		// (get) Token: 0x06002029 RID: 8233 RVA: 0x000594BD File Offset: 0x000584BD
		public string[] ErrorMessages
		{
			get
			{
				return Array.Empty<string>();
			}
		}

		// Token: 0x17000868 RID: 2152
		// (get) Token: 0x0600202A RID: 8234 RVA: 0x000594BD File Offset: 0x000584BD
		public string[] WarningMessages
		{
			get
			{
				return Array.Empty<string>();
			}
		}

		// Token: 0x17000869 RID: 2153
		// (get) Token: 0x0600202B RID: 8235 RVA: 0x000594C4 File Offset: 0x000584C4
		// (set) Token: 0x0600202C RID: 8236 RVA: 0x000594CC File Offset: 0x000584CC
		public IRelocationList RelocationList
		{
			get
			{
				return this._relocationlist;
			}
			set
			{
				this._relocationlist = value;
			}
		}

		// Token: 0x0600202D RID: 8237 RVA: 0x000594D5 File Offset: 0x000584D5
		public bool GetFlag(CompiledCodeFlags flag)
		{
			return (this.Flags & flag) == flag;
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x000594E2 File Offset: 0x000584E2
		public void SetFlag(CompiledCodeFlags flag, bool bSet)
		{
			if (bSet)
			{
				this.Flags |= flag;
				return;
			}
			this.Flags &= ~flag;
		}

		// Token: 0x0600202F RID: 8239 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void ClearListEntries()
		{
		}

		// Token: 0x04000652 RID: 1618
		[DefaultSerialization("DataLocation")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected IDataLocation m_datalocation;

		// Token: 0x04000653 RID: 1619
		private readonly object _locker = new object();

		// Token: 0x04000654 RID: 1620
		protected Stream m_msCodeStream;

		// Token: 0x04000655 RID: 1621
		[DefaultSerialization("reloclist")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IRelocationList _relocationlist;
	}
}
