using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using System;
using UnityEngine;
using BepInEx.Logging;

namespace LCCrouchHUD
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static Sprite crouchIcon;
        public static UnityEngine.UI.Image crouchImage;

        private void Awake()
        {
            // Plugin startup logic
            Plugin.Log = Logger;
            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder codeBaseUri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(codeBaseUri.Path);
            string basePath = Path.GetDirectoryName(path);

            byte[] imageData = File.ReadAllBytes(Path.Combine(basePath, "crouch.png"));
            Texture2D crouchTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            crouchTexture.LoadImage(imageData);
            crouchTexture.filterMode = FilterMode.Point;
            crouchIcon = Sprite.Create(
                crouchTexture,
                new Rect(0, 0, crouchTexture.width, crouchTexture.height),
                new Vector2(0, 0)
            );

            _harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(ref HUDManager __instance)
        {
            GameObject hudSelfObject = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/Self");
            GameObject crouchIconObject = new GameObject("CrouchIcon");
            crouchIconObject.transform.SetParent(hudSelfObject.transform, false);
            crouchIconObject.transform.SetAsLastSibling();
            crouchIconObject.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            RectTransform rectTransform = crouchIconObject.AddComponent<RectTransform>();
            Plugin.crouchImage = crouchIconObject.AddComponent<UnityEngine.UI.Image>();
            Plugin.crouchImage.sprite = Plugin.crouchIcon;
            rectTransform.sizeDelta = new Vector2(
                20,
                20
            );
            crouchIconObject.transform.position = hudSelfObject.transform.position + new Vector3(0.14f, -0.14f, 0);
            Plugin.crouchImage.enabled = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDManager), "Update")]
        public static void Update()
        {
            if (GameNetworkManager.Instance.localPlayerController.isCrouching)
            {
                Plugin.crouchImage.enabled = true;
            }
            else
            {
                Plugin.crouchImage.enabled = false;
            }
        }
    }

}
