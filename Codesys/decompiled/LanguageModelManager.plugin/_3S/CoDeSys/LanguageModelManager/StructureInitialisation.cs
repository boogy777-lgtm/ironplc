using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000072 RID: 114
	[TypeGuid("{30cfa52e-e32a-43c4-a033-2a152796aeaf}")]
	[StorageVersion("3.3.0.0")]
	public class StructureInitialisation : PositionExpression, _IStructureInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IStructureInitialization
	{
		// Token: 0x06000750 RID: 1872 RVA: 0x00012A44 File Offset: 0x00011A44
		public StructureInitialisation()
		{
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x00012A58 File Offset: 0x00011A58
		internal StructureInitialisation(IToken token) : base(token)
		{
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x00012A6D File Offset: 0x00011A6D
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00012A76 File Offset: 0x00011A76
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00012A80 File Offset: 0x00011A80
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor3 exprVisitor = visitor as IExprVisitor3;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000756 RID: 1878 RVA: 0x00012AA0 File Offset: 0x00011AA0
		public IAssignmentExpression[] CompoInits
		{
			get
			{
				AssignmentExpression[] array = new AssignmentExpression[this.m_initlist.Count];
				LList<_IAssignmentExpression> initlist = this.m_initlist;
				_IAssignmentExpression[] array2 = array;
				initlist.CopyTo(array2);
				return array;
			}
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000757 RID: 1879 RVA: 0x00012ACF File Offset: 0x00011ACF
		public IList<_IAssignmentExpression> _CompoInits
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x00012AD7 File Offset: 0x00011AD7
		public void AddInitValue(_IAssignmentExpression assign)
		{
			this.m_initlist.Add(assign);
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00012AE8 File Offset: 0x00011AE8
		public override _IExprement Duplicate()
		{
			StructureInitialisation structureInitialisation = new StructureInitialisation();
			this.DuplicateCommon(structureInitialisation);
			foreach (_IAssignmentExpression iassignmentExpression in this.m_initlist)
			{
				structureInitialisation.AddInitValue(iassignmentExpression.Duplicate() as _IAssignmentExpression);
			}
			structureInitialisation._CompiledType = this._CompiledType;
			structureInitialisation.DefaultInitializationDone = this.DefaultInitializationDone;
			return structureInitialisation;
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00012B68 File Offset: 0x00011B68
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			using (IEnumerator<_IAssignmentExpression> enumerator = this.m_initlist.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current._RValue.IsConstant(scope, bAllocatedOK))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x0600075B RID: 1883 RVA: 0x00012BC4 File Offset: 0x00011BC4
		// (set) Token: 0x0600075C RID: 1884 RVA: 0x00012BCC File Offset: 0x00011BCC
		public bool DefaultInitializationDone
		{
			get
			{
				return this._bDefaultInitializationDone;
			}
			set
			{
				this._bDefaultInitializationDone = value;
			}
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x0600075D RID: 1885 RVA: 0x00012BD5 File Offset: 0x00011BD5
		// (set) Token: 0x0600075E RID: 1886 RVA: 0x00012BDD File Offset: 0x00011BDD
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x0600075F RID: 1887 RVA: 0x00012BE6 File Offset: 0x00011BE6
		// (set) Token: 0x06000760 RID: 1888 RVA: 0x00012BF4 File Offset: 0x00011BF4
		[DefaultSerialization("InitList")]
		[StorageVersion("3.3.0.0-3.5.1.39")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private ArrayList OldList
		{
			get
			{
				return new ArrayList(this.m_initlist);
			}
			set
			{
				this.m_initlist = new LList<_IAssignmentExpression>();
				foreach (object obj in value)
				{
					_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)obj;
					this.m_initlist.Add(iassignmentExpression);
				}
			}
		}

		// Token: 0x040000F9 RID: 249
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000FA RID: 250
		[DefaultSerialization("InitListNew")]
		[StorageVersion("3.5.1.40")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<_IAssignmentExpression> m_initlist = new LList<_IAssignmentExpression>(1);

		// Token: 0x040000FB RID: 251
		[DefaultSerialization("DefInitDone")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private bool _bDefaultInitializationDone;
	}
}
