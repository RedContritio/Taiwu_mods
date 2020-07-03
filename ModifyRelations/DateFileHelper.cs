using GameData;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Diagnostics;

namespace ModifyRelations
{
    /*
	 * 我们可以发现这里有个标准关系表
	 * 一个可参考的来源是，通过源码中的 WindowManage.WindowSwitch 中的 case 21 进行查阅
	 * 这个过程中需要用到一个变量 DateFile.instance.massageDate[7][5]，可以找到对应的 txt 文件
	 * 特别的，每次取关系列表前，需要进行判断 DateFile.instance.HaveLifeDate(actorId, socialType)
	 * ·同道中人：双方的立场相同 DateFile.instance.GetActorGoodness
	 * ·休戚与共：对方的门派经历 DateFile.instance.GetLifeDateList(actorId, 202) 中包含主角所在门派 int.Parse(DateFile.instance.GetActorDate(mainActorId, 19, applyBonus: false))
	 * ·知心之交：DateFile.instance.GetActorSocial(actorId, 301, getDieActor: true)
	 * ·兄弟姐妹：DateFile.instance.GetActorSocial(actorId, 302, getDieActor: true)
	 * ·亲父亲母：DateFile.instance.GetActorSocial(actorId, 303, getDieActor: true)
	 * ·义父义母：DateFile.instance.GetActorSocial(actorId, 304, getDieActor: true)
	 * ·授业恩师：DateFile.instance.GetActorSocial(actorId, 305, getDieActor: true)
	 * ·两情相悦：DateFile.instance.GetActorSocial(actorId, 306, getDieActor: true)
	 * ·恩深义重：DateFile.instance.GetActorSocial(actorId, 307, getDieActor: true)
	 * ·义结金兰：DateFile.instance.GetActorSocial(actorId, 308, getDieActor: true)
	 * ·结发夫妻：DateFile.instance.GetActorSocial(actorId, 309, getDieActor: true)
	 * ·儿女子嗣：DateFile.instance.GetActorSocial(actorId, 310, getDieActor: true)
	 * ·嫡系传人：DateFile.instance.GetActorSocial(actorId, 311, getDieActor: true)
	 * ·倾心爱慕：DateFile.instance.GetActorSocial(actorId, 312, getDieActor: true)
	 * ·血浓于水：双方的父系血统（601）的第一个人或者母系血统（602）的第一个人相同
	 * ·太吾村民：对方的门派ID int.Parse(DateFile.instance.GetActorDate(actorId, 19, applyBonus: false)) 值为 16（太吾村）
	 * ·同乡同源：双方的人物模板来自同一地区 (int.Parse(DateFile.instance.GetActorDate(mainActorId, 997, applyBonus: false)) + 1 )/2 相等
	 * ·势不两立：DateFile.instance.GetActorSocial(actorId, 401, getDieActor: true)
	 * ·各为其主：DateFile.instance.GetLifeDateList(actorId, 201).Contains(16)
	 * ·欠恩失义：欠恩失义值大于 0  int.Parse(DateFile.instance.GetActorDate(actorId, 210, applyBonus: false)) > 0
	 * ·血海深仇：DateFile.instance.GetActorSocial(actorId, 402, getDieActor: true)
	 */

    public static class DateFileHelper
    {
        public readonly static Relation[] relations = new Relation[16] {
            new Relation(301, "知心之交", isOnly: true),
            new Relation(302, "兄弟姐妹", isOnly: true),
            new Relation(303, "亲父亲母", isOnly: false),
            new Relation(304, "义父义母", isOnly: false),
            new Relation(305, "授业恩师", isOnly: false),
            new Relation(306, "两情相悦", isOnly: true),
            new Relation(307, "恩深义重", isOnly: false),
            new Relation(308, "义结金兰", isOnly: true),
            new Relation(309, "结发夫妻", isOnly: true),
            new Relation(310, "儿女子嗣", isOnly: false),
            new Relation(311, "嫡系传人", isOnly: false),
            new Relation(312, "倾心爱慕", isOnly: false),
            new Relation(401, "势不两立", isOnly: false),
            new Relation(402, "血海深仇", isOnly: false),
            new Relation(601, "父系血统", isOnly: false, isNet: true),
            new Relation(602, "母系血统", isOnly: false, isNet: true),
        };

        /// <summary>
        /// 用来将 relations 数组的下标转换为对应的 socialType
        /// </summary>
        public readonly static int[] relationId2Key = new int[16] {
            relations[0].key,  relations[1].key,  relations[2].key,  relations[3].key,
            relations[4].key,  relations[5].key,  relations[6].key,  relations[7].key,
            relations[8].key,  relations[9].key,  relations[10].key, relations[11].key,
            relations[12].key, relations[13].key, relations[14].key, relations[15].key,
        };

        /// <summary>
        /// 用来将 socialType 转换为对应的 relations 数组的下标
        /// </summary>
        public readonly static Dictionary<int, int> relationKey2Id = new Dictionary<int, int> {
            {relations[0].key,  0},  {relations[1].key,  1},  {relations[2].key,  2},  {relations[3].key,  3},
            {relations[4].key,  4},  {relations[5].key,  5},  {relations[6].key,  6},  {relations[7].key,  7},
            {relations[8].key,  8},  {relations[9].key,  9},  {relations[10].key, 10}, {relations[11].key, 11},
            {relations[12].key, 12}, {relations[13].key, 13}, {relations[14].key, 14}, {relations[15].key, 15},
        };

        public static Relation GetRelationByKey(int key)
        {
            int id = relationKey2Id[key];
            if(id >= 0)
                return relations[id];
            return null;
        }

        public static void RemoveActorSocial(int actorId1, int actorId2, int socialTyp) => LeaveActorLifeDate(actorId1, actorId2, socialTyp);

        /// <summary>
        /// 从关系组内删掉某人
        /// 参考来自 DateFile.instance.RemoveActorSocial
        /// </summary>
        /// <param name="socialId">关系ID</param>
        /// <param name="actorId">删除的人物ID</param>
        /// <param name="socialTyp">关系类型</param>
        public static void LeaveActorLifeDate(int socialId, int actorId, int socialTyp)
        {
            if (!DateFile.instance.HaveLifeDate(actorId, socialTyp) || !DateFile.instance.actorSocialDate.ContainsKey(socialId))
            {
                return;
            }

            // 先从关系里删掉这个角色
            DateFile.instance.actorSocialDate[socialId].Remove(actorId);

            // 这个角色的此关系类删掉 socialId
            DateFile.instance.actorLife[actorId][socialTyp].Remove(socialId);
            // 若该角色的此关系类为空，则去掉此关系类
            if (DateFile.instance.GetLifeDateList(actorId, socialTyp, alreadyCheckHaveLife: true).Count <= 0)
            {
                DateFile.instance.actorLife[actorId].Remove(socialTyp);
            }

            // 303、304、305、307、310、311、312、401、402 不是 socialOnly[t][1] 的
            if (GetRelationByKey(socialTyp).net == false && DateFile.instance.socialOnly[socialTyp][1])
            {
                // 如果这个关系本来就只有最多只有两人，则删除剩下的一个人，否则不操作
                DateFile.instance.actorSocialDate[socialId].Clear();
            }
            // 网状关系，需要新建一个单人的网
            else 
            {
                if(DateFile.instance.actorLife[actorId].ContainsKey(socialTyp))
                {
                    DateFile.instance.actorLife[actorId].Remove(socialTyp);
                }

                int newid = DateFile.instance.GetNewSocialId();
                DateFile.instance.actorSocialDate.Add(newid, new List<int> { actorId });
                DateFile.instance.actorLife[actorId].Add(socialTyp, new List<int>{ newid}); 
            }

            // 如果关系内没人了，就删掉该关系
            if (DateFile.instance.actorSocialDate[socialId].Count <= 0)
            {
                DateFile.instance.actorSocialDate.Remove(socialId);

                foreach (int actorIt in DateFile.instance.actorLife.Keys)
                {
                    //public bool HaveLifeDate(int actorId, int socialTyp)
                    //{
                    //	Dictionary<int, List<int>> value; // 这个是这个人物的关系字典
                    //	List<int> socialIds; // 这个是这个人物满足某关系的所有关系ID
                    //	return actorLife.TryGetValue(actorId, out value) && value.TryGetValue(socialTyp, out socialIds) && socialIds.Count > 0;
                    //}
                    if (DateFile.instance.HaveLifeDate(actorIt, socialTyp))
                    {
                        DateFile.instance.actorLife[actorIt][socialTyp].Remove(socialId);
                        if (DateFile.instance.GetLifeDateList(actorIt, socialTyp, alreadyCheckHaveLife: true).Count <= 0)
                        {
                            DateFile.instance.actorLife[actorIt].Remove(socialTyp);
                        }
                    }
                }
            }
        }

        public static void EnterActorLifeDate(int socialId, int actorId, int socialTyp)
        {
            if(!DateFile.instance.actorSocialDate.ContainsKey(socialId))
                return ;

            if (!DateFile.instance.HaveLifeDate(actorId, socialTyp))
            {
                DateFile.instance.actorLife[actorId].Add(socialTyp, new List<int>());
            }
            else if (DateFile.instance.actorLife[actorId][socialTyp].Contains(socialId))
            {
                return;
            }

            DateFile.instance.actorLife[actorId][socialTyp].Insert(0, socialId);
        }

        /// <summary>
        /// 新增关系，直接派发到下层即可
        /// </summary>
        public static void AddActorSocial(int actorId1, int actorId2, int scoialTyp) => DateFile.instance.AddSocial(actorId1, actorId2, scoialTyp);

        public static int getLifeDateType(int socialId)
        {
            foreach (int actorIt in DateFile.instance.actorLife.Keys)
            {
                foreach (Relation relation in relations)
                {
                    if (DateFile.instance.HaveLifeDate(actorIt, relation.key))
                    {
                        if(DateFile.instance.actorLife[actorIt][relation.key].Contains(socialId))
                            return relation.key;
                    }
                }
            }
            return -1;
        }

        public static Dictionary<int, List<int>> getSpecifiedLifeData(int actorId, int socialTyp)
        {
            Dictionary<int, List<int>> ret = new Dictionary<int, List<int>>();
            
            if (DateFile.instance.HaveLifeDate(actorId, socialTyp))
            {
                List<int> socialIds = DateFile.instance.GetLifeDateList(actorId, socialTyp, alreadyCheckHaveLife: true);
                foreach (int socialId in socialIds)
                {
                    if (!DateFile.instance.actorSocialDate.ContainsKey(socialId))
                    {
                        ret.Add(socialId, new List<int>());
                    }
                    else
                    {
                        ret.Add(socialId, DateFile.instance.actorSocialDate[socialId]);
                    }
                }
            }
            return ret;
        }

        public static List<int> getActorsBySocialId(int socialId)
        {
            Trace.Assert(DateFile.instance.actorSocialDate.ContainsKey(socialId));
            return DateFile.instance.actorSocialDate[socialId];
        }

        public static bool HasSocial(int actorId, int socialTyp, int targetId, bool getDieActor = false, bool getNpc = false) => DateFile.instance.GetActorSocial(actorId, socialTyp, getDieActor, getNpc).Contains(targetId);

        public static bool HasAnySocial(int actorId, IEnumerable<int> socialTypList, int targetId, bool getDieActor = false, bool getNpc = false)
        {
            foreach (int t in socialTypList)
                if (HasSocial(actorId, t, targetId, getDieActor, getNpc))
                    return true;
            return false;
        }

        public static bool isValidID(int actorId)
        {
            return GameData.Characters.HasChar(actorId) && DateFile.instance.GetActorName(actorId) != null;
        }
    }
}
