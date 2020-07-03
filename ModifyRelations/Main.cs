using Harmony12;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using System.Collections.Generic;

namespace ModifyRelations
{
    public static class Main
    {
        public static bool enabled;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static Setting settings;

        public static Cache cache = null;

        public static List<String> inputs = new List<string>(20) { "", "", "", ""};

        public static int selectAddType = 0;

        public static HarmonyInstance GetInstance(UnityModManager.ModEntry modEntry) => HarmonyInstance.Create(modEntry.Info.Id);

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var instance = GetInstance(modEntry);
            //val.PatchAll(Assembly.GetExecutingAssembly());


            settings = UnityModManager.ModSettings.Load<Setting>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static void Debug(string str)
        {
            if (settings.DebugMode)
                Logger.Log("[关系修改]" + str);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Main.enabled)
            {
                GUILayout.Label("游戏尚未开始，请开始后再打开页面");
                return;
            }
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("角色ID：");

            inputs[0] = GUILayout.TextField(inputs[0], 6);
            string titleName = "Not Found";
            if (int.TryParse(inputs[0], out int act0) && DateFileHelper.isValidID(act0))
            {
                titleName = DateFile.instance.GetActorName(act0);
            }
            else
            {
            }
            GUILayout.Label(titleName);

            if (GUILayout.Button("读取关系") && DateFileHelper.isValidID(act0))
            {
                cache = new Cache(act0);
                cache.Read();
            }

            if (GUILayout.Button("写入关系"))
            {
                if (cache != null && DateFileHelper.isValidID(act0))
                {
                    cache.Write();
                }
            }

            GUILayout.EndHorizontal();

            if (cache != null)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("{0} {1} 的关系如下：", cache.actorId, DateFile.instance.GetActorName(cache.actorId)));
                GUILayout.EndHorizontal();

                foreach (Relation relation in DateFileHelper.relations)
                {
                    GUILayout.BeginHorizontal("Box");
                    GUILayout.Label(String.Format("<color={0}>{1}</color>： {2}", "#E4504DFF", relation.text, cache.getSocialList(relation.key)));
                    GUILayout.EndHorizontal();
                }


                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("要删去的关系：{0}", cache.getToLeaveSocials()));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("要删去的关系ID："));
                inputs[1] = GUILayout.TextField(inputs[1]);

                string output1 = "Not Found";
                if (int.TryParse(inputs[1], out int soc1) && cache.HasLifeDate(soc1))
                {
                    output1 = cache.Social2Str(soc1);
                }
                GUILayout.Label(output1);

                if (GUILayout.Button("确认")) {
                    inputs[1] = "";
                    cache.LeaveSocial(soc1);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("关系类型设置");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                for (int i = 0; i < DateFileHelper.relations.Length; ++i)
                {
                    if(GUILayout.Toggle(selectAddType == i, DateFileHelper.relations[i].text))
                        selectAddType = i;

                    if((i+1) % 6 == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal("Box");
                    }
                }
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("要进入的关系：{0}", cache.getToEnterSocials()));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("要进入的关系ID："));
                inputs[2] = GUILayout.TextField(inputs[2]);

                string output2 = "Not Found or Not Support Enter";
                if (int.TryParse(inputs[2], out int soc2) && DateFile.instance.actorSocialDate.ContainsKey(soc2) && DateFileHelper.relations[selectAddType].enterable)
                {
                    output2 = cache.Social2Str(soc2, inside: false);
                }
                GUILayout.Label(output2);

                if (GUILayout.Button("确认"))
                {
                    inputs[2] = "";
                    if (DateFile.instance.actorSocialDate.ContainsKey(soc2) && DateFileHelper.relations[selectAddType].enterable)
                    {
                        cache.EnterSocial(soc2, DateFileHelper.relationId2Key[selectAddType]);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("要新增的关系：{0}", cache.getToAddSocials()));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(String.Format("对方人物ID："));
                inputs[3] = GUILayout.TextField(inputs[3]);

                string output3 = "Not Found or Not Support Construct";
                if (int.TryParse(inputs[3], out int act3) && GameData.Characters.HasChar(act3) && DateFileHelper.relations[selectAddType].addable)
                {
                    output3 = DateFile.instance.GetActorName(act3);
                }
                GUILayout.Label(output3);

                if (GUILayout.Button("确认"))
                {
                    inputs[3] = "";
                    if (GameData.Characters.HasChar(act3) && DateFileHelper.relations[selectAddType].addable)
                    {
                        cache.AddSocial(act3, DateFileHelper.relationId2Key[selectAddType]);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
