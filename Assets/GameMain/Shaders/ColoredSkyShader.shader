Shader "Custom/SolidColorSkyboxWithFog"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _SkyColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = _SkyColor;
                UNITY_APPLY_FOG_COLOR(i.worldPos, color, _SkyColor);
                return color;
            }
            ENDCG
        }
    }
}
