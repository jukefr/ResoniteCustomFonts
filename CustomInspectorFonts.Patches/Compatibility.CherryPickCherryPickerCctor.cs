using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Elements.Core;
using HarmonyLib;

namespace CustomInspectorFonts;

/// <summary>
/// CherryPick: <c>CherryPicker</c> static ctor walks <see cref="F:FrooxEngine.WorkerInitializer.ComponentLibrary"/> via a lazy
/// <c>flatten(...)</c> <see cref="IEnumerable{T}"/>; concurrent library updates cause
/// <see cref="InvalidOperationException"/> (“Collection was modified”) when opening Attach Component.
/// Materializing that sequence with <see cref="Enumerable.ToList{TSource}"/> after each flatten call fixes it.
/// </summary>
public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        /// <summary>Harmony may skip the class on the first <see cref="Harmony.PatchAll"/> pass; retry is idempotent once patched.</summary>
        internal static void RetryCherryPickStaticCtorTranspilerPatch(Harmony harmony)
        {
            if (!CustomInspectorFonts.PatchSiteEnabled(CustomInspectorFonts.StyleCherryPickCherryPickerCctorFix))
                return;
            try
            {
                harmony.CreateClassProcessor(typeof(CherryPickCherryPickerStaticCtorTranspiler)).Patch();
            }
            catch (Exception ex)
            {
                UniLog.Log("[CustomInspectorFonts] CherryPick transpiler retry: " + ex.Message, false);
            }
        }

        [HarmonyPatch]
        private static class CherryPickCherryPickerStaticCtorTranspiler
        {
            private static bool Prepare()
            {
                if (!CustomInspectorFonts.PatchSiteEnabled(CustomInspectorFonts.StyleCherryPickCherryPickerCctorFix))
                    return false;
                var cherry = AccessTools.TypeByName("CherryPick.CherryPicker");
                if (cherry == null)
                    return false;
                var flatten = ResolveFlattenMethod(cherry);
                return flatten != null && ResolveToListForFlatten(flatten) != null;
            }

            [HarmonyTargetMethod]
            private static MethodBase? TargetMethod()
            {
                var t = AccessTools.TypeByName("CherryPick.CherryPicker");
                if (t == null)
                    return null;
                foreach (var c in t.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    if (c.GetParameters().Length == 0)
                        return c;
                }

                return null;
            }

            private static bool IsEnumerableOf(Type t, out Type element)
            {
                element = null!;
                if (!t.IsGenericType)
                    return false;
                if (t.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                    return false;
                element = t.GetGenericArguments()[0];
                return true;
            }

            private static bool IsCategoryNodeTree(Type categoryNodeOpenDef, Type elementType)
            {
                if (!elementType.IsGenericType)
                    return false;
                return elementType.GetGenericTypeDefinition() == categoryNodeOpenDef;
            }

            /// <summary>Compiler name for the static local <c>flatten</c> varies; match signature: static, same IEnumerable{T} in/out, T is CategoryNode&lt;…&gt;.</summary>
            private static MethodInfo? ResolveFlattenMethod(Type cherryPickerType)
            {
                var catOpen = AccessTools.TypeByName("FrooxEngine.CategoryNode`1")?.GetGenericTypeDefinition();
                if (catOpen == null)
                    return null;
                const BindingFlags bf = BindingFlags.Static | BindingFlags.NonPublic;
                foreach (var m in cherryPickerType.GetMethods(bf))
                {
                    if (m.DeclaringType != cherryPickerType)
                        continue;
                    if (!m.Name.StartsWith("<.cctor>g__flatten", StringComparison.Ordinal))
                        continue;
                    var ps = m.GetParameters();
                    if (ps.Length != 1)
                        continue;
                    if (!IsEnumerableOf(ps[0].ParameterType, out var argElem))
                        continue;
                    if (!IsEnumerableOf(m.ReturnType, out var retElem) || retElem != argElem)
                        continue;
                    if (!IsCategoryNodeTree(catOpen, argElem))
                        continue;
                    return m;
                }

                foreach (var m in cherryPickerType.GetMethods(bf))
                {
                    if (m.DeclaringType != cherryPickerType)
                        continue;
                    if (m.Name.IndexOf("flatten", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    var ps = m.GetParameters();
                    if (ps.Length != 1)
                        continue;
                    if (!IsEnumerableOf(ps[0].ParameterType, out var argElem))
                        continue;
                    if (!IsEnumerableOf(m.ReturnType, out var retElem) || retElem != argElem)
                        continue;
                    if (!IsCategoryNodeTree(catOpen, argElem))
                        continue;
                    return m;
                }

                var cands = new List<MethodInfo>();
                foreach (var m in cherryPickerType.GetMethods(bf))
                {
                    if (m.DeclaringType != cherryPickerType)
                        continue;
                    var ps = m.GetParameters();
                    if (ps.Length != 1)
                        continue;
                    if (!IsEnumerableOf(ps[0].ParameterType, out var argElem))
                        continue;
                    if (!IsEnumerableOf(m.ReturnType, out var retElem) || retElem != argElem)
                        continue;
                    if (!IsCategoryNodeTree(catOpen, argElem))
                        continue;
                    cands.Add(m);
                }

                if (cands.Count == 0)
                    return null;
                if (cands.Count == 1)
                    return cands[0];
                return cands.FirstOrDefault(m => m.Name.IndexOf("flatten", StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? cands[0];
            }

            private static MethodInfo? ResolveToListForFlatten(MethodInfo flatten)
            {
                var p0 = flatten.GetParameters()[0].ParameterType;
                if (!IsEnumerableOf(p0, out var elem))
                    return null;
                return typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m =>
                        m.Name == nameof(Enumerable.ToList)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    ?.MakeGenericMethod(elem);
            }

            private static bool IsFlattenCall(CodeInstruction ins, MethodInfo expectedFlatten)
            {
                if (ins.opcode != OpCodes.Call || ins.operand is not MethodInfo m)
                    return false;
                return m.MetadataToken == expectedFlatten.MetadataToken
                    && m.DeclaringType == expectedFlatten.DeclaringType;
            }

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var cherry = AccessTools.TypeByName("CherryPick.CherryPicker");
                var flatten = cherry == null ? null : ResolveFlattenMethod(cherry);
                var toList = flatten == null ? null : ResolveToListForFlatten(flatten);
                if (flatten == null || toList == null)
                {
                    foreach (var i in instructions)
                        yield return i;
                    yield break;
                }

                foreach (var ins in instructions)
                {
                    yield return ins;
                    if (IsFlattenCall(ins, flatten))
                        yield return new CodeInstruction(OpCodes.Call, toList);
                }
            }
        }
    }
}
