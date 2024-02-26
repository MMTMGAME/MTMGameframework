Shader "Custom/FadeOnApproachReverse" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _FadeStartDistance ("Fade Start Distance", Float) = 1.0
        _FadeEndDistance ("Fade End Distance", Float) = 3.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        float _FadeStartDistance;
        float _FadeEndDistance;

        struct Input {
            float3 worldPos;
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
            // 逆转淡出逻辑，当相机接近时物体变透明，远离时变不透明
            float fade = 1.0 - smoothstep(_FadeStartDistance, _FadeEndDistance, dist);
            o.Alpha *= fade;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
