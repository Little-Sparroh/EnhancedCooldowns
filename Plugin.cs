using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine.InputSystem;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.enhancedcooldowns";
    public const string PluginName = "EnhancedCooldowns";
    public const string PluginVersion = "1.0.0";

    internal static new ManualLogSource Logger;

    public enum ActivationMode
    {
        None,
        Hold,
        Toggle
    }

    public static ConfigEntry<ActivationMode> incendiaryMode;
    public static ConfigEntry<ActivationMode> voltaicMode;
    public static ConfigEntry<ActivationMode> acidMode;
    public static ConfigEntry<ActivationMode> salvoMode;
    public static ConfigEntry<bool> suppressSalvoModelAlways;

    private void Awake()
    {
        Logger = base.Logger;

        incendiaryMode = Config.Bind(
            "General",
            "IncendiaryGrenadeActivationMode",
            ActivationMode.Toggle,
            "Activation mode for incendiary grenades. Hold: prevent auto-activation, Toggle: toggle on/off, None: default");

        voltaicMode = Config.Bind(
            "General",
            "VoltaicGrenadeActivationMode",
            ActivationMode.Toggle,
            "Activation mode for voltaic (shock) grenades. Hold: prevent auto-activation, Toggle: toggle on/off, None: default");

        acidMode = Config.Bind(
            "General",
            "AcidGrenadeActivationMode",
            ActivationMode.Toggle,
            "Activation mode for acid grenades. Hold: prevent auto-activation, Toggle: toggle on/off, None: default");

        salvoMode = Config.Bind(
            "General",
            "SalvoActivationMode",
            ActivationMode.Toggle,
            "Activation mode for glider salvo. Toggle: tap to toggle on/off, then auto-fires upon recharge, None: default behavior");

        suppressSalvoModelAlways = Config.Bind(
            "General",
            "SuppressSalvoModelAlways",
            false,
            "Always hide the 3D salvo launcher model to save screen space. Manual salvo will not show targeting model when enabled.");

        try
        {
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll(typeof(ThrowablePatches));
            harmony.PatchAll(typeof(WingsuitPatches));

            Logger.LogInfo($"{PluginName} loaded");
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to patch methods: " + ex.Message);
        }

        try
        {
            string configPath = Config.ConfigFilePath;
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(configPath), Path.GetFileName(configPath));
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnConfigFileChanged;
            watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to set up config file watcher: " + ex.Message);
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            System.Threading.Thread.Sleep(100);
            Config.Reload();
            ResetToggles();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to reload config: " + ex.Message);
        }
    }

    private void ResetToggles()
    {
        ThrowablePatches.incendiaryToggle = false;
        ThrowablePatches.voltaicToggle = false;
        ThrowablePatches.acidToggle = false;
        WingsuitPatches.salvoAutoEnabled = false;
    }
}

public static class ThrowablePatches
{
    public static bool incendiaryToggle = false;
    public static bool voltaicToggle = false;
    public static bool acidToggle = false;
    private static bool isAutoThrow = false;

    [HarmonyPatch(typeof(Pigeon.Movement.Player), "TryThrow")]
    [HarmonyPrefix]
    private static bool TryThrowPrefix(Pigeon.Movement.Player __instance)
    {
        try
        {
            if (isAutoThrow)
                return true;

            var gearList = __instance.GetType().GetProperty("Gear")?.GetValue(__instance);
            var iList = gearList as System.Collections.IList;
            if (iList != null && iList.Count > 3)
            {
                var equippedThrow = iList[3];
                if (equippedThrow != null)
                {
                    SparrohPlugin.ActivationMode mode;
                    if (equippedThrow.GetType().Name == "IncendiaryGrenade")
                    {
                        mode = SparrohPlugin.incendiaryMode.Value;
                        if (mode == SparrohPlugin.ActivationMode.Toggle)
                        {
                            incendiaryToggle = !incendiaryToggle;
                            return false;
                        }
                        else if (mode == SparrohPlugin.ActivationMode.Hold)
                        {
                            return false;
                        }
                    }
                    else if (equippedThrow.GetType().Name == "VoltaicGrenade")
                    {
                        mode = SparrohPlugin.voltaicMode.Value;
                        if (mode == SparrohPlugin.ActivationMode.Toggle)
                        {
                            voltaicToggle = !voltaicToggle;
                            return false;
                        }
                        else if (mode == SparrohPlugin.ActivationMode.Hold)
                        {
                            return false;
                        }
                    }
                    else if (equippedThrow.GetType().Name == "AcidGrenade")
                    {
                        mode = SparrohPlugin.acidMode.Value;
                        if (mode == SparrohPlugin.ActivationMode.Toggle)
                        {
                            acidToggle = !acidToggle;
                            return false;
                        }
                        else if (mode == SparrohPlugin.ActivationMode.Hold)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in TryThrowPrefix: " + ex.Message);
            return true;
        }
    }

    [HarmonyPatch(typeof(Pigeon.Movement.Player), "Update")]
    [HarmonyPostfix]
    private static void PlayerUpdatePostfix(Pigeon.Movement.Player __instance)
    {
        try
        {
            var throwAction = PlayerInput.Controls.Player.Throw;
            if (!throwAction.IsPressed())
                return;

            var gearList = __instance.GetType().GetProperty("Gear")?.GetValue(__instance);
            var iList = gearList as System.Collections.IList;
            if (iList == null || iList.Count <= 3)
                return;

            var equippedThrow = iList[3];
            if (equippedThrow == null)
                return;

            SparrohPlugin.ActivationMode mode;
            if (equippedThrow.GetType().Name == "IncendiaryGrenade")
            {
                mode = SparrohPlugin.incendiaryMode.Value;
            }
            else if (equippedThrow.GetType().Name == "VoltaicGrenade")
            {
                mode = SparrohPlugin.voltaicMode.Value;
            }
            else if (equippedThrow.GetType().Name == "AcidGrenade")
            {
                mode = SparrohPlugin.acidMode.Value;
            }
            else
            {
                return;
            }

            if (mode == SparrohPlugin.ActivationMode.Hold)
            {
                if (equippedThrow is Throwable throwable)
                {
                    var cooldownData = (CooldownData)AccessTools.Field(typeof(Throwable), "cooldownData").GetValue(throwable);
                    if (cooldownData.IsCharged)
                    {
                        cooldownData.UseCharge();
                        isAutoThrow = true;
                        AccessTools.Method(typeof(Pigeon.Movement.Player), "TryThrow").Invoke(__instance, new object[] {});
                        isAutoThrow = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in PlayerUpdatePostfix: " + ex.Message);
        }
    }

    [HarmonyPatch(typeof(Throwable), "HandleCooldown")]
    [HarmonyPrefix]
    private static bool HandleCooldownPrefix(Throwable __instance, float charge)
    {
        try
        {
            SparrohPlugin.ActivationMode mode;
            bool toggleState;

            var typeName = __instance.GetType().Name;
            if (typeName == "IncendiaryGrenade")
            {
                mode = SparrohPlugin.incendiaryMode.Value;
                toggleState = incendiaryToggle;
            }
            else if (typeName == "VoltaicGrenade")
            {
                mode = SparrohPlugin.voltaicMode.Value;
                toggleState = voltaicToggle;
            }
            else if (typeName == "AcidGrenade")
            {
                mode = SparrohPlugin.acidMode.Value;
                toggleState = acidToggle;
            }
            else
            {
                return true;
            }

            if (mode == SparrohPlugin.ActivationMode.None)
                return true;

            var cooldownData = (CooldownData)AccessTools.Field(typeof(Throwable), "cooldownData").GetValue(__instance);
            if (!cooldownData.IsCharged)
                return true;

            if (mode == SparrohPlugin.ActivationMode.Hold)
            {
                var abilityInput = (InputAction)AccessTools.Property(typeof(Throwable), "AbilityInputAction").GetValue(__instance);
                if (abilityInput.WasPressedThisFrame() || abilityInput.IsPressed())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (mode == SparrohPlugin.ActivationMode.Toggle)
            {
                if (toggleState)
                {
                    cooldownData.UseCharge();
                    isAutoThrow = true;

                    var playerField = typeof(Throwable).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (playerField != null)
                    {
                        var player = playerField.GetValue(__instance);
                        if (player != null)
                        {
                            var gearList = player.GetType().GetProperty("Gear")?.GetValue(player);
                            var iList = gearList as System.Collections.IList;
                            if (iList != null && iList.Count > 3)
                            {
                                var equippedThrow = iList[3];
                                if (__instance == equippedThrow)
                                {
                                    AccessTools.Method(typeof(Pigeon.Movement.Player), "TryThrow").Invoke(player, new object[] {});
                                }
                            }
                        }
                    }
                    isAutoThrow = false;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in HandleCooldownPrefix: " + ex.Message);
            return true;
        }
    }
}

[HarmonyPatch(typeof(Wingsuit))]
public static class WingsuitPatches
{
    public static bool salvoAutoEnabled = false;
    private static bool isAutoPress = false;
    private static float lastAutoFireTime = -1f;
    private static float lastToggleTime = -1f;
    public static bool suppressSalvoHUD = false;

    [HarmonyPatch("OnSalvoPressed")]
    [HarmonyPrefix]
    private static bool OnSalvoPressedPrefix(Wingsuit __instance, InputAction.CallbackContext context)
    {
        var mode = SparrohPlugin.salvoMode.Value;

        var time = UnityEngine.Time.time;
        if (mode == SparrohPlugin.ActivationMode.Toggle && !isAutoPress &&
            time - lastAutoFireTime >= 0.1f && time - lastToggleTime >= 0.05f)
        {
            salvoAutoEnabled = !salvoAutoEnabled;
            lastToggleTime = time;
            return false;
        }

        return true;
    }

    [HarmonyPatch("OnSalvoPressed")]
    [HarmonyPostfix]
    private static void OnSalvoPressedPostfix(Wingsuit __instance)
    {
        try
        {
            if (SparrohPlugin.suppressSalvoModelAlways.Value)
            {
                var salvoModel = AccessTools.Field(typeof(Wingsuit), "salvoModel").GetValue(__instance) as UnityEngine.Transform;
                if (salvoModel != null)
                {
                    salvoModel.gameObject.SetActive(false);
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in OnSalvoPressedPostfix: " + ex.Message);
        }
    }

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePostfix(Wingsuit __instance)
    {
        try
        {
            var mode = SparrohPlugin.salvoMode.Value;
            if (mode != SparrohPlugin.ActivationMode.Toggle || !salvoAutoEnabled)
                return;

            var rocketSalvoCooldown = (Cooldown)AccessTools.Field(typeof(Wingsuit), "rocketSalvoCooldown").GetValue(__instance);
            if (rocketSalvoCooldown == null || !rocketSalvoCooldown.data.IsCharged)
                return;

            suppressSalvoHUD = true;
            isAutoPress = true;
            AccessTools.Method(typeof(Wingsuit), "OnSalvoPressed").Invoke(__instance, new object[] { default(InputAction.CallbackContext) });
            AccessTools.Method(typeof(Wingsuit), "OnSalvoReleased").Invoke(__instance, new object[] { default(InputAction.CallbackContext) });
            isAutoPress = false;
            suppressSalvoHUD = false;

            lastAutoFireTime = UnityEngine.Time.time;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in UpdatePostfix: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(HUD))]
public static class HUDPatches
{
    [HarmonyPatch("Enable")]
    [HarmonyPrefix]
    private static bool EnablePrefix(HUD __instance)
    {
        try
        {
            if (WingsuitPatches.suppressSalvoHUD)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError("Error in EnablePrefix: " + ex.Message);
            return true;
        }
    }
}
