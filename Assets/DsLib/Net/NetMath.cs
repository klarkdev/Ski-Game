using UnityEngine;
using System.Collections;
using System;

namespace DsLib
{
    [Serializable]
    public class Axis
    {
        public enum AxisType { XAxis, YAxis, ZAxis }
        public AxisType type;

        public Vector3 ToVector()
        {
            switch (type)
            {
                case AxisType.XAxis:
                    return Vector3.right;

                case AxisType.YAxis:
                    return Vector3.up;

                case AxisType.ZAxis:
                    return Vector3.forward;

                default:
                    return Vector3.zero;
            }
        }
        public Vector3 ToReverseVector()
        {
            switch (type)
            {
                case AxisType.XAxis:
                    return Vector3.left;
                case AxisType.YAxis:
                    return Vector3.down;
                case AxisType.ZAxis:
                    return Vector3.back;
                default:
                    return Vector3.zero;
            }
        }

        public Vector3 ChangeOnlyAxis(Vector3 input, float value)
        {
            switch (type)
            {
                case AxisType.XAxis:
                    return new Vector3(value, input.y, input.z);
                case AxisType.YAxis:
                    return new Vector3(input.x, value, input.z);
                case AxisType.ZAxis:
                    return new Vector3(input.x, input.y, value);
                default:
                    return input;
            }
        }

        public float GetAxisValue(Vector3 input)
        {
            switch (type)
            {
                case AxisType.XAxis:
                    return input.x;
                case AxisType.YAxis:
                    return input.y;
                case AxisType.ZAxis:
                    return input.z;
                default:
                    return 0;
            }
        }
    }

    public static class Math
    {
        public static float BoolToFloat(bool value)
        {
            if (value)
                return 1f;
            else
                return 0f;
        }
        public static float BoolToSign(bool value)
        {
            if (value)
                return 1f;
            else
                return -1f;
        }
        public static float BoolToInt(bool value)
        {
            if (value)
                return 1;
            else
                return 0;
        }

        public static float NormalizeFloat(float value)
        {
            if (value >= 0)
                return 1f;
            else
                return -1f;
        }

        public static Color ColorFromHSV(float h, float s, float v, float a = 1)
        {
            // no saturation, we can return the value across the board (grayscale)
            if (s == 0)
                return new Color(v, v, v, a);

            // which chunk of the rainbow are we in?
            float sector = h / 60;

            // split across the decimal (ie 3.87 into 3 and 0.87)
            int i = (int)sector;
            float f = sector - i;

            float p = v * (1 - s);
            float q = v * (1 - s * f);
            float t = v * (1 - s * (1 - f));

            // build our rgb color
            Color color = new Color(0, 0, 0, a);

            switch (i)
            {
                case 0:
                    color.r = v;
                    color.g = t;
                    color.b = p;
                    break;

                case 1:
                    color.r = q;
                    color.g = v;
                    color.b = p;
                    break;

                case 2:
                    color.r = p;
                    color.g = v;
                    color.b = t;
                    break;

                case 3:
                    color.r = p;
                    color.g = q;
                    color.b = v;
                    break;

                case 4:
                    color.r = t;
                    color.g = p;
                    color.b = v;
                    break;

                default:
                    color.r = v;
                    color.g = p;
                    color.b = q;
                    break;
            }

            return color;
        }
        public static void ColorToHSV(Color color, out float h, out float s, out float v)
        {
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float delta = max - min;

            // value is our max color
            v = max;

            // saturation is percent of max
            if (!Mathf.Approximately(max, 0))
                s = delta / max;
            else
            {
                // all colors are zero, no saturation and hue is undefined
                s = 0;
                h = -1;
                return;
            }

            // grayscale image if min and max are the same
            if (Mathf.Approximately(min, max))
            {
                v = max;
                s = 0;
                h = -1;
                return;
            }

            // hue depends which color is max (this creates a rainbow effect)
            if (color.r == max)
                h = (color.g - color.b) / delta;            // between yellow & magenta
            else if (color.g == max)
                h = 2 + (color.b - color.r) / delta;        // between cyan & yellow
            else
                h = 4 + (color.r - color.g) / delta;        // between magenta & cyan

            // turn hue into 0-360 degrees
            h *= 60;
            if (h < 0)
                h += 360;
        }

        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }
        public static float SlopeAngle(float slopeLength, float slopeHeight)
        {
            return Mathf.Rad2Deg * Mathf.Atan(slopeHeight / slopeLength);
        }
        public static float SlopeHeight(float slopeLength, float slopeAngle)
        {
            return slopeLength * Mathf.Atan(Mathf.Deg2Rad * slopeAngle);
        }

        public static float IsEven(int value)
        {
            if (value == 0)
                return 1f;

            if (value % 2 == 0)
                return 1f; //even number
            else
                return 0f; //odd number
        }
        public static float IsOdd(int value)
        {
            if (value == 0)
                return 0f;

            if (value % 2 == 0)
                return 0f; //even number
            else
                return 1f; //odd number
        }

        public static Vector3 HorizontalOverride(Vector3 source, Vector3 horizontal)
        {
            return new Vector3(horizontal.x, source.y, horizontal.z);
        }
        public static Vector3 VerticalOverride(Vector3 source, float vertical)
        {
            return new Vector3(source.x, vertical, source.z);
        }
        public static Vector3 OnlyHorizontal(Vector3 input)
        {
            return new Vector3(input.x, 0f, input.z);
        }

        #region Curves

        public static float Hermite(float start, float end, float value)
        {
            // This method will interpolate while easing in and out at the limits.
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }

        public static float HermiteNormalized(float value)
        {
            // This method will interpolate while easing in and out at the limits.
            return value * value * (3.0f - 2.0f * value);
        }

        public static float Sinerp(float start, float end, float value)
        {
            // Short for 'sinusoidal interpolation', this method will interpolate while easing around the end, when value is near one.
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

        public static float SinerpNormalized(float value)
        {
            // Short for 'sinusoidal interpolation', this method will interpolate while easing around the end, when value is near one.
            return Mathf.Sin(value * Mathf.PI * 0.5f);
        }

        public static float Coserp(float start, float end, float value)
        {
            // Similar to Sinerp, except it eases in, when value is near zero, instead of easing out (and uses cosine instead of sine).
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }

        public static float CoserpNormalized(float value)
        {
            // Similar to Sinerp, except it eases in, when value is near zero, instead of easing out (and uses cosine instead of sine).
            return 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f);
        }

        public static float Berp(float start, float end, float value)
        {
            // Short for 'boing-like interpolation', this method will first overshoot, then waver back and forth around the end value before coming to a rest.
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        public static float Bounce(float x)
        {
            // Returns a value between 0 and 1 that can be used to easily make bouncing GUI items (a la OS X's Dock)
            return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
        }

        public static float SmoothStep(float x, float min, float max)
        {
            // Works like Lerp, but has ease-in and ease-out of the values.
            x = Mathf.Clamp(x, min, max);
            float v1 = (x - min) / (max - min);
            float v2 = (x - min) / (max - min);
            return -2 * v1 * v1 * v1 + 3 * v2 * v2;
        }

        /*
         * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
         * This is useful when interpolating eulerAngles and the object
         * crosses the 0/360 boundary.  The standard Lerp function causes the object
         * to rotate in the wrong direction and looks stupid. Clerp fixes that.
         */
        public static float Clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
            float retval = 0.0f;
            float diff = 0.0f;

            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }
            else retval = start + (end - start) * value;

            // Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
            return retval;
        }
        #endregion

    }
}