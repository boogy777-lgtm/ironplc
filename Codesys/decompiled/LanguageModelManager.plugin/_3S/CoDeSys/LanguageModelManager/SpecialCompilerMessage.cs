using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200013C RID: 316
	[TypeGuid("{5334709E-3244-43A1-8104-F5000DF4A177}")]
	[StorageVersion("3.5.0.0")]
	internal class SpecialCompilerMessage : CompilerMessage
	{
		// Token: 0x06001AD5 RID: 6869 RVA: 0x0004C7C9 File Offset: 0x0004B7C9
		public SpecialCompilerMessage()
		{
		}

		// Token: 0x06001AD6 RID: 6870 RVA: 0x0004C7D1 File Offset: 0x0004B7D1
		internal SpecialCompilerMessage(IMinimalPosition position, string stError, Severity severity, short sLength, uint uiNumber, string stPrefix) : base(position, stError, severity, sLength, MessageId.None)
		{
			this._number = new uint?(uiNumber);
			this._stPrefix = stPrefix;
		}

		// Token: 0x17000725 RID: 1829
		// (get) Token: 0x06001AD7 RID: 6871 RVA: 0x0004C7F4 File Offset: 0x0004B7F4
		public override uint? Number
		{
			get
			{
				return this._number;
			}
		}

		// Token: 0x17000726 RID: 1830
		// (get) Token: 0x06001AD8 RID: 6872 RVA: 0x0004C7FC File Offset: 0x0004B7FC
		public override string Prefix
		{
			get
			{
				return this._stPrefix;
			}
		}

		// Token: 0x0400059B RID: 1435
		[DefaultSerialization("Number")]
		[StorageVersion("3.5.0.0")]
		[Obfuscation(Feature = "rename")]
		private uint? _number;

		// Token: 0x0400059C RID: 1436
		[DefaultSerialization("Prefix")]
		[StorageVersion("3.5.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stPrefix;
	}
}
