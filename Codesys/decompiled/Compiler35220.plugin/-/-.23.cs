using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0001;
using \u0017;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;

namespace \u0012
{
	// Token: 0x02000077 RID: 119
	internal class \u0002 : IExprementVisitorNoTraversion, ICheckSumVisitor352000, ICheckSumVisitor, IExprementVisitorNoTraversion2, IExprementVisitorNoTraversion352000, IExprementVisitorNoTraversion351900, IExprementVisitorNoTraversion351800, IExprementVisitorNoTraversion351500, IExprementVisitorNoTraversion351400, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590
	{
		// Token: 0x060009DC RID: 2524 RVA: 0x000136B4 File Offset: 0x000118B4
		private static byte[] \u0001(object \u0002)
		{
			LDictionary<Type, byte[]> u = global::\u0012.\u0002.\u0001;
			byte[] result;
			lock (u)
			{
				byte[] array;
				if (!global::\u0012.\u0002.\u0001.TryGetValue(\u0002.GetType(), ref array))
				{
					array = TypeGuidAttribute.FromObject(\u0002).Guid.ToByteArray();
					global::\u0012.\u0002.\u0001[\u0002.GetType()] = array;
				}
				result = array;
			}
			return result;
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x0001372C File Offset: 0x0001192C
		public \u0002(bool \u009E)
		{
			this.\u0001 = new MyChecksumStream(false);
			this.Writer = new BinaryWriter(this.\u0001);
			this.Traverser = new global::\u0001.\u0001(this, \u009E);
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x00013760 File Offset: 0x00011960
		static \u0002()
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "ConsiderSourcePositionForCodeChecksum"))
			{
				global::\u0012.\u0002.ConsiderSourcePosition = oemcustomization.GetBoolValue("LanguageModelManager", "ConsiderSourcePositionForCodeChecksum");
			}
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x000137B4 File Offset: 0x000119B4
		internal void \u0001(_ISignature \u0002)
		{
			if (\u0002.POUType != Operator.Method && \u0002.POUType != Operator.Function)
			{
				return;
			}
			foreach (IVariable variable in \u0002.Inputs)
			{
				if (!variable.GetFlag(VarFlag.Implicit))
				{
					if (variable.Initial == null)
					{
						this.Writer.Write(" ");
					}
					else
					{
						((_IExpression)variable.Initial).Accept(this.Traverser);
					}
				}
			}
		}

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x060009E0 RID: 2528 RVA: 0x00013830 File Offset: 0x00011A30
		// (set) Token: 0x060009E1 RID: 2529 RVA: 0x00013838 File Offset: 0x00011A38
		public static bool ConsiderSourcePosition { get; set; } = false;

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x060009E2 RID: 2530 RVA: 0x00013840 File Offset: 0x00011A40
		public BinaryWriter Writer { get; }

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x060009E3 RID: 2531 RVA: 0x00013848 File Offset: 0x00011A48
		// (set) Token: 0x060009E4 RID: 2532 RVA: 0x00013850 File Offset: 0x00011A50
		public IStandardTraverser Traverser { get; set; }

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x060009E5 RID: 2533 RVA: 0x0001385C File Offset: 0x00011A5C
		public bool DoCallExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x060009E6 RID: 2534 RVA: 0x00013860 File Offset: 0x00011A60
		public bool DoAssignExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x00013864 File Offset: 0x00011A64
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this.Traverser);
			this.Writer.Write(\u0002.GetFlag(CompiledPOUFlags.TopLevel));
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x0001388C File Offset: 0x00011A8C
		internal static long \u0001(long \u0002)
		{
			if (\u0002 == 0L)
			{
				return \u0002;
			}
			long num;
			for (num = 256L; num < \u0002; num *= 2L)
			{
			}
			return num;
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x060009E9 RID: 2537 RVA: 0x000138B4 File Offset: 0x00011AB4
		public uint Checksum
		{
			get
			{
				this.Writer.Flush();
				this.\u0001.Close();
				return this.\u0001.Checksum;
			}
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x000138D8 File Offset: 0x00011AD8
		public void \u0001(IExprement \u0002)
		{
			if ((\u0002 as _IExprement).MessagesList != null)
			{
				foreach (_ICompilerMessage icompilerMessage in (\u0002 as _IExprement).MessagesList)
				{
					if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
					{
						this.Writer.Write(icompilerMessage.Text);
					}
				}
			}
			byte[] buffer = global::\u0012.\u0002.\u0001(\u0002);
			this.Writer.Write(buffer);
			if (global::\u0012.\u0002.ConsiderSourcePosition && \u0002.Position != null)
			{
				this.Writer.Write(\u0002.Position.PositionCombination);
			}
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x0001398C File Offset: 0x00011B8C
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x00013998 File Offset: 0x00011B98
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x000139A4 File Offset: 0x00011BA4
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x000139B0 File Offset: 0x00011BB0
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x000139BC File Offset: 0x00011BBC
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x000139C8 File Offset: 0x00011BC8
		public void \u0001(_ISequenceStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x000139D4 File Offset: 0x00011BD4
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.KindOf);
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x000139F0 File Offset: 0x00011BF0
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x000139FC File Offset: 0x00011BFC
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x00013A08 File Offset: 0x00011C08
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Label);
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x00013A24 File Offset: 0x00011C24
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Text);
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x00013A40 File Offset: 0x00011C40
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x00013A44 File Offset: 0x00011C44
		public void \u0001(_IPragmaStatement \u0002)
		{
			string[] source = new string[]
			{
				CompileAttributes.ATTRIBUTE_NO_QUERY_INTERFACE_CHECK,
				CompileAttributes.ATTRIBUTE_HIDE,
				"conditionalshow",
				"conditionalshow_all_locals",
				"Name",
				CompileAttributes.ATTRIBUTE_GET,
				CompileAttributes.ATTRIBUTE_SET,
				CompileAttributes.ATTRIBUTE_SUPPRESS_WRN_C0410,
				CompileAttributes.ATTRIBUTE_ANALYSIS,
				CompileAttributes.ATTRIBUTE_NAMING,
				CompileAttributes.ATTRIBUTE_NO_QUERY_INTERFACE_CHECK,
				"monitoring_display",
				"monitoring_encoding",
				"suppress_warning",
				"variable_length_array_original_scope",
				CompileAttributes.ATTRIBUTE_OBSOLETE
			};
			string text = \u0002.Text;
			IPragmaScanner pragmaScanner = \u0082.\u0005.Singleton.Create(text);
			IPragmaToken pragmaToken;
			if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.attribute && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
			{
				string @string = pragmaToken.String;
				if (source.Contains(@string))
				{
					return;
				}
				if (@string.StartsWith("ieccodeconversion_"))
				{
					return;
				}
			}
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Text);
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x00013B50 File Offset: 0x00011D50
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x00013B5C File Offset: 0x00011D5C
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.ExpectedType != null)
			{
				this.Writer.Write(\u0002.ExpectedType.ToUpperString());
			}
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x00013B84 File Offset: 0x00011D84
		public void \u0001(_IOperatorExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.Code);
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x00013BA0 File Offset: 0x00011DA0
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x00013BA4 File Offset: 0x00011DA4
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002._TypeToCast != null)
			{
				this.Writer.Write(\u0002._TypeToCast.ToUpperString());
			}
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x00013BCC File Offset: 0x00011DCC
		public virtual void \u0001(_ITypeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Type != null)
			{
				this.Writer.Write(\u0002._CompiledType.ToString().ToUpperInvariant());
			}
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x00013BF8 File Offset: 0x00011DF8
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.From);
			this.Writer.Write((int)\u0002.To);
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x00013C24 File Offset: 0x00011E24
		public void \u0001(_IThisExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00013C30 File Offset: 0x00011E30
		public void \u0001(_IBaseExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00013C3C File Offset: 0x00011E3C
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.ConstantType);
			this.Writer.Write(\u0002.LongValue);
			this.Writer.Write(\u0002.ULongValue);
			this.Writer.Write(\u0002.RealValue);
			if (TypeClass.WString == \u0002.OriginalType)
			{
				byte[] buffer = \u0017.\u0003.Singleton.\u0001(ByteOrder.Intel, \u0002.OriginalType, 1, \u0002.StringValue, StringEncoding.Default);
				this.Writer.Write(buffer);
			}
			else
			{
				try
				{
					_IStringLiteralExpression2 istringLiteralExpression = \u0002 as _IStringLiteralExpression2;
					if (istringLiteralExpression != null && istringLiteralExpression.StringEncoding != StringEncoding.Default)
					{
						this.Writer.Write((uint)istringLiteralExpression.StringEncoding);
					}
					this.Writer.Write(\u0002.StringValue);
				}
				catch (EncoderFallbackException)
				{
					StringEncoding u = StringEncoding.Default;
					_IStringLiteralExpression2 istringLiteralExpression2 = \u0002 as _IStringLiteralExpression2;
					if (istringLiteralExpression2 != null)
					{
						u = istringLiteralExpression2.StringEncoding;
					}
					byte[] buffer2 = \u0017.\u0003.Singleton.\u0001(ByteOrder.Intel, \u0002.OriginalType, 1, \u0002.StringValue, u);
					this.Writer.Write(buffer2);
				}
			}
			this.Writer.Write(\u0002.Negative);
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x00013D5C File Offset: 0x00011F5C
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.DirectAddress.ToString());
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x00013D7C File Offset: 0x00011F7C
		public virtual void \u0001(_IVariableExpression \u0002, AccessFlag \u0003)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Name.ToUpperInvariant());
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x00013D9C File Offset: 0x00011F9C
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x00013DA8 File Offset: 0x00011FA8
		public bool bResolveCompoAccessExpression
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x00013DAC File Offset: 0x00011FAC
		public void \u0001(_ICompoAccessExpression \u0002, AccessFlag \u0003)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x00013DB8 File Offset: 0x00011FB8
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x00013DC4 File Offset: 0x00011FC4
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00013DD0 File Offset: 0x00011FD0
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00013DDC File Offset: 0x00011FDC
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x00013DE8 File Offset: 0x00011FE8
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x00013DF4 File Offset: 0x00011FF4
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x00013E00 File Offset: 0x00012000
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x00013E0C File Offset: 0x0001200C
		public void \u0001(_IEmptyStatement \u0002)
		{
			IList<_ICompilerMessage> messagesList = \u0002.MessagesList;
			if (messagesList != null && messagesList.Count > 0)
			{
				foreach (_ICompilerMessage icompilerMessage in \u0002.MessagesList)
				{
					this.Writer.Write(icompilerMessage.Text);
				}
			}
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x00013E7C File Offset: 0x0001207C
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x00013E88 File Offset: 0x00012088
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x00013E94 File Offset: 0x00012094
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x00013EA0 File Offset: 0x000120A0
		public void \u0001(_IErrorExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x00013EAC File Offset: 0x000120AC
		public void \u0001(_IErrorStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x00013EB8 File Offset: 0x000120B8
		public void \u0001(_INullExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x00013EC4 File Offset: 0x000120C4
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x00013ED0 File Offset: 0x000120D0
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Name);
			this.Writer.Write(\u0002.Namespace);
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x00013EFC File Offset: 0x000120FC
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Type != null)
			{
				if (\u0002.Type.Class == TypeClass.Reference)
				{
					this.Writer.Write("REFERENCE TO ");
				}
				this.Writer.Write(\u0002.Type.ToUpperString());
			}
			if (\u0002.Address != null)
			{
				this.Writer.Write(\u0002.Address.ToString());
			}
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x00013F6C File Offset: 0x0001216C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((long)\u0002.Flags);
		}

		// Token: 0x06000A19 RID: 2585 RVA: 0x00013F88 File Offset: 0x00012188
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.Class);
			this.Writer.Write(\u0002.Name);
			if (\u0002.Type != null)
			{
				this.Writer.Write(\u0002.Type.ToUpperString());
			}
			this.Writer.Write((long)\u0002.Access);
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x00013FF0 File Offset: 0x000121F0
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Type != null)
			{
				this.Writer.Write(\u0002.Type.ToUpperString());
			}
			this.Writer.Write(\u0002.Name);
			this.Writer.Write((long)\u0002.Flags);
			_IExpression defaultValue = \u0002._DefaultValue;
			if (defaultValue == null)
			{
				return;
			}
			defaultValue.Accept(this.Traverser);
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x0001405C File Offset: 0x0001225C
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Name);
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x00014078 File Offset: 0x00012278
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.BaseType != null)
			{
				this.Writer.Write(\u0002.BaseType.ToString());
			}
			if (\u0002._DefaultValue != null)
			{
				this.Writer.Write(\u0002._DefaultValue.ToString());
			}
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x000140C8 File Offset: 0x000122C8
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x000140D4 File Offset: 0x000122D4
		public void \u0001(_IArrayInitialization \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x000140E0 File Offset: 0x000122E0
		public void \u0001(_IStructureInitialization \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x000140EC File Offset: 0x000122EC
		public void \u0001(_IDefineReference \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Define != null)
			{
				this.Writer.Write(\u0002.Define);
			}
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x00014110 File Offset: 0x00012310
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x0001411C File Offset: 0x0001231C
		public void \u0001(_ITypeReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x00014128 File Offset: 0x00012328
		public void \u0001(_IPouReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00014134 File Offset: 0x00012334
		public void \u0001(_ITaskReference \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.TaskName != null)
			{
				this.Writer.Write(\u0002.TaskName);
			}
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x00014158 File Offset: 0x00012358
		public void \u0001(_IResourceReference \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.ResourceName != null)
			{
				this.Writer.Write(\u0002.ResourceName);
			}
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x0001417C File Offset: 0x0001237C
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x00014188 File Offset: 0x00012388
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.Operator);
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x000141A4 File Offset: 0x000123A4
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x000141B0 File Offset: 0x000123B0
		public void \u0001(_IPragmaIfStatement \u0002, out bool \u0003)
		{
			\u0003 = false;
			if (\u0002.ElseIf.Count != 0)
			{
				return;
			}
			if (\u0002.Condition is _ICompilerVersionExpression)
			{
				_ICompilerVersionExpression icompilerVersionExpression = \u0002.Condition as _ICompilerVersionExpression;
				if (icompilerVersionExpression.VersionToTest < new Version(3, 3, 0, 10))
				{
					return;
				}
				bool flag = false;
				Operator opComparison = icompilerVersionExpression.OpComparison;
				switch (opComparison)
				{
				case Operator.Eq:
					goto IL_FB;
				case Operator.Ne:
					goto IL_113;
				case Operator.Ge:
					break;
				case Operator.Gt:
					goto IL_B3;
				case Operator.Le:
					goto IL_E3;
				case Operator.Lt:
					goto IL_CB;
				default:
					switch (opComparison)
					{
					case Operator.Less:
						goto IL_CB;
					case Operator.Greater:
						goto IL_B3;
					case Operator.LessEqual:
						goto IL_E3;
					case Operator.GreaterEqual:
						break;
					case Operator.Equal:
						goto IL_FB;
					case Operator.NotEqual:
						goto IL_113;
					default:
						goto IL_129;
					}
					break;
				}
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() >= icompilerVersionExpression.VersionToTest);
				goto IL_129;
				IL_B3:
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() > icompilerVersionExpression.VersionToTest);
				goto IL_129;
				IL_CB:
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() < icompilerVersionExpression.VersionToTest);
				goto IL_129;
				IL_E3:
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() <= icompilerVersionExpression.VersionToTest);
				goto IL_129;
				IL_FB:
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() == icompilerVersionExpression.VersionToTest);
				goto IL_129;
				IL_113:
				flag = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() != icompilerVersionExpression.VersionToTest);
				IL_129:
				IList<_IStatement> list = null;
				if (flag)
				{
					_ISequenceStatement isequenceStatement = \u0002.IfThen as _ISequenceStatement;
					list = ((isequenceStatement != null) ? isequenceStatement._StatementList : null);
				}
				else if (\u0002.IfElse != null)
				{
					list = (\u0002.IfElse as _ISequenceStatement)._StatementList;
				}
				if (list != null)
				{
					foreach (_IStatement istatement in list)
					{
						istatement.Accept(this.Traverser);
					}
				}
				\u0003 = true;
			}
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x00014368 File Offset: 0x00012568
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x0001436C File Offset: 0x0001256C
		public void \u0001(_IDefineStatement \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write(\u0002.Define);
			if (\u0002.Ident != null)
			{
				this.Writer.Write(\u0002.Ident);
			}
			if (\u0002.Value != null)
			{
				this.Writer.Write(\u0002.Value);
			}
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x000143C4 File Offset: 0x000125C4
		public void \u0001(_IXRefExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x000143D0 File Offset: 0x000125D0
		public void \u0001(_IHasTypeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.ReferencedType != null)
			{
				this.Writer.Write(\u0002.ReferencedType.ToString().ToUpperInvariant());
			}
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x000143FC File Offset: 0x000125FC
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.ReferencedType != null)
			{
				this.Writer.Write(\u0002.ReferencedType.ToString().ToUpperInvariant());
			}
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x00014428 File Offset: 0x00012628
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Attribute != null)
			{
				this.Writer.Write(\u0002.Attribute);
			}
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0001444C File Offset: 0x0001264C
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Define != null)
			{
				this.Writer.Write(\u0002.Define);
			}
			if (\u0002.DefineValue != null)
			{
				this.Writer.Write(\u0002.DefineValue);
			}
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x00014488 File Offset: 0x00012688
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002._Constant != null)
			{
				this.Writer.Write(\u0002._Constant.ToString());
			}
			if (\u0002._ConstantValue != null)
			{
				this.Writer.Write(\u0002._ConstantValue.ToString());
			}
			this.Writer.Write((int)\u0002.OpComparison);
		}

		// Token: 0x06000A32 RID: 2610 RVA: 0x000144EC File Offset: 0x000126EC
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Constant != null)
			{
				this.Writer.Write(\u0002.Constant.ToString());
			}
			this.Writer.Write(\u0002.ConstantTypeReplaced);
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x00014524 File Offset: 0x00012724
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x00014530 File Offset: 0x00012730
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.\u0001(\u0002);
			if (null != \u0002.VersionToTest)
			{
				this.Writer.Write(\u0002.VersionToTest.ToString());
			}
			this.Writer.Write((int)\u0002.OpComparison);
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x00014570 File Offset: 0x00012770
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.\u0001(\u0002);
			if (null != \u0002.VersionToTest)
			{
				this.Writer.Write(\u0002.VersionToTest.ToString());
			}
			this.Writer.Write((int)\u0002.OpComparison);
		}

		// Token: 0x06000A36 RID: 2614 RVA: 0x000145B0 File Offset: 0x000127B0
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000A37 RID: 2615 RVA: 0x000145BC File Offset: 0x000127BC
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			this.Writer.Write((int)\u0002.PartSize);
			this.Writer.Write(\u0002.PartOffset);
		}

		// Token: 0x06000A38 RID: 2616 RVA: 0x000145E8 File Offset: 0x000127E8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0400013B RID: 315
		private readonly MyChecksumStream \u0001;

		// Token: 0x0400013C RID: 316
		private static readonly LDictionary<Type, byte[]> \u0001 = new LDictionary<Type, byte[]>();

		// Token: 0x0400013D RID: 317
		private const string \u0001 = "ConsiderSourcePositionForCodeChecksum";

		// Token: 0x0400013E RID: 318
		[CompilerGenerated]
		private static bool \u0001;

		// Token: 0x0400013F RID: 319
		[CompilerGenerated]
		private readonly BinaryWriter \u0001;

		// Token: 0x04000140 RID: 320
		[CompilerGenerated]
		private IStandardTraverser \u0001;

		// Token: 0x04000141 RID: 321
		private const string \u0002 = "Name";
	}
}
