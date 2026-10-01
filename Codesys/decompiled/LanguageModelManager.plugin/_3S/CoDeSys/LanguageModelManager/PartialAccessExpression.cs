using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000067 RID: 103
	[TypeGuid("F331E365-36D3-4C8F-8EA4-B58AE4443573")]
	[StorageVersion("3.5.19.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Cannot cannot be divided into subclasses because of released interfaces")]
	public class PartialAccessExpression : Expression, _IPartialAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPartialAccessExpression, ILengthExprement
	{
		// Token: 0x06000687 RID: 1671 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public PartialAccessExpression()
		{
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00011983 File Offset: 0x00010983
		internal PartialAccessExpression(_IExpression expLeft, IToken token) : base(token)
		{
			this.m_Left = expLeft;
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x00011993 File Offset: 0x00010993
		internal PartialAccessExpression(_IExpression expLeft)
		{
			this.m_Left = expLeft;
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x000119A2 File Offset: 0x000109A2
		public IExpression Left
		{
			get
			{
				return this._Left;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x0600068B RID: 1675 RVA: 0x000119AA File Offset: 0x000109AA
		// (set) Token: 0x0600068C RID: 1676 RVA: 0x000119C0 File Offset: 0x000109C0
		public _IExpression _Left
		{
			get
			{
				if (this.m_Left == null)
				{
					return new NullExpression();
				}
				return this.m_Left;
			}
			set
			{
				this.m_Left = value;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x0600068D RID: 1677 RVA: 0x000119C9 File Offset: 0x000109C9
		// (set) Token: 0x0600068E RID: 1678 RVA: 0x000119D1 File Offset: 0x000109D1
		public DirectVariableSize PartSize
		{
			get
			{
				return this.m_PartSize;
			}
			set
			{
				this.m_PartSize = value;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x0600068F RID: 1679 RVA: 0x000119DA File Offset: 0x000109DA
		// (set) Token: 0x06000690 RID: 1680 RVA: 0x000119E2 File Offset: 0x000109E2
		public int PartOffset
		{
			get
			{
				return this.m_PartOffset;
			}
			set
			{
				this.m_PartOffset = value;
			}
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x000119EC File Offset: 0x000109EC
		public bool GetByteOffsetAndSize(IScope5 scope, out int byteOffset, out int byteSize)
		{
			byteOffset = 0;
			byteSize = 0;
			ITypeTable3 typeTable = (ITypeTable3)CompilerProxy._TypeTable;
			int directVariableSizeInBits = typeTable.GetDirectVariableSizeInBits(this.PartSize);
			if (directVariableSizeInBits % 8 != 0)
			{
				return false;
			}
			byteSize = directVariableSizeInBits / 8;
			int num;
			if (!typeTable.IsPartialAccessSupportedType(this._Left._CompiledType.Class, out num, (ICommonScope)scope))
			{
				return false;
			}
			if (scope.Codegenerator.MotorolaByteOrder)
			{
				byteOffset = num / 8 - byteSize * (this.PartOffset + 1);
			}
			else
			{
				byteOffset = byteSize * this.PartOffset;
			}
			return true;
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00011A74 File Offset: 0x00010A74
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351900 exprementVisitor = visitor as IExprementVisitor351900;
			if (exprementVisitor != null)
			{
				exprementVisitor.visit(this);
			}
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00011A92 File Offset: 0x00010A92
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00011A9C File Offset: 0x00010A9C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor8 exprVisitor = visitor as IExprVisitor8;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x00011ABA File Offset: 0x00010ABA
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Left.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x00011ACC File Offset: 0x00010ACC
		public override ILiteralValue Literal(IScope scope)
		{
			ILiteralValue litLeft = this._Left.Literal(scope);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00011AFC File Offset: 0x00010AFC
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			ILiteralValue litLeft = this._Left.Literal(scope);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00011B2C File Offset: 0x00010B2C
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			ILiteralValue litLeft = this._Left.Literal(scope, bAllocatedOK);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00011B5C File Offset: 0x00010B5C
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			ILiteralValue litLeft = this._Left.LiteralUnchecked(scope);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00011BA4 File Offset: 0x00010BA4
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			ILiteralValue litLeft = ((_IExpression2)this._Left).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x00011BF4 File Offset: 0x00010BF4
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			ILiteralValue litLeft = ((_IExpression2)this._Left).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			int partOffset = this.PartOffset;
			DirectVariableSize partSize = this.PartSize;
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, partOffset, partSize);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00011C48 File Offset: 0x00010C48
		public override IDataLocation DataLocation(IScope scope)
		{
			IDataLocation dataLocation = this._Left.DataLocation(scope);
			if (dataLocation == null)
			{
				return null;
			}
			if (this.PartSize == DirectVariableSize.X)
			{
				if (dataLocation.IsRelativ)
				{
					return LanguageModelBuilder.Singleton.CreateRelativeBitDataLocation(dataLocation.Offset, (byte)this.PartOffset);
				}
				return LanguageModelBuilder.Singleton.CreateBitDataLocation(dataLocation.Area, dataLocation.Offset, (byte)this.PartOffset);
			}
			else
			{
				int num;
				int num2;
				if (!this.GetByteOffsetAndSize(scope as IScope5, out num, out num2))
				{
					return null;
				}
				if (dataLocation.IsRelativ)
				{
					return LanguageModelBuilder.Singleton.CreateRelativeDataLocation(dataLocation.Offset + num);
				}
				return LanguageModelBuilder.Singleton.CreateDataLocation(dataLocation.Area, dataLocation.Offset + num);
			}
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x00011CF8 File Offset: 0x00010CF8
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this.m_Left.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x0600069E RID: 1694 RVA: 0x00011D26 File Offset: 0x00010D26
		// (set) Token: 0x0600069F RID: 1695 RVA: 0x00011D33 File Offset: 0x00010D33
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_Left.PositionIntern;
			}
			set
			{
				if (this.m_Left != null)
				{
					this.m_Left.PositionIntern = value;
				}
			}
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x00011D49 File Offset: 0x00010D49
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this.m_Left.PositionIntern = minpos;
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060006A1 RID: 1697 RVA: 0x00011D57 File Offset: 0x00010D57
		// (set) Token: 0x060006A2 RID: 1698 RVA: 0x00011D5F File Offset: 0x00010D5F
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x00011D68 File Offset: 0x00010D68
		public override _IExprement Duplicate()
		{
			PartialAccessExpression partialAccessExpression = new PartialAccessExpression();
			if (this.m_Left != null)
			{
				partialAccessExpression.m_Left = (this.m_Left.Duplicate() as Expression);
			}
			this.DuplicateCommon(partialAccessExpression);
			partialAccessExpression.m_PartOffset = this.m_PartOffset;
			partialAccessExpression.m_PartSize = this.m_PartSize;
			return partialAccessExpression;
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x00011DB9 File Offset: 0x00010DB9
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this.m_PartSize != DirectVariableSize.X && this._Left.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x0000C865 File Offset: 0x0000B865
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.IsLValue(scope, bWriteToConstants, false, false);
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00011DD4 File Offset: 0x00010DD4
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			if (this.m_PartSize == DirectVariableSize.X)
			{
				return this._Left.IsLValue(scope, bWriteToConstants);
			}
			_IVariable ivariable = this._Left.GetVariable(scope) as _IVariable;
			if (ivariable != null && ivariable.IsProperty && this._Left.Type != null)
			{
				return this._Left.Type.Class == TypeClass.Reference;
			}
			return this._Left.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x00011E47 File Offset: 0x00010E47
		public override IVariable GetVariable(IScope scope)
		{
			return this.m_Left.GetVariable(scope);
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x00011E55 File Offset: 0x00010E55
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this.m_Left.GetVariable(scope);
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00011E63 File Offset: 0x00010E63
		public override ISignature GetSignature(IScope scope)
		{
			return this.m_Left.GetSignature(scope);
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060006AA RID: 1706 RVA: 0x00011E71 File Offset: 0x00010E71
		// (set) Token: 0x060006AB RID: 1707 RVA: 0x00011E7E File Offset: 0x00010E7E
		public override int SignatureId
		{
			get
			{
				return this.m_Left.SignatureId;
			}
			set
			{
				throw new NotSupportedException("signature id cant be set in partial access");
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060006AC RID: 1708 RVA: 0x00011E8A File Offset: 0x00010E8A
		// (set) Token: 0x060006AD RID: 1709 RVA: 0x00011E97 File Offset: 0x00010E97
		public override int PrecompileVariableId
		{
			get
			{
				return this.m_Left.PrecompileVariableId;
			}
			set
			{
				this.m_Left.PrecompileVariableId = value;
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060006AE RID: 1710 RVA: 0x00011EA5 File Offset: 0x00010EA5
		// (set) Token: 0x060006AF RID: 1711 RVA: 0x00011EB2 File Offset: 0x00010EB2
		public override int PrecompileSignatureId
		{
			get
			{
				return this.m_Left.PrecompileSignatureId;
			}
			set
			{
				this.m_Left.PrecompileSignatureId = value;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060006B0 RID: 1712 RVA: 0x00011EC0 File Offset: 0x00010EC0
		// (set) Token: 0x060006B1 RID: 1713 RVA: 0x00011EC8 File Offset: 0x00010EC8
		[DefaultSerialization("Type")]
		[StorageVersion("3.5.19.0")]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType { get; set; }

		// Token: 0x040000DF RID: 223
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000E1 RID: 225
		[DefaultSerialization("Left")]
		[StorageVersion("3.5.19.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IExpression m_Left;

		// Token: 0x040000E2 RID: 226
		[DefaultSerialization("PartSize")]
		[StorageVersion("3.5.19.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private DirectVariableSize m_PartSize;

		// Token: 0x040000E3 RID: 227
		[DefaultSerialization("PartOffset")]
		[StorageVersion("3.5.19.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private int m_PartOffset;
	}
}
