using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0081
{
	// Token: 0x020000E7 RID: 231
	internal sealed class \u0004
	{
		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x06001025 RID: 4133 RVA: 0x0002D4A0 File Offset: 0x0002B6A0
		private _ICompileContext CompileContext { get; }

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x06001026 RID: 4134 RVA: 0x0002D4A8 File Offset: 0x0002B6A8
		private DownloadInfoFlags DownloadInfoFlags { get; }

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x06001027 RID: 4135 RVA: 0x0002D4B0 File Offset: 0x0002B6B0
		private bool OnlineChange
		{
			get
			{
				return this.DownloadInfoFlags.OnlineChange;
			}
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x06001028 RID: 4136 RVA: 0x0002D4CC File Offset: 0x0002B6CC
		private int[] AreaMapping { get; }

		// Token: 0x06001029 RID: 4137 RVA: 0x0002D4D4 File Offset: 0x0002B6D4
		public \u0004(_ICompileContext \u0001\u0002, DownloadInfoFlags \u0018\u0005, int[] \u0019\u0005)
		{
			this.CompileContext = \u0001\u0002;
			this.DownloadInfoFlags = \u0018\u0005;
			this.AreaMapping = \u0019\u0005;
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x0002D4F4 File Offset: 0x0002B6F4
		internal LList<ICodePiece2> \u0001(IList<ICompiledPOU4> \u0002)
		{
			LList<ICodePiece2> llist = new LList<ICodePiece2>();
			for (int i = 0; i < \u0002.Count; i++)
			{
				_ICompiledPOU icompiledPOU = \u0002[i] as _ICompiledPOU;
				if ((!this.DownloadInfoFlags.OnlineChange || icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile) || this.CompileContext.DataManager._MemorySettings.OnlineChangeInOwnSegment) && !icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode))
				{
					ICompiledCode compiledCode = icompiledPOU.CompiledCode;
					Debug.\u0001(compiledCode != null);
					byte[] u = this.\u0001(icompiledPOU, compiledCode);
					LList<ICodePiece2> u2 = LogicalCodePieceBuilder.\u0001(icompiledPOU, compiledCode, u);
					this.\u0001(llist, icompiledPOU, compiledCode, u2);
				}
			}
			return llist;
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x0002D598 File Offset: 0x0002B798
		private void \u0001(LList<ICodePiece2> \u0002, _ICompiledPOU \u0003, ICompiledCode \u0004, LList<ICodePiece2> \u0005)
		{
			foreach (ICodePiece2 codePiece in \u0005)
			{
				if (!this.DownloadInfoFlags.BootProject || \u0003.GetFlag(CompiledPOUFlags.BootProjectRelevant) || this.DownloadInfoFlags.OfflineBootProject)
				{
					if (\u0004 is ICompiledCode2 && codePiece.Length >= 65536)
					{
						ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream(codePiece.Code);
						int u = (int)chunkedMemoryStream.Length / 65536;
						\u0081.\u0004.\u0001(\u0002, 65536, codePiece, chunkedMemoryStream, u);
						int num = (int)(chunkedMemoryStream.Length - chunkedMemoryStream.Position);
						if (num > 0)
						{
							\u0081.\u0004.\u0001(\u0002, 65536, codePiece, chunkedMemoryStream, u, num);
						}
					}
					else
					{
						\u0002.Add(codePiece);
					}
				}
				else
				{
					\u0002.Add(new CodePiece(codePiece.Length, codePiece.Destination));
				}
			}
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x0002D694 File Offset: 0x0002B894
		private static void \u0001(LList<ICodePiece2> \u0002, int \u0003, ICodePiece2 \u0004, ChunkedMemoryStream \u0005, int \u0006, int \u0007)
		{
			byte[] array = new byte[\u0007];
			\u0005.Read(array, 0, \u0007);
			IDataLocation datalocDestination = \u0019.\u0003.\u0001(\u0004.Destination.Area, \u0004.Destination.Offset + \u0006 * \u0003);
			\u0002.Add(new CodePiece(array, datalocDestination));
		}

		// Token: 0x0600102D RID: 4141 RVA: 0x0002D6E4 File Offset: 0x0002B8E4
		private static void \u0001(LList<ICodePiece2> \u0002, int \u0003, ICodePiece2 \u0004, ChunkedMemoryStream \u0005, int \u0006)
		{
			for (int i = 0; i < \u0006; i++)
			{
				byte[] array = new byte[\u0003];
				\u0005.Read(array, 0, \u0003);
				IDataLocation datalocDestination = \u0019.\u0003.\u0001(\u0004.Destination.Area, \u0004.Destination.Offset + i * \u0003);
				\u0002.Add(new CodePiece(array, datalocDestination));
			}
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x0002D73C File Offset: 0x0002B93C
		private byte[] \u0001(_ICompiledPOU \u0002, ICompiledCode \u0003)
		{
			byte[] array = new byte[\u0003.CodeSize];
			BinaryWriter binaryWriter = new BinaryWriter(new ChunkedMemoryStream(array));
			\u0003.GetCode(binaryWriter);
			binaryWriter.Flush();
			if (this.AreaMapping != null)
			{
				ICompiledCode2 compiledCode = \u0003 as ICompiledCode2;
				Debug.\u0001(compiledCode != null);
				\u001A.\u0001(this.CompileContext, this.OnlineChange, this.AreaMapping).\u0001(\u0002, compiledCode, binaryWriter.BaseStream);
				binaryWriter.Flush();
			}
			return array;
		}

		// Token: 0x040002DE RID: 734
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040002DF RID: 735
		[CompilerGenerated]
		private readonly DownloadInfoFlags \u0001;

		// Token: 0x040002E0 RID: 736
		[CompilerGenerated]
		private readonly int[] \u0001;
	}
}
