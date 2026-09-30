using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000157 RID: 343
	[DebuggerDisplay("{Signature}: {MemberName} (Level = {Level}}")]
	[TypeGuid("{9DC10D74-9075-499D-943F-0DA8FC49A15C}")]
	public class SignatureMemberHierachyInfo : ISignatureMemberHierachyInfo
	{
		// Token: 0x06001BAD RID: 7085 RVA: 0x00002476 File Offset: 0x00001476
		public SignatureMemberHierachyInfo()
		{
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x0004E50A File Offset: 0x0004D50A
		internal SignatureMemberHierachyInfo(string stMemberName, ISignature signature, int iLevel)
		{
			this.MemberName = stMemberName;
			this.Signature = signature;
			this.Level = iLevel;
			this.EmptyLevel = false;
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x0004E52E File Offset: 0x0004D52E
		internal SignatureMemberHierachyInfo(ISignature signature, int iLevel)
		{
			this.MemberName = string.Empty;
			this.Signature = signature;
			this.Level = iLevel;
			this.EmptyLevel = true;
		}

		// Token: 0x17000772 RID: 1906
		// (get) Token: 0x06001BB0 RID: 7088 RVA: 0x0004E556 File Offset: 0x0004D556
		// (set) Token: 0x06001BB1 RID: 7089 RVA: 0x0004E55E File Offset: 0x0004D55E
		public string MemberName { get; set; }

		// Token: 0x17000773 RID: 1907
		// (get) Token: 0x06001BB2 RID: 7090 RVA: 0x0004E567 File Offset: 0x0004D567
		// (set) Token: 0x06001BB3 RID: 7091 RVA: 0x0004E56F File Offset: 0x0004D56F
		public ISignature Signature { get; private set; }

		// Token: 0x17000774 RID: 1908
		// (get) Token: 0x06001BB4 RID: 7092 RVA: 0x0004E578 File Offset: 0x0004D578
		// (set) Token: 0x06001BB5 RID: 7093 RVA: 0x0004E580 File Offset: 0x0004D580
		public int Level { get; private set; }

		// Token: 0x17000775 RID: 1909
		// (get) Token: 0x06001BB6 RID: 7094 RVA: 0x0004E589 File Offset: 0x0004D589
		// (set) Token: 0x06001BB7 RID: 7095 RVA: 0x0004E591 File Offset: 0x0004D591
		public object Tag { get; set; }

		// Token: 0x17000776 RID: 1910
		// (get) Token: 0x06001BB8 RID: 7096 RVA: 0x0004E59A File Offset: 0x0004D59A
		// (set) Token: 0x06001BB9 RID: 7097 RVA: 0x0004E5A2 File Offset: 0x0004D5A2
		public bool EmptyLevel { get; set; }

		// Token: 0x06001BBA RID: 7098 RVA: 0x0004E5AC File Offset: 0x0004D5AC
		public override bool Equals(object obj)
		{
			SignatureMemberHierachyInfo signatureMemberHierachyInfo = obj as SignatureMemberHierachyInfo;
			return signatureMemberHierachyInfo != null && this.MemberName == signatureMemberHierachyInfo.MemberName && this.Signature.Equals(signatureMemberHierachyInfo.Signature) && this.Level == signatureMemberHierachyInfo.Level;
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x0004E5FC File Offset: 0x0004D5FC
		public override int GetHashCode()
		{
			return ((739478832 * -1424358940 + EqualityComparer<string>.Default.GetHashCode(this.MemberName)) * -1424358940 + this.Signature.Id.GetHashCode()) * -1424358940 + this.Level.GetHashCode();
		}
	}
}
