using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RekaMainPatch
{
    [BepInPlugin("dev.reka.rabbitfix", "Rabbit Fix", "1.0.0")]
    public class RabbitFixPlugin : BaseUnityPlugin
    {
        List<string> _sceneNames = new List<string>();
        List<string> _exceptionMessages = new List<string>();

        private void Awake()
        {
            Logger.LogInfo("Rabbit Fix Plugin is loaded!");
            SceneManager.sceneLoaded += OnSceneLoaded;

            //var harmony = new Harmony("dev.reka.rabbitfix");
            //harmony.PatchAll();


            //Harmony.CreateAndPatchAll(typeof(RabbitFixPlugin));
            //Harmony.CreateAndPatchAll(typeof(Patch_Addressables_Instantiate));
            //Harmony.CreateAndPatchAll(typeof(Patch_SetActive));
        }

        [HarmonyPatch(typeof(Object), "Instantiate", new[] { typeof(Object) })]
        [HarmonyPostfix]
        static void OnInstantiate(Object __result)
        {
            if (__result is GameObject go)
            {
                File.AppendAllText("Log_TryToFind.txt", $"{go.name}\n");
                if (go.name.Contains("YourTargetGameObjectName"))
                {
                    Debug.Log($"[SpawnWatcher] Spawned {go.name}");
                    // обработка
                    go.transform.localScale *= 1.5f;
                }
            }
        }



        private void Freeze()
        {
            while (true) ;
        }

        private void LogSceneName(string name)
        {
            _sceneNames.Add(name);
            File.WriteAllLines("Log_SceneNames.txt", _sceneNames);
        }

        private void LogException(string message)
        {
            _exceptionMessages.Add(message);
            File.WriteAllLines("Log_Exceptions.txt", _exceptionMessages);
        }

        private void LogCurrentSceneInfo(string sceneName)
        {
            string sceneInfo = GetSceneInfo(true);
            File.WriteAllText($"Log_SceneInfo({sceneName}).txt", sceneInfo);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogSceneName(scene.name);
            LogCurrentSceneInfo(scene.name);

            GameObject watcher = new GameObject("ObjectWatcher");
            watcher.AddComponent<ObjectWatcher>();
            DontDestroyOnLoad(watcher);


            return;
            //FindAllMonos();

            //var assetManager = Reka.Managers.AssetManager.Instance;
            Logger.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            var carriableObject = FindObjectsOfType<CarriableObject>();
            if (carriableObject == null && carriableObject.Length > 0)
            {
                Logger.LogError(carriableObject.Length);
                while (true)
                {

                }

            }
            else
            {
                Logger.LogError("------------------------------------------");

            }
        }

        private IEnumerator CheckForObject()
        {
            bool objectFound = false;
            while (!objectFound)
            {
                Logger.LogInfo($"[YourMod] Found !!!!!!!!!!");

                //var obj = GameObject.Find(targetName);
                //if (obj != null)
                //{
                //    objectFound = true;
                //    Logger.LogInfo($"[YourMod] Found {targetName}!");
                //    OnObjectSpawned(obj);
                //}
                yield return new WaitForSeconds(0.5f); // проверяем 2 раза в секунду
            }
        }

        bool IsCustomScript(Component mb)
        {
            Type type = mb.GetType();
            string ns = type.Namespace;

            if (string.IsNullOrEmpty(ns))
            {
                return true;
            }

            bool isUnity = ns.Contains("Unity") || ns.Contains("UnityEngine") || ns.Contains("TMPro");

            return !isUnity;
        }

        private string GetSceneInfo(bool onlyCustom = false)
        {
            string monosAndComponentsInfo = "";
            //var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var gameObjects = FindObjectsOfType<GameObject>();
            int addedGameObjectCount = 0;
            foreach (var gameObject in gameObjects)
            {
                //if (gameObject.name == "Baba Jaga_AtHome")
                //{
                //    gameObject.SetActive(false);
                //}
                var components = gameObject.GetComponents<Component>();
                //components.Where(x => x.name == "HomeAmbience").Select;
                if (onlyCustom)
                {
                    if (!components.Any(x => IsCustomScript(x)))
                    {
                        continue;
                    }
                }
                string componentNamesString = string.Join("\n\t", components.Select(c => c.GetType().Name));
                monosAndComponentsInfo += $"{gameObject.ToString()} [[parent: {gameObject.transform.parent}, components: {components.Length}]]:\n\t{componentNamesString}";
                monosAndComponentsInfo += "\n\n\n";
                addedGameObjectCount++;
            }

            string summaryInfo = $"GameObjects: {addedGameObjectCount}\n\n\n";

            string result = summaryInfo + monosAndComponentsInfo;

            return result;
        }

        private List<string> FindAllMonos2()
        {
            Logger.LogInfo("Finding all MonoBehaviour...");

            List<string> result = new List<string>();

            foreach (var mb in GameObject.FindObjectsOfType<MonoBehaviour>())
            {
                string monoName = mb.name;
                string fullMonoName = mb.GetType().FullName;
                try
                {
                    var components = mb.GetComponents<Component>();
                    string componentNamesString = string.Join(", ", components.Select(c => c.GetType().Name));
                    string str = $"\"{monoName}\"\t\"{componentNamesString}\"";
                    result.Add(str);
                }
                catch (Exception ex)
                {
                    LogException(ex.ToString());
                }

                //string componentNamesString = string.Join(", ", components.Select(c => c.GetType().Name));

                //Logger.LogInfo($"Component!!!!!: {mb.GetType().FullName}");
            }

            return result;
        }

    }


    [HarmonyPatch]
    class Patch_Addressables_Instantiate
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("UnityEngine.AddressableAssets.Addressables");
            return AccessTools.Method(t, "InstantiateAsync", new Type[] { typeof(string) });
        }

        static void Postfix(string key, object __result)
        {
            File.WriteAllText("Log_Patch_Addressables_Instantiate.txt", key);
            if (key.Contains("BB_cat_orange_tabby_basket_1"))
            {
                Debug.Log("[Patch] Addressables.InstantiateAsync called: " + key);
            }
        }
    }

    [HarmonyPatch(typeof(GameObject), "SetActive")]
    class Patch_SetActive
    {
        static void Postfix(GameObject __instance, bool value)
        {
            File.AppendAllText("Log_Active.txt", $"{__instance.name}\n");
            return;

            if (!value) return;
            if (__instance.name.Contains("BB_cat_orange_tabby_basket_1"))
            {
                Debug.Log("[Patch] Activated: " + __instance.name);
                // модификации тут
            }
        }
    }


    public class ObjectWatcher : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(CheckForObject());
        }

        private IEnumerator CheckForObject()
        {
            bool objectFound = false;
            while (!objectFound)
            {
                //int cooldownCount = 100;
                //int cooldownCounter = 0;

                //string targetName = "_cat_orange";
                string targetName = "BB_Hare(C";
                foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (gameObject != null && !gameObject.name.Contains(targetName))
                    {
                        continue;
                    }

                    //Debug.LogWarning(gameObject.name);
                    //var components = gameObject.GetComponents<Component>();
                    //string componentNamesString = string.Join(", ", components.Select(c => c.GetType().Name));
                    //Debug.Log(componentNamesString);
                    //Debug.Log(gameObject.transform.childCount);
                    //string children = "";
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        var child = gameObject.transform.GetChild(i);
                        if (child.gameObject.name.ToLower().Contains("mesh"))
                        {
                            var comp = child.GetComponents<Component>();
                            Debug.Log(string.Join(", ", comp.Select(c => c.GetType().Name)));
                            //child.gameObject.SetActive(false);
                            if (child.GetComponents<Collider>().Length == 1)
                            {
                                BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
                                boxCollider.size = boxCollider.size * 0.5f;
                            }
                            //if (child.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
                            //{
                            //    int include = ColliderLayerOverrideUtil.GetIncludeMask(meshCollider);
                            //    int exclude = ColliderLayerOverrideUtil.GetExcludeMask(meshCollider);
                            //    int prio = ColliderLayerOverrideUtil.GetPriority(meshCollider);
                            //    Debug.LogError($"{include}, {exclude}, {prio}");
                            //    Debug.LogError($"enabled {meshCollider.enabled}");
                            //    Debug.LogError($"isTrigger {meshCollider.isTrigger}");
                            //    Debug.LogError($"Layer {meshCollider.gameObject.layer}");
                                //meshCollider.sharedMesh = MeshBox
                                //Debug.LogError($"!!!!!!!! {meshCollider.sharedMesh}");
                            //}
                        }

                        //children += $"{gameObject.transform.GetChild(i).gameObject.name}, ";
                    }
                    //Debug.Log(children);

                    yield return null;

                    //if (++cooldownCounter % cooldownCount == 0)
                    //{
                    //    yield return new WaitForSeconds(1.0f);
                    //}
                }
                //var obj = GameObject.Find(targetName);
                //if (obj != null)
                //{
                //    objectFound = true;
                //    Logger.LogInfo($"[YourMod] Found {targetName}!");
                //    OnObjectSpawned(obj);
                //}
                yield return new WaitForSeconds(5.0f);
            }
        }

    }


    public static class ColliderLayerOverrideUtil
    {
        static readonly FieldInfo includeField =
            typeof(Collider).GetField("m_IncludeLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo excludeField =
            typeof(Collider).GetField("m_ExcludeLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo priorityField =
            typeof(Collider).GetField("m_LayerOverridePriority", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetIncludeMask(Collider col)
            => (int)(includeField?.GetValue(col) ?? 0);

        public static int GetExcludeMask(Collider col)
            => (int)(excludeField?.GetValue(col) ?? 0);

        public static int GetPriority(Collider col)
            => (int)(priorityField?.GetValue(col) ?? 0);

        public static void SetIncludeMask(Collider col, int mask)
            => includeField?.SetValue(col, mask);

        public static void SetExcludeMask(Collider col, int mask)
            => excludeField?.SetValue(col, mask);

        public static void SetPriority(Collider col, int priority)
            => priorityField?.SetValue(col, priority);
    }
}