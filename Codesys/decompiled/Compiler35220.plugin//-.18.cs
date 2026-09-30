using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u0004;
using \u0007;
using \u0014;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0081
{
	// Token: 0x020003DF RID: 991
	internal sealed class \u0017
	{
		// Token: 0x17000901 RID: 2305
		// (get) Token: 0x0600375F RID: 14175 RVA: 0x000E3F0C File Offset: 0x000E210C
		private _ICompileContext CompileContext { get; }

		// Token: 0x17000902 RID: 2306
		// (get) Token: 0x06003760 RID: 14176 RVA: 0x000E3F14 File Offset: 0x000E2114
		private Codegeneration Codegeneration { get; }

		// Token: 0x17000903 RID: 2307
		// (get) Token: 0x06003761 RID: 14177 RVA: 0x000E3F1C File Offset: 0x000E211C
		// (set) Token: 0x06003762 RID: 14178 RVA: 0x000E3F24 File Offset: 0x000E2124
		public bool AllocationError { get; set; }

		// Token: 0x06003763 RID: 14179 RVA: 0x000E3F30 File Offset: 0x000E2130
		public \u0017(_ICompileContext \u001C\u0004, Codegeneration \u000E\u0002)
		{
			this.CompileContext = \u001C\u0004;
			this.Codegeneration = \u000E\u0002;
		}

		// Token: 0x06003764 RID: 14180 RVA: 0x000E3F48 File Offset: 0x000E2148
		internal _ISignature \u0001(int \u0002, int \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine(string.Format("__RELOCATIONTABLE : ARRAY [0..{0}] OF WORD;", \u0002));
			lstringBuilder.AppendLine(string.Format("__RELOCATIONTABLENEW : ARRAY [0..{0}] OF WORD;", \u0003));
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001("__GLOBAL_RELOC_DEFINITIONS", lstringBuilder.ToString(), true);
			isignature = isignature.CreateCompiledSignature(null, this.CompileContext.HasByteSupport());
			IScope5 scope = global::\u0007.\u0005.\u0001(this.CompileContext, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, this.CompileContext);
			return isignature;
		}

		// Token: 0x06003765 RID: 14181 RVA: 0x000E3FF0 File Offset: 0x000E21F0
		internal _ICompiledPOU \u0001(global::\u0016.\u0016 \u0002, out int \u0003)
		{
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(IdentifierConstants.RelocateOffsetTable);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
			IList<\u001A> list = \u0002.Lists;
			\u0002.\u0001 = new int[list.Count];
			BinaryWriter binaryWriter = new BinaryWriter(new ChunkedMemoryStream());
			Swapper swapper = new Swapper(this.Codegeneration.Codegenerator.MotorolaByteOrder);
			\u0003 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				\u001A u001A = list[i];
				short num = 0;
				long position = 0L;
				short num2 = 0;
				\u0002.\u0001[i] = 0;
				for (int j = 0; j < u001A.\u0001.Length; j++)
				{
					IntegerUnion integerUnion = default(IntegerUnion);
					integerUnion.m_int0 = u001A.\u0001[j];
					if (j == 0 || num != integerUnion.m_short1)
					{
						num = integerUnion.m_short1;
						if (j != 0)
						{
							long position2 = binaryWriter.BaseStream.Position;
							binaryWriter.BaseStream.Position = position;
							binaryWriter.Write(swapper.Swap(num2));
							binaryWriter.BaseStream.Position = position2;
							\u0002.\u0001[i]++;
						}
						binaryWriter.Write(swapper.Swap(num));
						position = binaryWriter.BaseStream.Position;
						binaryWriter.Write(0);
						num2 = 0;
					}
					binaryWriter.Write(swapper.Swap(integerUnion.m_short0));
					num2 += 1;
				}
				if (u001A.\u0001.Length != 0)
				{
					long position3 = binaryWriter.BaseStream.Position;
					binaryWriter.BaseStream.Position = position;
					binaryWriter.Write(swapper.Swap(num2));
					binaryWriter.BaseStream.Position = position3;
				}
				\u0003 += u001A.\u0001.Length;
			}
			binaryWriter.Flush();
			binaryWriter.BaseStream.Flush();
			icompiledPOU.CompiledCode = \u0019.\u0003.\u0001(binaryWriter.BaseStream as ChunkedMemoryStream, this.CompileContext.Codegenerator.MotorolaByteOrder);
			icompiledPOU.SetParseTree(\u0019.\u0003.\u0001());
			return icompiledPOU;
		}

		// Token: 0x04000ADE RID: 2782
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000ADF RID: 2783
		[CompilerGenerated]
		private readonly Codegeneration \u0001;

		// Token: 0x04000AE0 RID: 2784
		[CompilerGenerated]
		private bool \u0001;
	}
}
