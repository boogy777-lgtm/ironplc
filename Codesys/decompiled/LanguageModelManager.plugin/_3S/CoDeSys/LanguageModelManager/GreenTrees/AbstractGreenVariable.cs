using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000235 RID: 565
	[DebuggerDisplay("Variable {OrgName} : {(Type == null) ? \"null\" : Type.ToString()}")]
	internal abstract class AbstractGreenVariable : _IVariable2, _IVariable, IVariable5, IVariable4, IVariable3, IVariable2, IVariable, IVariableWithCompactedInitialValue
	{
		// Token: 0x17000A6F RID: 2671
		// (get) Token: 0x0600250C RID: 9484 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsCommentDocu
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000A70 RID: 2672
		// (get) Token: 0x0600250D RID: 9485 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600250E RID: 9486 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual string CommentValue
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A71 RID: 2673
		// (get) Token: 0x0600250F RID: 9487 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002510 RID: 9488 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual IDirectVariable Address
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A72 RID: 2674
		// (get) Token: 0x06002511 RID: 9489 RVA: 0x0005CFBE File Offset: 0x0005BFBE
		// (set) Token: 0x06002512 RID: 9490 RVA: 0x0005CFD0 File Offset: 0x0005BFD0
		public virtual string DocuComment
		{
			get
			{
				if (!this.IsCommentDocu)
				{
					return null;
				}
				return this.CommentValue;
			}
			set
			{
				if (this.IsCommentDocu)
				{
					this.CommentValue = value;
					return;
				}
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A73 RID: 2675
		// (get) Token: 0x06002513 RID: 9491 RVA: 0x0005CFE7 File Offset: 0x0005BFE7
		// (set) Token: 0x06002514 RID: 9492 RVA: 0x0005CFEF File Offset: 0x0005BFEF
		public virtual string Comment
		{
			get
			{
				return this.CommentValue;
			}
			set
			{
				if (!this.IsCommentDocu)
				{
					this.CommentValue = value;
					return;
				}
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A74 RID: 2676
		// (get) Token: 0x06002515 RID: 9493 RVA: 0x000042F0 File Offset: 0x000032F0
		// (set) Token: 0x06002516 RID: 9494 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual int Id
		{
			get
			{
				return -1;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A75 RID: 2677
		// (get) Token: 0x06002517 RID: 9495 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002518 RID: 9496 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual IExpression Initial
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A76 RID: 2678
		// (get) Token: 0x06002519 RID: 9497 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600251A RID: 9498 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual IAssignmentExpression[] InputAssignments
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A77 RID: 2679
		// (get) Token: 0x0600251B RID: 9499 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600251C RID: 9500 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual _IExpression _Initial
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A78 RID: 2680
		// (get) Token: 0x0600251D RID: 9501 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600251E RID: 9502 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual ExpressionWithCompactedInformation InitialWithCompactedInformation
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A79 RID: 2681
		// (get) Token: 0x0600251F RID: 9503 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002520 RID: 9504 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual ICompactedParseTreeInformation CompactedInitialValueInformation
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A7A RID: 2682
		// (get) Token: 0x06002521 RID: 9505 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06002522 RID: 9506 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual GreenVariableProperies GTFlags
		{
			get
			{
				return GreenVariableProperies.None;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A7B RID: 2683
		// (get) Token: 0x06002523 RID: 9507 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002524 RID: 9508 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual _IExpression OriginalInitial
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A7C RID: 2684
		// (get) Token: 0x06002525 RID: 9509 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual ICompiledType CompiledType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000A7D RID: 2685
		// (get) Token: 0x06002526 RID: 9510 RVA: 0x0005D006 File Offset: 0x0005C006
		public virtual ICompiledType CompiledTypeInternal
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000A7E RID: 2686
		// (get) Token: 0x06002527 RID: 9511 RVA: 0x0005D006 File Offset: 0x0005C006
		public virtual ICompiledType OriginalType
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000A7F RID: 2687
		// (get) Token: 0x06002528 RID: 9512 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002529 RID: 9513 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual IDataLocation DataLocation
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000A80 RID: 2688
		// (get) Token: 0x0600252A RID: 9514 RVA: 0x0005D010 File Offset: 0x0005C010
		public virtual Guid MessageGuid
		{
			get
			{
				if (this.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID))
				{
					string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID);
					Guid result;
					try
					{
						result = new Guid(attributeValue);
					}
					catch
					{
						result = Guid.Empty;
					}
					return result;
				}
				return Guid.Empty;
			}
		}

		// Token: 0x17000A81 RID: 2689
		// (get) Token: 0x0600252B RID: 9515 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsProperty
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000A82 RID: 2690
		// (get) Token: 0x0600252C RID: 9516 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsPropertyMonitor
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000A83 RID: 2691
		// (get) Token: 0x0600252D RID: 9517 RVA: 0x0005D060 File Offset: 0x0005C060
		public virtual bool IsVarInoutConstant
		{
			get
			{
				return this.GetFlag(VarFlag.Inout | VarFlag.Constant);
			}
		}

		// Token: 0x17000A84 RID: 2692
		// (get) Token: 0x0600252E RID: 9518
		// (set) Token: 0x0600252F RID: 9519
		public abstract string Name { get; set; }

		// Token: 0x17000A85 RID: 2693
		// (get) Token: 0x06002530 RID: 9520
		// (set) Token: 0x06002531 RID: 9521
		public abstract string OrgName { get; set; }

		// Token: 0x17000A86 RID: 2694
		// (get) Token: 0x06002532 RID: 9522
		// (set) Token: 0x06002533 RID: 9523
		public abstract int PrecompileId { get; set; }

		// Token: 0x17000A87 RID: 2695
		// (get) Token: 0x06002534 RID: 9524
		public abstract IType Type { get; }

		// Token: 0x17000A88 RID: 2696
		// (get) Token: 0x06002535 RID: 9525
		public abstract string VersionedName { get; }

		// Token: 0x17000A89 RID: 2697
		// (get) Token: 0x06002536 RID: 9526
		// (set) Token: 0x06002537 RID: 9527
		public abstract _IType _Type { get; set; }

		// Token: 0x06002538 RID: 9528 RVA: 0x0005D06B File Offset: 0x0005C06B
		public IVariable Duplicate()
		{
			return this.Duplicate(false);
		}

		// Token: 0x06002539 RID: 9529 RVA: 0x0005D074 File Offset: 0x0005C074
		public _IVariable Duplicate(bool bDeep)
		{
			return GreenVariableFactory.CreateGreenVariable(this);
		}

		// Token: 0x0600253A RID: 9530 RVA: 0x0005D07C File Offset: 0x0005C07C
		public bool InitialValueEquals(IVariable varCompile)
		{
			return this.InitialValueEquals(varCompile, true);
		}

		// Token: 0x0600253B RID: 9531 RVA: 0x000557D0 File Offset: 0x000547D0
		private bool InitialValueEquals(IVariable other, bool bCompiled)
		{
			return CompilerProxy.ComparisonService.InitialValueEquals(this, other as _IVariable, bCompiled);
		}

		// Token: 0x0600253C RID: 9532 RVA: 0x0005D086 File Offset: 0x0005C086
		public bool IsEqual(IVariable varRight, bool bCompareInitValues)
		{
			return this.IsEqual(varRight, bCompareInitValues, true, true, null);
		}

		// Token: 0x0600253D RID: 9533 RVA: 0x0005D093 File Offset: 0x0005C093
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes)
		{
			return this.IsEqual(varRight, bCompareInitValues, bCompareAttributes, true, null);
		}

		// Token: 0x0600253E RID: 9534 RVA: 0x0005D0A0 File Offset: 0x0005C0A0
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope)
		{
			bool flag = false;
			return this.IsEqual(varRight, bCompareInitValues, bCompareAttributes, bCompiled, scope, null, null, ref flag);
		}

		// Token: 0x0600253F RID: 9535 RVA: 0x0005D0C0 File Offset: 0x0005C0C0
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			return CompilerProxy.ComparisonService.IsEqual(this, varRight as _IVariable, bCompareInitValues, bCompareAttributes, bCompiled, scope, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged);
		}

		// Token: 0x06002540 RID: 9536 RVA: 0x0000677E File Offset: 0x0000577E
		public void SetInitial(IExpression expInitial)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002541 RID: 9537 RVA: 0x0000677E File Offset: 0x0000577E
		public void SetInputAssignments(ICollection<_IAssignmentExpression> inputs)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002542 RID: 9538 RVA: 0x0000677E File Offset: 0x0000577E
		public void SetType(ICompiledType ctype)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000A8A RID: 2698
		// (get) Token: 0x06002543 RID: 9539 RVA: 0x0005D0EA File Offset: 0x0005C0EA
		public virtual ICrossReference[] CrossReferences
		{
			get
			{
				return Array.Empty<ICrossReference>();
			}
		}

		// Token: 0x17000A8B RID: 2699
		// (get) Token: 0x06002544 RID: 9540 RVA: 0x0005D0F4 File Offset: 0x0005C0F4
		public virtual IEnumerable<ICrossReference> PrecompileCrossReferences
		{
			get
			{
				IEnumerable<ICrossReference> result;
				lock (this)
				{
					if (this._crf == null)
					{
						result = Array.Empty<ICrossReference>();
					}
					else if (this._crf is int)
					{
						result = new ICrossReference[]
						{
							new CrossReference((int)this._crf)
						};
					}
					else
					{
						int[] array;
						if (this._crf is int[])
						{
							array = (this._crf as int[]);
						}
						else
						{
							if (!(this._crf is LHashSet<int>))
							{
								return Array.Empty<ICrossReference>();
							}
							array = (this._crf as LHashSet<int>).ToArray<int>();
						}
						ICrossReference[] array2 = new ICrossReference[array.Count<int>()];
						for (int i = 0; i < array.Count<int>(); i++)
						{
							array2[i] = new CrossReference(array[i]);
						}
						result = array2;
					}
				}
				return result;
			}
		}

		// Token: 0x06002545 RID: 9541 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void AddCrossReference(int nCodeId, ICodePosition copos)
		{
		}

		// Token: 0x06002546 RID: 9542 RVA: 0x0005D1E4 File Offset: 0x0005C1E4
		private bool HasCrossReference(int nCodeId)
		{
			if (this._crf == null)
			{
				return false;
			}
			if (this._crf is int)
			{
				return (int)this._crf == nCodeId;
			}
			if (this._crf is int[])
			{
				int[] array = this._crf as int[];
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == nCodeId)
					{
						return true;
					}
				}
				return false;
			}
			return this._crf is LHashSet<int> && (this._crf as LHashSet<int>).Contains(nCodeId);
		}

		// Token: 0x06002547 RID: 9543 RVA: 0x0005D268 File Offset: 0x0005C268
		public virtual void AddPrecompileCrossReference(int nCodeId)
		{
			lock (this)
			{
				if (!this.HasCrossReference(nCodeId))
				{
					if (this._crf == null)
					{
						this._crf = nCodeId;
					}
					else if (this._crf is int && (int)this._crf != nCodeId)
					{
						this._crf = new int[]
						{
							(int)this._crf,
							nCodeId
						};
					}
					else if (this._crf is int[])
					{
						int[] array = this._crf as int[];
						if (array.Count<int>() < 10)
						{
							int[] array2 = new int[array.Count<int>() + 1];
							array.CopyTo(array2, 0);
							array2[array.Count<int>()] = nCodeId;
							this._crf = array2;
						}
						else
						{
							LHashSet<int> lhashSet = new LHashSet<int>();
							Enumerable.AddRange<int>(lhashSet, array);
							lhashSet.Add(nCodeId);
							this._crf = lhashSet;
						}
					}
					else if (this._crf is LHashSet<int>)
					{
						(this._crf as LHashSet<int>).Add(nCodeId);
					}
				}
			}
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x0005D390 File Offset: 0x0005C390
		public virtual bool HasFlag(VarFlag vfFlag)
		{
			return (this.Flags & vfFlag) > VarFlag.None;
		}

		// Token: 0x06002549 RID: 9545 RVA: 0x0005D39E File Offset: 0x0005C39E
		public virtual bool GetFlag(VarFlag vfFlag)
		{
			return (this.Flags & vfFlag) == vfFlag;
		}

		// Token: 0x17000A8C RID: 2700
		// (get) Token: 0x0600254A RID: 9546 RVA: 0x0005D3AB File Offset: 0x0005C3AB
		// (set) Token: 0x0600254B RID: 9547 RVA: 0x0005D3B3 File Offset: 0x0005C3B3
		public virtual VarFlag Flags { get; set; }

		// Token: 0x0600254C RID: 9548 RVA: 0x0005D3BC File Offset: 0x0005C3BC
		public virtual void SetFlag(VarFlag vf, bool bSet)
		{
			if (bSet)
			{
				this.Flags |= vf;
				return;
			}
			this.Flags &= ~vf;
		}

		// Token: 0x0600254D RID: 9549 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual void AddAttribute(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000A8D RID: 2701
		// (get) Token: 0x0600254E RID: 9550 RVA: 0x0005D3DF File Offset: 0x0005C3DF
		public virtual string[] Attributes
		{
			get
			{
				if (!string.IsNullOrEmpty(this.CommentValue))
				{
					return new string[]
					{
						this.IsCommentDocu ? CompileAttributes.ATTRIBUTE_DOCUCOMMENT : CompileAttributes.ATTRIBUTE_COMMENT
					};
				}
				return Array.Empty<string>();
			}
		}

		// Token: 0x0600254F RID: 9551 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual void SetAttributes(IDictionary<string, string> attributes)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002550 RID: 9552 RVA: 0x0005D411 File Offset: 0x0005C411
		public virtual string GetAttributeValue(string stAttribute)
		{
			if (stAttribute == CompileAttributes.ATTRIBUTE_COMMENT && !this.IsCommentDocu)
			{
				return this.CommentValue;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_DOCUCOMMENT && this.IsCommentDocu)
			{
				return this.CommentValue;
			}
			return null;
		}

		// Token: 0x06002551 RID: 9553 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual void SetAttributeValue(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002552 RID: 9554 RVA: 0x0005D44C File Offset: 0x0005C44C
		public virtual bool HasAttribute(string stAttribute)
		{
			if (stAttribute == CompileAttributes.ATTRIBUTE_COMMENT)
			{
				return !this.IsCommentDocu && !string.IsNullOrEmpty(this.CommentValue);
			}
			return stAttribute == CompileAttributes.ATTRIBUTE_DOCUCOMMENT && this.IsCommentDocu && !string.IsNullOrEmpty(this.CommentValue);
		}

		// Token: 0x06002553 RID: 9555 RVA: 0x0000677E File Offset: 0x0000577E
		public virtual void RemoveAttribute(string stAttribute)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000A8E RID: 2702
		// (get) Token: 0x06002554 RID: 9556 RVA: 0x0005D4A6 File Offset: 0x0005C4A6
		public virtual ISourcePosition SourcePosition
		{
			get
			{
				return this._SourcePosition;
			}
		}

		// Token: 0x17000A8F RID: 2703
		// (get) Token: 0x06002555 RID: 9557 RVA: 0x0005D4B0 File Offset: 0x0005C4B0
		// (set) Token: 0x06002556 RID: 9558 RVA: 0x0005D4E6 File Offset: 0x0005C4E6
		public virtual _ISourcePosition _SourcePosition
		{
			get
			{
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(this.lSourcePosition, ref nPosition, ref sPositionOffset);
				return new SourcePosition(-1, this.MessageGuid, nPosition, sPositionOffset, (short)this.OrgName.Length);
			}
			set
			{
				this.lSourcePosition = value.PositionCombination;
			}
		}

		// Token: 0x0400071A RID: 1818
		private object _crf;

		// Token: 0x0400071C RID: 1820
		private long lSourcePosition;
	}
}
