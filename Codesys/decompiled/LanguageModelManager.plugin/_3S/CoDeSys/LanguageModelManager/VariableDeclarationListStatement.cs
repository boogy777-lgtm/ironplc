using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A5 RID: 165
	[TypeGuid("{a53412cb-edf2-4199-87ce-c8f41defd63b}")]
	[StorageVersion("3.3.0.0")]
	public class VariableDeclarationListStatement : PositionStatement, _IVariableDeclarationListStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IVariableDeclarationListStatement
	{
		// Token: 0x060009F7 RID: 2551 RVA: 0x000149DB File Offset: 0x000139DB
		public VariableDeclarationListStatement()
		{
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x000149E3 File Offset: 0x000139E3
		internal VariableDeclarationListStatement(IToken token) : base(token)
		{
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x060009F9 RID: 2553 RVA: 0x00016C01 File Offset: 0x00015C01
		// (set) Token: 0x060009FA RID: 2554 RVA: 0x00016C09 File Offset: 0x00015C09
		public new VarFlag Flags
		{
			get
			{
				return this.m_varflag;
			}
			set
			{
				this.m_varflag = value;
			}
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x00016C12 File Offset: 0x00015C12
		public bool GetFlag(VarFlag vfFlag)
		{
			return (this.m_varflag & vfFlag) == vfFlag;
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x00016C1F File Offset: 0x00015C1F
		public void SetFlag(VarFlag vfFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_varflag |= vfFlag;
				return;
			}
			this.m_varflag &= ~vfFlag;
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x060009FD RID: 2557 RVA: 0x00016C42 File Offset: 0x00015C42
		// (set) Token: 0x060009FE RID: 2558 RVA: 0x00016C58 File Offset: 0x00015C58
		public _IStatement VariableDeclaration
		{
			get
			{
				if (this.m_statVariableDeclaration == null)
				{
					return new NullStatement();
				}
				return this.m_statVariableDeclaration;
			}
			set
			{
				if (value is SequenceStatement)
				{
					this.m_statVariableDeclaration = (value as SequenceStatement);
					return;
				}
				this.m_statVariableDeclaration = new SequenceStatement();
				this.m_statVariableDeclaration.Add(value);
			}
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x00016C86 File Offset: 0x00015C86
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00016C8F File Offset: 0x00015C8F
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x00016C98 File Offset: 0x00015C98
		public override _IExprement Duplicate()
		{
			VariableDeclarationListStatement variableDeclarationListStatement = new VariableDeclarationListStatement();
			this.DuplicateCommon(variableDeclarationListStatement);
			variableDeclarationListStatement.Flags = this.Flags;
			variableDeclarationListStatement.VariableDeclaration = (this.VariableDeclaration.Duplicate() as Statement);
			return variableDeclarationListStatement;
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x00016CD8 File Offset: 0x00015CD8
		public void SetVariableDeclarations(IList<IVariableDeclarationStatement> declarations)
		{
			this.m_statVariableDeclaration = new SequenceStatement(declarations.Count);
			foreach (IVariableDeclarationStatement state in declarations)
			{
				this.m_statVariableDeclaration.AddStatement(state);
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000A04 RID: 2564 RVA: 0x00016D38 File Offset: 0x00015D38
		public IVariableDeclarationStatement[] Declarations
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				foreach (_IStatement value in from _IStatement stmt in this.m_statVariableDeclaration._StatementList
				where stmt is IVariableDeclarationStatement
				select stmt)
				{
					arrayList.Add(value);
				}
				IVariableDeclarationStatement[] array = new IVariableDeclarationStatement[arrayList.Count];
				arrayList.CopyTo(array);
				return array;
			}
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x00016DD0 File Offset: 0x00015DD0
		public void AddVariableDeclaration(IVariableDeclarationStatement vds)
		{
			if (this.m_statVariableDeclaration == null)
			{
				this.m_statVariableDeclaration = new SequenceStatement();
			}
			this.m_statVariableDeclaration.AddStatement(vds as VariableDeclarationStatement);
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x00016DF8 File Offset: 0x00015DF8
		public void InsertVariableDeclaration(int index, IVariableDeclarationStatement vds)
		{
			if (this.m_statVariableDeclaration == null)
			{
				this.m_statVariableDeclaration = new SequenceStatement();
			}
			if (index < 0 || index > this.m_statVariableDeclaration._StatementList.Count)
			{
				this.AddVariableDeclaration(vds);
			}
			this.m_statVariableDeclaration.InsertStatement(index, vds as VariableDeclarationStatement);
		}

		// Token: 0x0400016B RID: 363
		[DefaultSerialization("Flags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private VarFlag m_varflag;

		// Token: 0x0400016C RID: 364
		[DefaultSerialization("Declarations")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _ISequenceStatement m_statVariableDeclaration;
	}
}
