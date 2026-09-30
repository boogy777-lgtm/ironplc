using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x0200008D RID: 141
	public class DebugLog
	{
		// Token: 0x06000BDA RID: 3034 RVA: 0x0001BD10 File Offset: 0x00019F10
		[Conditional("LOG_DEBUG")]
		public static void Init()
		{
			DebugLog.\u0001 = new StringBuilder();
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x0001BD1C File Offset: 0x00019F1C
		[Conditional("LOG_DEBUG")]
		public static void WriteToFile()
		{
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x0001BD20 File Offset: 0x00019F20
		[Conditional("LOG_DEBUG")]
		public static void Log(string stText, long nPosition)
		{
			DebugLog.\u0001.AppendLine(string.Format("{0}  {1}", nPosition, stText));
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x0001BD40 File Offset: 0x00019F40
		[Conditional("LOG_DEBUG")]
		public static void LogList<ELEMENT>(string stText, long nPosition, IList<ELEMENT> list)
		{
			DebugLog.\u0001.AppendLine(string.Format("{0}  {1} {2}: ", nPosition, stText, list.Count));
			foreach (ELEMENT element in list)
			{
				StringBuilder u = DebugLog.\u0001;
				string str = "\t";
				ELEMENT element2 = element;
				u.AppendLine(str + ((element2 != null) ? element2.ToString() : null));
			}
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x0001BDD8 File Offset: 0x00019FD8
		[Conditional("LOG_DEBUG")]
		public static void LogKeyValuePairs<KEY, VALUE>(string stText, long nPosition, IEnumerable<KeyValuePair<KEY, VALUE>> pairs)
		{
			foreach (KeyValuePair<KEY, VALUE> keyValuePair in pairs)
			{
				KEY key = keyValuePair.Key;
				string str = key.ToString();
				string str2 = string.Empty;
				if (keyValuePair.Value != null)
				{
					VALUE value = keyValuePair.Value;
					str2 = value.ToString();
				}
				DebugLog.\u0001.AppendLine("\t" + str + ": " + str2);
			}
		}

		// Token: 0x040001BA RID: 442
		private static int \u0001;

		// Token: 0x040001BB RID: 443
		private static StringBuilder \u0001;
	}
}
