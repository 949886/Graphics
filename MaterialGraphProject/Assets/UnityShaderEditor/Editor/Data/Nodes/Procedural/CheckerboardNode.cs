using System.Reflection;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Title("Procedural/Checkerboard")]
    public class CheckerboardNode : CodeFunctionNode
    {
        public CheckerboardNode()
        {
            name = "Checkerboard";
        }
        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("Unity_Checkerboard", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string Unity_Checkerboard(
            [Slot(0, Binding.MeshUV0)] Vector2 UV,
            [Slot(1, Binding.None, 0.2f, 0.2f, 0.2f, 1f)] Color ColorA,
            [Slot(2, Binding.None, 0.7f, 0.7f, 0.7f, 1f)] Color ColorB,
            [Slot(3, Binding.None, 1f, 1f, 1f, 1f)] Vector2 Frequency,
            [Slot(4, Binding.None)] out Vector4 Out)
        {
            Out = Vector2.zero;
            return
                @"
{
    UV = UV + 0.25 / Frequency;
    {precision}4 derivatives = {precision}4(ddx(UV), ddy(UV));
    {precision}2 duv_length = sqrt({precision}2(dot(derivatives.xz, derivatives.xz), dot(derivatives.yw, derivatives.yw)));
    {precision} width = 0.5;
    {precision}2 distance3 = 2.0 * abs(frac((UV.xy + 0.5) * Frequency) - 0.5) - width;
    {precision}2 scale = 0.5 / duv_length.xy;
    {precision}2 blend_out = saturate(scale / 3);
    {precision}2 vector_alpha = clamp(distance3 * scale.xy * blend_out.xy, -1.0, 1.0);
    {precision} alpha = saturate(vector_alpha.x * vector_alpha.y);
    Out = lerp(ColorA, ColorB, alpha.xxxx);
}";
        }
    }
}
