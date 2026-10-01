using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000188 RID: 392
	public class VarDeclarationChecksumGenerator : _IVariableDeclarationChecksumGenerator
	{
		// Token: 0x06001B68 RID: 7016 RVA: 0x0005AD90 File Offset: 0x00058F90
		public VarDeclarationChecksumGenerator()
		{
			this.\u0001 = \u0019.\u0001.\u0001(false);
			this.\u0001 = new BinaryWriter(this.\u0001);
		}

		// Token: 0x06001B69 RID: 7017 RVA: 0x0005ADB8 File Offset: 0x00058FB8
		public uint GenerateChecksum(IEnumerable<IVariable> varList)
		{
			int num = 0;
			foreach (IVariable variable in varList)
			{
				_IVariable ivariable = (_IVariable)variable;
				num++;
				this.\u0001.Write(ivariable.Name.ToString(CultureInfo.InvariantCulture));
				if (ivariable.Type != null)
				{
					if (ivariable.Type.Class == TypeClass.Reference)
					{
						this.\u0001.Write("REFERENCE TO ");
					}
					this.\u0001.Write(ivariable.Type.ToString());
				}
				this.\u0001.Write((long)ivariable.Flags);
				if (ivariable.Address != null)
				{
					this.\u0001.Write(ivariable.Address.ToString());
				}
			}
			this.\u0001.Write(num);
			this.\u0001.Flush();
			this.\u0001.Close();
			return this.\u0001.Checksum;
		}

		// Token: 0x040004BC RID: 1212
		private readonly ChecksumStream \u0001;

		// Token: 0x040004BD RID: 1213
		private readonly BinaryWriter \u0001;
	}
}
