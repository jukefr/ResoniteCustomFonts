using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

/// <summary>
/// Targeted bolder stack for remaining <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/> entry points
/// (see SETUP_EDITOR_STYLE.md). Texture packer / unpacker wizards build UI inside async continuations after
/// <c>OnAttach</c> returns — no stable outer sync method to patch for those two.
/// </summary>
public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedExportDialogSetupPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ExportDialog"), "Setup") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ExportDialog"), "Setup");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleExportDialogSetup);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedComponentSelectorSetupUiPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ComponentSelector"), "SetupUI") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ComponentSelector"), "SetupUI");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleComponentSelectorSetupUi);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedComponentSelectorBuildUiPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ComponentSelector"), "BuildUI") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ComponentSelector"), "BuildUI");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleComponentSelectorBuildUi);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedInspectorHelperSetupProxyVisualPatch
        {
            private static bool Prepare() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.InspectorHelper"), "SetupProxyVisual") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.InspectorHelper"), "SetupProxyVisual");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object slot, ref bool __state) =>
                TargetedBolderScopePrefixFromRootSlot(slot, ref __state, CustomFonts.StyleInspectorHelperSetupProxyVisual);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedDevCreateNewFormOpenCategoryPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.DevCreateNewForm"), "OpenCategory") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.DevCreateNewForm"), "OpenCategory");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleDevCreateNewFormOpenCategory);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedWizardFormOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WizardForm"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WizardForm"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleWizardFormOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedFolderImportDialogOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.FolderImportDialog"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.FolderImportDialog"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleFolderImportDialogOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedRecordEditFormOpenDialogWindowPatch
        {
            private static bool Prepare() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.RecordEditForm"), "OpenDialogWindow") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.RecordEditForm"), "OpenDialogWindow");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object root, ref bool __state) =>
                TargetedBolderScopePrefixFromRootSlot(root, ref __state, CustomFonts.StyleRecordEditFormOpenDialogWindow);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedHostAccessDialogOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.HostAccessDialog"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.HostAccessDialog"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleHostAccessDialogOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedHyperlinkOpenDialogOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.HyperlinkOpenDialog"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.HyperlinkOpenDialog"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleHyperlinkOpenDialogOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedBrowserCreateDirectoryDialogOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.BrowserCreateDirectoryDialog"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.BrowserCreateDirectoryDialog"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleBrowserCreateDirectoryDialogOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedNewWorldDialogOpenDialogWindowPatch
        {
            private static bool Prepare() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.NewWorldDialog"), "OpenDialogWindow") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.NewWorldDialog"), "OpenDialogWindow");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object root, ref bool __state) =>
                TargetedBolderScopePrefixFromRootSlot(root, ref __state, CustomFonts.StyleNewWorldDialogOpenDialogWindow);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedProtoFluxNodeVisualGenerateVisualPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ProtoFluxNodeVisual"), "GenerateVisual") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ProtoFluxNodeVisual"), "GenerateVisual");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleProtoFluxNodeVisualGenerateVisual);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedAssetOptimizationWizardOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.AssetOptimizationWizard"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.AssetOptimizationWizard"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleAssetOptimizationWizardOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedAvatarCreatorOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.AvatarCreator"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.AvatarCreator"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleAvatarCreatorOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedCubemapCreatorOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.CubemapCreator"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.CubemapCreator"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleCubemapCreatorOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedReflectionProbeWizardOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ReflectionProbeWizard"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ReflectionProbeWizard"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleReflectionProbeWizardOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedVhacdDialogOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.VHACD_Dialog"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.VHACD_Dialog"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleVhacdDialogOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedWorldLightSourcesWizardOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorldLightSourcesWizard"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorldLightSourcesWizard"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleWorldLightSourcesWizardOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }

        [HarmonyPatch]
        private static class TargetedWorldTextRendererWizardOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorldTextRendererWizard"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorldTextRendererWizard"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleWorldTextRendererWizardOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
