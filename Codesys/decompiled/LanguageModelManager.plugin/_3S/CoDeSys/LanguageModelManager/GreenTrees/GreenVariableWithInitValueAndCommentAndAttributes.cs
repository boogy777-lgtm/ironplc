using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200023B RID: 571
	internal class GreenVariableWithInitValueAndCommentAndAttributes : AbstractGreenVariable
	{
		// Token: 0x17000AC9 RID: 2761
		// (get) Token: 0x060025C2 RID: 9666 RVA: 0x0005DA72 File Offset: 0x0005CA72
		public override string DocuComment
		{
			get
			{
				return this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
			}
		}

		// Token: 0x17000ACA RID: 2762
		// (get) Token: 0x060025C3 RID: 9667 RVA: 0x0005DA80 File Offset: 0x0005CA80
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

		// Token: 0x17000ACB RID: 2763
		// (get) Token: 0x060025C4 RID: 9668 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x060025C5 RID: 9669 RVA: 0x0005D501 File Offset: 0x0005C501
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

		// Token: 0x17000ACC RID: 2764
		// (get) Token: 0x060025C6 RID: 9670 RVA: 0x0005DAAE File Offset: 0x0005CAAE
		// (set) Token: 0x060025C7 RID: 9671 RVA: 0x0005DAB6 File Offset: 0x0005CAB6
		public override string OrgName { get; set; }

		// Token: 0x17000ACD RID: 2765
		// (get) Token: 0x060025C8 RID: 9672 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000ACE RID: 2766
		// (get) Token: 0x060025C9 RID: 9673 RVA: 0x0005DABF File Offset: 0x0005CABF
		// (set) Token: 0x060025CA RID: 9674 RVA: 0x0005DAC7 File Offset: 0x0005CAC7
		public override int PrecompileId { get; set; }

		// Token: 0x17000ACF RID: 2767
		// (get) Token: 0x060025CB RID: 9675 RVA: 0x0005DAD0 File Offset: 0x0005CAD0
		// (set) Token: 0x060025CC RID: 9676 RVA: 0x0005DAD8 File Offset: 0x0005CAD8
		public override _IType _Type { get; set; }

		// Token: 0x17000AD0 RID: 2768
		// (get) Token: 0x060025CD RID: 9677 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000AD1 RID: 2769
		// (get) Token: 0x060025CE RID: 9678 RVA: 0x0005D5DB File Offset: 0x0005C5DB
		// (set) Token: 0x060025CF RID: 9679 RVA: 0x0005D5E3 File Offset: 0x0005C5E3
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

		// Token: 0x17000AD2 RID: 2770
		// (get) Token: 0x060025D0 RID: 9680 RVA: 0x0005DAE4 File Offset: 0x0005CAE4
		// (set) Token: 0x060025D1 RID: 9681 RVA: 0x0005DB2F File Offset: 0x0005CB2F
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

		// Token: 0x17000AD3 RID: 2771
		// (get) Token: 0x060025D2 RID: 9682 RVA: 0x0005DB38 File Offset: 0x0005CB38
		// (set) Token: 0x060025D3 RID: 9683 RVA: 0x0005DB2F File Offset: 0x0005CB2F
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

		// Token: 0x17000AD4 RID: 2772
		// (get) Token: 0x060025D4 RID: 9684 RVA: 0x0005D655 File Offset: 0x0005C655
		// (set) Token: 0x060025D5 RID: 9685 RVA: 0x0005D668 File Offset: 0x0005C668
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

		// Token: 0x17000AD5 RID: 2773
		// (get) Token: 0x060025D6 RID: 9686 RVA: 0x0005DB45 File Offset: 0x0005CB45
		// (set) Token: 0x060025D7 RID: 9687 RVA: 0x0005DB4D File Offset: 0x0005CB4D
		private object StoredInitial { get; set; }

		// Token: 0x17000AD6 RID: 2774
		// (get) Token: 0x060025D8 RID: 9688 RVA: 0x0005DB56 File Offset: 0x0005CB56
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

		// Token: 0x17000AD7 RID: 2775
		// (get) Token: 0x060025D9 RID: 9689 RVA: 0x0005DB84 File Offset: 0x0005CB84
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

		// Token: 0x17000AD8 RID: 2776
		// (get) Token: 0x060025DA RID: 9690 RVA: 0x0005DBF8 File Offset: 0x0005CBF8
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

		// Token: 0x060025DB RID: 9691 RVA: 0x0000677E File Offset: 0x0000577E
		public override void AddAttribute(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000AD9 RID: 2777
		// (get) Token: 0x060025DC RID: 9692 RVA: 0x0005DC50 File Offset: 0x0005CC50
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

		// Token: 0x060025DD RID: 9693 RVA: 0x0005DCA0 File Offset: 0x0005CCA0
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

		// Token: 0x060025DE RID: 9694 RVA: 0x0005DD00 File Offset: 0x0005CD00
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

		// Token: 0x060025DF RID: 9695 RVA: 0x0000677E File Offset: 0x0000577E
		public override void SetAttributeValue(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060025E0 RID: 9696 RVA: 0x0005DD50 File Offset: 0x0005CD50
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

		// Token: 0x060025E1 RID: 9697 RVA: 0x0000677E File Offset: 0x0000577E
		public override void RemoveAttribute(string stAttribute)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400073A RID: 1850
		private const string _GET_ACCESS = "get_access";

		// Token: 0x0400073B RID: 1851
		private const string _SET_ACCESS = "set_access";

		// Token: 0x0400073C RID: 1852
		private const string _DEVICE_PARAMETER = "device_parameter";

		// Token: 0x0400073D RID: 1853
		private const string _ATTRIBUTE_PROPERTY = "property";

		// Token: 0x0400073E RID: 1854
		private Tuple<string, string>[] _attributes;
	}
}
