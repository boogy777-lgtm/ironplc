using System;
using System.Collections.Generic;
using System.IO;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0018
{
	// Token: 0x02000294 RID: 660
	internal sealed class \u0008 : \u0081.\u0011, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060029B6 RID: 10678 RVA: 0x00091490 File Offset: 0x0008F690
		public \u0008(BinaryWriter \u0012\u0004, IScope5 \u009B\u0002, bool \u0013\u0004, _IVariable \u001A\u0002) : base(\u001A\u0002)
		{
			this.\u0001 = \u0012\u0004;
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u0013\u0004;
			this.\u0001 = new LStack<\u001A.\u0010>(new \u001A.\u0010[]
			{
				new \u001A.\u0010(\u001A\u0002._Type)
			});
		}

		// Token: 0x060029B7 RID: 10679 RVA: 0x000914FC File Offset: 0x0008F6FC
		public new static byte[] \u0001(IScope5 \u0002, bool \u0003, IVariable \u0004, out IRelocationList2 \u0005)
		{
			\u0005 = null;
			if (\u0004 == null)
			{
				return null;
			}
			_IVariable ivariable = (_IVariable)\u0004;
			_IExpression iexpression = ivariable._Initial;
			if (iexpression == null)
			{
				if (ivariable.CompiledType.Class != TypeClass.Userdef)
				{
					return null;
				}
				_IStructureInitialization istructureInitialization = (_IStructureInitialization)APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder().CreateStructureInitialisation(null, new List<IAssignmentExpression>());
				istructureInitialization._CompiledType = \u0004.CompiledType;
				iexpression = istructureInitialization;
			}
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					try
					{
						\u0018.\u0008 u = new \u0018.\u0008(binaryWriter, \u0002, \u0003, (_IVariable)\u0004);
						iexpression.Accept(u);
						result = memoryStream.ToArray();
						\u0005 = u.Relocationlist;
					}
					catch
					{
						result = null;
					}
				}
			}
			return result;
		}

		// Token: 0x17000749 RID: 1865
		// (get) Token: 0x060029B8 RID: 10680 RVA: 0x000915E4 File Offset: 0x0008F7E4
		public IRelocationList2 Relocationlist
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x060029B9 RID: 10681 RVA: 0x000915EC File Offset: 0x0008F7EC
		private new void \u0001(_IExpression \u0002)
		{
			if (!\u0002.IsConstant(this.\u0001, true))
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			_ILiteralValue iliteralValue;
			try
			{
				iliteralValue = (_ILiteralValue)\u0002.Literal(this.\u0001);
			}
			catch
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			if (iliteralValue != null)
			{
				this.\u0001(\u0002, iliteralValue);
				return;
			}
			_IOperatorExpression ioperatorExpression = \u0002 as _IOperatorExpression;
			if (ioperatorExpression != null)
			{
				this.\u0001(ioperatorExpression);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x060029BA RID: 10682 RVA: 0x0009166C File Offset: 0x0008F86C
		private void \u0002(_IExpression \u0002)
		{
			_IVariable ivariable = \u0002.GetVariable(this.\u0001) as _IVariable;
			if (ivariable != null && TypeTable.IsBlock(ivariable.Type.Class) && ivariable.Initial != null)
			{
				ivariable._Initial.Accept(this);
				return;
			}
			throw new BlobInitException(\u0002, this.\u0001);
		}

		// Token: 0x060029BB RID: 10683 RVA: 0x000916C4 File Offset: 0x0008F8C4
		private new void \u0001(_IOperatorExpression \u0002)
		{
			if (\u0002.Code == Operator.__vcSetReal || \u0002.Code == Operator.__vcSetLReal)
			{
				using (IEnumerator<_IExpression> enumerator = \u0002._OperandsList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IExpression u = enumerator.Current;
						this.\u0001(u);
					}
					return;
				}
			}
			throw new BlobInitException(\u0002, this.\u0001);
		}

		// Token: 0x060029BC RID: 10684 RVA: 0x00091738 File Offset: 0x0008F938
		private new void \u0001(_IExpression \u0002, _ILiteralValue \u0003)
		{
			if (\u0002.Type.Class == TypeClass.Bit)
			{
				if (\u0003.Bool)
				{
					this.\u0001 |= this.\u0001[this.\u0001];
				}
				this.\u0001++;
				if (this.\u0001 == 8)
				{
					this.\u0001 = 0;
					this.\u0001.Write(this.\u0001);
					this.\u0001 = 0;
					return;
				}
			}
			else
			{
				if (\u0002.Type.Class == TypeClass.String || \u0002.Type.Class == TypeClass.WString)
				{
					_IType itype = this.\u0001.Peek().ExpectedType;
					Debug.\u0001(itype.Class == TypeClass.String || itype.Class == TypeClass.WString);
					StringEncoding u = StringEncoding.Default;
					_IStringLiteralExpression2 istringLiteralExpression = \u0002 as _IStringLiteralExpression2;
					if (istringLiteralExpression != null)
					{
						u = istringLiteralExpression.StringEncoding;
					}
					Helper.\u0001(\u0003, itype, this.\u0001, this.\u0001, this.\u0001, u);
					return;
				}
				Helper.\u0001(\u0003, \u0002.Type as _IType, this.\u0001, this.\u0001, this.\u0001, StringEncoding.Default);
			}
		}

		// Token: 0x060029BD RID: 10685 RVA: 0x00091850 File Offset: 0x0008FA50
		private new void \u0001(_IStructureInitialization \u0002)
		{
			long position = this.\u0001.BaseStream.Position;
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				_IVariable ivariable = (_IVariable)iassignmentExpression._LValue.GetVariable(this.\u0001);
				if (ivariable.DataLocation == null || !ivariable.DataLocation.IsRelativ)
				{
					throw new BlobInitException(\u0002, this.\u0001);
				}
				try
				{
					this.\u0001.Push(new \u001A.\u0010(ivariable._Type));
					this.\u0001.BaseStream.Position = position + (long)ivariable.DataLocation.Offset;
					iassignmentExpression._RValue.Accept(this);
				}
				finally
				{
					this.\u0001.Pop();
				}
			}
		}

		// Token: 0x060029BE RID: 10686 RVA: 0x00091944 File Offset: 0x0008FB44
		private new void \u0001(_ISignature \u0002)
		{
			long position = this.\u0001.BaseStream.Position;
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				try
				{
					this.\u0001.Push(new \u001A.\u0010(ivariable._Type));
					this.\u0001.BaseStream.Position = position + (long)ivariable.DataLocation.Offset;
					if (ivariable.Initial != null)
					{
						((_IExpression)ivariable.Initial).Accept(this);
					}
					else
					{
						_ISignature isignature = \u0018.\u0008.\u0001(ivariable, this.\u0001);
						if (isignature != null)
						{
							this.\u0001(isignature);
						}
					}
				}
				finally
				{
					this.\u0001.Pop();
				}
			}
		}

		// Token: 0x060029BF RID: 10687 RVA: 0x00091A20 File Offset: 0x0008FC20
		private new static _ISignature \u0001(IVariable \u0002, IScope \u0003)
		{
			_IUserdefType iuserdefType = \u0002.Type as _IUserdefType;
			if (iuserdefType == null)
			{
				return null;
			}
			return \u0003[iuserdefType.SignatureId] as _ISignature;
		}

		// Token: 0x060029C0 RID: 10688 RVA: 0x00091A50 File Offset: 0x0008FC50
		public new void \u0001(_IArrayInitialization \u0002)
		{
			long position = this.\u0001.BaseStream.Position;
			Debug.\u0001(\u0002.Type is _IArrayType);
			ICompiledType baseType = ((_IArrayType)\u0002.Type).BaseType;
			try
			{
				this.\u0001.Push(new \u001A.\u0010(baseType as _IType));
				foreach (_IExpression iexpression in \u0002._InitValues)
				{
					long position2 = this.\u0001.BaseStream.Position;
					iexpression.Accept(this);
					int num = 1;
					_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
					if (imultipleIndexInitialization != null)
					{
						int num2;
						if (!imultipleIndexInitialization._Number.Literal(this.\u0001, true).GetInt(out num2))
						{
							throw new BlobInitException(imultipleIndexInitialization, this.\u0001);
						}
						num = num2;
					}
					this.\u0001.BaseStream.Position = position2 + (long)(num * baseType.Size(this.\u0001));
				}
			}
			finally
			{
				this.\u0001.Pop();
			}
			this.\u0001.BaseStream.Position = position + (long)\u0002.Type.Size(this.\u0001);
		}

		// Token: 0x060029C1 RID: 10689 RVA: 0x00091B98 File Offset: 0x0008FD98
		public new void \u0001(_IMultipleIndexInitialization \u0002)
		{
			int num;
			if (!\u0002._Number.Literal(this.\u0001, true).GetInt(out num))
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			for (int i = 0; i < num; i++)
			{
				\u0002._Value.Accept(this);
			}
		}

		// Token: 0x060029C2 RID: 10690 RVA: 0x00091BE8 File Offset: 0x0008FDE8
		public void \u0002(_IStructureInitialization \u0002)
		{
			_IUserdefType iuserdefType = \u0002.Type as _IUserdefType;
			if (iuserdefType == null)
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			_ISignature isignature = this.\u0001[iuserdefType.SignatureId] as _ISignature;
			if (isignature == null)
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			long position = this.\u0001.BaseStream.Position;
			this.\u0001(isignature);
			this.\u0001.BaseStream.Position = position;
			this.\u0001(\u0002);
			this.\u0001.BaseStream.Position = position + (long)iuserdefType.Size(this.\u0001);
		}

		// Token: 0x060029C3 RID: 10691 RVA: 0x00091C88 File Offset: 0x0008FE88
		public void \u0002(_IOperatorExpression \u0002)
		{
			if (\u0002.Code != Operator.Adr)
			{
				this.\u0001(\u0002);
				return;
			}
			_IDataLocation idataLocation = \u0002._OperandsList[0].DataLocation(this.\u0001) as _IDataLocation;
			if (\u0002._OperandsList[0] is _IAddressExpression)
			{
				_IAddressExpression iaddressExpression = (_IAddressExpression)\u0002._OperandsList[0];
				bool flag;
				idataLocation = (this.\u0001.ApplicationContext.LocateAddress(out flag, iaddressExpression.DirectAddress) as _IDataLocation);
			}
			if (idataLocation == null || idataLocation.IsRelativ)
			{
				throw new BlobInitException(\u0002, this.\u0001);
			}
			this.\u0001.AddRelocation((int)idataLocation.Area, (int)this.\u0001.BaseStream.Position);
			if (this.\u0001)
			{
				this.\u0001.Write(BitHelper.Swap((uint)idataLocation.Offset));
				return;
			}
			this.\u0001.Write((uint)idataLocation.Offset);
		}

		// Token: 0x060029C4 RID: 10692 RVA: 0x00091D74 File Offset: 0x0008FF74
		public new void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
		}

		// Token: 0x060029C5 RID: 10693 RVA: 0x00091D84 File Offset: 0x0008FF84
		public new void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029C6 RID: 10694 RVA: 0x00091D90 File Offset: 0x0008FF90
		public new void \u0001(_ILiteralExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029C7 RID: 10695 RVA: 0x00091D9C File Offset: 0x0008FF9C
		public new void \u0001(_IVariableExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029C8 RID: 10696 RVA: 0x00091DA8 File Offset: 0x0008FFA8
		public new void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029C9 RID: 10697 RVA: 0x00091DB4 File Offset: 0x0008FFB4
		public new void \u0001(_ICompoAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029CA RID: 10698 RVA: 0x00091DC0 File Offset: 0x0008FFC0
		public new void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029CB RID: 10699 RVA: 0x00091DCC File Offset: 0x0008FFCC
		public new void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060029CC RID: 10700 RVA: 0x00091DD8 File Offset: 0x0008FFD8
		public new void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0400079C RID: 1948
		private new readonly BinaryWriter \u0001;

		// Token: 0x0400079D RID: 1949
		private new readonly IScope5 \u0001;

		// Token: 0x0400079E RID: 1950
		private new readonly bool \u0001;

		// Token: 0x0400079F RID: 1951
		private new readonly _IRelocationList \u0001 = \u0019.\u0003.\u0001();

		// Token: 0x040007A0 RID: 1952
		private new readonly LStack<\u001A.\u0010> \u0001;

		// Token: 0x040007A1 RID: 1953
		private new byte \u0001;

		// Token: 0x040007A2 RID: 1954
		private new int \u0001;

		// Token: 0x040007A3 RID: 1955
		private const byte \u0002 = 1;

		// Token: 0x040007A4 RID: 1956
		private const byte \u0003 = 2;

		// Token: 0x040007A5 RID: 1957
		private const byte \u0004 = 4;

		// Token: 0x040007A6 RID: 1958
		private const byte \u0005 = 8;

		// Token: 0x040007A7 RID: 1959
		private const byte \u0006 = 16;

		// Token: 0x040007A8 RID: 1960
		private const byte \u0007 = 32;

		// Token: 0x040007A9 RID: 1961
		private const byte \u0008 = 64;

		// Token: 0x040007AA RID: 1962
		private const byte \u000E = 128;

		// Token: 0x040007AB RID: 1963
		private new readonly byte[] \u0001 = new byte[]
		{
			1,
			2,
			4,
			8,
			16,
			32,
			64,
			128
		};
	}
}
