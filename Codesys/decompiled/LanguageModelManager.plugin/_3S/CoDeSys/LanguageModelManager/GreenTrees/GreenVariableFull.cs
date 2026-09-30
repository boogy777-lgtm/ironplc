using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200023C RID: 572
	internal class GreenVariableFull : AbstractGreenVariable
	{
		// Token: 0x17000ADA RID: 2778
		// (get) Token: 0x060025E3 RID: 9699 RVA: 0x0005DD92 File Offset: 0x0005CD92
		// (set) Token: 0x060025E4 RID: 9700 RVA: 0x0005DD9A File Offset: 0x0005CD9A
		public override IDirectVariable Address { get; set; }

		// Token: 0x17000ADB RID: 2779
		// (get) Token: 0x060025E5 RID: 9701 RVA: 0x0005DA72 File Offset: 0x0005CA72
		public override string DocuComment
		{
			get
			{
				return this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
			}
		}

		// Token: 0x17000ADC RID: 2780
		// (get) Token: 0x060025E6 RID: 9702 RVA: 0x0005DDA4 File Offset: 0x0005CDA4
		public override string Comment
		{
			get
			{
				string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
				if (string.IsNullOrEmpty(attributeValue))
				{
					attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMMENT);
				}
				return attributeValue;
			}
		}

		// Token: 0x17000ADD RID: 2781
		// (get) Token: 0x060025E7 RID: 9703 RVA: 0x0005DDD2 File Offset: 0x0005CDD2
		// (set) Token: 0x060025E8 RID: 9704 RVA: 0x0005DDDA File Offset: 0x0005CDDA
		public override IAssignmentExpression[] InputAssignments { get; set; }

		// Token: 0x17000ADE RID: 2782
		// (get) Token: 0x060025E9 RID: 9705 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x060025EA RID: 9706 RVA: 0x0005D501 File Offset: 0x0005C501
		public override string Name
		{
			get
			{
				return this.OrgName.ToUpperInvariant();
			}
			set
			{
				this.OrgName = value;
			}
		}

		// Token: 0x17000ADF RID: 2783
		// (get) Token: 0x060025EB RID: 9707 RVA: 0x0005DDE3 File Offset: 0x0005CDE3
		// (set) Token: 0x060025EC RID: 9708 RVA: 0x0005DDEB File Offset: 0x0005CDEB
		public override string OrgName { get; set; }

		// Token: 0x17000AE0 RID: 2784
		// (get) Token: 0x060025ED RID: 9709 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000AE1 RID: 2785
		// (get) Token: 0x060025EE RID: 9710 RVA: 0x0005DDF4 File Offset: 0x0005CDF4
		// (set) Token: 0x060025EF RID: 9711 RVA: 0x0005DDFC File Offset: 0x0005CDFC
		public override int PrecompileId { get; set; }

		// Token: 0x17000AE2 RID: 2786
		// (get) Token: 0x060025F0 RID: 9712 RVA: 0x0005DE05 File Offset: 0x0005CE05
		// (set) Token: 0x060025F1 RID: 9713 RVA: 0x0005DE0D File Offset: 0x0005CE0D
		public override _IType _Type { get; set; }

		// Token: 0x17000AE3 RID: 2787
		// (get) Token: 0x060025F2 RID: 9714 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000AE4 RID: 2788
		// (get) Token: 0x060025F3 RID: 9715 RVA: 0x0005D5DB File Offset: 0x0005C5DB
		// (set) Token: 0x060025F4 RID: 9716 RVA: 0x0005D5E3 File Offset: 0x0005C5E3
		public override IExpression Initial
		{
			get
			{
				return this._Initial;
			}
			set
			{
				this._Initial = (value as _IExpression);
			}
		}

		// Token: 0x17000AE5 RID: 2789
		// (get) Token: 0x060025F5 RID: 9717 RVA: 0x0005DE18 File Offset: 0x0005CE18
		// (set) Token: 0x060025F6 RID: 9718 RVA: 0x0005DE63 File Offset: 0x0005CE63
		public override _IExpression _Initial
		{
			get
			{
				if (this.StoredInitial is ExpressionWithCompactedInformation)
				{
					ExpressionWithCompactedInformation expressionWithCompactedInformation = this.StoredInitial as ExpressionWithCompactedInformation;
					_IExpression result;
					GreenTreeContext.Singleton.ConvertInitialValueToRedTree(expressionWithCompactedInformation.Expression, expressionWithCompactedInformation.CompactedInitialValueInformation, out result);
					return result;
				}
				return this.StoredInitial as _IExpression;
			}
			set
			{
				this.StoredInitial = value;
			}
		}

		// Token: 0x17000AE6 RID: 2790
		// (get) Token: 0x060025F7 RID: 9719 RVA: 0x0005DE6C File Offset: 0x0005CE6C
		// (set) Token: 0x060025F8 RID: 9720 RVA: 0x0005DE63 File Offset: 0x0005CE63
		public override ExpressionWithCompactedInformation InitialWithCompactedInformation
		{
			get
			{
				return this.StoredInitial as ExpressionWithCompactedInformation;
			}
			set
			{
				this.StoredInitial = value;
			}
		}

		// Token: 0x17000AE7 RID: 2791
		// (get) Token: 0x060025F9 RID: 9721 RVA: 0x0005D655 File Offset: 0x0005C655
		// (set) Token: 0x060025FA RID: 9722 RVA: 0x0005D668 File Offset: 0x0005C668
		public override ICompactedParseTreeInformation CompactedInitialValueInformation
		{
			get
			{
				ExpressionWithCompactedInformation initialWithCompactedInformation = this.InitialWithCompactedInformation;
				if (initialWithCompactedInformation == null)
				{
					return null;
				}
				return initialWithCompactedInformation.CompactedInitialValueInformation;
			}
			set
			{
				if (this.InitialWithCompactedInformation != null)
				{
					this.InitialWithCompactedInformation.CompactedInitialValueInformation = value;
				}
			}
		}

		// Token: 0x17000AE8 RID: 2792
		// (get) Token: 0x060025FB RID: 9723 RVA: 0x0005DE79 File Offset: 0x0005CE79
		// (set) Token: 0x060025FC RID: 9724 RVA: 0x0005DE81 File Offset: 0x0005CE81
		private object StoredInitial { get; set; }

		// Token: 0x17000AE9 RID: 2793
		// (get) Token: 0x060025FD RID: 9725 RVA: 0x0005DE8A File Offset: 0x0005CE8A
		public override _IExpression OriginalInitial
		{
			get
			{
				if (this.StoredInitial is ExpressionWithCompactedInformation)
				{
					return (this.StoredInitial as ExpressionWithCompactedInformation).Expression;
				}
				return this.StoredInitial as _IExpression;
			}
		}

		// Token: 0x17000AEA RID: 2794
		// (get) Token: 0x060025FE RID: 9726 RVA: 0x0005DEB8 File Offset: 0x0005CEB8
		public override bool IsProperty
		{
			get
			{
				if (this._attributes == null)
				{
					return false;
				}
				Tuple<string, string>[] attributes = this._attributes;
				for (int i = 0; i < attributes.Length; i++)
				{
					string item = attributes[i].Item1;
					if (item == "property")
					{
						return true;
					}
					if (item == "device_parameter")
					{
						return true;
					}
					if (item == "get_access" || item == "set_access")
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x17000AEB RID: 2795
		// (get) Token: 0x060025FF RID: 9727 RVA: 0x0005DF2C File Offset: 0x0005CF2C
		public override bool IsPropertyMonitor
		{
			get
			{
				if (this._attributes == null)
				{
					return false;
				}
				foreach (Tuple<string, string> tuple in this._attributes)
				{
					if (tuple.Item1 == CompileAttributes.ATTRIBUTE_MONITORING && tuple.Item2 == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x06002600 RID: 9728 RVA: 0x0000677E File Offset: 0x0000577E
		public override void AddAttribute(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000AEC RID: 2796
		// (get) Token: 0x06002601 RID: 9729 RVA: 0x0005DF84 File Offset: 0x0005CF84
		public override string[] Attributes
		{
			get
			{
				if (this._attributes == null)
				{
					return Array.Empty<string>();
				}
				string[] array = new string[this._attributes.Length];
				for (int i = 0; i < this._attributes.Length; i++)
				{
					array[i] = this._attributes[i].Item1;
				}
				return array;
			}
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x0005DFD4 File Offset: 0x0005CFD4
		public override void SetAttributes(IDictionary<string, string> attributes)
		{
			if (attributes == null || attributes.Keys.Count == 0)
			{
				return;
			}
			string[] array = attributes.Keys.ToArray<string>();
			this._attributes = new Tuple<string, string>[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				this._attributes[i] = new Tuple<string, string>(array[i], attributes[array[i]]);
			}
		}

		// Token: 0x06002603 RID: 9731 RVA: 0x0005E034 File Offset: 0x0005D034
		public override string GetAttributeValue(string stAttribute)
		{
			if (this._attributes == null)
			{
				return null;
			}
			for (int i = 0; i < this._attributes.Length; i++)
			{
				if (this._attributes[i].Item1 == stAttribute)
				{
					return this._attributes[i].Item2;
				}
			}
			return null;
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x0000677E File Offset: 0x0000577E
		public override void SetAttributeValue(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002605 RID: 9733 RVA: 0x0005E084 File Offset: 0x0005D084
		public override bool HasAttribute(string stAttribute)
		{
			if (this._attributes == null)
			{
				return false;
			}
			for (int i = 0; i < this._attributes.Length; i++)
			{
				if (this._attributes[i].Item1 == stAttribute)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002606 RID: 9734 RVA: 0x0000677E File Offset: 0x0000577E
		public override void RemoveAttribute(string stAttribute)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000745 RID: 1861
		private const string _GET_ACCESS = "get_access";

		// Token: 0x04000746 RID: 1862
		private const string _SET_ACCESS = "set_access";

		// Token: 0x04000747 RID: 1863
		private const string _DEVICE_PARAMETER = "device_parameter";

		// Token: 0x04000748 RID: 1864
		private const string _ATTRIBUTE_PROPERTY = "property";

		// Token: 0x04000749 RID: 1865
		private Tuple<string, string>[] _attributes;
	}
}
