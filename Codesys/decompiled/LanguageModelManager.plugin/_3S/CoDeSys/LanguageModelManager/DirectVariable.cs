using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000144 RID: 324
	[TypeGuid("{bb2125fa-9cb7-4860-ac03-e52ee526027e}")]
	[StorageVersion("3.3.0.0")]
	public class DirectVariable : GenericObject2, _IDirectVariable, IDirectVariable
	{
		// Token: 0x06001B2C RID: 6956 RVA: 0x0000AC39 File Offset: 0x00009C39
		public DirectVariable()
		{
		}

		// Token: 0x06001B2D RID: 6957 RVA: 0x0004D507 File Offset: 0x0004C507
		public DirectVariable(DirectVariableLocation location, DirectVariableSize size, int[] components)
		{
			this.m_location = location;
			this.m_size = size;
			this.m_components = components;
		}

		// Token: 0x06001B2E RID: 6958 RVA: 0x0004D524 File Offset: 0x0004C524
		public DirectVariable(DirectVariableLocation location, bool bIncomplete)
		{
			this.m_location = location;
			this.m_size = DirectVariableSize.None;
			this.m_components = Array.Empty<int>();
			this.m_bIncomplete = true;
		}

		// Token: 0x06001B2F RID: 6959 RVA: 0x0004D54C File Offset: 0x0004C54C
		public static DirectVariable CopyFrom(IDirectVariable dirvar)
		{
			if (!dirvar.Incomplete)
			{
				return new DirectVariable(dirvar.Location, dirvar.Size, dirvar.Components);
			}
			return new DirectVariable(dirvar.Location, true);
		}

		// Token: 0x06001B30 RID: 6960 RVA: 0x0004D57C File Offset: 0x0004C57C
		public override int GetHashCode()
		{
			int num = this.m_location.GetHashCode() ^ 7 * this.m_size.GetHashCode() ^ 11 * this.Incomplete.GetHashCode();
			for (int i = 0; i < this.Components.Length; i++)
			{
				num = num * 13 + this.Components[i];
			}
			return num;
		}

		// Token: 0x06001B31 RID: 6961 RVA: 0x0004D5E4 File Offset: 0x0004C5E4
		public override bool Equals(object obj)
		{
			DirectVariable directVariable = obj as DirectVariable;
			return directVariable != null && directVariable.ToString() == this.ToString();
		}

		// Token: 0x06001B32 RID: 6962 RVA: 0x0004D610 File Offset: 0x0004C610
		public bool IsEqual(IDirectVariable rhs)
		{
			if (rhs == null)
			{
				return false;
			}
			if (rhs.Location != this.Location)
			{
				return false;
			}
			if (rhs.Size != this.Size)
			{
				return false;
			}
			if (rhs.Components.Length != this.Components.Length)
			{
				return false;
			}
			for (int i = 0; i < rhs.Components.Length; i++)
			{
				if (rhs.Components[i] != this.Components[i])
				{
					return false;
				}
			}
			return rhs.Incomplete == this.Incomplete;
		}

		// Token: 0x06001B33 RID: 6963 RVA: 0x0004D690 File Offset: 0x0004C690
		public override string ToString()
		{
			string text = string.Empty;
			text += "%";
			switch (this.m_location)
			{
			case DirectVariableLocation.Input:
				text += "I";
				break;
			case DirectVariableLocation.Output:
				text += "Q";
				break;
			case DirectVariableLocation.Memory:
				text += "M";
				break;
			default:
				text += "?";
				break;
			}
			if (this.Incomplete)
			{
				return text + "*";
			}
			switch (this.m_size)
			{
			case DirectVariableSize.X:
				text += "X";
				break;
			case DirectVariableSize.B:
				text += "B";
				break;
			case DirectVariableSize.W:
				text += "W";
				break;
			case DirectVariableSize.D:
				text += "D";
				break;
			case DirectVariableSize.L:
				text += "L";
				break;
			default:
				text += "?";
				break;
			}
			int[] components = this.Components;
			for (int i = 0; i < components.Length; i++)
			{
				if (i > 0)
				{
					text += ".";
				}
				text += components[i].ToString();
			}
			return text;
		}

		// Token: 0x17000748 RID: 1864
		// (get) Token: 0x06001B34 RID: 6964 RVA: 0x0004D7CE File Offset: 0x0004C7CE
		// (set) Token: 0x06001B35 RID: 6965 RVA: 0x0004D7D6 File Offset: 0x0004C7D6
		public DirectVariableLocation Location
		{
			get
			{
				return this.m_location;
			}
			set
			{
				this.m_location = value;
			}
		}

		// Token: 0x17000749 RID: 1865
		// (get) Token: 0x06001B36 RID: 6966 RVA: 0x0004D7DF File Offset: 0x0004C7DF
		// (set) Token: 0x06001B37 RID: 6967 RVA: 0x0004D7E7 File Offset: 0x0004C7E7
		public DirectVariableSize Size
		{
			get
			{
				return this.m_size;
			}
			set
			{
				this.m_size = value;
			}
		}

		// Token: 0x1700074A RID: 1866
		// (get) Token: 0x06001B38 RID: 6968 RVA: 0x0004D7F0 File Offset: 0x0004C7F0
		public int BitSize
		{
			get
			{
				switch (this.m_size)
				{
				case DirectVariableSize.X:
					return 1;
				case DirectVariableSize.B:
					return 8;
				case DirectVariableSize.W:
					return 16;
				case DirectVariableSize.D:
					return 32;
				case DirectVariableSize.L:
					return 64;
				default:
					return 0;
				}
			}
		}

		// Token: 0x1700074B RID: 1867
		// (get) Token: 0x06001B39 RID: 6969 RVA: 0x0004D830 File Offset: 0x0004C830
		// (set) Token: 0x06001B3A RID: 6970 RVA: 0x0004D838 File Offset: 0x0004C838
		public int[] Components
		{
			get
			{
				return this.m_components;
			}
			set
			{
				this.m_components = value;
			}
		}

		// Token: 0x1700074C RID: 1868
		// (get) Token: 0x06001B3B RID: 6971 RVA: 0x0004D841 File Offset: 0x0004C841
		// (set) Token: 0x06001B3C RID: 6972 RVA: 0x0004D849 File Offset: 0x0004C849
		public bool Incomplete
		{
			get
			{
				return this.m_bIncomplete;
			}
			set
			{
				this.m_bIncomplete = value;
			}
		}

		// Token: 0x040005B4 RID: 1460
		[DefaultSerialization("Location")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DirectVariableLocation m_location;

		// Token: 0x040005B5 RID: 1461
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DirectVariableSize m_size;

		// Token: 0x040005B6 RID: 1462
		[DefaultSerialization("Components")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int[] m_components;

		// Token: 0x040005B7 RID: 1463
		[DefaultSerialization("Incomplete")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bIncomplete;
	}
}
