using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using \u0003;
using \u0004;
using \u0007;
using \u000F;
using \u001E;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;

namespace \u001B
{
	// Token: 0x020001E9 RID: 489
	internal sealed class \u0005 : IApplicationContent2, IApplicationContent
	{
		// Token: 0x0600216A RID: 8554 RVA: 0x00071DB4 File Offset: 0x0006FFB4
		static \u0005()
		{
			global::\u001B.\u0005.\u0001 = new HashSet<uint>();
			global::\u001B.\u0005.\u0001.Add(0U);
			global::\u001B.\u0005.\u0001.Add(50664960U);
			global::\u001B.\u0005.\u0001.Add(50665216U);
			global::\u001B.\u0005.\u0001 = 50665216U;
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x00071E5C File Offset: 0x0007005C
		private void \u0001(global::\u0004.\u0008 \u0002, LStringBuilder \u0003)
		{
			\u0003.AppendFormat(global::\u001B.\u0005.\u0005, new object[]
			{
				\u0002.\u0001,
				\u0002.\u0002,
				\u0002.\u0003,
				\u0002.\u0001,
				\u0002.\u0002,
				\u0002.\u0004,
				\u0002.\u0005
			});
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x00071EE0 File Offset: 0x000700E0
		internal string \u0001()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (global::\u0004.\u0008 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				this.\u0001(u, lstringBuilder);
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x00071F80 File Offset: 0x00070180
		internal string \u0002()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (\u007F.\u0007 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u000E, new object[]
				{
					u.\u0001,
					u.\u0002
				});
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x00072048 File Offset: 0x00070248
		internal string \u0003()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (global::\u0003.\u0008 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u000E, new object[]
				{
					u.\u0001,
					u.\u0002
				});
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x00072110 File Offset: 0x00070310
		internal string \u0004()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (\u007F.\u0006 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				this.\u0001(u, lstringBuilder);
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u0006, new object[]
				{
					u.\u0001,
					u.\u0002,
					u.\u0003
				});
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x000721F0 File Offset: 0x000703F0
		internal string \u0005()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (global::\u0007.\u0008 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				this.\u0001(u, lstringBuilder);
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u0007, new object[]
				{
					u.\u0001
				});
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x000722B0 File Offset: 0x000704B0
		internal string \u0006()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (\u001E.\u0007 u in this.\u0001)
			{
				if (flag)
				{
					lstringBuilder.Append("(");
				}
				else
				{
					lstringBuilder.Append(",(");
				}
				flag = false;
				this.\u0001(u, lstringBuilder);
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u0007, new object[]
				{
					u.\u0001
				});
				lstringBuilder.AppendFormat(global::\u001B.\u0005.\u0008, new object[]
				{
					(uint)u.ParentPOUType
				});
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x00072394 File Offset: 0x00070594
		internal string \u0007()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (uint num in this.\u0001)
			{
				if (!flag)
				{
					lstringBuilder.Append(",");
				}
				flag = false;
				lstringBuilder.Append(num);
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x0007241C File Offset: 0x0007061C
		internal string \u0001(bool \u0002)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			if (\u0002)
			{
				foreach (byte b in this.\u0001)
				{
					if (!flag)
					{
						lstringBuilder.Append(",");
					}
					flag = false;
					lstringBuilder.Append(b.ToString(CultureInfo.InvariantCulture));
				}
			}
			else
			{
				for (int j = 0; j < this.\u0001.Length; j += 2)
				{
					if (!flag)
					{
						lstringBuilder.Append(",");
					}
					flag = false;
					lstringBuilder.Append(this.\u0001[j].ToString(CultureInfo.InvariantCulture));
				}
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x06002174 RID: 8564 RVA: 0x000724D8 File Offset: 0x000706D8
		private void \u0001(string \u0002, bool \u0003)
		{
			if (Scanner.UnicodeIdentifierOption || !\u0003)
			{
				byte[] bytes = Encoding.Unicode.GetBytes(\u0002);
				this.\u0001.Write(bytes);
				this.\u0001.Write(0);
				this.\u0001.Write(0);
				return;
			}
			byte[] bytes2 = Encoding.UTF8.GetBytes(\u0002);
			this.\u0001.Write(bytes2);
			this.\u0001.Write(0);
		}

		// Token: 0x06002175 RID: 8565 RVA: 0x00072544 File Offset: 0x00070744
		private void \u0001(global::\u0004.\u0008 \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			\u0004.GetInterfaceCRC(\u0003, out \u0002.\u0002);
			\u0002.\u0003 = (uint)this.\u0001.BaseStream.Position;
			\u0002.\u0001 = \u0003.OrgName;
			this.\u0001(\u0003.OrgName, \u0004.TypeIsSupported(TypeClass.Byte));
			_ICompiledPOU icompiledPOU = \u0004._GetCompiledPOUById(\u0003.Id);
			if (icompiledPOU == null)
			{
				return;
			}
			\u0002.\u0001 = Helper.\u0001(icompiledPOU);
			\u0002.\u0001 = icompiledPOU.CompiledCode.Location.Area;
			\u0002.\u0004 = (uint)icompiledPOU.CompiledCode.Location.Offset;
			\u0002.\u0002 = \u0003.FPDataLocation.Area;
			\u0002.\u0005 = (uint)\u0003.FPDataLocation.Offset;
		}

		// Token: 0x06002176 RID: 8566 RVA: 0x00072600 File Offset: 0x00070800
		private uint \u0001()
		{
			uint num = 0U;
			foreach (global::\u0004.\u0008 u in this.\u0001)
			{
				num = Math.Max(num, u.\u0003);
			}
			foreach (\u007F.\u0007 u2 in this.\u0001)
			{
				num = Math.Max(num, u2.\u0002);
			}
			foreach (global::\u0003.\u0008 u3 in this.\u0001)
			{
				num = Math.Max(num, u3.\u0002);
			}
			foreach (\u007F.\u0006 u4 in this.\u0001)
			{
				num = Math.Max(num, u4.\u0003);
			}
			foreach (global::\u0007.\u0008 u5 in this.\u0001)
			{
				num = Math.Max(num, u5.\u0003);
			}
			foreach (uint val in this.\u0001)
			{
				num = Math.Max(num, val);
			}
			return num;
		}

		// Token: 0x06002177 RID: 8567 RVA: 0x000727BC File Offset: 0x000709BC
		private BinaryReader \u0001(uint \u0002, bool \u0003, Swapper \u0004)
		{
			BinaryReader binaryReader = this.\u0001(\u0002, \u0003, \u0004, Scanner.UnicodeIdentifierOption);
			if (binaryReader == null)
			{
				binaryReader = this.\u0001(\u0002, \u0003, \u0004, !Scanner.UnicodeIdentifierOption);
			}
			return binaryReader;
		}

		// Token: 0x06002178 RID: 8568 RVA: 0x000727F0 File Offset: 0x000709F0
		private BinaryReader \u0001(uint \u0002, bool \u0003, Swapper \u0004, bool \u0005)
		{
			BinaryReader binaryReader = null;
			uint u = this.\u0001();
			uint num;
			if (\u0005 || !\u0003)
			{
				num = this.\u0002(u);
			}
			else
			{
				num = this.\u0001(u);
			}
			if (50664960U <= \u0002)
			{
				bool flag = false;
				int num2 = 0;
				while (!flag && num2 < 16)
				{
					num2++;
					int num3 = this.\u0001.Length - (int)num;
					binaryReader = new BinaryReader(new ChunkedMemoryStream(this.\u0001, (int)num, num3, false));
					uint num4 = \u0004.Swap(binaryReader.ReadUInt32());
					flag = (4092925695U == num4);
					num += 1U;
				}
				if (!flag)
				{
					binaryReader = null;
				}
			}
			return binaryReader;
		}

		// Token: 0x06002179 RID: 8569 RVA: 0x00072880 File Offset: 0x00070A80
		internal void \u0001(byte[] \u0002, bool \u0003, bool \u0004)
		{
			BinaryReader binaryReader = new BinaryReader(new ChunkedMemoryStream(\u0002));
			Swapper swapper = new Swapper(\u0003);
			swapper.Swap(binaryReader.ReadUInt32());
			uint num = swapper.Swap(binaryReader.ReadUInt32());
			ushort num2 = swapper.Swap(binaryReader.ReadUInt16());
			ushort num3 = swapper.Swap(binaryReader.ReadUInt16());
			ushort num4 = swapper.Swap(binaryReader.ReadUInt16());
			ushort num5 = swapper.Swap(binaryReader.ReadUInt16());
			ushort num6 = swapper.Swap(binaryReader.ReadUInt16());
			ushort num7 = swapper.Swap(binaryReader.ReadUInt16());
			for (ushort num8 = 0; num8 < num2; num8 += 1)
			{
				this.\u0001(binaryReader, swapper);
			}
			for (ushort num9 = 0; num9 < num3; num9 += 1)
			{
				this.\u0002(binaryReader, swapper);
			}
			for (ushort num10 = 0; num10 < num4; num10 += 1)
			{
				this.\u0003(binaryReader, swapper);
			}
			for (ushort num11 = 0; num11 < num5; num11 += 1)
			{
				this.\u0004(binaryReader, swapper);
			}
			for (ushort num12 = 0; num12 < num6; num12 += 1)
			{
				this.\u0005(binaryReader, swapper);
			}
			for (ushort num13 = 0; num13 < num7; num13 += 1)
			{
				uint num14 = swapper.Swap(binaryReader.ReadUInt32());
				this.\u0001.Add(num14);
			}
			int num15 = (int)binaryReader.BaseStream.Position;
			int count = \u0002.Length - num15;
			this.\u0001 = binaryReader.ReadBytes(count);
			binaryReader = this.\u0001(num, \u0004, swapper);
			if (global::\u001B.\u0005.\u0001.Contains(num))
			{
				this.\u0001 = this.\u0001(num, binaryReader, swapper, \u0004);
			}
			foreach (global::\u0004.\u0008 u in this.\u0001)
			{
				u.\u0001 = this.\u0001(u.\u0003, \u0004);
			}
			foreach (\u007F.\u0007 u2 in this.\u0001)
			{
				u2.\u0001 = this.\u0001(u2.\u0002, \u0004);
			}
			foreach (global::\u0003.\u0008 u3 in this.\u0001)
			{
				u3.\u0001 = this.\u0001(u3.\u0002, \u0004);
			}
			foreach (\u007F.\u0006 u4 in this.\u0001)
			{
				u4.\u0001 = this.\u0001(u4.\u0003, \u0004);
			}
			foreach (global::\u0007.\u0008 u5 in this.\u0001)
			{
				u5.\u0001 = this.\u0001(u5.\u0003, \u0004);
			}
			foreach (uint u6 in this.\u0001)
			{
				this.\u0001.Add(this.\u0001(u6, \u0004));
			}
		}

		// Token: 0x0600217A RID: 8570 RVA: 0x00072BEC File Offset: 0x00070DEC
		internal global::\u000F.\u000E \u0001(uint \u0002, BinaryReader \u0003, Swapper \u0004, bool \u0005)
		{
			global::\u000F.\u000E result = global::\u000F.\u000E.\u0003;
			if (\u0003 != null)
			{
				ushort num = \u0004.Swap(\u0003.ReadUInt16());
				\u0003.ReadUInt16();
				for (ushort num2 = 0; num2 < num; num2 += 1)
				{
					this.\u0006(\u0003, \u0004);
				}
				foreach (\u001E.\u0007 u in this.\u0001)
				{
					u.\u0001 = this.\u0001(u.\u0003, \u0005);
				}
				if (50665216U <= \u0002)
				{
					uint num3 = \u0004.Swap(\u0003.ReadUInt32());
					if (2913787119U == num3)
					{
						result = (global::\u000F.\u000E)\u0004.Swap(\u0003.ReadUInt16());
						\u0003.ReadUInt16();
					}
				}
			}
			return result;
		}

		// Token: 0x0600217B RID: 8571 RVA: 0x00072CB0 File Offset: 0x00070EB0
		private string \u0001(uint \u0002, bool \u0003)
		{
			if (this.UnicodeIdentifiers || !\u0003)
			{
				return this.\u0001(\u0002);
			}
			uint num = \u0002;
			while ((ulong)num < (ulong)((long)this.\u0001.Length) && this.\u0001[(int)num] != 0)
			{
				num += 1U;
			}
			uint count = num - \u0002;
			return Encoding.UTF8.GetString(this.\u0001, (int)\u0002, (int)count);
		}

		// Token: 0x0600217C RID: 8572 RVA: 0x00072D08 File Offset: 0x00070F08
		private string \u0001(uint \u0002)
		{
			uint num = \u0002;
			while ((ulong)num < (ulong)((long)this.\u0001.Length) && (this.\u0001[(int)num] != 0 || this.\u0001[(int)(num + 1U)] != 0))
			{
				num += 2U;
			}
			uint count = num - \u0002;
			return Encoding.Unicode.GetString(this.\u0001, (int)\u0002, (int)count);
		}

		// Token: 0x0600217D RID: 8573 RVA: 0x00072D58 File Offset: 0x00070F58
		private uint \u0001(uint \u0002)
		{
			uint num = \u0002;
			while ((ulong)num < (ulong)((long)this.\u0001.Length) && this.\u0001[(int)num] != 0)
			{
				num += 1U;
			}
			return num;
		}

		// Token: 0x0600217E RID: 8574 RVA: 0x00072D88 File Offset: 0x00070F88
		private uint \u0002(uint \u0002)
		{
			uint num = \u0002;
			while ((ulong)num < (ulong)((long)this.\u0001.Length) && (this.\u0001[(int)num] != 0 || this.\u0001[(int)(num + 1U)] != 0))
			{
				num += 2U;
			}
			return num;
		}

		// Token: 0x0600217F RID: 8575 RVA: 0x00072DC4 File Offset: 0x00070FC4
		private void \u0001(BinaryReader \u0002, global::\u0004.\u0008 \u0003, Swapper \u0004)
		{
			\u0003.\u0001 = \u0004.Swap(\u0002.ReadUInt32());
			\u0003.\u0002 = \u0004.Swap(\u0002.ReadUInt32());
			\u0003.\u0003 = \u0004.Swap(\u0002.ReadUInt32());
			\u0003.\u0001 = \u0004.Swap(\u0002.ReadUInt16());
			\u0003.\u0002 = \u0004.Swap(\u0002.ReadUInt16());
			\u0003.\u0004 = \u0004.Swap(\u0002.ReadUInt32());
			\u0003.\u0005 = \u0004.Swap(\u0002.ReadUInt32());
		}

		// Token: 0x06002180 RID: 8576 RVA: 0x00072E50 File Offset: 0x00071050
		private void \u0001(BinaryReader \u0002, Swapper \u0003)
		{
			global::\u0004.\u0008 u = new global::\u0004.\u0008();
			this.\u0001(\u0002, u, \u0003);
			this.\u0001.Add(u);
		}

		// Token: 0x06002181 RID: 8577 RVA: 0x00072E78 File Offset: 0x00071078
		private void \u0002(BinaryReader \u0002, Swapper \u0003)
		{
			\u007F.\u0007 u = new \u007F.\u0007();
			u.\u0001 = \u0003.Swap(\u0002.ReadUInt32());
			u.\u0002 = \u0003.Swap(\u0002.ReadUInt32());
			this.\u0001.Add(u);
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x00072EBC File Offset: 0x000710BC
		private void \u0003(BinaryReader \u0002, Swapper \u0003)
		{
			global::\u0003.\u0008 u = new global::\u0003.\u0008();
			u.\u0001 = \u0003.Swap(\u0002.ReadUInt32());
			u.\u0002 = \u0003.Swap(\u0002.ReadUInt32());
			this.\u0001.Add(u);
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x00072F00 File Offset: 0x00071100
		private void \u0004(BinaryReader \u0002, Swapper \u0003)
		{
			\u007F.\u0006 u = new \u007F.\u0006();
			this.\u0001(\u0002, u, \u0003);
			u.\u0001 = \u0003.Swap(\u0002.ReadUInt32());
			u.\u0002 = \u0003.Swap(\u0002.ReadUInt32());
			u.\u0003 = \u0003.Swap(\u0002.ReadUInt32());
			this.\u0001.Add(u);
		}

		// Token: 0x06002184 RID: 8580 RVA: 0x00072F60 File Offset: 0x00071160
		private void \u0005(BinaryReader \u0002, Swapper \u0003)
		{
			global::\u0007.\u0008 u = new global::\u0007.\u0008();
			this.\u0001(\u0002, u, \u0003);
			u.\u0001 = \u0003.Swap(\u0002.ReadUInt32());
			this.\u0001.Add(u);
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x00072F9C File Offset: 0x0007119C
		private void \u0006(BinaryReader \u0002, Swapper \u0003)
		{
			\u001E.\u0007 u = new \u001E.\u0007();
			this.\u0001(\u0002, u, \u0003);
			u.\u0001 = \u0003.Swap(\u0002.ReadUInt32());
			u.ParentPOUType = (Operator)\u0003.Swap(\u0002.ReadUInt32());
			this.\u0001.Add(u);
		}

		// Token: 0x06002186 RID: 8582 RVA: 0x00072FE8 File Offset: 0x000711E8
		private void \u0001(_ICompileContext \u0002, _ISignature \u0003, uint \u0004)
		{
			foreach (_ISignature isignature in \u0003.SubSignatures)
			{
				if (!isignature.GetFlag(SignatureFlag.External) && (!isignature.GetFlag(SignatureFlag.Generated) || !(isignature.OrgName != "__MAIN")) && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE) && string.IsNullOrEmpty(isignature.LibraryPath) && (!isignature.OrgName.Contains("__") || !(isignature.OrgName != "__MAIN") || isignature.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY)))
				{
					global::\u0007.\u0008 u;
					if (Operator.FunctionBlock == \u0003.POUType)
					{
						u = new global::\u0007.\u0008();
						u.\u0001 = \u0004;
						this.\u0001.Add(u);
					}
					else
					{
						\u001E.\u0007 u2 = new \u001E.\u0007();
						u = u2;
						u2.\u0001 = \u0004;
						u2.ParentPOUType = \u0003.POUType;
						this.\u0001.Add(u2);
					}
					this.\u0001(u, isignature, \u0002);
				}
			}
		}

		// Token: 0x06002187 RID: 8583 RVA: 0x000730F8 File Offset: 0x000712F8
		internal bool \u0001(_ISignature \u0002)
		{
			return !\u0002.GetFlag(SignatureFlag.External) && !\u0002.GetFlag(SignatureFlag.Generated) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE) && string.IsNullOrEmpty(\u0002.LibraryPath) && !\u0002.OrgName.Contains("__");
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x00073158 File Offset: 0x00071358
		internal void \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				if (this.\u0001(isignature))
				{
					if (isignature.POUType == Operator.Function || isignature.POUType == Operator.Program || isignature.POUType == Operator.FunctionBlock)
					{
						Operator poutype = isignature.POUType;
						if (poutype != Operator.Function)
						{
							if (poutype != Operator.FunctionBlock)
							{
								if (poutype == Operator.Program)
								{
									global::\u0004.\u0008 u = new global::\u0004.\u0008();
									this.\u0001.Add(u);
									this.\u0001(u, isignature, \u0002);
									this.\u0001(\u0002, isignature, (uint)(this.\u0001.Count - 1));
								}
							}
							else
							{
								\u007F.\u0006 u2 = new \u007F.\u0006();
								this.\u0001.Add(u2);
								this.\u0001(u2, isignature, \u0002);
								u2.\u0001 = 0U;
								ISignature subSignature = isignature.GetSubSignature(IdentifierConstants.VFInitMethodName);
								if (subSignature != null)
								{
									_ICompiledPOU icompiledPOU = \u0002.GetCompiledPOUById(subSignature.Id) as _ICompiledPOU;
									if (icompiledPOU != null)
									{
										u2.\u0001 = Helper.\u0001(icompiledPOU);
									}
								}
								u2.\u0002 = (uint)isignature.VirtualFunctionTable.DataLocation.Area;
								u2.\u0003 = (uint)isignature.VirtualFunctionTable.DataLocation.Offset;
								this.\u0001(\u0002, isignature, (uint)(this.\u0001.Count - 1));
							}
						}
						else
						{
							global::\u0004.\u0008 u3 = new global::\u0004.\u0008();
							this.\u0001.Add(u3);
							this.\u0001(u3, isignature, \u0002);
						}
					}
					else if (isignature.POUType == Operator.VarGlobal)
					{
						global::\u0003.\u0008 u4 = new global::\u0003.\u0008();
						\u0002.GetInterfaceCRC(isignature, out u4.\u0001);
						u4.\u0002 = (uint)this.\u0001.BaseStream.Position;
						u4.\u0001 = isignature.OrgName;
						this.\u0001(isignature.OrgName, \u0002.TypeIsSupported(TypeClass.Byte));
						this.\u0001.Add(u4);
						this.\u0001(\u0002, isignature, (uint)(this.\u0001.Count - 1));
					}
					else if (isignature.POUType == Operator.Type)
					{
						\u007F.\u0007 u5 = new \u007F.\u0007();
						\u0002.GetInterfaceCRC(isignature, out u5.\u0001);
						u5.\u0002 = (uint)this.\u0001.BaseStream.Position;
						u5.\u0001 = isignature.OrgName;
						this.\u0001(isignature.OrgName, \u0002.TypeIsSupported(TypeClass.Byte));
						this.\u0001.Add(u5);
					}
				}
			}
			foreach (IPreCompileContext preCompileContext in \u0002.LibraryContexts)
			{
				if (preCompileContext != null)
				{
					uint num = (uint)this.\u0001.BaseStream.Position;
					this.\u0001.Add(num);
					this.\u0001.Add(preCompileContext.LibraryPath);
					this.\u0001(preCompileContext.LibraryPath, \u0002.TypeIsSupported(TypeClass.Byte));
				}
			}
			this.\u0001 = new byte[this.\u0001.BaseStream.Position];
			this.\u0001.BaseStream.Position = 0L;
			this.\u0001.BaseStream.Read(this.\u0001, 0, this.\u0001.Length);
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00073490 File Offset: 0x00071690
		internal string \u0001(_ICompileContext \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.TypeIsSupported(TypeClass.Byte))
			{
				return string.Format(global::\u001B.\u0005.\u0001, new object[]
				{
					this.\u0001.Count,
					this.\u0001.Count,
					this.\u0001.Count,
					this.\u0001.Count,
					this.\u0001.Count,
					this.\u0001.Count,
					this.\u0001.Length - 1,
					this.\u0001.Count,
					global::\u001B.\u0005.\u0001,
					Scanner.UnicodeIdentifierOption ? "1" : "0"
				});
			}
			return string.Format(global::\u001B.\u0005.\u0002, new object[]
			{
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				(this.\u0001.Length - 2) / 2,
				this.\u0001.Count,
				global::\u001B.\u0005.\u0001,
				Scanner.UnicodeIdentifierOption ? "1" : "0"
			});
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x0007364C File Offset: 0x0007184C
		internal string \u0002(_ICompileContext \u0002)
		{
			return global::\u001B.\u0005.\u0003 + string.Format(global::\u001B.\u0005.\u0004, new object[]
			{
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001.Count,
				this.\u0001(),
				this.\u0002(),
				this.\u0003(),
				this.\u0004(),
				this.\u0005(),
				this.\u0007(),
				this.\u0001(\u0002.TypeIsSupported(TypeClass.Byte)),
				this.\u0001.Count,
				this.\u0006()
			});
		}

		// Token: 0x17000647 RID: 1607
		// (get) Token: 0x0600218B RID: 8587 RVA: 0x00073750 File Offset: 0x00071950
		public IPOUInfoStruct[] POUs
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x17000648 RID: 1608
		// (get) Token: 0x0600218C RID: 8588 RVA: 0x0007376C File Offset: 0x0007196C
		public IDUTInfoStruct[] DUTs
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x17000649 RID: 1609
		// (get) Token: 0x0600218D RID: 8589 RVA: 0x00073788 File Offset: 0x00071988
		public IGVLInfoStruct[] GVLs
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x1700064A RID: 1610
		// (get) Token: 0x0600218E RID: 8590 RVA: 0x000737A4 File Offset: 0x000719A4
		public IFBInfoStruct[] FBs
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x1700064B RID: 1611
		// (get) Token: 0x0600218F RID: 8591 RVA: 0x000737C0 File Offset: 0x000719C0
		public IMethodInfoStruct[] Methods
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x1700064C RID: 1612
		// (get) Token: 0x06002190 RID: 8592 RVA: 0x000737DC File Offset: 0x000719DC
		public string[] Libs
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x1700064D RID: 1613
		// (get) Token: 0x06002191 RID: 8593 RVA: 0x000737EC File Offset: 0x000719EC
		public IEnumerable<IPOUMethodInfoStruct> POUMethods
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x06002192 RID: 8594 RVA: 0x000737F4 File Offset: 0x000719F4
		private bool UnicodeIdentifiers
		{
			get
			{
				if (global::\u000F.\u000E.\u0003 == this.\u0001)
				{
					return Scanner.UnicodeIdentifierOption;
				}
				return global::\u000F.\u000E.\u0002 == this.\u0001;
			}
		}

		// Token: 0x0400059E RID: 1438
		internal LList<global::\u0004.\u0008> \u0001 = new LList<global::\u0004.\u0008>();

		// Token: 0x0400059F RID: 1439
		internal LList<\u007F.\u0007> \u0001 = new LList<\u007F.\u0007>();

		// Token: 0x040005A0 RID: 1440
		internal LList<global::\u0003.\u0008> \u0001 = new LList<global::\u0003.\u0008>();

		// Token: 0x040005A1 RID: 1441
		internal LList<\u007F.\u0006> \u0001 = new LList<\u007F.\u0006>();

		// Token: 0x040005A2 RID: 1442
		internal LList<global::\u0007.\u0008> \u0001 = new LList<global::\u0007.\u0008>();

		// Token: 0x040005A3 RID: 1443
		internal LList<\u001E.\u0007> \u0001 = new LList<\u001E.\u0007>();

		// Token: 0x040005A4 RID: 1444
		internal LList<uint> \u0001 = new LList<uint>();

		// Token: 0x040005A5 RID: 1445
		internal LList<string> \u0001 = new LList<string>();

		// Token: 0x040005A6 RID: 1446
		internal BinaryWriter \u0001 = new BinaryWriter(new ChunkedMemoryStream());

		// Token: 0x040005A7 RID: 1447
		private byte[] \u0001;

		// Token: 0x040005A8 RID: 1448
		private global::\u000F.\u000E \u0001 = global::\u000F.\u000E.\u0003;

		// Token: 0x040005A9 RID: 1449
		private static HashSet<uint> \u0001;

		// Token: 0x040005AA RID: 1450
		private static uint \u0001;

		// Token: 0x040005AB RID: 1451
		private static string \u0001 = "\r\nTYPE APPLICATION_CONTENT :\r\nSTRUCT\r\n\tudiInfoSize: UDINT := SIZEOF(APPLICATION_CONTENT);\r\n    // Starting with SP16 we have version 0x03051600, that provides additional information at the end of the GVL\r\n\tudiInfoVersion: UDINT := {8};\r\n\tuiNumPOUs: UINT := {0};\r\n\tuiNumDUTs: UINT := {1};\r\n\tuiNumGVLs: UINT := {2};\r\n\tuiNumFBs: UINT := {3};\r\n\tuiNumMethods: UINT := {4};\r\n\tuiNumLibs: UINT := {5};\r\n\t\r\n\tarPOUs : ARRAY [0..{0}-1] OF __SYSTEM.__POUInfoStruct;\r\n\tarDUTs : ARRAY [0..{1}-1] OF __SYSTEM.__DUTInfoStruct;\r\n\tarGVLs : ARRAY [0..{2}-1] OF __SYSTEM.__GVLInfoStruct;\r\n\tarFBs : ARRAY [0..{3}-1] OF __SYSTEM.__FBInfoStruct;\r\n\tarMethods : ARRAY [0..{4}-1] OF __SYSTEM.__MethodInfoStruct;\r\n\t// Offsets in stringtable!\r\n\tarLibs : ARRAY [0..{5}-1] OF UDINT;\r\n\r\n\tstringtable : ARRAY [0..{6}] OF BYTE; \r\n\r\n\t// members introduced with version 0x03051600\r\n\tudiMagic: UDINT := 16#F3F516FF;\r\n\tuiNumPOUMethods: UINT := {7};\r\n\tarPOUMethods : ARRAY [0..{7}-1] OF __SYSTEM.__POUMethodInfoStruct;\r\n\r\n\t// members introduced with version 0x03051700\r\n\tudiMagic17: UDINT := 16#ADACDCEF;\r\n\tuiUnicodeIdentifiers: UINT := {9};\r\nEND_STRUCT\r\nEND_TYPE\r\n";

		// Token: 0x040005AC RID: 1452
		private static string \u0002 = "\r\nTYPE APPLICATION_CONTENT :\r\nSTRUCT\r\n\tudiInfoSize: UDINT := SIZEOF(APPLICATION_CONTENT);\r\n    // Starting with SP16 we have version 0x03051600, that provides additional information at the end of the GVL\r\n\tudiInfoVersion: UDINT := {8};\r\n\tuiNumPOUs: UINT := {0};\r\n\tuiNumDUTs: UINT := {1};\r\n\tuiNumGVLs: UINT := {2};\r\n\tuiNumFBs: UINT := {3};\r\n\tuiNumMethods: UINT := {4};\r\n\tuiNumLibs: UINT := {5};\r\n\t\r\n\tarPOUs : ARRAY [0..{0}-1] OF __SYSTEM.__POUInfoStruct;\r\n\tarDUTs : ARRAY [0..{1}-1] OF __SYSTEM.__DUTInfoStruct;\r\n\tarGVLs : ARRAY [0..{2}-1] OF __SYSTEM.__GVLInfoStruct;\r\n\tarFBs : ARRAY [0..{3}-1] OF __SYSTEM.__FBInfoStruct;\r\n\tarMethods : ARRAY [0..{4}-1] OF __SYSTEM.__MethodInfoStruct;\r\n\t// Offsets in stringtable!\r\n\tarLibs : ARRAY [0..{5}-1] OF UDINT;\r\n\r\n\tstringtable : ARRAY [0..{6}] OF WORD; \r\n\r\n\t// members introduced with version 0x03051600\r\n\tudiMagic: UDINT := 16#F3F516FF;\r\n\tuiNumPOUMethods: UINT := {7};\r\n\tarPOUMethods : ARRAY [0..{7}-1] OF __SYSTEM.__POUMethodInfoStruct;\r\n\r\n\t// members introduced with version 0x03051700\r\n\tudiMagic17: UDINT := 16#ADACDCEF;\r\n\tuiUnicodeIdentifiers: UINT := {9};\r\nEND_STRUCT\r\nEND_TYPE\r\n";

		// Token: 0x040005AD RID: 1453
		private static string \u0003 = "\r\n{attribute 'hide'}\r\n{attribute 'qualified_only'}\r\nVAR_GLOBAL CONSTANT\r\n\t{attribute 'no_init'}";

		// Token: 0x040005AE RID: 1454
		private static string \u0004 = "\r\n\tappContent : APPLICATION_CONTENT := (udiInfoSize := SIZEOF(APPLICATION_CONTENT)\r\n\t\t\t\t\t, udiInfoVersion := 0\r\n\t\t\t\t\t, uiNumPOUs := {0}\r\n\t\t\t\t\t, uiNumDUTs := {1}\r\n\t\t\t\t\t, uiNumGVLs := {2}\r\n\t\t\t\t\t, uiNumFBs := {3}\r\n\t\t\t\t\t, uiNumMethods := {4}\r\n\t\t\t\t\t, uiNumLibs := {5}\r\n\t\t\t\t\t, arPOUs := {6}\r\n\t\t\t\t\t, arDUTs := {7}\r\n\t\t\t\t\t, arGVLs := {8}\r\n\t\t\t\t\t, arFBs := {9} \r\n\t\t\t\t\t, arMethods := {10}\r\n\t\t\t\t\t, arLibs := {11}\r\n\t\t\t\t\t, stringtable := {12}\r\n\t\t\t\t\t, uiNumPOUMethods := {13}\r\n\t\t\t\t\t, arPOUMethods := {14}\r\n\t\t\t\t\t);\r\nEND_VAR\r\n";

		// Token: 0x040005AF RID: 1455
		internal static string \u0005 = "\r\ndwCRCCode := {0},\r\ndwCRCInterface := {1},\r\nudiNameStringIndex := {2},\r\nusiAreaCodeLocation := {3},\r\nusiAreaFPPointerLocation := {4},\r\nudiOffsetCodeLocation := {5},\r\nudiOffsetFPPointerLocation := {6}\r\n";

		// Token: 0x040005B0 RID: 1456
		internal static string \u0006 = "\r\n,dwCRCVFTable := {0},\r\nudiAreaVFTableLocation := {1},\r\nudiOffsetVFTableLocation := {2}\r\n";

		// Token: 0x040005B1 RID: 1457
		internal static string \u0007 = "\r\n,udiParentPOUIndex := {0}\r\n";

		// Token: 0x040005B2 RID: 1458
		internal static string \u0008 = "\r\n,udiPOUType := {0}\r\n";

		// Token: 0x040005B3 RID: 1459
		internal static string \u000E = "\r\ndwCRCInterface := {0},\r\nudiNameStringIndex := {1}\r\n";
	}
}
