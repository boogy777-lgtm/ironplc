using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008B RID: 139
	[ReleasedClass]
	public class PSChangedEventArgs : EventArgs
	{
		// Token: 0x0600023F RID: 575 RVA: 0x0000486F File Offset: 0x00002A6F
		public PSChangedEventArgs(IPSChange[] changes)
		{
			this._changes = changes;
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000240 RID: 576 RVA: 0x0000487E File Offset: 0x00002A7E
		public IPSChange[] Changes
		{
			get
			{
				return this._changes;
			}
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00004888 File Offset: 0x00002A88
		public int[] FindChanges(IPSNode node, FindChangesFlags flags)
		{
			return this.FindChanges(node, flags, null);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x000048A6 File Offset: 0x00002AA6
		public int[] FindChanges(IPSNode node, FindChangesFlags flags, PSChangeAction action)
		{
			return this.FindChanges(node, flags, new PSChangeAction?(action));
		}

		// Token: 0x06000243 RID: 579 RVA: 0x000048B8 File Offset: 0x00002AB8
		private int[] FindChanges(IPSNode node, FindChangesFlags flags, PSChangeAction? nAction)
		{
			List<int> list = new List<int>();
			if (node != null && this._changes != null)
			{
				for (int i = 0; i < this._changes.Length; i++)
				{
					int num;
					if ((nAction == null || this._changes[i].Action == nAction.Value) && this._changes[i].AffectedNode != null && PSChangedEventArgs.GetRelationship(this._changes[i].AffectedNode, node, out num) && ((num == 0 && (flags & FindChangesFlags.Node) != (FindChangesFlags)0) || (num == -1 && (flags & FindChangesFlags.ImmediateChild) != (FindChangesFlags)0) || (num < 0 && (flags & FindChangesFlags.AnyChild) != (FindChangesFlags)0) || (num == 1 && (flags & FindChangesFlags.ImmediateParent) != (FindChangesFlags)0) || (num > 0 && (flags & FindChangesFlags.AnyParent) != (FindChangesFlags)0)))
					{
						list.Add(i);
					}
				}
			}
			return list.ToArray();
		}

		// Token: 0x06000244 RID: 580 RVA: 0x00004974 File Offset: 0x00002B74
		private static bool GetRelationship(IPSNode node0, IPSNode node1, out int nDistance)
		{
			if (node0.Equals(node1))
			{
				nDistance = 0;
				return true;
			}
			IPSNode ipsnode = node0;
			int num = 0;
			while (ipsnode != null)
			{
				if (ipsnode.Equals(node1))
				{
					nDistance = -num;
					return true;
				}
				ipsnode = ipsnode.ParentNode;
				num++;
			}
			ipsnode = node1;
			int num2 = 0;
			while (ipsnode != null)
			{
				if (ipsnode.Equals(node0))
				{
					nDistance = num2;
					return true;
				}
				ipsnode = ipsnode.ParentNode;
				num2++;
			}
			nDistance = 0;
			return false;
		}

		// Token: 0x040000C6 RID: 198
		private IPSChange[] _changes;
	}
}
