using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
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

    public static ConfigEntry<ActivationMode> throwableMode;
    public static ConfigEntry<ActivationMode> salvoMode;
    public static ConfigEntry<bool> suppressSalvoModelAlways;

    private void Awake()
    {
        Logger = base.Logger;

        throwableMode = Config.Bind(
            "General",
            "ThrowableActivationMode",
            ActivationMode.Toggle,
            "Activation mode for throwables (grenades). Hold: prevent auto-activation, Toggle: toggle on/off for grenades, None: default");

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

        var harmony = new Harmony(PluginGUID);
        harmony.PatchAll(typeof(ThrowablePatches));
        harmony.PatchAll(typeof(WingsuitPatches));

        Logger.LogInfo($"{PluginName} loaded, ThrowableMode={throwableMode.Value}, SalvoMode={salvoMode.Value}, SuppressSalvoModel={suppressSalvoModelAlways.Value}");
    }
}

public static class ThrowablePatches
{
    public static bool globalThrowToggle = false;
    private static bool isAutoThrow = false;

    [HarmonyPatch(typeof(Pigeon.Movement.Player), "TryThrow")]
    [HarmonyPrefix]
    private static bool TryThrowPrefix(Pigeon.Movement.Player __instance)
    {
        var mode = SparrohPlugin.throwableMode.Value;

        if (mode == SparrohPlugin.ActivationMode.Toggle && !isAutoThrow)
        {
            globalThrowToggle = !globalThrowToggle;
            SparrohPlugin.Logger.LogInfo($"Global throw toggle {globalThrowToggle}");
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Pigeon.Movement.Player), "Update")]
    [HarmonyPostfix]
    private static void PlayerUpdatePostfix(Pigeon.Movement.Player __instance)
    {
        if (SparrohPlugin.throwableMode.Value == SparrohPlugin.ActivationMode.Hold)
        {
            var throwAction = PlayerInput.Controls.Player.Throw;
            if (throwAction.IsPressed())
            {
                var gearList = __instance.GetType().GetProperty("Gear")?.GetValue(__instance);
                var iList = gearList as System.Collections.IList;
                if (iList != null && iList.Count > 3)
                {
                    var equippedThrow = iList[3];
                    if (equippedThrow is Throwable throwable)
                    {
                        var cooldownData = (CooldownData)AccessTools.Field(typeof(Throwable), "cooldownData").GetValue(throwable);
                        if (cooldownData.IsCharged)
                        {
                            SparrohPlugin.Logger.LogInfo("Hold auto-throwing grenade!");
                            cooldownData.UseCharge();
                            isAutoThrow = true;
                            AccessTools.Method(typeof(Pigeon.Movement.Player), "TryThrow").Invoke(__instance, new object[] {});
                            isAutoThrow = false;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Throwable), "HandleCooldown")]
    [HarmonyPrefix]
    private static bool HandleCooldownPrefix(Throwable __instance, float charge)
    {
        var mode = SparrohPlugin.throwableMode.Value;

        if (mode == SparrohPlugin.ActivationMode.None)
            return true;

        var cooldownData = (CooldownData)AccessTools.Field(typeof(Throwable), "cooldownData").GetValue(__instance);
        if (!cooldownData.IsCharged)
            return true;

        var gearType = (GearType)AccessTools.Property(typeof(Throwable), "GearType").GetValue(__instance);

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
        else if (mode == SparrohPlugin.ActivationMode.Toggle && gearType == GearType.Throwable)
        {
            if (globalThrowToggle)
            {
                SparrohPlugin.Logger.LogInfo("Auto-throwing grenade!");
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
}

[HarmonyPatch(typeof(Wingsuit))]
public static class WingsuitPatches
{
    private static bool salvoAutoEnabled = false;
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
            SparrohPlugin.Logger.LogInfo($"Salvo auto-toggle {salvoAutoEnabled}");
            return false;
        }

        return true;
    }

    [HarmonyPatch("OnSalvoPressed")]
    [HarmonyPostfix]
    private static void OnSalvoPressedPostfix(Wingsuit __instance)
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

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePostfix(Wingsuit __instance)
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

        SparrohPlugin.Logger.LogInfo("Salvo auto-macro executed");
    }
}

[HarmonyPatch(typeof(HUD))]
public static class HUDPatches
{
    [HarmonyPatch("Enable")]
    [HarmonyPrefix]
    private static bool EnablePrefix(HUD __instance)
    {
        if (WingsuitPatches.suppressSalvoHUD)
            return false;

        return true;
    }
}
