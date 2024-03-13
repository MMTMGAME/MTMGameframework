// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TransparentOnApproach"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _FadeStartDistance ("Fade Start Distance", Float) = 1.0
        _FadeEndDistance ("Fade End Distance", Float) = 3.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaTest Greater 0.01

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _FadeStartDistance;
            float _FadeEndDistance;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul(v.normal, (float3x3)unity_ObjectToWorld));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.worldPos - _WorldSpaceCameraPos);
                float alpha = smoothstep(_FadeEndDistance, _FadeStartDistance, dist);

                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;
                texColor.a *= (1 - alpha);

                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                float3 norm = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                float diff = max(dot(norm, lightDir), 0.0);

                // Correctly combine the ambient and diffuse lighting and convert to float4
                fixed3 color = ambient + _LightColor0.xyz * diff;
                fixed4 finalColor = fixed4(color, 1.0); // Explicitly set alpha to 1.0

                // Combine the texture color with the lighting calculations
                fixed4 col = texColor * finalColor;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}