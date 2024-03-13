Shader "Custom/TransparentOnApproach" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _FadeStartDistance ("Fade Start Distance", Float) = 1.0
        _FadeEndDistance ("Fade End Distance", Float) = 3.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaTest Greater 0.01

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _FadeStartDistance;
            float _FadeEndDistance;

            v2f vert (appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float dist = length(i.worldPos - _WorldSpaceCameraPos);
                // Calculate alpha based on distance, fading out as we approach the object
                float alpha = smoothstep(_FadeEndDistance, _FadeStartDistance, dist);

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *=(1- alpha);

                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
