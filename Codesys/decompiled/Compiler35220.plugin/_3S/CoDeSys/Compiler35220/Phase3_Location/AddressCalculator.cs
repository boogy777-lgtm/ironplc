using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase3_Location
{
	// Token: 0x020002EF RID: 751
	public class AddressCalculator : IAddressCalculator, IAddressCalculatorUnittestSupport
	{
		// Token: 0x170007BF RID: 1983
		// (get) Token: 0x06002DED RID: 11757 RVA: 0x000A8B64 File Offset: 0x000A6D64
		// (set) Token: 0x06002DEE RID: 11758 RVA: 0x000A8B6C File Offset: 0x000A6D6C
		private IMessage Message { get; set; }

		// Token: 0x170007C0 RID: 1984
		// (get) Token: 0x06002DEF RID: 11759 RVA: 0x000A8B78 File Offset: 0x000A6D78
		// (set) Token: 0x06002DF0 RID: 11760 RVA: 0x000A8B80 File Offset: 0x000A6D80
		private bool Handled { get; set; }

		// Token: 0x170007C1 RID: 1985
		// (get) Token: 0x06002DF1 RID: 11761 RVA: 0x000A8B8C File Offset: 0x000A6D8C
		// (set) Token: 0x06002DF2 RID: 11762 RVA: 0x000A8B94 File Offset: 0x000A6D94
		private bool Error { get; set; }

		// Token: 0x170007C2 RID: 1986
		// (get) Token: 0x06002DF3 RID: 11763 RVA: 0x000A8BA0 File Offset: 0x000A6DA0
		// (set) Token: 0x06002DF4 RID: 11764 RVA: 0x000A8BA8 File Offset: 0x000A6DA8
		private ISourcePosition SourcePosition { get; set; }

		// Token: 0x170007C3 RID: 1987
		// (get) Token: 0x06002DF5 RID: 11765 RVA: 0x000A8BB4 File Offset: 0x000A6DB4
		// (set) Token: 0x06002DF6 RID: 11766 RVA: 0x000A8BBC File Offset: 0x000A6DBC
		private _ICompileContext CompileContext { get; set; }

		// Token: 0x170007C4 RID: 1988
		// (get) Token: 0x06002DF7 RID: 11767 RVA: 0x000A8BC8 File Offset: 0x000A6DC8
		// (set) Token: 0x06002DF8 RID: 11768 RVA: 0x000A8BD0 File Offset: 0x000A6DD0
		private IDirectVariable DirectVariable { get; set; }

		// Token: 0x170007C5 RID: 1989
		// (get) Token: 0x06002DF9 RID: 11769 RVA: 0x000A8BDC File Offset: 0x000A6DDC
		// (set) Token: 0x06002DFA RID: 11770 RVA: 0x000A8BE4 File Offset: 0x000A6DE4
		private IVariable2 Variable { get; set; }

		// Token: 0x170007C6 RID: 1990
		// (get) Token: 0x06002DFB RID: 11771 RVA: 0x000A8BF0 File Offset: 0x000A6DF0
		// (set) Token: 0x06002DFC RID: 11772 RVA: 0x000A8BF8 File Offset: 0x000A6DF8
		private _IDataManager DataManager { get; set; }

		// Token: 0x170007C7 RID: 1991
		// (get) Token: 0x06002DFD RID: 11773 RVA: 0x000A8C04 File Offset: 0x000A6E04
		// (set) Token: 0x06002DFE RID: 11774 RVA: 0x000A8C0C File Offset: 0x000A6E0C
		private bool ByteAddressing { get; set; }

		// Token: 0x170007C8 RID: 1992
		// (get) Token: 0x06002DFF RID: 11775 RVA: 0x000A8C18 File Offset: 0x000A6E18
		// (set) Token: 0x06002E00 RID: 11776 RVA: 0x000A8C20 File Offset: 0x000A6E20
		private bool BitByteAddressing { get; set; }

		// Token: 0x170007C9 RID: 1993
		// (get) Token: 0x06002E01 RID: 11777 RVA: 0x000A8C2C File Offset: 0x000A6E2C
		// (set) Token: 0x06002E02 RID: 11778 RVA: 0x000A8C34 File Offset: 0x000A6E34
		private bool BitWordAddressing { get; set; }

		// Token: 0x06002E03 RID: 11779 RVA: 0x000A8C40 File Offset: 0x000A6E40
		public void Initialize(ISourcePosition sp, ICompileContext compilecontext, IDirectVariable dirvar, IVariable2 var)
		{
			_IDataManager u = AddressCalculator.\u0001(compilecontext);
			this.Message = null;
			this.Handled = true;
			this.Error = false;
			this.SourcePosition = sp;
			this.CompileContext = (compilecontext as _ICompileContext);
			this.DirectVariable = dirvar;
			this.Variable = var;
			this.HasByteSupport = this.CompileContext.HasByteSupport();
			this.DataManager = u;
			this.ByteAddressing = this.DataManager.ByteAddressing;
			this.BitByteAddressing = this.DataManager.BitByteAddressing;
			this.BitWordAddressing = this.DataManager.BitWordAddressing;
		}

		// Token: 0x170007CA RID: 1994
		// (get) Token: 0x06002E04 RID: 11780 RVA: 0x000A8CD8 File Offset: 0x000A6ED8
		// (set) Token: 0x06002E05 RID: 11781 RVA: 0x000A8CE0 File Offset: 0x000A6EE0
		public bool HasByteSupport { get; set; }

		// Token: 0x06002E06 RID: 11782 RVA: 0x000A8CEC File Offset: 0x000A6EEC
		public IDataLocation CalculateAddressInternal()
		{
			_IDataSegment u;
			int num;
			byte u2;
			if (!this.\u0001(out u, out num, out u2))
			{
				return null;
			}
			Debug.\u0001((this.DirectVariable.Size == DirectVariableSize.X) ? (this.DirectVariable.Components.Length > 1) : (this.DirectVariable.Components.Length != 0));
			num = this.DirectVariable.Components[0];
			int num2 = AddressCalculator.\u0001(this.DirectVariable);
			int num3 = this.\u0001(num2);
			num *= num3;
			this.\u0001(ref num, ref u2);
			int u3 = this.\u0001(num, num2);
			this.\u0001(u, num, u3);
			return this.\u0001(u, num, u2);
		}

		// Token: 0x06002E07 RID: 11783 RVA: 0x000A8D8C File Offset: 0x000A6F8C
		[ExcludeFromCodeCoverage]
		private bool \u0001(out _IDataSegment \u0002, out int \u0003, out byte \u0004)
		{
			\u0002 = null;
			\u0003 = 0;
			\u0004 = 0;
			if (this.DataManager.Count == 0)
			{
				return false;
			}
			Debug.\u0001(this.DirectVariable != null);
			return !this.DirectVariable.Incomplete && this.\u0001(ref \u0002) && this.\u0006();
		}

		// Token: 0x06002E08 RID: 11784 RVA: 0x000A8DE4 File Offset: 0x000A6FE4
		public IDataLocation CalculateAddress(out IMessage message, out bool bHandled, out bool bError, ISourcePosition sp, ICompileContext compilecontext, IDirectVariable dirvar, IVariable2 var)
		{
			this.Initialize(sp, compilecontext, dirvar, var);
			IDataLocation result = this.CalculateAddressInternal();
			message = this.Message;
			bError = this.Error;
			bHandled = this.Handled;
			return result;
		}

		// Token: 0x06002E09 RID: 11785 RVA: 0x000A8E14 File Offset: 0x000A7014
		internal IDataLocation \u0001(_IDataSegment \u0002, int \u0003, byte \u0004)
		{
			if (this.DirectVariable.Size == DirectVariableSize.X)
			{
				return \u0019.\u0003.\u0001(\u0002.Area, \u0003 + \u0002.Address, \u0004);
			}
			return \u0019.\u0003.\u0001(\u0002.Area, \u0003 + \u0002.Address);
		}

		// Token: 0x06002E0A RID: 11786 RVA: 0x000A8E4C File Offset: 0x000A704C
		internal void \u0001(_IDataSegment \u0002, int \u0003, int \u0004)
		{
			if (\u0003 + \u0004 > \u0002.Size)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_AddressOutOfRange, new object[]
				{
					this.DirectVariable.ToString(),
					\u0003 + \u0004,
					\u0002.Size
				});
				this.Message = \u0019.\u0003.\u0001(this.SourcePosition, u, Severity.Error, MessageId.Err_AddressOutOfRange);
				this.Error = true;
			}
		}

		// Token: 0x06002E0B RID: 11787 RVA: 0x000A8EB8 File Offset: 0x000A70B8
		internal int \u0001(int \u0002, int \u0003)
		{
			int num;
			if (this.Variable != null && this.Variable.CompiledType != null)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.CompileContext);
				num = this.Variable.CompiledType.Size(scope);
				if (num != 0 && !this.Variable.HasAttribute("no_misalignment_check"))
				{
					ITargetSettings deviceSettings = CompilerServicesInternal.\u0001(this.CompileContext.ApplicationGuid);
					if (global::\u0016.\u0004.CheckMisalignedAddress.GetBoolValue(deviceSettings))
					{
						int num2 = Locator.\u0002(this.Variable.CompiledType, 0, scope);
						if (\u0002 % num2 != 0)
						{
							string u = global::\u0003.\u0006.\u0001(MessageId.Err_AddressMisaligned, new object[]
							{
								this.DirectVariable.ToString(),
								this.Variable.CompiledType.ToString()
							});
							this.Message = \u0019.\u0003.\u0001(this.SourcePosition, u, Severity.Error, MessageId.Err_AddressMisaligned);
							this.Error = true;
						}
					}
				}
				if (this.DirectVariable.Size == DirectVariableSize.X && !TypeTable.IsBoolean(this.Variable.CompiledType.Class))
				{
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_NoBitTypeOnBitAddress, new object[]
					{
						this.Variable.CompiledType.ToString(),
						this.DirectVariable.ToString()
					});
					this.Message = \u0019.\u0003.\u0001(this.SourcePosition, u2, Severity.Error, MessageId.Err_NoBitTypeOnBitAddress);
					this.Error = true;
				}
				if (this.Variable.CompiledType.Class == TypeClass.Bool && this.DirectVariable.Size != DirectVariableSize.X)
				{
					string u3 = global::\u0003.\u0006.\u0001(MessageId.Wrn_BoolNotAtBitAddress, Array.Empty<object>());
					this.Message = \u0019.\u0003.\u0001(this.SourcePosition, u3, Messages.\u0001(this.Variable, MessageId.Wrn_BoolNotAtBitAddress), MessageId.Wrn_BoolNotAtBitAddress);
				}
			}
			else
			{
				num = \u0003;
			}
			return num;
		}

		// Token: 0x06002E0C RID: 11788 RVA: 0x000A9070 File Offset: 0x000A7270
		internal void \u0001(ref int \u0002, ref byte \u0003)
		{
			if (this.DirectVariable.Size == DirectVariableSize.X)
			{
				if (this.DirectVariable.Components[1] > 7)
				{
					\u0002 += this.DirectVariable.Components[1] / 8;
					\u0003 = (byte)(this.DirectVariable.Components[1] % 8);
				}
				else
				{
					\u0003 = (byte)this.DirectVariable.Components[1];
				}
				if (!this.HasByteSupport && \u0002 % 2 != 0)
				{
					\u0002--;
					\u0003 += 8;
				}
			}
		}

		// Token: 0x06002E0D RID: 11789 RVA: 0x000A90F0 File Offset: 0x000A72F0
		internal int \u0001(int \u0002)
		{
			int result = 1;
			if (!this.ByteAddressing && (!this.BitByteAddressing || this.DirectVariable.Size != DirectVariableSize.X))
			{
				result = \u0002;
			}
			if (this.DirectVariable.Size == DirectVariableSize.X && this.BitWordAddressing)
			{
				result = 2;
			}
			return result;
		}

		// Token: 0x06002E0E RID: 11790 RVA: 0x000A9138 File Offset: 0x000A7338
		internal static int \u0001(IDirectVariable \u0002)
		{
			int result = 1;
			switch (\u0002.Size)
			{
			case DirectVariableSize.B:
				result = 1;
				break;
			case DirectVariableSize.W:
				result = 2;
				break;
			case DirectVariableSize.D:
				result = 4;
				break;
			case DirectVariableSize.L:
				result = 8;
				break;
			}
			return result;
		}

		// Token: 0x06002E0F RID: 11791 RVA: 0x000A9178 File Offset: 0x000A7378
		internal bool \u0006()
		{
			bool flag = (this.DirectVariable.Size == DirectVariableSize.X && this.DirectVariable.Components.Length != 2) || (this.DirectVariable.Size != DirectVariableSize.X && this.DirectVariable.Components.Length != 1);
			if (this.DirectVariable.Size == DirectVariableSize.None)
			{
				flag = true;
			}
			if (flag)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_MalformedAddress, new object[]
				{
					this.DirectVariable.ToString()
				});
				this.Message = \u0019.\u0003.\u0001(this.SourcePosition, u, Severity.Error, MessageId.Err_MalformedAddress);
				this.Error = true;
				return false;
			}
			return true;
		}

		// Token: 0x06002E10 RID: 11792 RVA: 0x000A9220 File Offset: 0x000A7420
		internal bool \u0001(ref _IDataSegment \u0002)
		{
			bool flag = false;
			for (ushort num = (ushort)(this.DataManager.Count - 1); num >= 0; num -= 1)
			{
				\u0002 = this.DataManager[num];
				switch (this.DirectVariable.Location)
				{
				case DirectVariableLocation.Input:
					if (\u0002.GetFlag(DataSegmentFlags.Input))
					{
						flag = true;
					}
					break;
				case DirectVariableLocation.Output:
					if (\u0002.GetFlag(DataSegmentFlags.Output))
					{
						flag = true;
					}
					break;
				case DirectVariableLocation.Memory:
					if (\u0002.GetFlag(DataSegmentFlags.Memory))
					{
						flag = true;
					}
					break;
				}
				if (flag || num == 0)
				{
					break;
				}
			}
			if (!flag)
			{
				string text = string.Empty;
				MessageId u;
				switch (this.DirectVariable.Location)
				{
				case DirectVariableLocation.Input:
					text = global::\u0003.\u0006.\u0001(MessageId.Err_NoInputMemory, Array.Empty<object>());
					u = MessageId.Err_NoInputMemory;
					break;
				case DirectVariableLocation.Output:
					text = global::\u0003.\u0006.\u0001(MessageId.Err_NoOutputMemory, Array.Empty<object>());
					u = MessageId.Err_NoOutputMemory;
					break;
				case DirectVariableLocation.Memory:
					text = global::\u0003.\u0006.\u0001(MessageId.Err_NoMemoryMemory, Array.Empty<object>());
					u = MessageId.Err_NoMemoryMemory;
					break;
				default:
					text = global::\u0003.\u0006.\u0001(MessageId.Err_MalformedAddress, new object[]
					{
						this.DirectVariable.ToString()
					});
					u = MessageId.Err_MalformedAddress;
					break;
				}
				if (text != string.Empty)
				{
					this.Message = \u0019.\u0003.\u0001(this.SourcePosition, text, Severity.Error, u);
					this.Error = true;
				}
				return false;
			}
			return true;
		}

		// Token: 0x06002E11 RID: 11793 RVA: 0x000A9364 File Offset: 0x000A7564
		internal static _IDataManager \u0001(ICompileContext \u0002)
		{
			_IDataManager dataManager = (\u0002 as _ICompileContext).DataManager;
			for (_ICompileContext parentContext = (\u0002 as _ICompileContext).ParentContext; parentContext != null; parentContext = parentContext.ParentContext)
			{
				dataManager = parentContext.DataManager;
			}
			return dataManager;
		}

		// Token: 0x040008B7 RID: 2231
		[CompilerGenerated]
		private IMessage \u0001;

		// Token: 0x040008B8 RID: 2232
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040008B9 RID: 2233
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040008BA RID: 2234
		[CompilerGenerated]
		private ISourcePosition \u0001;

		// Token: 0x040008BB RID: 2235
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x040008BC RID: 2236
		[CompilerGenerated]
		private IDirectVariable \u0001;

		// Token: 0x040008BD RID: 2237
		[CompilerGenerated]
		private IVariable2 \u0001;

		// Token: 0x040008BE RID: 2238
		[CompilerGenerated]
		private _IDataManager \u0001;

		// Token: 0x040008BF RID: 2239
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x040008C0 RID: 2240
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x040008C1 RID: 2241
		[CompilerGenerated]
		private bool \u0005;

		// Token: 0x040008C2 RID: 2242
		[CompilerGenerated]
		private bool \u0006;
	}
}
