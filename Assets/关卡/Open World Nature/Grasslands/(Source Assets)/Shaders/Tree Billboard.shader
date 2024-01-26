Shader "Open World Nature/Tree Billboard"
{
    Properties
    {
        _Hue("Hue", Range( -0.5 , 0.5)) = 0
		_Saturation("Saturation", Range( -1 , 1)) = 0
		_Lightness("Lightness", Range( -1 , 1)) = 0

		_TranslucencyColor ("Translucency Color", Color) = (0,0,0,0)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
    }
    SubShader
    {
        Tags 
		{ 
			"Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout" 
			"DisableBatching"="LODFading"
		}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardCustom vertex:SpeedTreeBillboardVert fullforwardshadows dithercrossfade
		
		// Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
		
		#define EFFECT_BUMP

		#include "SpeedTreeBillboardCommon.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"

		fixed4 _TranslucencyColor;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		#ifdef LIGHTMAP_ON
		uniform sampler2D _BumpMap;
		#endif

		UNITY_INSTANCING_BUFFER_START(OpenWorldNatureTreeBillboard)
			UNITY_DEFINE_INSTANCED_PROP(float, _Lightness)
#define _Lightness_arr OpenWorldNatureTreeBillboard
			UNITY_DEFINE_INSTANCED_PROP(float, _Saturation)
#define _Saturation_arr OpenWorldNatureTreeBillboard
			UNITY_DEFINE_INSTANCED_PROP(float, _Hue)
#define _Hue_arr OpenWorldNatureTreeBillboard
		UNITY_INSTANCING_BUFFER_END(OpenWorldNatureTreeBillboard)

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}

		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

        void surf (Input IN, inout SurfaceOutputStandardCustom o)
        {
            fixed4 c = tex2D (_MainTex, IN.mainTexUV);
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);

			float3 hsv = RGBToHSV( c.rgb );
			float hue = UNITY_ACCESS_INSTANCED_PROP(_Hue_arr, _Hue);
			float saturation = UNITY_ACCESS_INSTANCED_PROP(_Saturation_arr, _Saturation);
			float lightness = UNITY_ACCESS_INSTANCED_PROP(_Lightness_arr, _Lightness);
			o.Albedo = HSVToRGB( float3(( hue + hsv.x ),( hsv.y + saturation ),( hsv.z + lightness )) );
			o.Translucency = _TranslucencyColor;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.mainTexUV));
        }
        ENDCG
    }
    FallBack "Diffuse"
	CustomEditor "VisualDesignCafe.Nature.Editor.NatureMaterialEditor"
}