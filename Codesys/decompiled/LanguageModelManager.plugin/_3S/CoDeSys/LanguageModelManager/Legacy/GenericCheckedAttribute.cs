using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000272 RID: 626
	internal class GenericCheckedAttribute : ICheckedAttribute, IAttribute
	{
		// Token: 0x06002A41 RID: 10817 RVA: 0x0006B516 File Offset: 0x0006A516
		public GenericCheckedAttribute(string name, string description, Version compilerVersion, AttributeScope scope, Func<string, AttributeScope, ISignature, IVariable, string> checkFunc)
		{
			this._name = name;
			this._description = description;
			this._compilerVersion = compilerVersion;
			this._scope = scope;
			this._checkFunc = checkFunc;
		}

		// Token: 0x17000BDB RID: 3035
		// (get) Token: 0x06002A42 RID: 10818 RVA: 0x0006B543 File Offset: 0x0006A543
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x17000BDC RID: 3036
		// (get) Token: 0x06002A43 RID: 10819 RVA: 0x0006B54B File Offset: 0x0006A54B
		public string Description
		{
			get
			{
				return this._description;
			}
		}

		// Token: 0x17000BDD RID: 3037
		// (get) Token: 0x06002A44 RID: 10820 RVA: 0x0006B553 File Offset: 0x0006A553
		public Version RequiredCompilerVersion
		{
			get
			{
				return this._compilerVersion;
			}
		}

		// Token: 0x17000BDE RID: 3038
		// (get) Token: 0x06002A45 RID: 10821 RVA: 0x0006B55B File Offset: 0x0006A55B
		public AttributeScope Scope
		{
			get
			{
				return this._scope;
			}
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x0006B563 File Offset: 0x0006A563
		public bool CheckValue(string value, AttributeScope scope, ISignature signature, IVariable variable, out string error)
		{
			error = this._checkFunc(value, scope, signature, variable);
			return string.IsNullOrWhiteSpace(error);
		}

		// Token: 0x04000804 RID: 2052
		private readonly string _name;

		// Token: 0x04000805 RID: 2053
		private readonly string _description;

		// Token: 0x04000806 RID: 2054
		private readonly Version _compilerVersion;

		// Token: 0x04000807 RID: 2055
		private readonly AttributeScope _scope;

		// Token: 0x04000808 RID: 2056
		private readonly Func<string, AttributeScope, ISignature, IVariable, string> _checkFunc;
	}
}
