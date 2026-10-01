using System;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000009 RID: 9
	public class ObjectRelatedPositionTextProvider : IPositionTextProvider
	{
		// Token: 0x06000086 RID: 134 RVA: 0x000027D0 File Offset: 0x000009D0
		public ObjectRelatedPositionTextProvider(IObject obj)
		{
			this.\u0001 = obj;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000027E0 File Offset: 0x000009E0
		public string GetPositionText(long nPosition)
		{
			if (this.\u0001 != null)
			{
				return this.\u0001.GetPositionText(nPosition);
			}
			return null;
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000088 RID: 136 RVA: 0x000027F8 File Offset: 0x000009F8
		public string Name
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				IMetaObject metaObject = this.\u0001.MetaObject;
				if (metaObject == null)
				{
					return null;
				}
				return metaObject.Name;
			}
		}

		// Token: 0x04000006 RID: 6
		private readonly IObject \u0001;
	}
}
