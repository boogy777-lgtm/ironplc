using System;
using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B3 RID: 179
	[TypeGuid("{3504087F-D7CB-485E-A95E-35E81D5FC2B0}")]
	[StorageVersion("3.5.6.10")]
	public class CompiledCode : GenericObject2, ICompiledCode6, ICompiledCode5, ICompiledCode4, ICompiledCode3, ICompiledCode2, ICompiledCode, ICloneable, _ICompiledCodeStub, ICompiledCodeSerializable2, ICompiledCodeSerializable
	{
		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000A64 RID: 2660 RVA: 0x00017BF8 File Offset: 0x00016BF8
		// (set) Token: 0x06000A65 RID: 2661 RVA: 0x00017C00 File Offset: 0x00016C00
		[DefaultSerialization("flags")]
		[StorageVersion("3.5.6.10")]
		public CompiledCodeFlags Flags { get; set; }

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000A66 RID: 2662 RVA: 0x00017C09 File Offset: 0x00016C09
		// (set) Token: 0x06000A67 RID: 2663 RVA: 0x00017C11 File Offset: 0x00016C11
		[DefaultSerialization("RelocationList")]
		[StorageVersion("3.5.6.10")]
		public IRelocationList RelocationList { get; set; }

		// Token: 0x06000A68 RID: 2664 RVA: 0x00017C1A File Offset: 0x00016C1A
		public CompiledCode()
		{
		}

		// Token: 0x06000A69 RID: 2665 RVA: 0x00017C30 File Offset: 0x00016C30
		public CompiledCode(ICompiledCode compiledcode)
		{
			this._dataloc = compiledcode.Location;
			this.RelocationList = compiledcode.RelocationList;
			byte[] array = new byte[compiledcode.CodeSize];
			this._msCodeStream = new ChunkedMemoryStream(array);
			BinaryWriter binaryWriter = new BinaryWriter(this._msCodeStream);
			compiledcode.GetCode(binaryWriter);
			binaryWriter.Flush();
			if (compiledcode is ICompiledCode4)
			{
				ICompiledCode4 compiledCode = compiledcode as ICompiledCode4;
				foreach (object obj in Enum.GetValues(typeof(CompiledCodeFlags)))
				{
					CompiledCodeFlags compiledCodeFlags = (CompiledCodeFlags)obj;
					if (compiledCode.GetFlag(compiledCodeFlags))
					{
						this.Flags |= compiledCodeFlags;
					}
				}
			}
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x00017D14 File Offset: 0x00016D14
		public bool GetFlag(CompiledCodeFlags flag)
		{
			return (this.Flags & flag) == flag;
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x00017D21 File Offset: 0x00016D21
		[Obsolete("use GetCode with binary writer instead")]
		public Stream GetCode()
		{
			return this._msCodeStream;
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x00017D2C File Offset: 0x00016D2C
		public void GetCode(BinaryWriter binwriter)
		{
			binwriter.Flush();
			object locker = this._locker;
			lock (locker)
			{
				this._msCodeStream.Seek(0L, SeekOrigin.Begin);
				this._msCodeStream.CopyTo(binwriter.BaseStream);
			}
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00017D8C File Offset: 0x00016D8C
		public void WriteCodeBytesToWriter(BinaryWriter bw)
		{
			this.GetCode(bw);
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000A6E RID: 2670 RVA: 0x00017D95 File Offset: 0x00016D95
		public int NumCodeBytes
		{
			get
			{
				return (int)this._msCodeStream.Length;
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000A6F RID: 2671 RVA: 0x00017DA4 File Offset: 0x00016DA4
		// (set) Token: 0x06000A70 RID: 2672 RVA: 0x00017E94 File Offset: 0x00016E94
		[DefaultSerialization("CodeBytesX")]
		[StorageVersion("3.5.6.10")]
		[StorageIgnorable]
		private LList<byte[]> CodeBytesX
		{
			get
			{
				object locker = this._locker;
				LList<byte[]> result;
				lock (locker)
				{
					if (this._msCodeStream == null)
					{
						result = null;
					}
					else
					{
						LList<byte[]> llist = new LList<byte[]>();
						int num = (int)this._msCodeStream.Length / 65536;
						this._msCodeStream.Position = 0L;
						for (int i = 0; i < num; i++)
						{
							byte[] array = new byte[65536];
							this._msCodeStream.Read(array, 0, 65536);
							llist.Add(array);
						}
						int num2 = (int)(this._msCodeStream.Length - this._msCodeStream.Position);
						if (num2 > 0)
						{
							byte[] array2 = new byte[num2];
							this._msCodeStream.Read(array2, 0, num2);
							llist.Add(array2);
						}
						result = llist;
					}
				}
				return result;
			}
			set
			{
				object locker = this._locker;
				lock (locker)
				{
					if (value == null)
					{
						this._msCodeStream = null;
					}
					else
					{
						this._msCodeStream = new ChunkedMemoryStream();
						foreach (byte[] array in value)
						{
							this._msCodeStream.Write(array, 0, array.Length);
						}
					}
				}
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000A71 RID: 2673 RVA: 0x00017D21 File Offset: 0x00016D21
		// (set) Token: 0x06000A72 RID: 2674 RVA: 0x00017F24 File Offset: 0x00016F24
		[Obsolete("do not use stream directly")]
		public Stream CodeBytes
		{
			get
			{
				return this._msCodeStream;
			}
			set
			{
				this._msCodeStream = value;
			}
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00017F24 File Offset: 0x00016F24
		public void SetCode(Stream stream)
		{
			this._msCodeStream = stream;
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x00017F2D File Offset: 0x00016F2D
		// (set) Token: 0x06000A75 RID: 2677 RVA: 0x00017F35 File Offset: 0x00016F35
		public IDataLocation Location
		{
			get
			{
				return this._dataloc;
			}
			set
			{
				this._dataloc = value;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000A76 RID: 2678 RVA: 0x00017F3E File Offset: 0x00016F3E
		public int CodeSize
		{
			get
			{
				if (this._msCodeStream == null)
				{
					return 0;
				}
				return (int)this._msCodeStream.Length;
			}
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x0000677E File Offset: 0x0000577E
		public bool GenerateDisassembly(TextWriter writer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x0000677E File Offset: 0x0000577E
		public void ClearListEntries()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00017F56 File Offset: 0x00016F56
		public void SetFlag(CompiledCodeFlags flag, bool bSet)
		{
			if (!bSet)
			{
				throw new NotImplementedException();
			}
			if (!this.GetFlag(flag) && flag == CompiledCodeFlags.Relocated)
			{
				this.Flags |= flag;
				return;
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void FinishRelocations(ICompileContext comcon)
		{
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x0000677E File Offset: 0x0000577E
		public void PadWithNops(int nSizeBytes)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000187 RID: 391
		[DefaultSerialization("DataLocation")]
		[StorageVersion("3.5.6.10")]
		private IDataLocation _dataloc;

		// Token: 0x04000189 RID: 393
		private readonly object _locker = new object();

		// Token: 0x0400018A RID: 394
		private Stream _msCodeStream;
	}
}
