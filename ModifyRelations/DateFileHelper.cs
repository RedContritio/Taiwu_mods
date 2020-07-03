using GameData;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Diagnostics;

namespace ModifyRelations
{
    /*
	 * ���ǿ��Է��������и���׼��ϵ��
	 * һ���ɲο�����Դ�ǣ�ͨ��Դ���е� WindowManage.WindowSwitch �е� case 21 ���в���
	 * �����������Ҫ�õ�һ������ DateFile.instance.massageDate[7][5]�������ҵ���Ӧ�� txt �ļ�
	 * �ر�ģ�ÿ��ȡ��ϵ�б�ǰ����Ҫ�����ж� DateFile.instance.HaveLifeDate(actorId, socialType)
	 * ��ͬ�����ˣ�˫����������ͬ DateFile.instance.GetActorGoodness
	 * �������빲���Է������ɾ��� DateFile.instance.GetLifeDateList(actorId, 202) �а��������������� int.Parse(DateFile.instance.GetActorDate(mainActorId, 19, applyBonus: false))
	 * ��֪��֮����DateFile.instance.GetActorSocial(actorId, 301, getDieActor: true)
	 * ���ֵܽ��ã�DateFile.instance.GetActorSocial(actorId, 302, getDieActor: true)
	 * ���׸���ĸ��DateFile.instance.GetActorSocial(actorId, 303, getDieActor: true)
	 * ���常��ĸ��DateFile.instance.GetActorSocial(actorId, 304, getDieActor: true)
	 * ����ҵ��ʦ��DateFile.instance.GetActorSocial(actorId, 305, getDieActor: true)
	 * ���������ã�DateFile.instance.GetActorSocial(actorId, 306, getDieActor: true)
	 * ���������أ�DateFile.instance.GetActorSocial(actorId, 307, getDieActor: true)
	 * ����������DateFile.instance.GetActorSocial(actorId, 308, getDieActor: true)
	 * ���ᷢ���ޣ�DateFile.instance.GetActorSocial(actorId, 309, getDieActor: true)
	 * ����Ů���ã�DateFile.instance.GetActorSocial(actorId, 310, getDieActor: true)
	 * ����ϵ���ˣ�DateFile.instance.GetActorSocial(actorId, 311, getDieActor: true)
	 * �����İ�Ľ��DateFile.instance.GetActorSocial(actorId, 312, getDieActor: true)
	 * ��ѪŨ��ˮ��˫���ĸ�ϵѪͳ��601���ĵ�һ���˻���ĸϵѪͳ��602���ĵ�һ������ͬ
	 * ��̫����񣺶Է�������ID int.Parse(DateFile.instance.GetActorDate(actorId, 19, applyBonus: false)) ֵΪ 16��̫��壩
	 * ��ͬ��ͬԴ��˫��������ģ������ͬһ���� (int.Parse(DateFile.instance.GetActorDate(mainActorId, 997, applyBonus: false)) + 1 )/2 ���
	 * ���Ʋ�������DateFile.instance.GetActorSocial(actorId, 401, getDieActor: true)
	 * ����Ϊ������DateFile.instance.GetLifeDateList(actorId, 201).Contains(16)
	 * ��Ƿ��ʧ�壺Ƿ��ʧ��ֵ���� 0  int.Parse(DateFile.instance.GetActorDate(actorId, 210, applyBonus: false)) > 0
	 * ��Ѫ�����DateFile.instance.GetActorSocial(actorId, 402, getDieActor: true)
	 */

    public static class DateFileHelper
    {
        public readonly static Relation[] relations = new Relation[16] {
            new Relation(301, "֪��֮��", isOnly: true),
            new Relation(302, "�ֵܽ���", isOnly: true),
            new Relation(303, "�׸���ĸ", isOnly: false),
            new Relation(304, "�常��ĸ", isOnly: false),
            new Relation(305, "��ҵ��ʦ", isOnly: false),
            new Relation(306, "��������", isOnly: true),
            new Relation(307, "��������", isOnly: false),
            new Relation(308, "������", isOnly: true),
            new Relation(309, "�ᷢ����", isOnly: true),
            new Relation(310, "��Ů����", isOnly: false),
            new Relation(311, "��ϵ����", isOnly: false),
            new Relation(312, "���İ�Ľ", isOnly: false),
            new Relation(401, "�Ʋ�����", isOnly: false),
            new Relation(402, "Ѫ�����", isOnly: false),
            new Relation(601, "��ϵѪͳ", isOnly: false, isNet: true),
            new Relation(602, "ĸϵѪͳ", isOnly: false, isNet: true),
        };

        /// <summary>
        /// ������ relations ������±�ת��Ϊ��Ӧ�� socialType
        /// </summary>
        public readonly static int[] relationId2Key = new int[16] {
            relations[0].key,  relations[1].key,  relations[2].key,  relations[3].key,
            relations[4].key,  relations[5].key,  relations[6].key,  relations[7].key,
            relations[8].key,  relations[9].key,  relations[10].key, relations[11].key,
            relations[12].key, relations[13].key, relations[14].key, relations[15].key,
        };

        /// <summary>
        /// ������ socialType ת��Ϊ��Ӧ�� relations ������±�
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
        /// �ӹ�ϵ����ɾ��ĳ��
        /// �ο����� DateFile.instance.RemoveActorSocial
        /// </summary>
        /// <param name="socialId">��ϵID</param>
        /// <param name="actorId">ɾ��������ID</param>
        /// <param name="socialTyp">��ϵ����</param>
        public static void LeaveActorLifeDate(int socialId, int actorId, int socialTyp)
        {
            if (!DateFile.instance.HaveLifeDate(actorId, socialTyp) || !DateFile.instance.actorSocialDate.ContainsKey(socialId))
            {
                return;
            }

            // �ȴӹ�ϵ��ɾ�������ɫ
            DateFile.instance.actorSocialDate[socialId].Remove(actorId);

            // �����ɫ�Ĵ˹�ϵ��ɾ�� socialId
            DateFile.instance.actorLife[actorId][socialTyp].Remove(socialId);
            // ���ý�ɫ�Ĵ˹�ϵ��Ϊ�գ���ȥ���˹�ϵ��
            if (DateFile.instance.GetLifeDateList(actorId, socialTyp, alreadyCheckHaveLife: true).Count <= 0)
            {
                DateFile.instance.actorLife[actorId].Remove(socialTyp);
            }

            // 303��304��305��307��310��311��312��401��402 ���� socialOnly[t][1] ��
            if (GetRelationByKey(socialTyp).net == false && DateFile.instance.socialOnly[socialTyp][1])
            {
                // ��������ϵ������ֻ�����ֻ�����ˣ���ɾ��ʣ�µ�һ���ˣ����򲻲���
                DateFile.instance.actorSocialDate[socialId].Clear();
            }
            // ��״��ϵ����Ҫ�½�һ�����˵���
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

            // �����ϵ��û���ˣ���ɾ���ù�ϵ
            if (DateFile.instance.actorSocialDate[socialId].Count <= 0)
            {
                DateFile.instance.actorSocialDate.Remove(socialId);

                foreach (int actorIt in DateFile.instance.actorLife.Keys)
                {
                    //public bool HaveLifeDate(int actorId, int socialTyp)
                    //{
                    //	Dictionary<int, List<int>> value; // ������������Ĺ�ϵ�ֵ�
                    //	List<int> socialIds; // ����������������ĳ��ϵ�����й�ϵID
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
        /// ������ϵ��ֱ���ɷ����²㼴��
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
