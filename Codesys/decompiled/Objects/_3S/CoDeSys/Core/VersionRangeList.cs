using System;
using System.Collections.Generic;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000012 RID: 18
	internal class VersionRangeList
	{
		// Token: 0x06000047 RID: 71 RVA: 0x0000253D File Offset: 0x0000073D
		internal VersionRangeList(string st)
		{
			if (st == null)
			{
				throw new ArgumentNullException("st");
			}
			this._st = st;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x0000255A File Offset: 0x0000075A
		public override string ToString()
		{
			return this._st;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00002564 File Offset: 0x00000764
		internal bool Contains(Version version)
		{
			if (version == null)
			{
				return false;
			}
			if (this._list == null)
			{
				this._list = new List<VersionRange>();
				string[] array = this._st.Split(new char[]
				{
					';'
				});
				if (array != null)
				{
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						string[] array3 = array2[i].Split(new char[]
						{
							'-'
						});
						if (array3 != null)
						{
							int num = array3.Length;
							if (num != 1)
							{
								if (num == 2)
								{
									this._list.Add(new VersionRange(new Version(array3[0]), new Version(array3[1])));
								}
							}
							else
							{
								this._list.Add(new VersionRange(new Version(array3[0]), new Version(32767, 32767, 32767, 32767)));
							}
						}
					}
				}
			}
			foreach (VersionRange versionRange in this._list)
			{
				if (versionRange.From <= version && version <= versionRange.To)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400000F RID: 15
		private string _st;

		// Token: 0x04000010 RID: 16
		private List<VersionRange> _list;
	}
}
