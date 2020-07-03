using GameData;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;

namespace ModifyRelations
{
	/// <summary>
	/// 用于显示信息的数据
	/// 加载及写入的时候采用 DateFileHelper 实现
	/// </summary>
	public class Cache
	{
		public int actorId = 0;

		public Dictionary<int, List<int>>[] lifeData = null;

		public Dictionary<int, List<int>> to_enter_social = new Dictionary<int, List<int>>();
		public Dictionary<int, List<int>> to_leave_social = new Dictionary<int, List<int>>();
		public Dictionary<int, List<int>> to_add_social = new Dictionary<int, List<int>>();

		public Cache(int actorId)
		{
			this.actorId = actorId;
			foreach (Relation relation in DateFileHelper.relations)
			{
				to_enter_social.Add(relation.key, new List<int>());
				to_leave_social.Add(relation.key, new List<int>());
				to_add_social.Add(relation.key, new List<int>());
			}
		}

		public void ClearBuffer() {
			foreach (int key in to_enter_social.Keys) {
				to_enter_social[key].Clear();
				to_leave_social[key].Clear();
				to_add_social[key].Clear();
			}
		}

		public bool Read() {
			if(!DateFile.instance) {
				return false;
			}

			ClearBuffer();

			lifeData = new Dictionary<int, List<int>>[DateFileHelper.relations.Length];

			for (int i=0; i< DateFileHelper.relations.Length; ++i)
			{
				int key = DateFileHelper.relations[i].key;
				lifeData[i] = DateFileHelper.getSpecifiedLifeData(actorId, key);
				Main.Debug(key + " " + lifeData[i].Count);
			}
			return true;
		}

		public bool Write()
		{
			if (!DateFile.instance)
			{
				return false;
			}

			foreach (Relation relation in DateFileHelper.relations)
			{
				foreach (var socialId in to_leave_social[relation.key])
				{
					DateFileHelper.LeaveActorLifeDate(socialId, actorId, relation.key);
				}
				foreach (var socialId in to_enter_social[relation.key])
				{
					DateFileHelper.EnterActorLifeDate(socialId, actorId, relation.key);
				}
				foreach (var act in to_add_social[relation.key])
				{
					DateFileHelper.AddActorSocial(actorId, act, relation.key);
				}
			}

			Read();
			return true;
		}

		public string Social2Str(int socialId, bool inside = true) {
			string names = "";
			if (inside)
			{
				int relationId = DateFileHelper.relationKey2Id[getSocialTypeBySocialId(socialId)];
				foreach (int id in lifeData[relationId][socialId])
				{
					names += String.Format("<{0}, {1}> ", id, DateFile.instance.GetActorName(id));
				}
			}
			else
			{
				foreach (int id in DateFile.instance.actorSocialDate[socialId])
				{
					names += String.Format("<{0}, {1}> ", id, DateFile.instance.GetActorName(id));
				}
			}
			return String.Format("[{0} $ {1}]", socialId, names);
		}

		/// <summary>
		/// 获取当前缓存区的 socialType 对应的列表
		/// </summary>
		/// <param name="socialType"></param>
		/// <returns></returns>
		public string getSocialList(int socialType) {
			string text = "";
			int relationId = DateFileHelper.relationKey2Id[socialType];
			var a = lifeData[relationId].Keys;
			foreach (int socialId in a)
			{
				text += Social2Str(socialId) + "  ";
			}

			return text;
		}
		
		/// <summary>
		 /// 获取当前缓存区的 to_leave_social 对应的列表文本
		 /// </summary>
		 /// <returns></returns>
		public string getToLeaveSocials()
		{
			string text = "";
			foreach (List<int> il in to_leave_social.Values)
			{
				foreach (int id in il)
				{
					text += Social2Str(id) + " ";
				}
			}

			return text;
		}
		
		/// <summary>
		/// 获取当前缓存区的 to_enter_social 对应的列表文本
		/// </summary>
		/// <returns></returns>
		public string getToEnterSocials()
		{
			string text = "";
			foreach (List<int> il in to_enter_social.Values)
			{
				foreach (int id in il)
				{
					text += Social2Str(id, false) + " ";
				}
			}

			return text;
		}

		/// <summary>
		/// 获取当前缓存区的 to_add_social 对应的列表文本
		/// </summary>
		/// <returns></returns>
		public string getToAddSocials()
		{
			Main.Logger.Log("debug point 1");
			string text = "";
			string names;
			foreach (int key in to_add_social.Keys)
			{
				names = "";
				foreach (int id in to_add_social[key])
				{
					names += String.Format("<{0}, {1}> ", id, DateFile.instance.GetActorName(id));
				}
				if (to_add_social[key].Count > 0)
				{
					text += String.Format("[{0} : {1}] ", DateFileHelper.GetRelationByKey(key).text, names);
				}
			}
			Main.Logger.Log("debug point 2");

			return text;
		}
		/// <summary>
		/// 添加关系
		/// 表示该人物主动进入某个关系组
		/// 该函数给外部用的，表示将一个要进入的关系放到缓冲区
		/// </summary>
		public void AddSocial(int targetId, int socialTyp)
		{
			to_add_social[socialTyp].Add(targetId);
		}

		/// <summary>
		/// 添加关系
		/// 表示该人物主动进入某个关系组
		/// 该函数给外部用的，表示将一个要进入的关系放到缓冲区
		/// </summary>
		public void EnterSocial(int socialId, int socialTyp)
		{
			if (!to_leave_social[socialTyp].Remove(socialId))
				to_enter_social[socialTyp].Add(socialId);
		}

		/// <summary>
		/// 离开关系
		/// 表示该人物主动离开某个关系组
		/// 该函数给外部用的，表示将一个要离开的关系放到缓冲区
		/// </summary>
		public void LeaveSocial(int socialId)
		{
			int socialTyp = getSocialTypeBySocialId(socialId);
			if (!to_enter_social[socialTyp].Remove(socialId))
				to_leave_social[socialTyp].Add(socialId);
		}

		public bool HasLifeDate(int socialId) {
			foreach (Dictionary<int, List<int>> dic in lifeData)
			{
				if(dic.Keys.Contains(socialId))
					return true;
			}
			return false;
		}

		public int getSocialTypeBySocialId(int socialId)
		{
			for (int i = 0; i < lifeData.Length; ++i)
			{
				if (lifeData[i].Keys.Contains(socialId))
				{
					return DateFileHelper.relationId2Key[i];
				}
			}
			//return DateFileHelper.getLifeDateType(socialId);
			return -1;
		}
	}
}
