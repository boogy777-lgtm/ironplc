using System;
using System.IO;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0012
{
	// Token: 0x020003E9 RID: 1001
	internal sealed class \u0016
	{
		// Token: 0x17000916 RID: 2326
		// (get) Token: 0x0600379B RID: 14235 RVA: 0x000E4754 File Offset: 0x000E2954
		// (set) Token: 0x0600379C RID: 14236 RVA: 0x000E475C File Offset: 0x000E295C
		private ICodeRelocater SpecificCodeRelocator { get; set; }

		// Token: 0x0600379D RID: 14237 RVA: 0x000E4768 File Offset: 0x000E2968
		internal \u0016(bool \u008C\u0004, ICodegenerator \u0007\u0004)
		{
			this.\u0001 = \u008C\u0004;
			this.SpecificCodeRelocator = (\u0007\u0004 as ICodeRelocater);
		}

		// Token: 0x0600379E RID: 14238 RVA: 0x000E4784 File Offset: 0x000E2984
		private static void \u0001(ref IntegerUnion \u0002)
		{
			byte @byte = \u0002.m_byte0;
			byte byte2 = \u0002.m_byte1;
			\u0002.m_byte0 = \u0002.m_byte3;
			\u0002.m_byte1 = \u0002.m_byte2;
			\u0002.m_byte2 = byte2;
			\u0002.m_byte3 = @byte;
		}

		// Token: 0x0600379F RID: 14239 RVA: 0x000E47C8 File Offset: 0x000E29C8
		internal void \u0001(bool \u0002, Stream \u0003, int \u0004, int \u0005)
		{
			if (\u0002)
			{
				this.\u0002(\u0003, \u0004, \u0005);
				return;
			}
			this.\u0001(\u0003, \u0004, \u0005);
		}

		// Token: 0x060037A0 RID: 14240 RVA: 0x000E47E4 File Offset: 0x000E29E4
		internal void \u0001(Stream \u0002, IRelocation \u0003, int \u0004)
		{
			if (this.SpecificCodeRelocator == null)
			{
				this.\u0002(\u0002, \u0003, \u0004);
				return;
			}
			if (this.SpecificCodeRelocator is ICodeRelocater2)
			{
				(this.SpecificCodeRelocator as ICodeRelocater2).DoRelocation(\u0002, \u0003, \u0004);
				return;
			}
			this.SpecificCodeRelocator.DoRelocation(\u0002, \u0003.Offset, \u0004);
		}

		// Token: 0x060037A1 RID: 14241 RVA: 0x000E4838 File Offset: 0x000E2A38
		private void \u0001(Stream \u0002, int \u0003, int \u0004)
		{
			if (this.SpecificCodeRelocator != null)
			{
				this.SpecificCodeRelocator.DoRelocation(\u0002, \u0003, \u0004);
				return;
			}
			this.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x060037A2 RID: 14242 RVA: 0x000E485C File Offset: 0x000E2A5C
		private void \u0002(Stream \u0002, int \u0003, int \u0004)
		{
			long position = \u0002.Position;
			byte[] array = new byte[4];
			IntegerUnion integerUnion = default(IntegerUnion);
			\u0002.Position = (long)\u0003;
			\u0002.Read(array, 0, 4);
			integerUnion.\u0002(array);
			if (this.\u0001)
			{
				\u0016.\u0001(ref integerUnion);
			}
			integerUnion.m_uint0 += (uint)\u0004;
			if (this.\u0001)
			{
				\u0016.\u0001(ref integerUnion);
			}
			integerUnion.\u0001(array);
			\u0002.Position = (long)\u0003;
			\u0002.Write(array, 0, 4);
			\u0002.Position = position;
		}

		// Token: 0x060037A3 RID: 14243 RVA: 0x000E48E8 File Offset: 0x000E2AE8
		private void \u0002(Stream \u0002, IRelocation \u0003, int \u0004)
		{
			if (\u0003 is IDirectCallRelocation)
			{
				byte[] buffer = new byte[]
				{
					0,
					0,
					0,
					0
				};
				long position = \u0002.Position;
				\u0002.Position = (long)\u0003.Offset;
				\u0002.Write(buffer, 0, 4);
				\u0002.Position = position;
			}
			this.\u0001(\u0002, \u0003.Offset, \u0004);
		}

		// Token: 0x04000AEF RID: 2799
		private readonly bool \u0001;

		// Token: 0x04000AF0 RID: 2800
		[CompilerGenerated]
		private ICodeRelocater \u0001;
	}
}
