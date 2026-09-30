using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009A RID: 154
	[TypeGuid("{b602c470-d264-4363-b253-143478a44561}")]
	[StorageVersion("3.3.0.0")]
	public class POUDeclarationStatement : PositionStatement, _IPOUDeclarationStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPOUDeclarationStatement
	{
		// Token: 0x06000945 RID: 2373 RVA: 0x000159A2 File Offset: 0x000149A2
		public POUDeclarationStatement()
		{
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x000159C0 File Offset: 0x000149C0
		internal POUDeclarationStatement(IToken token) : base(token)
		{
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x000159DF File Offset: 0x000149DF
		// (set) Token: 0x06000948 RID: 2376 RVA: 0x000159E7 File Offset: 0x000149E7
		public Operator Class
		{
			get
			{
				return this.m_opClass;
			}
			set
			{
				this.m_opClass = value;
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000949 RID: 2377 RVA: 0x000159F0 File Offset: 0x000149F0
		// (set) Token: 0x0600094A RID: 2378 RVA: 0x000159F8 File Offset: 0x000149F8
		public SignatureFlag Access
		{
			get
			{
				return this.m_sfFlag;
			}
			set
			{
				this.m_sfFlag = value;
			}
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x00015A01 File Offset: 0x00014A01
		public void SetAccessFlag(SignatureFlag sf)
		{
			this.m_sfFlag |= sf;
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x0600094C RID: 2380 RVA: 0x00015A11 File Offset: 0x00014A11
		// (set) Token: 0x0600094D RID: 2381 RVA: 0x00015A19 File Offset: 0x00014A19
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x0600094E RID: 2382 RVA: 0x00015A22 File Offset: 0x00014A22
		// (set) Token: 0x0600094F RID: 2383 RVA: 0x00015A43 File Offset: 0x00014A43
		public _IExpression NameExpression
		{
			get
			{
				if (this.m_nameExpression == null)
				{
					this.m_nameExpression = new VariableExpression(this.m_stName);
				}
				return this.m_nameExpression;
			}
			set
			{
				this.m_nameExpression = value;
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000950 RID: 2384 RVA: 0x00015A4C File Offset: 0x00014A4C
		// (set) Token: 0x06000951 RID: 2385 RVA: 0x00015A54 File Offset: 0x00014A54
		public IList<_IExpression> Extends
		{
			get
			{
				return this.m_lExtends;
			}
			set
			{
				this.m_lExtends = (value as LList<_IExpression>);
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000952 RID: 2386 RVA: 0x00015A62 File Offset: 0x00014A62
		public IList<_IExpression> Implements
		{
			get
			{
				return this.m_alImplements;
			}
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x00015A6A File Offset: 0x00014A6A
		public void AddInterfaceImplementation(_IExpression expImplements)
		{
			if (this.m_alImplements == null)
			{
				this.m_alImplements = new LList<_IExpression>(1);
			}
			this.m_alImplements.Add(expImplements);
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000954 RID: 2388 RVA: 0x00015A8C File Offset: 0x00014A8C
		// (set) Token: 0x06000955 RID: 2389 RVA: 0x00015A94 File Offset: 0x00014A94
		public _IType Type
		{
			get
			{
				return this.m_typeReturnValue;
			}
			set
			{
				this.m_typeReturnValue = value;
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000956 RID: 2390 RVA: 0x00015A9D File Offset: 0x00014A9D
		// (set) Token: 0x06000957 RID: 2391 RVA: 0x00015AB3 File Offset: 0x00014AB3
		public _IStatement Declarations
		{
			get
			{
				if (this.m_statDeclarations == null)
				{
					return new NullStatement();
				}
				return this.m_statDeclarations;
			}
			set
			{
				if (value is SequenceStatement)
				{
					this.m_statDeclarations = (value as SequenceStatement);
					return;
				}
				this.m_statDeclarations = new SequenceStatement();
				this.m_statDeclarations.AddStatement(value);
			}
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00015AE1 File Offset: 0x00014AE1
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x00015AEA File Offset: 0x00014AEA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x00015AF4 File Offset: 0x00014AF4
		internal void CopyContentTo(POUDeclarationStatement pdsRet)
		{
			this.DuplicateCommon(pdsRet);
			pdsRet.Name = this.Name;
			pdsRet.NameExpression = this.NameExpression;
			pdsRet.Type = this.Type;
			pdsRet.Class = this.Class;
			pdsRet.Access = this.Access;
			pdsRet.Declarations = (this.Declarations.Duplicate() as Statement);
			if (this.Implements != null)
			{
				foreach (_IExpression iexpression in this.Implements)
				{
					pdsRet.AddInterfaceImplementation(iexpression.Duplicate() as _IExpression);
				}
			}
			if (this.Extends != null)
			{
				foreach (_IExpression iexpression2 in this.Extends)
				{
					pdsRet.Extends.Add(iexpression2.Duplicate() as _IExpression);
				}
			}
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00015C00 File Offset: 0x00014C00
		public override _IExprement Duplicate()
		{
			POUDeclarationStatement poudeclarationStatement = new POUDeclarationStatement();
			this.CopyContentTo(poudeclarationStatement);
			return poudeclarationStatement;
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x0600095D RID: 2397 RVA: 0x00015C1C File Offset: 0x00014C1C
		// (set) Token: 0x0600095E RID: 2398 RVA: 0x00015C70 File Offset: 0x00014C70
		public List<IExpression> ExtendsList
		{
			get
			{
				List<IExpression> list = new List<IExpression>();
				foreach (IExpression item in this.m_lExtends)
				{
					list.Add(item);
				}
				return list;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.m_lExtends = new LList<_IExpression>();
				foreach (IExpression expression in value)
				{
					this.m_lExtends.Add(expression as _IExpression);
				}
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x0600095F RID: 2399 RVA: 0x00015CD8 File Offset: 0x00014CD8
		// (set) Token: 0x06000960 RID: 2400 RVA: 0x00015D2C File Offset: 0x00014D2C
		public List<IExpression> ImplementsList
		{
			get
			{
				List<IExpression> list = new List<IExpression>();
				foreach (IExpression item in this.m_alImplements)
				{
					list.Add(item);
				}
				return list;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.m_alImplements = new LList<_IExpression>(value.Count);
				foreach (IExpression expression in value)
				{
					this.m_alImplements.Add(expression as _IExpression);
				}
			}
		}

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000961 RID: 2401 RVA: 0x00015A8C File Offset: 0x00014A8C
		// (set) Token: 0x06000962 RID: 2402 RVA: 0x00015D9C File Offset: 0x00014D9C
		public IType ReturnType
		{
			get
			{
				return this.m_typeReturnValue;
			}
			set
			{
				this.m_typeReturnValue = (value as _IType);
			}
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x00015DAA File Offset: 0x00014DAA
		public void SetReturnType(TypeClass tc)
		{
			this.m_typeReturnValue = TypeTable.Get(tc);
		}

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000964 RID: 2404 RVA: 0x00015DB8 File Offset: 0x00014DB8
		// (set) Token: 0x06000965 RID: 2405 RVA: 0x00015E2C File Offset: 0x00014E2C
		public List<IVariableDeclarationListStatement> DeclarationLists
		{
			get
			{
				List<IVariableDeclarationListStatement> list = new List<IVariableDeclarationListStatement>();
				ISequenceStatement statDeclarations = this.m_statDeclarations;
				list.AddRange(from IStatement state in statDeclarations.Statements
				where state is IVariableDeclarationListStatement
				select state as IVariableDeclarationListStatement);
				return list;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				SequenceStatement sequenceStatement = new SequenceStatement(value.Count);
				foreach (_IStatement sm in value.OfType<_IStatement>())
				{
					sequenceStatement.Add(sm);
				}
				this.m_statDeclarations = sequenceStatement;
			}
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x00015E90 File Offset: 0x00014E90
		public void AddVariableDeclaration(VarFlag vfFlag, string stName, TypeClass tc, IExpression expInitial)
		{
			this.AddVariableDeclaration(vfFlag, stName, TypeTable.Get(tc), expInitial);
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x00015EA4 File Offset: 0x00014EA4
		public void AddVariableDeclaration(VarFlag vfFlag, string stName, ICompiledType ctype, IExpression expInitial)
		{
			VariableDeclarationStatement variableDeclarationStatement = new VariableDeclarationStatement();
			variableDeclarationStatement.AddName(new VariableExpression(stName));
			variableDeclarationStatement.Type = (ctype as _IType);
			variableDeclarationStatement.Initial = (expInitial as Expression);
			this.AddVariableDeclaration(variableDeclarationStatement, vfFlag);
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x00015EE4 File Offset: 0x00014EE4
		internal void AddVariableDeclaration(VariableDeclarationStatement vds, VarFlag vf)
		{
			if (this.m_statDeclarations == null)
			{
				this.m_statDeclarations = new SequenceStatement();
			}
			bool flag = false;
			foreach (_IStatement istatement in this.m_statDeclarations._StatementList)
			{
				if (istatement is IVariableDeclarationListStatement && (istatement as IVariableDeclarationListStatement).Flags == vf)
				{
					(istatement as VariableDeclarationListStatement).AddVariableDeclaration(vds);
					flag = true;
				}
			}
			if (!flag)
			{
				VariableDeclarationListStatement variableDeclarationListStatement = new VariableDeclarationListStatement();
				variableDeclarationListStatement.Flags = vf;
				variableDeclarationListStatement.AddVariableDeclaration(vds);
				this.m_statDeclarations.AddStatement(variableDeclarationListStatement);
			}
		}

		// Token: 0x04000145 RID: 325
		[DefaultSerialization("Class")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected Operator m_opClass;

		// Token: 0x04000146 RID: 326
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected string m_stName = string.Empty;

		// Token: 0x04000147 RID: 327
		[DefaultSerialization("ReturnValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected _IType m_typeReturnValue;

		// Token: 0x04000148 RID: 328
		[DefaultSerialization("Declarations")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected SequenceStatement m_statDeclarations;

		// Token: 0x04000149 RID: 329
		[DefaultSerialization("QNEExtends")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValueEmptyCollection]
		protected LList<_IExpression> m_lExtends = new LList<_IExpression>();

		// Token: 0x0400014A RID: 330
		[DefaultSerialization("QNEImplements")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[StorageDefaultValueEmptyCollection]
		[Obfuscation(Feature = "rename")]
		protected LList<_IExpression> m_alImplements;

		// Token: 0x0400014B RID: 331
		[DefaultSerialization("NameExpression")]
		[StorageVersion("3.5.5.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		protected _IExpression m_nameExpression;

		// Token: 0x0400014C RID: 332
		[DefaultSerialization("AccessFlag")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(SignatureFlag.None)]
		protected SignatureFlag m_sfFlag;
	}
}
