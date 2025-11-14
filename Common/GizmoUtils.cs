using UnityEngine;

namespace NoSlimes.Utils
{
    public static class GizmoUtils
    {
        /// <summary>
        /// Draws a circle using Debug.DrawLine.
        /// </summary>
        public static void DebugDrawCircle(Vector3 center, float radius, Color color, float duration, int divisions)
        {
            float angleStep = 360f / divisions;
            for (int i = 0; i < divisions; i++)
            {
                float a1 = Mathf.Deg2Rad * angleStep * i;
                float a2 = Mathf.Deg2Rad * angleStep * (i + 1);

                Vector3 vec1 = center + new Vector3(Mathf.Sin(a1), Mathf.Cos(a1)) * radius;
                Vector3 vec2 = center + new Vector3(Mathf.Sin(a2), Mathf.Cos(a2)) * radius;

                Debug.DrawLine(vec1, vec2, color, duration);
            }
        }

        /// <summary>
        /// Draws a rectangle using Debug.DrawLine.
        /// </summary>
        public static void DebugDrawRectangle(Vector3 minXY, Vector3 maxXY, Color color, float duration)
        {
            Debug.DrawLine(new Vector3(minXY.x, minXY.y), new Vector3(maxXY.x, minXY.y), color, duration);
            Debug.DrawLine(new Vector3(minXY.x, minXY.y), new Vector3(minXY.x, maxXY.y), color, duration);
            Debug.DrawLine(new Vector3(minXY.x, maxXY.y), new Vector3(maxXY.x, maxXY.y), color, duration);
            Debug.DrawLine(new Vector3(maxXY.x, minXY.y), new Vector3(maxXY.x, maxXY.y), color, duration);
        }

        /// <summary>
        /// Draws text using Debug.DrawLine with inline character definitions.
        /// </summary>
        public static void DebugDrawText(string text, Vector3 position, Color color, float size, float duration, Quaternion rotation = default)
        {
            text = text.ToUpper();
            float kerning = size * 0.6f;
            Vector3 basePos = position;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '\n':
                        position = basePos + rotation * new Vector3(0, size * (i + 1), 0); // or adjust as needed
                        break;
                    case ' ':
                        position += rotation * new Vector3(kerning, 0, 0);

                        break;
                    default:
                        DebugDrawChar(c, position, color, size, duration, rotation);
                        position += rotation * new Vector3(kerning, 0, 0);
                        break;
                }
            }
        }

        /// <summary>
        /// Draws a character using inline float arrays for each glyph.
        /// </summary>
        public static void DebugDrawChar(char c, Vector3 position, Color color, float size, float duration, Quaternion rotation = default)
        {
            c = char.ToUpper(c);

            switch (c)
            {
                default:
                case 'A':
                    DebugDrawLines(position, color, size, duration, new[] {
                         0.317f,0.041f, 0.5f,0.98f, 0.749f,0.062f, 0.625f,0.501f, 0.408f,0.507f }, rotation); break;
                case 'B':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.289f,0.069f, 0.274f,0.937f, 0.609f,0.937f, 0.801f,0.879f, 0.829f,0.708f, 0.756f,0.538f, 0.655f,0.492f, 0.442f,0.495f, 0.271f,0.495f, 0.567f,0.474f, 0.676f,0.465f, 0.722f,0.385f, 0.719f,0.181f, 0.664f,0.087f, 0.527f,0.053f, 0.396f,0.05f, 0.271f,0.078f }, rotation); break;
                case 'C':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.695f,0.946f, 0.561f,0.949f, 0.426f,0.937f, 0.317f,0.867f, 0.265f,0.733f, 0.262f,0.553f, 0.292f,0.27f, 0.323f,0.172f, 0.417f,0.12f, 0.512f,0.096f, 0.637f,0.093f, 0.743f,0.117f, }, rotation); break;
                case 'D':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.314f,0.909f, 0.329f,0.096f, 0.53f,0.123f, 0.594f,0.197f, 0.673f,0.334f, 0.716f,0.498f, 0.692f,0.666f, 0.609f,0.806f, 0.457f,0.891f, 0.323f,0.919f }, rotation); break;
                case 'E':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.344f,0.919f, 0.363f,0.078f, 0.713f,0.096f, 0.359f,0.096f, 0.347f,0.48f, 0.53f,0.492f, 0.356f,0.489f, 0.338f,0.913f, 0.625f,0.919f }, rotation); break;
                case 'F':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.682f,0.916f, 0.329f,0.909f, 0.341f,0.66f, 0.503f,0.669f, 0.341f,0.669f, 0.317f,0.087f }, rotation); break;
                case 'G':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.618f,0.867f, 0.399f,0.849f, 0.292f,0.654f, 0.241f,0.404f, 0.253f,0.178f, 0.481f,0.075f, 0.612f,0.078f, 0.725f,0.169f, 0.728f,0.334f, 0.71f,0.437f, 0.609f,0.462f, 0.463f,0.462f }, rotation); break;
                case 'H':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.277f,0.876f, 0.305f,0.133f, 0.295f,0.507f, 0.628f,0.501f, 0.643f,0.139f, 0.637f,0.873f }, rotation); break;
                case 'I':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.487f,0.906f, 0.484f,0.096f }, rotation); break;
                case 'J':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.628f,0.882f, 0.679f,0.242f, 0.603f,0.114f, 0.445f,0.066f, 0.317f,0.114f, 0.262f,0.209f, 0.253f,0.3f, 0.259f,0.367f }, rotation); break;
                case 'K':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.292f,0.879f, 0.311f,0.111f, 0.305f,0.498f, 0.594f,0.876f, 0.305f,0.516f, 0.573f,0.154f,  }, rotation); break;
                case 'L':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.311f,0.879f, 0.308f,0.133f, 0.682f,0.148f,  }, rotation); break;
                case 'M':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.262f,0.12f, 0.265f,0.909f, 0.509f,0.608f, 0.71f,0.919f, 0.713f,0.151f,  }, rotation); break;
                case 'N':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.737f,0.885f, 0.679f,0.114f, 0.335f,0.845f, 0.353f,0.175f,  }, rotation); break;
                case 'O':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.609f,0.906f, 0.396f,0.894f, 0.271f,0.687f, 0.232f,0.474f, 0.241f,0.282f, 0.356f,0.142f, 0.527f,0.087f, 0.655f,0.09f, 0.719f,0.181f, 0.737f,0.379f, 0.737f,0.638f, 0.71f,0.836f, 0.628f,0.919f, 0.582f,0.919f, }, rotation); break;
                case 'P':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.314f,0.129f, 0.311f,0.873f, 0.658f,0.906f, 0.746f,0.8f, 0.746f,0.66f, 0.673f,0.544f, 0.509f,0.51f, 0.359f,0.51f, 0.311f,0.516f,  }, rotation); break;
                case 'Q':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.497f,0.925f, 0.335f,0.894f, 0.228f,0.681f, 0.213f,0.379f, 0.25f,0.145f, 0.396f,0.096f, 0.573f,0.105f, 0.631f,0.166f, 0.542f,0.245f, 0.752f,0.108f, 0.628f,0.187f, 0.685f,0.261f, 0.728f,0.398f, 0.759f,0.605f, 0.722f,0.794f, 0.64f,0.916f, 0.475f,0.946f,  }, rotation); break;
                case 'R':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.347f,0.142f, 0.332f,0.9f, 0.667f,0.897f, 0.698f,0.699f, 0.655f,0.58f, 0.521f,0.553f, 0.396f,0.553f, 0.344f,0.553f, 0.564f,0.37f, 0.655f,0.206f, 0.71f,0.169f }, rotation); break;
                case 'S':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.695f,0.842f, 0.576f,0.882f, 0.439f,0.885f, 0.329f,0.8f, 0.289f,0.626f, 0.317f,0.489f, 0.439f,0.44f, 0.621f,0.434f, 0.695f,0.358f, 0.713f,0.224f, 0.646f,0.111f, 0.494f,0.093f, 0.338f,0.105f, 0.289f,0.151f, }, rotation); break;
                case 'T':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.497f,0.172f, 0.5f,0.864f, 0.286f,0.858f, 0.719f,0.852f,  }, rotation); break;
                case 'U':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.232f,0.858f, 0.247f,0.251f, 0.366f,0.105f, 0.466f,0.078f, 0.615f,0.084f, 0.704f,0.123f, 0.746f,0.276f, 0.74f,0.559f, 0.737f,0.806f, 0.722f,0.864f, }, rotation); break;
                case 'V':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.238f,0.855f, 0.494f,0.105f, 0.707f,0.855f,  }, rotation); break;
                case 'W':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.228f,0.815f, 0.381f,0.093f, 0.503f,0.434f, 0.615f,0.151f, 0.722f,0.818f,  }, rotation); break;
                case 'X':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.783f,0.852f, 0.256f,0.133f, 0.503f,0.498f, 0.305f,0.824f, 0.789f,0.117f,  }, rotation); break;
                case 'Y':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.299f,0.842f, 0.497f,0.529f, 0.646f,0.842f, 0.49f,0.541f, 0.487f,0.105f, }, rotation); break;
                case 'Z':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.25f,0.87f, 0.795f,0.842f, 0.274f,0.133f, 0.716f,0.142f }, rotation); break;

                case '0':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.536f,0.891f, 0.509f,0.891f, 0.42f,0.809f, 0.378f,0.523f, 0.372f,0.215f, 0.448f,0.087f, 0.539f,0.069f, 0.609f,0.099f, 0.637f,0.242f, 0.646f,0.416f, 0.646f,0.608f, 0.631f,0.809f, 0.554f,0.888f, 0.527f,0.894f,  }, rotation); break;
                case '1':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.652f,0.108f, 0.341f,0.114f, 0.497f,0.12f, 0.497f,0.855f, 0.378f,0.623f,  }, rotation); break;
                case '2':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.311f,0.714f, 0.375f,0.83f, 0.564f,0.894f, 0.722f,0.839f, 0.765f,0.681f, 0.634f,0.483f, 0.5f,0.331f, 0.366f,0.245f, 0.299f,0.126f, 0.426f,0.126f, 0.621f,0.136f, 0.679f,0.136f, 0.737f,0.139f,  }, rotation); break;
                case '3':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.289f,0.855f, 0.454f,0.876f, 0.606f,0.818f, 0.685f,0.702f, 0.664f,0.547f, 0.564f,0.459f, 0.484f,0.449f, 0.417f,0.455f, 0.53f,0.434f, 0.655f,0.355f, 0.664f,0.233f, 0.591f,0.105f, 0.466f,0.075f, 0.335f,0.084f, 0.259f,0.142f,  }, rotation); break;
                case '4':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.353f,0.836f, 0.262f,0.349f, 0.579f,0.367f, 0.5f,0.376f, 0.49f,0.471f, 0.509f,0.069f,  }, rotation); break;
                case '5':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.67f,0.852f, 0.335f,0.858f, 0.347f,0.596f, 0.582f,0.602f, 0.698f,0.513f, 0.749f,0.343f, 0.719f,0.187f, 0.561f,0.133f, 0.363f,0.151f,  }, rotation); break;
                case '6':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.567f,0.888f, 0.442f,0.782f, 0.35f,0.544f, 0.326f,0.288f, 0.39f,0.157f, 0.615f,0.142f, 0.679f,0.245f, 0.676f,0.37f, 0.573f,0.48f, 0.454f,0.48f, 0.378f,0.41f, 0.335f,0.367f,  }, rotation); break;
                case '7':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.286f,0.852f, 0.731f,0.864f, 0.417f,0.117f, 0.57f,0.498f, 0.451f,0.483f, 0.688f,0.501f, }, rotation); break;
                case '8':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.518f,0.541f, 0.603f,0.623f, 0.649f,0.748f, 0.612f,0.858f, 0.497f,0.888f, 0.375f,0.824f, 0.341f,0.708f, 0.381f,0.611f, 0.494f,0.55f, 0.557f,0.513f, 0.6f,0.416f, 0.631f,0.312f, 0.579f,0.178f, 0.509f,0.108f, 0.436f,0.102f, 0.335f,0.181f, 0.308f,0.279f, 0.347f,0.401f, 0.423f,0.486f, 0.497f,0.547f,  }, rotation); break;
                case '9':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.475f,0.129f, 0.573f,0.495f, 0.646f,0.824f, 0.509f,0.97f, 0.28f,0.94f, 0.189f,0.827f, 0.262f,0.708f, 0.396f,0.69f, 0.564f,0.745f, 0.646f,0.83f,  }, rotation); break;


                case '.':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.515f,0.157f, 0.469f,0.148f, 0.469f,0.117f, 0.515f,0.123f, 0.503f,0.169f }, rotation); break;
                case ':':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.515f,0.157f, 0.469f,0.148f, 0.469f,0.117f, 0.515f,0.123f, 0.503f,0.169f }, rotation);
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.515f,.5f+0.157f, 0.469f,.5f+0.148f, 0.469f,.5f+0.117f, 0.515f,.5f+0.123f, 0.503f,.5f+0.169f }, rotation);
                    break;
                case '-':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.277f,0.51f, 0.716f,0.51f,  }, rotation); break;
                case '+':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.265f,0.513f, 0.676f,0.516f, 0.497f,0.529f, 0.49f,0.699f, 0.497f,0.27f,  }, rotation); break;

                case '(':
                    DebugDrawLines(position, color, size, duration, new[] {
                         0.542f,0.934f, 0.411f,0.797f, 0.344f,0.587f, 0.341f,0.434f, 0.375f,0.257f, 0.457f,0.12f, 0.567f,0.075f, }, rotation); break;
                case ')':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.442f,0.94f, 0.548f,0.757f, 0.625f,0.568f, 0.64f,0.392f, 0.554f,0.129f, 0.472f,0.056f,  }, rotation); break;
                case ';':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.515f,0.157f, 0.469f,0.148f, 0.469f,0.117f, 0.515f,0.123f, 0.503f,0.169f, }, rotation); break;
                case ',':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.533f,0.239f, 0.527f,0.154f, 0.487f,0.099f, 0.451f,0.062f,  }, rotation); break;
                case '_':
                    DebugDrawLines(position, color, size, duration, new[] {
                        0.274f,0.133f, 0.716f,0.142f }, rotation); break;

                    /*
             case ':': DebugDrawLines(position, color, size, duration, new [] {
                    0f }); break;
                    */
            }
        }

        /// <summary>
        /// Draws lines from an array of floats with rotation applied.
        /// </summary>
        public static void DebugDrawLines(Vector3 position, Color color, float size, float duration, float[] points, Quaternion rotation)
        {
            if (rotation == default) rotation = Quaternion.identity;

            // Stop 2 elements before the last pair
            for (int i = 0; i < points.Length - 3; i += 2)
            {
                Vector3 p1 = rotation * new Vector3(points[i], points[i + 1], 0);
                Vector3 p2 = rotation * new Vector3(points[i + 2], points[i + 3], 0);
                Debug.DrawLine(position + p1 * size, position + p2 * size, color, duration);
            }
        }

    }
}