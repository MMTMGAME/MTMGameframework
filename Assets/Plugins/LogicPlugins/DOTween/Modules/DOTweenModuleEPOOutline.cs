using UnityEngine;

#if true || EPO_DOTWEEN // MODULE_MARKER

using EPOOutline;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using DG.Tweening.Core;

namespace DG.Tweening
{
    public static class DOTweenModuleEPOOutline
    {
        public static int DOKillOutLine(this SerializedPass target, bool complete)
        {
            return DOTween.Kill(target, complete);
        }

        public static TweenerCore<float, float, FloatOptions> DOFloatOutLine(this SerializedPass target, string propertyName, float endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetFloat(propertyName), x => target.SetFloat(propertyName, x), endValue, duration);
            tweener.SetOptions(true).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOFadeOutLine(this SerializedPass target, string propertyName, float endValue, float duration)
        {
            var tweener = DOTween.ToAlpha(() => target.GetColor(propertyName), x => target.SetColor(propertyName, x), endValue, duration);
            tweener.SetOptions(true).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOColorOutLine(this SerializedPass target, string propertyName, Color endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetColor(propertyName), x => target.SetColor(propertyName, x), endValue, duration);
            tweener.SetOptions(false).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Vector4, Vector4, VectorOptions> DOVectorOutLine(this SerializedPass target, string propertyName, Vector4 endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetVector(propertyName), x => target.SetVector(propertyName, x), endValue, duration);
            tweener.SetOptions(false).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<float, float, FloatOptions> DOFloatOutLine(this SerializedPass target, int propertyId, float endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetFloat(propertyId), x => target.SetFloat(propertyId, x), endValue, duration);
            tweener.SetOptions(true).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOFadeOutLine(this SerializedPass target, int propertyId, float endValue, float duration)
        {
            var tweener = DOTween.ToAlpha(() => target.GetColor(propertyId), x => target.SetColor(propertyId, x), endValue, duration);
            tweener.SetOptions(true).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOColorOutLine(this SerializedPass target, int propertyId, Color endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetColor(propertyId), x => target.SetColor(propertyId, x), endValue, duration);
            tweener.SetOptions(false).SetTarget(target);
            return tweener;
        }

        public static TweenerCore<Vector4, Vector4, VectorOptions> DOVectorOutLine(this SerializedPass target, int propertyId, Vector4 endValue, float duration)
        {
            var tweener = DOTween.To(() => target.GetVector(propertyId), x => target.SetVector(propertyId, x), endValue, duration);
            tweener.SetOptions(false).SetTarget(target);
            return tweener;
        }

        public static int DOKillOutLine(this Outlinable.OutlineProperties target, bool complete = false)
        {
            return DOTween.Kill(target, complete);
        }

        public static int DOKillOutLine(this Outliner target, bool complete = false)
        {
            return DOTween.Kill(target, complete);
        }

        /// <summary>
        /// Controls the alpha (transparency) of the outline
        /// </summary>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeOutLine(this Outlinable.OutlineProperties target, float endValue, float duration)
        {
            var tweener = DOTween.ToAlpha(() => target.Color, x => target.Color = x, endValue, duration);
            tweener.SetOptions(true).SetTarget(target);
            return tweener;
        }

        /// <summary>
        /// Controls the color of the outline
        /// </summary>
        public static TweenerCore<Color, Color, ColorOptions> DOColorOutLine(this Outlinable.OutlineProperties target, Color endValue, float duration)
        {
            var tweener = DOTween.To(() => target.Color, x => target.Color = x, endValue, duration);
            tweener.SetOptions(false).SetTarget(target);
            return tweener;
        }

        /// <summary>
        /// Controls the amount of blur applied to the outline
        /// </summary>
        public static TweenerCore<float, float, FloatOptions> DOBlurShiftOutLine(this Outlinable.OutlineProperties target, float endValue, float duration, bool snapping = false)
        {
            var tweener = DOTween.To(() => target.BlurShift, x => target.BlurShift = x, endValue, duration);
            tweener.SetOptions(snapping).SetTarget(target);
            return tweener;
        }

        /// <summary>
        /// Controls the amount of blur applied to the outline
        /// </summary>
        public static TweenerCore<float, float, FloatOptions> DOBlurShiftOutLine(this Outliner target, float endValue, float duration, bool snapping = false)
        {
            var tweener = DOTween.To(() => target.BlurShift, x => target.BlurShift = x, endValue, duration);
            tweener.SetOptions(snapping).SetTarget(target);
            return tweener;
        }

        /// <summary>
        /// Controls the amount of dilation applied to the outline
        /// </summary>
        public static TweenerCore<float, float, FloatOptions> DODilateShiftOutLine(this Outlinable.OutlineProperties target, float endValue, float duration, bool snapping = false)
        {
            var tweener = DOTween.To(() => target.DilateShift, x => target.DilateShift = x, endValue, duration);
            tweener.SetOptions(snapping).SetTarget(target);
            return tweener;
        }

        /// <summary>
        /// Controls the amount of dilation applied to the outline
        /// </summary>
        public static TweenerCore<float, float, FloatOptions> DODilateShiftOutLine(this Outliner target, float endValue, float duration, bool snapping = false)
        {
            var tweener = DOTween.To(() => target.DilateShift, x => target.DilateShift = x, endValue, duration);
            tweener.SetOptions(snapping).SetTarget(target);
            return tweener;
        }
    }
}
#endif
