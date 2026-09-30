using System;
using System.Diagnostics;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027A RID: 634
	[DebuggerDisplay("{GetStringRepresentation()}")]
	internal sealed class ChangedLMObject : IChangedLMObject2, IChangedLMObject
	{
		// Token: 0x06002AAB RID: 10923 RVA: 0x0006E95D File Offset: 0x0006D95D
		internal ChangedLMObject(string stName, Operator ePOUType, string stDescription, EPouSetChange eChange)
		{
			this._stName = stName;
			this._ePOUType = ePOUType;
			this._stDescription = stDescription;
			this._eChange = eChange;
			this._stChildName = string.Empty;
		}

		// Token: 0x17000BEB RID: 3051
		// (get) Token: 0x06002AAC RID: 10924 RVA: 0x0006E98D File Offset: 0x0006D98D
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x17000BEC RID: 3052
		// (get) Token: 0x06002AAD RID: 10925 RVA: 0x0006E995 File Offset: 0x0006D995
		public Operator POUType
		{
			get
			{
				return this._ePOUType;
			}
		}

		// Token: 0x17000BED RID: 3053
		// (get) Token: 0x06002AAE RID: 10926 RVA: 0x0006E99D File Offset: 0x0006D99D
		public string Description
		{
			get
			{
				return this._stDescription;
			}
		}

		// Token: 0x17000BEE RID: 3054
		// (get) Token: 0x06002AAF RID: 10927 RVA: 0x0006E9A5 File Offset: 0x0006D9A5
		public EPouSetChange Change
		{
			get
			{
				return this._eChange;
			}
		}

		// Token: 0x17000BEF RID: 3055
		// (get) Token: 0x06002AB0 RID: 10928 RVA: 0x0006E9AD File Offset: 0x0006D9AD
		public bool OnlineChangePossible
		{
			get
			{
				return ((EPouSetChange)(-1) & this._eChange) == EPouSetChange.Undefined;
			}
		}

		// Token: 0x17000BF0 RID: 3056
		// (get) Token: 0x06002AB1 RID: 10929 RVA: 0x0006E9BC File Offset: 0x0006D9BC
		// (set) Token: 0x06002AB2 RID: 10930 RVA: 0x0006E9C4 File Offset: 0x0006D9C4
		public string ChildName
		{
			get
			{
				return this._stChildName;
			}
			internal set
			{
				this._stChildName = value;
			}
		}

		// Token: 0x06002AB3 RID: 10931 RVA: 0x0006E9CD File Offset: 0x0006D9CD
		public override int GetHashCode()
		{
			return this.GetStringRepresentation().GetHashCode();
		}

		// Token: 0x06002AB4 RID: 10932 RVA: 0x0006E9DC File Offset: 0x0006D9DC
		public override bool Equals(object obj)
		{
			ChangedLMObject changedLMObject = obj as ChangedLMObject;
			return changedLMObject != null && this.GetStringRepresentation().Equals(changedLMObject.GetStringRepresentation());
		}

		// Token: 0x06002AB5 RID: 10933 RVA: 0x0006EA08 File Offset: 0x0006DA08
		private string GetStringRepresentation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((this._stName == null) ? string.Empty : this._stName);
			stringBuilder.Append("\t");
			stringBuilder.Append(this._ePOUType);
			stringBuilder.Append("\t");
			stringBuilder.Append(this._stDescription);
			stringBuilder.Append("\t");
			stringBuilder.Append(this._eChange.ToString());
			return stringBuilder.ToString();
		}

		// Token: 0x0400081F RID: 2079
		private readonly string _stName;

		// Token: 0x04000820 RID: 2080
		private readonly Operator _ePOUType;

		// Token: 0x04000821 RID: 2081
		private readonly string _stDescription;

		// Token: 0x04000822 RID: 2082
		private readonly EPouSetChange _eChange;

		// Token: 0x04000823 RID: 2083
		private string _stChildName;
	}
}
