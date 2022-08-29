Shader "INK/Ink_SDR"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        CGINCLUDE
#include "UnityCG.cginc"

        sampler2D _MainTex, _CameraDepthTexture;
    sampler2D _PaperTex;
    sampler2D _NoiseTex;
    sampler2D _StippleTex;
    sampler2D _LuminanceTex;
    sampler2D _InkTex;
    float4 _NoiseTex_TexelSize;
    float4 _MainTex_TexelSize;
    float _ContrastThreshold;
    float _HighThreshold;
    float _LowThreshold;
    float _LuminanceCorrection;
    float _Contrast;
    float _StippleSize;
    uint _UsingImage;

    struct VertexData
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float4 screenPosition : TEXCOORD1;
    };

    v2f vert(VertexData v)
    {
        v2f f;
        f.vertex = UnityObjectToClipPos(v.vertex);
        f.uv = v.uv;
        f.screenPosition = ComputeScreenPos(f.vertex);

        return f;
    }
    ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                return LinearRgbToLuminance(tex2D(_MainTex, i.uv));
            }
            ENDCG
        }

        //edge detection by contrast
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half SampleLuminance(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            half SampleLuminance(float2 uv, float uOffset, float vOffset) {
                uv += _MainTex_TexelSize * float2(uOffset, vOffset);
                return SampleLuminance(uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half m = SampleLuminance(i.uv);
                half n = SampleLuminance(i.uv, 0, 1);
                half e = SampleLuminance(i.uv, 1, 0);
                half s = SampleLuminance(i.uv, 0, -1);
                half w = SampleLuminance(i.uv, -1, 0);
                half highest = max(max(max(max(n, e), s), w), m);
                half lowest = min(min(min(min(n, e), s), w), m);
                half contrast = highest - lowest;

                return contrast;
            }
            ENDCG
        }
        //edge detection by sobel feldman operator
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                int x, y;

                int3x3 Kx = {
                1, 0, -1,
                2, 0, -1,
                1, 0, -1
                };
                int3x3 Ky = {
                    1, 2, 1,
                    0, 0, 0,
                    -1, -2, -1
                };

                float Gx = 0.0f;
                float Gy = 0.0f;

                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        float2 uv = i.uv + _MainTex_TexelSize * float2(x, y);

                        half c = tex2D(_MainTex, uv).a;
                        Gx += Kx[x + 1][y + 1] * c;
                        Gy += Ky[x + 1][y + 1] * c;
                    }
                }

                float Mag = sqrt(Gx * Gx + Gy * Gy);

                return Mag;
            }
            ENDCG
        }
        //Canny intensity pass
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target
            {
                int x, y;
                int3x3 Kx = {
                    1, 0, -1,
                    1, 0, -1,
                    1, 0, -1
                };

                int3x3 Ky = {
                    1, 1, 1,
                    0, 0, 0,
                    -1, -1, -1
                };

                float Gx = 0.0f;
                float Gy = 0.0f;

                for (x = -1; x <= 1; ++x) {
                    for (y = -1; y <= 1; ++y) {
                        float2 uv = i.uv + _MainTex_TexelSize * float2(x, y);

                        half c = tex2D(_MainTex, uv).a;
                        Gx += Kx[x + 1][y + 1] * c;
                        Gy += Ky[x + 1][y + 1] * c;
                    }
                }
                float Mag = sqrt(Gx * Gx + Gy * Gy);
                float theta = abs(atan2(Gy, Gx));
                    
                return float4(Gx, Gy, theta, Mag);
            }
            ENDCG
        }
        //canny mag supression pass
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target
            {
                float4 canny = tex2D(_MainTex, i.uv);

                float mag = canny.a;
                float theta = degrees(canny.b);

                if ((0 <= theta && theta <= 45.0f) || (135.0f <= theta && theta <= 180.0f))
                {
                    float northMag = tex2D(_MainTex, i.uv + MainTex_TexelSize * float2(0, -1)).a;
                    float southMag = tex2D(_MainTex, i.uv + MainTex_TexelSize * float2(0, 1)).a;

                    canny = Mag >= northMag && Mag >= southMag ? canny : 0.0f;
                }
                else if (45.0f <= theta && theta <= 135.0f)
                {
                    float westMag = tex2D(_MainTex, i.uv + MainTex_TexelSize * float2(-1, 0)).a;
                    float eastMag = tex2D(_MainTex, i.uv + MainTex_TexelSize * float2(1, 0)).a;

                    canny = Mag >= westMag && Mag >= eastMag ? canny : 0.0f;
                }
                return canny;
            }
            ENDCG
        }
    }
}
