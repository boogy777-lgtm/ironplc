using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A6 RID: 166
	[TypeGuid("{16eb15d0-8726-4d02-821e-7e69043cc423}")]
	[StorageVersion("3.3.0.0")]
	public class VariableDeclarationStatement : PositionStatement, _IVariableDeclarationStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IVariableDeclarationStatement
	{
		// Token: 0x06000A07 RID: 2567 RVA: 0x00016E48 File Offset: 0x00015E48
		public VariableDeclarationStatement()
		{
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00016E5C File Offset: 0x00015E5C
		internal VariableDeclarationStatement(IToken token) : base(token)
		{
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000A09 RID: 2569 RVA: 0x00016E74 File Offset: 0x00015E74
		public _IExpression[] Names
		{
			get
			{
				Expression[] array = new Expression[this.m_alNames.Count];
				LList<_IExpression> alNames = this.m_alNames;
				_IExpression[] array2 = array;
				alNames.CopyTo(array2);
				return array;
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000A0A RID: 2570 RVA: 0x00016EA3 File Offset: 0x00015EA3
		// (set) Token: 0x06000A0B RID: 2571 RVA: 0x00016EAB File Offset: 0x00015EAB
		public IList<_IExpression> NameList
		{
			get
			{
				return this.m_alNames;
			}
			set
			{
				this.m_alNames = (value as LList<_IExpression>);
			}
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x00016EB9 File Offset: 0x00015EB9
		public void AddName(_IExpression exp)
		{
			this.m_alNames.Add(exp);
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000A0D RID: 2573 RVA: 0x00016EC7 File Offset: 0x00015EC7
		// (set) Token: 0x06000A0E RID: 2574 RVA: 0x00016ECF File Offset: 0x00015ECF
		public _IType Type
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000A0F RID: 2575 RVA: 0x00016ED8 File Offset: 0x00015ED8
		// (set) Token: 0x06000A10 RID: 2576 RVA: 0x00016EE0 File Offset: 0x00015EE0
		public _IExpression Initial
		{
			get
			{
				return this.m_expInitial;
			}
			set
			{
				this.m_expInitial = value;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000A11 RID: 2577 RVA: 0x00016EE9 File Offset: 0x00015EE9
		// (set) Token: 0x06000A12 RID: 2578 RVA: 0x00016EF1 File Offset: 0x00015EF1
		public IDirectVariable Address
		{
			get
			{
				return this.m_address;
			}
			set
			{
				this.m_address = value;
			}
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x00016EFA File Offset: 0x00015EFA
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x00016F03 File Offset: 0x00015F03
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x00016F0C File Offset: 0x00015F0C
		public void AddParam(_IExpression exp, _IExpression expVariable)
		{
			if (this.m_alexpActualParams == null)
			{
				this.m_alexpActualParams = new LList<_IExpression>(1);
				this.m_alexpFormalParams = new LList<_IExpression>(1);
			}
			this.m_alexpActualParams.Add(exp);
			this.m_alexpFormalParams.Add(expVariable);
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x00016F46 File Offset: 0x00015F46
		public void SetFormalParam(_IExpression expInput, int iIndex)
		{
			this.m_alexpFormalParams[iIndex] = expInput;
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x00016F55 File Offset: 0x00015F55
		public void SetActualParam(_IExpression expInput, int iIndex)
		{
			this.m_alexpActualParams[iIndex] = expInput;
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000A19 RID: 2585 RVA: 0x00016F64 File Offset: 0x00015F64
		[SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Behaviour cannot be changed because of released interfaces")]
		public ICollection<_IAssignmentExpression> InputAssigns
		{
			get
			{
				if (this.m_alexpActualParams == null)
				{
					return null;
				}
				_IAssignmentExpression[] array = new _IAssignmentExpression[this.m_alexpActualParams.Count];
				for (int i = 0; i < this.m_alexpActualParams.Count; i++)
				{
					array[i] = new AssignmentExpression(this.m_alexpFormalParams[i]);
					array[i]._RValue = this.m_alexpActualParams[i];
				}
				return array;
			}
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x00016FCC File Offset: 0x00015FCC
		public override _IExprement Duplicate()
		{
			VariableDeclarationStatement variableDeclarationStatement = new VariableDeclarationStatement();
			this.DuplicateCommon(variableDeclarationStatement);
			variableDeclarationStatement.NameList = new LList<_IExpression>(this.NameList);
			variableDeclarationStatement.Type = this.Type;
			variableDeclarationStatement.Address = this.Address;
			VariableDeclarationStatement variableDeclarationStatement2 = variableDeclarationStatement;
			_IExpression initial = this.Initial;
			variableDeclarationStatement2.Initial = (((initial != null) ? initial.Duplicate() : null) as Expression);
			variableDeclarationStatement.RefAssignInitialisation = this.RefAssignInitialisation;
			if (this.m_alexpActualParams != null)
			{
				variableDeclarationStatement.m_alexpActualParams = new LList<_IExpression>(this.m_alexpActualParams.Count);
				foreach (_IExpression iexpression in this.m_alexpActualParams)
				{
					variableDeclarationStatement.m_alexpActualParams.Add(iexpression.Duplicate() as _IExpression);
				}
			}
			if (this.m_alexpFormalParams != null)
			{
				variableDeclarationStatement.m_alexpFormalParams = new LList<_IExpression>(this.m_alexpFormalParams.Count);
				foreach (_IExpression iexpression2 in this.m_alexpFormalParams)
				{
					if (iexpression2 != null)
					{
						variableDeclarationStatement.m_alexpFormalParams.Add(iexpression2.Duplicate() as _IExpression);
					}
					else
					{
						variableDeclarationStatement.m_alexpFormalParams.Add(null);
					}
				}
			}
			return variableDeclarationStatement;
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000A1B RID: 2587 RVA: 0x00017120 File Offset: 0x00016120
		// (set) Token: 0x06000A1C RID: 2588 RVA: 0x00017128 File Offset: 0x00016128
		public bool OldInputAssigns { get; set; }

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000A1D RID: 2589 RVA: 0x00017134 File Offset: 0x00016134
		public IExpression[] VariableNames
		{
			get
			{
				IExpression[] array = new IExpression[this.Names.Length];
				Array.Copy(this.Names, array, this.Names.Length);
				return array;
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000A1E RID: 2590 RVA: 0x00017164 File Offset: 0x00016164
		public IType DeclaredType
		{
			get
			{
				return this.Type;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000A1F RID: 2591 RVA: 0x0001716C File Offset: 0x0001616C
		public IExpression InitializationExpression
		{
			get
			{
				return this.Initial;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000A20 RID: 2592 RVA: 0x00017174 File Offset: 0x00016174
		public IDirectVariable AddressLocation
		{
			get
			{
				return this.Address;
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x0001717C File Offset: 0x0001617C
		// (set) Token: 0x06000A22 RID: 2594 RVA: 0x00017184 File Offset: 0x00016184
		public bool RefAssignInitialisation
		{
			get
			{
				return this._bRefAssignInitialisation;
			}
			set
			{
				this._bRefAssignInitialisation = value;
			}
		}

		// Token: 0x0400016D RID: 365
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alNames = new LList<_IExpression>(1);

		// Token: 0x0400016E RID: 366
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_type;

		// Token: 0x0400016F RID: 367
		[DefaultSerialization("Initial")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expInitial;

		// Token: 0x04000170 RID: 368
		[DefaultSerialization("Address")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDirectVariable m_address;

		// Token: 0x04000171 RID: 369
		[DefaultSerialization("ActualParams")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpActualParams;

		// Token: 0x04000172 RID: 370
		[DefaultSerialization("FormalParams")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpFormalParams;

		// Token: 0x04000173 RID: 371
		[DefaultSerialization("RefAssign")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool _bRefAssignInitialisation;
	}
}
