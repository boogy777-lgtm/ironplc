using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200013D RID: 317
	[TypeGuid("{bc2be951-49f6-4f0f-b731-e31e36606f1e}")]
	[StorageVersion("3.3.0.0")]
	[DebuggerDisplay("{Severity} {Text}}")]
	public class CompilerMessage : GenericObject2, _ICompilerMessage, IMessage4, IMessage3, IMessage2, IMessage
	{
		// Token: 0x06001AD9 RID: 6873 RVA: 0x0004C804 File Offset: 0x0004B804
		public CompilerMessage()
		{
		}

		// Token: 0x06001ADA RID: 6874 RVA: 0x0004C824 File Offset: 0x0004B824
		internal CompilerMessage(ISourcePosition position, string stError, Severity severity, MessageId Number)
		{
			this.m_position = position;
			this.m_stError = stError;
			this.m_severity = severity;
			if (position != null)
			{
				this.m_sLength = position.Length;
			}
			else
			{
				this.m_sLength = 0;
				this.m_ShowAttribute &= ~ShowAttribute.Precompile;
			}
			this._mid = Number;
		}

		// Token: 0x06001ADB RID: 6875 RVA: 0x0004C894 File Offset: 0x0004B894
		internal CompilerMessage(IMinimalPosition position, string stError, Severity severity, short sLength, MessageId Number)
		{
			if (position == null)
			{
				this.m_position = new SourcePosition(-1, Guid.Empty, 0L, 0, sLength);
			}
			else
			{
				this.m_position = new SourcePosition(-1, Guid.Empty, position.EditorPosition, position.PositionOffset, sLength);
			}
			this.m_stError = stError;
			this.m_severity = severity;
			if (position != null)
			{
				this.m_sLength = sLength;
			}
			else
			{
				this.m_sLength = 0;
				this.m_ShowAttribute &= ~ShowAttribute.Precompile;
			}
			this._mid = Number;
		}

		// Token: 0x17000727 RID: 1831
		// (get) Token: 0x06001ADC RID: 6876 RVA: 0x0004C933 File Offset: 0x0004B933
		// (set) Token: 0x06001ADD RID: 6877 RVA: 0x0004C94C File Offset: 0x0004B94C
		public int ProjectHandle
		{
			get
			{
				if (this.m_position == null)
				{
					return -1;
				}
				return this.m_position.ProjectHandle;
			}
			set
			{
				if (this.m_position != null)
				{
					SourcePosition position = new SourcePosition(value, this.m_position.ObjectGuid, this.m_position.Position, this.m_position.PositionOffset, this.m_position.Length);
					this.m_position = position;
				}
			}
		}

		// Token: 0x17000728 RID: 1832
		// (get) Token: 0x06001ADE RID: 6878 RVA: 0x0004C99B File Offset: 0x0004B99B
		// (set) Token: 0x06001ADF RID: 6879 RVA: 0x0004C9B8 File Offset: 0x0004B9B8
		public Guid ObjectGuid
		{
			get
			{
				if (this.m_position == null)
				{
					return Guid.Empty;
				}
				return this.m_position.ObjectGuid;
			}
			set
			{
				if (this.m_position != null && !typeof(FixedSourcePosition).IsAssignableFrom(this.m_position.GetType()))
				{
					SourcePosition position = new SourcePosition(this.m_position.ProjectHandle, value, this.m_position.Position, this.m_position.PositionOffset, this.m_position.Length);
					this.m_position = position;
				}
			}
		}

		// Token: 0x17000729 RID: 1833
		// (get) Token: 0x06001AE0 RID: 6880 RVA: 0x0004CA23 File Offset: 0x0004BA23
		public long Position
		{
			get
			{
				if (this.m_position == null)
				{
					return -1L;
				}
				return this.m_position.Position;
			}
		}

		// Token: 0x1700072A RID: 1834
		// (get) Token: 0x06001AE1 RID: 6881 RVA: 0x0004CA3B File Offset: 0x0004BA3B
		public short PositionOffset
		{
			get
			{
				if (this.m_position == null)
				{
					return -1;
				}
				return this.m_position.PositionOffset;
			}
		}

		// Token: 0x1700072B RID: 1835
		// (get) Token: 0x06001AE2 RID: 6882 RVA: 0x0004CA52 File Offset: 0x0004BA52
		// (set) Token: 0x06001AE3 RID: 6883 RVA: 0x0004CA5A File Offset: 0x0004BA5A
		public short Length
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x1700072C RID: 1836
		// (get) Token: 0x06001AE4 RID: 6884 RVA: 0x0004CA63 File Offset: 0x0004BA63
		// (set) Token: 0x06001AE5 RID: 6885 RVA: 0x0004CA6B File Offset: 0x0004BA6B
		public string Text
		{
			get
			{
				return this.m_stError;
			}
			set
			{
				this.m_stError = value;
			}
		}

		// Token: 0x1700072D RID: 1837
		// (get) Token: 0x06001AE6 RID: 6886 RVA: 0x0004CA74 File Offset: 0x0004BA74
		// (set) Token: 0x06001AE7 RID: 6887 RVA: 0x0004CA7C File Offset: 0x0004BA7C
		public Severity Severity
		{
			get
			{
				return this.m_severity;
			}
			set
			{
				this.m_severity = value;
			}
		}

		// Token: 0x1700072E RID: 1838
		// (get) Token: 0x06001AE8 RID: 6888 RVA: 0x0004CA85 File Offset: 0x0004BA85
		// (set) Token: 0x06001AE9 RID: 6889 RVA: 0x0004CA8D File Offset: 0x0004BA8D
		public ShowAttribute ShowAttribute
		{
			get
			{
				return this.m_ShowAttribute;
			}
			set
			{
				this.m_ShowAttribute = value;
			}
		}

		// Token: 0x1700072F RID: 1839
		// (get) Token: 0x06001AEA RID: 6890 RVA: 0x0004CA96 File Offset: 0x0004BA96
		public bool ShowPrecompile
		{
			get
			{
				return (this.m_ShowAttribute & ShowAttribute.Precompile) == ShowAttribute.Precompile;
			}
		}

		// Token: 0x17000730 RID: 1840
		// (get) Token: 0x06001AEB RID: 6891 RVA: 0x0004CAA3 File Offset: 0x0004BAA3
		public bool ShowCompile
		{
			get
			{
				return (this.m_ShowAttribute & ShowAttribute.Compile) == ShowAttribute.Compile;
			}
		}

		// Token: 0x17000731 RID: 1841
		// (get) Token: 0x06001AEC RID: 6892 RVA: 0x0004CAB0 File Offset: 0x0004BAB0
		public MessageId MessageId
		{
			get
			{
				return this._mid;
			}
		}

		// Token: 0x17000732 RID: 1842
		// (get) Token: 0x06001AED RID: 6893 RVA: 0x0004CAB8 File Offset: 0x0004BAB8
		// (set) Token: 0x06001AEE RID: 6894 RVA: 0x0004CAC0 File Offset: 0x0004BAC0
		public Guid SignatureGuid
		{
			get
			{
				return this._signid;
			}
			set
			{
				this._signid = value;
			}
		}

		// Token: 0x17000733 RID: 1843
		// (get) Token: 0x06001AEF RID: 6895 RVA: 0x0004CAB0 File Offset: 0x0004BAB0
		// (set) Token: 0x06001AF0 RID: 6896 RVA: 0x0004CACC File Offset: 0x0004BACC
		[DefaultSerialization("mid")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint MIDSerialize
		{
			get
			{
				return (uint)this._mid;
			}
			set
			{
				try
				{
					this._mid = (MessageId)value;
				}
				catch
				{
					this._mid = MessageId.None;
				}
			}
		}

		// Token: 0x17000734 RID: 1844
		// (get) Token: 0x06001AF1 RID: 6897 RVA: 0x00005F0F File Offset: 0x00004F0F
		public Icon Icon
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000735 RID: 1845
		// (get) Token: 0x06001AF2 RID: 6898 RVA: 0x0004CAFC File Offset: 0x0004BAFC
		public virtual uint? Number
		{
			get
			{
				if (this._mid == MessageId.None)
				{
					return null;
				}
				return new uint?((uint)this._mid);
			}
		}

		// Token: 0x17000736 RID: 1846
		// (get) Token: 0x06001AF3 RID: 6899 RVA: 0x0004CB26 File Offset: 0x0004BB26
		public virtual string Prefix
		{
			get
			{
				return "C";
			}
		}

		// Token: 0x17000737 RID: 1847
		// (get) Token: 0x06001AF4 RID: 6900 RVA: 0x00005F0F File Offset: 0x00004F0F
		public MessageDetailsHandler DetailsHandler
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000738 RID: 1848
		// (get) Token: 0x06001AF5 RID: 6901 RVA: 0x00005F0F File Offset: 0x00004F0F
		public object DetailsHandlerData
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000739 RID: 1849
		// (get) Token: 0x06001AF6 RID: 6902 RVA: 0x0004CB2D File Offset: 0x0004BB2D
		public Color FontColor
		{
			get
			{
				return Color.Empty;
			}
		}

		// Token: 0x0400059D RID: 1437
		private MessageId _mid;

		// Token: 0x0400059E RID: 1438
		[DefaultSerialization("Position")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ISourcePosition m_position;

		// Token: 0x0400059F RID: 1439
		[DefaultSerialization("Error")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stError;

		// Token: 0x040005A0 RID: 1440
		[DefaultSerialization("Severity")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Severity m_severity;

		// Token: 0x040005A1 RID: 1441
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private short m_sLength;

		// Token: 0x040005A2 RID: 1442
		[DefaultSerialization("Precompile")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ShowAttribute m_ShowAttribute = ShowAttribute.All;

		// Token: 0x040005A3 RID: 1443
		[DefaultSerialization("_id")]
		[StorageVersion("3.5.5.0")]
		[StorageIgnorable]
		private Guid _signid = Guid.Empty;
	}
}
