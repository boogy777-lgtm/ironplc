using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000170 RID: 368
	public readonly struct LanguageModelResult : IEquatable<LanguageModelResult>
	{
		// Token: 0x060018EA RID: 6378 RVA: 0x0004DA28 File Offset: 0x0004BC28
		internal LanguageModelResult(_IPreCompileContext precom, _ISignature sign)
		{
			if (sign == null)
			{
				throw new ArgumentNullException("sign");
			}
			this.\u0001 = sign;
			if (precom == null)
			{
				throw new ArgumentNullException("precom");
			}
			this.\u0001 = precom;
		}

		// Token: 0x060018EB RID: 6379 RVA: 0x0004DA58 File Offset: 0x0004BC58
		public bool Equals(LanguageModelResult other)
		{
			return this.\u0001.Equals(other.\u0001) && this.\u0001.Equals(other.\u0001);
		}

		// Token: 0x060018EC RID: 6380 RVA: 0x0004DA80 File Offset: 0x0004BC80
		public override bool Equals(object obj)
		{
			if (obj is LanguageModelResult)
			{
				LanguageModelResult other = (LanguageModelResult)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x060018ED RID: 6381 RVA: 0x0004DAA8 File Offset: 0x0004BCA8
		public override int GetHashCode()
		{
			return this.\u0001.GetHashCode() ^ 31 * this.\u0001.GetHashCode();
		}

		// Token: 0x04000468 RID: 1128
		internal readonly _ISignature \u0001;

		// Token: 0x04000469 RID: 1129
		internal readonly _IPreCompileContext \u0001;
	}
}
