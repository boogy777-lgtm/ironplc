using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0004;
using \u0008;
using \u000E;
using \u0014;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001D1 RID: 465
	internal sealed class ReflectionCodinglistGenerator
	{
		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x060020CD RID: 8397 RVA: 0x000703E8 File Offset: 0x0006E5E8
		// (set) Token: 0x060020CE RID: 8398 RVA: 0x000703F0 File Offset: 0x0006E5F0
		public int NumOfArrayIndexVariables { get; set; }

		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x060020CF RID: 8399 RVA: 0x000703FC File Offset: 0x0006E5FC
		// (set) Token: 0x060020D0 RID: 8400 RVA: 0x00070404 File Offset: 0x0006E604
		public int NumOfCharactersNeeded { get; set; }

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x060020D1 RID: 8401 RVA: 0x00070410 File Offset: 0x0006E610
		// (set) Token: 0x060020D2 RID: 8402 RVA: 0x00070418 File Offset: 0x0006E618
		public int NumOfWCharactersNeeded { get; set; }

		// Token: 0x060020D3 RID: 8403 RVA: 0x00070424 File Offset: 0x0006E624
		internal void \u0001(_ICompileContext \u0002, _ISignature \u0003, LStringBuilder \u0004, bool \u0005)
		{
			LList<IVariable> llist = Enumerable.ToLList<IVariable>(\u0003.AllVariables.Where(new Func<_IVariable, bool>(ReflectionCodinglistGenerator.<>c.<>9.\u0001)));
			this.NumOfArrayIndexVariables = 0;
			this.NumOfCharactersNeeded = 0;
			this.NumOfWCharactersNeeded = 0;
			if (llist.Count<IVariable>() == 0)
			{
				return;
			}
			_IVariable ivariable = llist[0] as _IVariable;
			llist.RemoveAt(0);
			ReflectionCodinglistGenerator.\u0001 u = new ReflectionCodinglistGenerator.\u0001(\u0002, \u0003, ivariable);
			ReflectionCodinglistGenerator.\u0001(ref u);
			LList<global::\u0008.\u0008> llist2 = new LList<global::\u0008.\u0008>();
			global::\u0004.\u0007.\u0001(u.StatementlistOutput, llist2);
			LList<global::\u0008.\u0008> llist3 = new LList<global::\u0008.\u0008>();
			\u0084.\u0010.\u0001(llist2, llist3);
			if (\u0005)
			{
				LList<global::\u0008.\u0008> u2 = llist3;
				llist3 = new LList<global::\u0008.\u0008>();
				global::\u000E.\u0010.\u0001(u2, llist3);
			}
			if (ivariable.Type.Class == TypeClass.String)
			{
				global::\u0016.\u000E u000E = new global::\u0016.\u000E((ivariable.Type as _IStringType).Size(u.GlobalScope) - 1, u.GlobalScope, llist, \u0004);
				u000E.\u0001(llist3);
				this.NumOfArrayIndexVariables = u000E.NumArrayIndexVariables;
				this.NumOfCharactersNeeded = u000E.MaxNumOfCharactersInConcatenatedString;
				return;
			}
			if (ivariable.Type.Class == TypeClass.WString)
			{
				global::\u0016.\u000E u000E = new global::\u0019.\u0004((ivariable.Type as _IWStringType).Size(u.GlobalScope) / 2 - 1, u.GlobalScope, llist, \u0004);
				u000E.\u0001(llist3);
				this.NumOfArrayIndexVariables = u000E.NumArrayIndexVariables;
				this.NumOfWCharactersNeeded = u000E.MaxNumOfCharactersInConcatenatedString;
				return;
			}
		}

		// Token: 0x060020D4 RID: 8404 RVA: 0x00070594 File Offset: 0x0006E794
		private static bool \u0001(ReflectionCodinglistGenerator.\u0001 \u0002)
		{
			return \u0002.Comcon[\u0002.Signature.Id] != null;
		}

		// Token: 0x060020D5 RID: 8405 RVA: 0x000705B4 File Offset: 0x0006E7B4
		private static void \u0001(ref ReflectionCodinglistGenerator.\u0001 \u0002)
		{
			if (\u0002.Variable.GetFlag(VarFlag.Static) && (\u0002.Signature.POUType == Operator.Method || \u0002.Signature.POUType == Operator.Function))
			{
				return;
			}
			if (\u0002.Variable.GetFlag(VarFlag.Absolut) && !\u0002.Variable.GetFlag(VarFlag.Static))
			{
				if (ReflectionCodinglistGenerator.\u0001(\u0002))
				{
					ReflectionCodinglistGenerator.\u0002(ref \u0002);
					return;
				}
			}
			else
			{
				Debug.\u0001(\u0002.Variable.GetFlag(VarFlag.RelativeInstance) || \u0002.Variable.GetFlag(VarFlag.Static));
				ReflectionCodinglistGenerator.\u0003(ref \u0002);
			}
		}

		// Token: 0x060020D6 RID: 8406 RVA: 0x00070660 File Offset: 0x0006E860
		private static void \u0002(ref ReflectionCodinglistGenerator.\u0001 \u0002)
		{
			string text = string.Empty;
			if (!string.IsNullOrEmpty(\u0002.Signature.LibraryPath))
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.Signature.LibraryPath);
				text = Helper.\u0001(\u0002.Comcon, libraryContext) + "#";
			}
			string str;
			if (\u0002.Signature.POUType == Operator.VarGlobal)
			{
				if (\u0002.Signature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY) || \u0002.Signature.Name != \u0002.Variable.Name)
				{
					str = \u0002.Signature.OrgName + ".";
				}
				else
				{
					str = string.Empty;
				}
			}
			else
			{
				str = \u0002.Signature.OrgName + ".";
			}
			string u = text + str + \u0002.LValue;
			string applicationNameByGuid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(\u0002.Comcon.ApplicationGuid, \u0002.Comcon.SimulationMode);
			global::\u0014.\u0008.\u0001(\u0002.StatementlistOutput);
			global::\u0014.\u0008.\u0001(\u0002.StatementlistOutput, string.Concat(new string[]
			{
				applicationNameByGuid,
				".",
				text,
				\u0002.Signature.OrgName,
				"."
			}));
			\u0002.StatementlistOutput.AddRange(\u0002.StatementlistInput);
			global::\u0014.\u0008.\u0001(\u0002.StatementlistOutput, u, \u0002.AddedViaOnlineChange);
		}

		// Token: 0x060020D7 RID: 8407 RVA: 0x000707D0 File Offset: 0x0006E9D0
		private static void \u0003(ref ReflectionCodinglistGenerator.\u0001 \u0002)
		{
			int[] declarerIds = \u0002.Signature.DeclarerIds;
			_IScope iscope = \u0002.GlobalScope;
			for (int i = 0; i < declarerIds.Length; i++)
			{
				_ISignature isignature = iscope[declarerIds[i]] as _ISignature;
				if (isignature != null)
				{
					bool flag = isignature.GetFlag(SignatureFlag.Alias);
					if (isignature.BaseSignatureId == \u0002.Signature.Id || flag)
					{
						ReflectionCodinglistGenerator.\u0001 u = new ReflectionCodinglistGenerator.\u0001(\u0002)
						{
							Signature = isignature
						};
						u.StatementlistInput.AddRange(\u0002.StatementlistInput);
						ReflectionCodinglistGenerator.\u0001(ref u);
						\u0002.StatementlistOutput.AddRange(u.StatementlistOutput);
					}
					if (!flag)
					{
						ReflectionCodinglistGenerator.\u0001(ref \u0002, isignature);
					}
				}
			}
		}

		// Token: 0x060020D8 RID: 8408 RVA: 0x00070888 File Offset: 0x0006EA88
		private static void \u0001(ref ReflectionCodinglistGenerator.\u0001 \u0002, _ISignature \u0003)
		{
			ReflectionCodinglistGenerator.\u0002 u = new ReflectionCodinglistGenerator.\u0002();
			u.\u0001 = \u0002.Signature.Id;
			foreach (IVariable variable in \u0003.AllVariables.Where(new Func<_IVariable, bool>(u.\u0001)))
			{
				if (variable.Type.Class == TypeClass.Array)
				{
					_IArrayType u2 = variable.Type as _IArrayType;
					ReflectionCodinglistGenerator.\u0001(ref \u0002, \u0003, variable, u2);
				}
				else
				{
					ReflectionCodinglistGenerator.\u0001(ref \u0002, \u0003, variable);
				}
			}
		}

		// Token: 0x060020D9 RID: 8409 RVA: 0x00070924 File Offset: 0x0006EB24
		internal static bool \u0001(IVariable \u0002, int \u0003)
		{
			if (\u0002.GetFlag(VarFlag.Inout))
			{
				return false;
			}
			if (\u0002.Type.Class != TypeClass.Userdef && \u0002.Type.Class != TypeClass.Array)
			{
				return false;
			}
			if ((\u0002 as _IVariable).IsProperty)
			{
				return false;
			}
			if (\u0002.GetFlag(VarFlag.RelativeStack))
			{
				return false;
			}
			_IUserdefType iuserdefType = null;
			if (\u0002.Type.Class == TypeClass.Userdef)
			{
				iuserdefType = ((\u0002 as _IVariable).CompiledTypeInternal as _IUserdefType);
			}
			else if (\u0002.Type.Class == TypeClass.Array)
			{
				iuserdefType = (\u0084.\u0004.\u0002((\u0002 as _IVariable).CompiledTypeInternal as _IArrayType) as _IUserdefType);
			}
			return iuserdefType != null && iuserdefType.SignatureId == \u0003;
		}

		// Token: 0x060020DA RID: 8410 RVA: 0x000709D8 File Offset: 0x0006EBD8
		private static void \u0001(ref ReflectionCodinglistGenerator.\u0001 \u0002, _ISignature \u0003, IVariable \u0004)
		{
			ReflectionCodinglistGenerator.\u0001 u = new ReflectionCodinglistGenerator.\u0001(\u0002)
			{
				Signature = \u0003,
				Variable = (\u0004 as _IVariable),
				AddedViaOnlineChange = \u0004.GetFlag(VarFlag.OnlChangeInit)
			};
			string text = \u0004.OrgName;
			if (\u0002.StatementlistInput.Any<global::\u0008.\u0008>())
			{
				text += ".";
			}
			global::\u0014.\u0008.\u0001(u.StatementlistInput, text);
			u.StatementlistInput.AddRange(\u0002.StatementlistInput);
			string u2 = \u0004.OrgName + "." + \u0002.LValue;
			u.LValue = u2;
			ReflectionCodinglistGenerator.\u0001(ref u);
			\u0002.StatementlistOutput.AddRange(u.StatementlistOutput);
		}

		// Token: 0x060020DB RID: 8411 RVA: 0x00070A94 File Offset: 0x0006EC94
		private static string \u0001(string \u0002, _IArrayType \u0003, int \u0004)
		{
			string text = \u0002 + "[";
			for (int i = 0; i < \u0003.Dimensions.Count<IArrayDimension>(); i++)
			{
				if (i > 0)
				{
					text += string.Format(", ", Array.Empty<object>());
				}
				text += string.Format("Index_{0}", \u0004 + i);
			}
			text += "]";
			if (\u0003._Base.Class == TypeClass.Array)
			{
				text = ReflectionCodinglistGenerator.\u0001(text, \u0003._Base as _IArrayType, \u0004 + \u0003.Dimensions.Count<IArrayDimension>());
			}
			return text;
		}

		// Token: 0x060020DC RID: 8412 RVA: 0x00070B34 File Offset: 0x0006ED34
		private static int \u0001(_IArrayType \u0002, int \u0003)
		{
			int num = \u0003 + \u0002.Dimensions.Count<IArrayDimension>();
			if (\u0002._Base.Class == TypeClass.Array)
			{
				num = ReflectionCodinglistGenerator.\u0001(\u0002._Base as _IArrayType, num);
			}
			return num;
		}

		// Token: 0x060020DD RID: 8413 RVA: 0x00070B74 File Offset: 0x0006ED74
		private static void \u0001(ref ReflectionCodinglistGenerator.\u0001 \u0002, _ISignature \u0003, IVariable \u0004, _IArrayType \u0005)
		{
			int num = \u0002.ArrayIndex;
			string u = ReflectionCodinglistGenerator.\u0001(\u0004.OrgName, \u0005, num) + "." + \u0002.LValue;
			LList<global::\u0008.\u0008> llist = new LList<global::\u0008.\u0008>();
			global::\u0014.\u0008.\u0001(llist, \u0004.OrgName);
			global::\u0014.\u0008.\u0001(llist, \u0005, num);
			num = ReflectionCodinglistGenerator.\u0001(\u0005, num);
			if (\u0002.StatementlistInput.Any<global::\u0008.\u0008>())
			{
				global::\u0014.\u0008.\u0001(llist, ".");
			}
			ReflectionCodinglistGenerator.\u0001 u2 = new ReflectionCodinglistGenerator.\u0001(\u0002)
			{
				Signature = \u0003,
				Variable = (\u0004 as _IVariable),
				ArrayIndex = num,
				AddedViaOnlineChange = \u0004.GetFlag(VarFlag.OnlChangeInit),
				LValue = u
			};
			u2.StatementlistInput.AddRange(llist);
			u2.StatementlistInput.AddRange(\u0002.StatementlistInput);
			ReflectionCodinglistGenerator.\u0001(ref u2);
			global::\u0014.\u0008.\u0001(\u0002.StatementlistOutput, \u0005, u2.StatementlistOutput, \u0002.ArrayIndex);
		}

		// Token: 0x04000566 RID: 1382
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000567 RID: 1383
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000568 RID: 1384
		[CompilerGenerated]
		private int \u0003;

		// Token: 0x020001D2 RID: 466
		internal struct \u0001
		{
			// Token: 0x1700061A RID: 1562
			// (get) Token: 0x060020DF RID: 8415 RVA: 0x00070C70 File Offset: 0x0006EE70
			public _ICompileContext Comcon { get; }

			// Token: 0x1700061B RID: 1563
			// (get) Token: 0x060020E0 RID: 8416 RVA: 0x00070C78 File Offset: 0x0006EE78
			public _IScope GlobalScope { get; }

			// Token: 0x1700061C RID: 1564
			// (get) Token: 0x060020E1 RID: 8417 RVA: 0x00070C80 File Offset: 0x0006EE80
			// (set) Token: 0x060020E2 RID: 8418 RVA: 0x00070C88 File Offset: 0x0006EE88
			public string LValue { get; set; }

			// Token: 0x1700061D RID: 1565
			// (get) Token: 0x060020E3 RID: 8419 RVA: 0x00070C94 File Offset: 0x0006EE94
			// (set) Token: 0x060020E4 RID: 8420 RVA: 0x00070C9C File Offset: 0x0006EE9C
			public int ArrayIndex { get; set; }

			// Token: 0x1700061E RID: 1566
			// (get) Token: 0x060020E5 RID: 8421 RVA: 0x00070CA8 File Offset: 0x0006EEA8
			// (set) Token: 0x060020E6 RID: 8422 RVA: 0x00070CB0 File Offset: 0x0006EEB0
			public _ISignature Signature { get; set; }

			// Token: 0x1700061F RID: 1567
			// (get) Token: 0x060020E7 RID: 8423 RVA: 0x00070CBC File Offset: 0x0006EEBC
			// (set) Token: 0x060020E8 RID: 8424 RVA: 0x00070CC4 File Offset: 0x0006EEC4
			public _IVariable Variable { get; set; }

			// Token: 0x17000620 RID: 1568
			// (get) Token: 0x060020E9 RID: 8425 RVA: 0x00070CD0 File Offset: 0x0006EED0
			// (set) Token: 0x060020EA RID: 8426 RVA: 0x00070CD8 File Offset: 0x0006EED8
			public bool AddedViaOnlineChange { get; set; }

			// Token: 0x17000621 RID: 1569
			// (get) Token: 0x060020EB RID: 8427 RVA: 0x00070CE4 File Offset: 0x0006EEE4
			// (set) Token: 0x060020EC RID: 8428 RVA: 0x00070CEC File Offset: 0x0006EEEC
			public LList<global::\u0008.\u0008> StatementlistInput { get; set; }

			// Token: 0x17000622 RID: 1570
			// (get) Token: 0x060020ED RID: 8429 RVA: 0x00070CF8 File Offset: 0x0006EEF8
			// (set) Token: 0x060020EE RID: 8430 RVA: 0x00070D00 File Offset: 0x0006EF00
			public LList<global::\u0008.\u0008> StatementlistOutput { get; set; }

			// Token: 0x060020EF RID: 8431 RVA: 0x00070D0C File Offset: 0x0006EF0C
			public \u0001(_ICompileContext \u0001\u0002, _ISignature \u001C\u0002, _IVariable \u001A\u0002)
			{
				this.Comcon = \u0001\u0002;
				this.GlobalScope = (\u0001\u0002.CreateGlobalIScope() as _IScope);
				this.LValue = \u001A\u0002.OrgName;
				this.Signature = \u001C\u0002;
				this.Variable = \u001A\u0002;
				this.StatementlistInput = new LList<global::\u0008.\u0008>();
				this.StatementlistOutput = new LList<global::\u0008.\u0008>();
				this.AddedViaOnlineChange = \u001A\u0002.GetFlag(VarFlag.OnlChangeInit);
				this.ArrayIndex = 0;
			}

			// Token: 0x060020F0 RID: 8432 RVA: 0x00070D7C File Offset: 0x0006EF7C
			public \u0001(ReflectionCodinglistGenerator.\u0001 \u009E\u0004)
			{
				this.Comcon = \u009E\u0004.Comcon;
				this.GlobalScope = \u009E\u0004.GlobalScope;
				this.LValue = \u009E\u0004.LValue;
				this.Signature = \u009E\u0004.Signature;
				this.Variable = \u009E\u0004.Variable;
				this.StatementlistInput = new LList<global::\u0008.\u0008>();
				this.StatementlistOutput = new LList<global::\u0008.\u0008>();
				this.ArrayIndex = \u009E\u0004.ArrayIndex;
				this.AddedViaOnlineChange = \u009E\u0004.AddedViaOnlineChange;
			}

			// Token: 0x04000569 RID: 1385
			[CompilerGenerated]
			private readonly _ICompileContext \u0001;

			// Token: 0x0400056A RID: 1386
			[CompilerGenerated]
			private readonly _IScope \u0001;

			// Token: 0x0400056B RID: 1387
			[CompilerGenerated]
			private string \u0001;

			// Token: 0x0400056C RID: 1388
			[CompilerGenerated]
			private int \u0001;

			// Token: 0x0400056D RID: 1389
			[CompilerGenerated]
			private _ISignature \u0001;

			// Token: 0x0400056E RID: 1390
			[CompilerGenerated]
			private _IVariable \u0001;

			// Token: 0x0400056F RID: 1391
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x04000570 RID: 1392
			[CompilerGenerated]
			private LList<global::\u0008.\u0008> \u0001;

			// Token: 0x04000571 RID: 1393
			[CompilerGenerated]
			private LList<global::\u0008.\u0008> \u0002;
		}

		// Token: 0x020001D4 RID: 468
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x060020F5 RID: 8437 RVA: 0x00070E28 File Offset: 0x0006F028
			internal bool \u0001(_IVariable \u0002)
			{
				return ReflectionCodinglistGenerator.\u0001(\u0002, this.\u0001);
			}

			// Token: 0x04000574 RID: 1396
			public int \u0001;
		}
	}
}
