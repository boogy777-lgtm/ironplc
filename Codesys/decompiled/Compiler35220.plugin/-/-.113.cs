using System;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0007
{
	// Token: 0x02000150 RID: 336
	internal sealed class \u0006
	{
		// Token: 0x06001787 RID: 6023 RVA: 0x0004874C File Offset: 0x0004694C
		public \u0006(_IVariable \u001A\u0002, _ISignature \u001C\u0002, IScope \u009B\u0002, \u0005 \u001C\u0003, _IPreCompileContext \u001D\u0003)
		{
			this.\u0001 = \u001A\u0002;
			this.\u0001 = \u001C\u0002;
			this.\u0001 = \u001D\u0003;
			this.\u0001 = \u001C\u0003;
		}

		// Token: 0x06001788 RID: 6024 RVA: 0x00048774 File Offset: 0x00046974
		public bool \u0001()
		{
			return this.\u0001 == null && this.\u0001 == null && this.\u0001 == null && (this.\u0001 != null || this.\u0001 != null || this.\u0001 != null);
		}

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001789 RID: 6025 RVA: 0x000487AC File Offset: 0x000469AC
		// (set) Token: 0x0600178A RID: 6026 RVA: 0x000487B4 File Offset: 0x000469B4
		public _IVariable SimpleVar
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x0600178B RID: 6027 RVA: 0x000487C0 File Offset: 0x000469C0
		// (set) Token: 0x0600178C RID: 6028 RVA: 0x000487C8 File Offset: 0x000469C8
		public _ISignature SimpleSign
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x0600178D RID: 6029 RVA: 0x000487D4 File Offset: 0x000469D4
		// (set) Token: 0x0600178E RID: 6030 RVA: 0x000487DC File Offset: 0x000469DC
		public _IPreCompileContext SimplePreCompileContext
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x0600178F RID: 6031 RVA: 0x000487E8 File Offset: 0x000469E8
		public bool \u0002()
		{
			return this.\u0001 == \u0011.\u0005.\u0001 || this.\u0001 == \u0011.\u0005.\u0002 || this.\u0001 == \u0011.\u0005.\u0003 || this.\u0001 == \u0011.\u0005.\u0004 || this.\u0001 == \u0011.\u0005.\u0005;
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x00048818 File Offset: 0x00046A18
		public bool \u0003()
		{
			return this.\u0001 == \u0011.\u0005.\u0006 || this.\u0001 == \u0011.\u0005.\u0002 || this.\u0001 == \u0011.\u0005.\u0008 || this.\u0001 == \u0011.\u0005.\u000E || this.\u0001 == \u0011.\u0005.\u000F;
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x0004884C File Offset: 0x00046A4C
		public bool \u0004()
		{
			return this.\u0001 != null;
		}

		// Token: 0x06001792 RID: 6034 RVA: 0x00048858 File Offset: 0x00046A58
		public int \u0001()
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.Count;
			}
			if (this.\u0001 != null)
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x06001793 RID: 6035 RVA: 0x0004887C File Offset: 0x00046A7C
		public int \u0002()
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.Count;
			}
			if (this.\u0001 != null)
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x06001794 RID: 6036 RVA: 0x000488A0 File Offset: 0x00046AA0
		public bool \u0005()
		{
			return this.\u0006() && !this.\u0004();
		}

		// Token: 0x06001795 RID: 6037 RVA: 0x000488B8 File Offset: 0x00046AB8
		public bool \u0006()
		{
			return this.\u0001 != null;
		}

		// Token: 0x06001796 RID: 6038 RVA: 0x000488C4 File Offset: 0x00046AC4
		public _IVariable[] \u0001()
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.ToArray();
			}
			if (this.\u0001 != null)
			{
				return new _IVariable[]
				{
					this.\u0001
				};
			}
			return null;
		}

		// Token: 0x06001797 RID: 6039 RVA: 0x000488F4 File Offset: 0x00046AF4
		public _ISignature[] \u0001()
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.ToArray();
			}
			if (this.\u0001 != null)
			{
				return new _ISignature[]
				{
					this.\u0001
				};
			}
			return null;
		}

		// Token: 0x06001798 RID: 6040 RVA: 0x00048924 File Offset: 0x00046B24
		public void \u0001(_ISignature[] \u0002)
		{
			this.\u0001 = null;
			this.\u0001 = null;
			if (\u0002.Length > 1)
			{
				this.\u0001 = new LList<_ISignature>(\u0002);
			}
			if (\u0002.Length == 1)
			{
				this.\u0001 = \u0002[0];
			}
		}

		// Token: 0x06001799 RID: 6041 RVA: 0x00048958 File Offset: 0x00046B58
		public void \u0001(_IVariable[] \u0002)
		{
			this.\u0001 = null;
			this.\u0001 = null;
			if (\u0002.Length > 1)
			{
				this.\u0001 = new LList<_IVariable>(\u0002);
			}
			if (\u0002.Length == 1)
			{
				this.\u0001 = \u0002[0];
			}
		}

		// Token: 0x0600179A RID: 6042 RVA: 0x0004898C File Offset: 0x00046B8C
		public _IPreCompileContext[] \u0001()
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.ToArray();
			}
			if (this.\u0001 != null)
			{
				return new _IPreCompileContext[]
				{
					this.\u0001
				};
			}
			return null;
		}

		// Token: 0x0600179B RID: 6043 RVA: 0x000489BC File Offset: 0x00046BBC
		public void \u0001(_IVariable \u0002, _ISignature \u0003, IScope \u0004, \u0005 \u0005, _IPreCompileContext \u0006)
		{
			if (\u0005 == this.\u0001 && !this.\u0002)
			{
				if (\u0002 != null)
				{
					if (this.\u0001 == null)
					{
						this.\u0001 = new LList<_IVariable>();
					}
					if (this.\u0001 != null)
					{
						this.\u0001.Add(this.\u0001);
					}
					this.\u0001.Add(\u0002);
				}
				if (\u0003 != null)
				{
					if (this.\u0001 == null)
					{
						this.\u0001 = new LList<_ISignature>();
					}
					if (this.\u0001 != null)
					{
						this.\u0001.Add(this.\u0001);
					}
					this.\u0001.Add(\u0003);
				}
				if (\u0006 != null)
				{
					if (this.\u0001 == null)
					{
						this.\u0001 = new LList<_IPreCompileContext>();
					}
					if (this.\u0001 != null)
					{
						this.\u0001.Add(this.\u0001);
					}
					this.\u0001.Add(\u0006);
				}
			}
		}

		// Token: 0x04000425 RID: 1061
		private LList<_IVariable> \u0001;

		// Token: 0x04000426 RID: 1062
		private LList<_ISignature> \u0001;

		// Token: 0x04000427 RID: 1063
		private LList<_IPreCompileContext> \u0001;

		// Token: 0x04000428 RID: 1064
		public _IVariable \u0001;

		// Token: 0x04000429 RID: 1065
		public _ISignature \u0001;

		// Token: 0x0400042A RID: 1066
		public _IPreCompileContext \u0001;

		// Token: 0x0400042B RID: 1067
		public \u0005 \u0001;

		// Token: 0x0400042C RID: 1068
		public bool \u0001;

		// Token: 0x0400042D RID: 1069
		public bool \u0002;
	}
}
