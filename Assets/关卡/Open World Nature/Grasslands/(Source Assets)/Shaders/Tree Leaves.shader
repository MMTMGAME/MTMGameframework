// Upgrade NOTE: upgraded instancing buffer 'OpenWorldNatureTreeLeaves' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Open World Nature/Tree Leaves"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.3
		_Hue("Hue", Range( -0.5 , 0.5)) = 0
		_Saturation("Saturation", Range( -1 , 1)) = 0
		_Lightness("Lightness", Range( -1 , 1)) = 0
		[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
		_BumpScale("Bump Scale", Range( 0 , 1)) = 1
		[NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
		_OcclusionRemap("Occlusion Remap", Vector) = (0,1,0,0)
		[NoScaleOffset]_OcclusionMap("Occlusion Map", 2D) = "white" {}
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[Toggle(_UVMASK_ON)] _UVMask("UV Mask", Float) = 0
		_TranslucencyTint("TranslucencyTint", Color) = (0,0,0,0)
		[Toggle(_BAKEDMASK_ON)] _BakedMask("Baked Mask", Float) = 1
		[NoScaleOffset]_TranslucencyMap("TranslucencyMap", 2D) = "white" {}
		_WindDirectionAndStrength("WindDirectionAndStrength", Vector) = (1,1,1,1)
		_Shiver("Shiver", Vector) = (1,1,1,1)
		[NoScaleOffset]_MetallicGlossMap("Metallic Gloss Map", 2D) = "white" {}
		[Toggle(_DEBUGGUST_ON)] _DebugGust("Debug Gust", Float) = 0
		_StiffnessVariation("StiffnessVariation", Range( 0 , 0.99)) = 0
		[Toggle(_METALLICGLOSSMAP_ON)] _METALLICGLOSSMAP("_METALLICGLOSSMAP", Float) = 0
		_GlossRemap("Gloss Remap", Vector) = (0,1,0,0)
		_Glossiness("_Glossiness", Range( 0 , 1)) = 0.2
		_Metallic("_Metallic", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "DisableBatching" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _BAKEDMASK_ON
		#pragma shader_feature _UVMASK_ON
		#pragma shader_feature _DEBUGGUST_ON
		#pragma shader_feature _METALLICGLOSSMAP_ON
		#pragma surface surf StandardCustom keepalpha addshadow fullforwardshadows exclude_path:deferred dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float2 uv2_texcoord2;
		};

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

		uniform float4 GlobalWindDirectionAndStrength;
		uniform float4 _WindDirectionAndStrength;
		uniform sampler2D _GustNoise;
		uniform float _StiffnessVariation;
		uniform sampler2D _ShiverNoise;
		uniform float4 GlobalShiver;
		uniform float4 _Shiver;
		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float _Metallic;
		uniform sampler2D _MetallicGlossMap;
		uniform float _Glossiness;
		uniform float2 _GlossRemap;
		uniform sampler2D _OcclusionMap;
		uniform float2 _OcclusionRemap;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float4 _TranslucencyTint;
		uniform sampler2D _TranslucencyMap;
		uniform float _Cutoff = 0.3;

		UNITY_INSTANCING_BUFFER_START(OpenWorldNatureTreeLeaves)
			UNITY_DEFINE_INSTANCED_PROP(float, _Hue)
#define _Hue_arr OpenWorldNatureTreeLeaves
			UNITY_DEFINE_INSTANCED_PROP(float, _Saturation)
#define _Saturation_arr OpenWorldNatureTreeLeaves
			UNITY_DEFINE_INSTANCED_PROP(float, _Lightness)
#define _Lightness_arr OpenWorldNatureTreeLeaves
		UNITY_INSTANCING_BUFFER_END(OpenWorldNatureTreeLeaves)


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

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 break15_g418 = ( GlobalWindDirectionAndStrength * _WindDirectionAndStrength );
			float4 appendResult3_g418 = (float4(break15_g418.x , 0.0 , break15_g418.y , 0.0));
			float clampResult36_g418 = clamp( ( length( appendResult3_g418 ) * 1000.0 ) , 0.0 , 1.0 );
			float4 lerpResult29_g418 = lerp( float4( float3(0.7,0,0.3) , 0.0 ) , appendResult3_g418 , clampResult36_g418);
			float4 normalizeResult5_g418 = normalize( lerpResult29_g418 );
			float4 globalGustDirection107 = normalizeResult5_g418;
			float4 transform15_g307 = mul(unity_ObjectToWorld,float4(0,0,0,1));
			float4 objectPivotInWS113 = transform15_g307;
			float edgeFlutter450 = v.color.g;
			float4 lerpResult590 = lerp( objectPivotInWS113 , float4( ase_worldPos , 0.0 ) , edgeFlutter450);
			float3 break9_g432 = lerpResult590.xyz;
			float4 appendResult11_g432 = (float4(break9_g432.x , break9_g432.z , 0.0 , 0.0));
			float4 pivotXY50_g432 = appendResult11_g432;
			float temp_output_204_0_g432 = 0.0;
			float3 break12_g432 = globalGustDirection107.xyz;
			float4 appendResult13_g432 = (float4(break12_g432.x , break12_g432.z , 0.0 , 0.0));
			float4 gustDirection53_g432 = appendResult13_g432;
			#ifdef _UVMASK_ON
				float staticSwitch550 = ( v.texcoord.xy.y * 0.1 );
			#else
				float staticSwitch550 = 0.0;
			#endif
			float4 break497 = ( float4( ase_worldPos , 0.0 ) - objectPivotInWS113 );
			float temp_output_493_0 = ( frac( ( ( break497.x + break497.y + break497.z ) * 0.4 ) ) * 0.15 );
			float branchPhase447 = v.color.r;
			#ifdef _BAKEDMASK_ON
				float staticSwitch537 = ( ( 0.3 * branchPhase447 ) + _Time.y + ( edgeFlutter450 * 0.5 ) + ( temp_output_493_0 * edgeFlutter450 ) );
			#else
				float staticSwitch537 = ( ( _Time.y + staticSwitch550 ) + ( temp_output_493_0 * 0.5 ) );
			#endif
			float time109_g432 = staticSwitch537;
			float globalGustSpeed105 = max( ( GlobalWindDirectionAndStrength.z * _WindDirectionAndStrength.z ) , 0.01 );
			float4 tex2DNode6_g433 = tex2Dlod( _GustNoise, float4( ( ( ( pivotXY50_g432.xy + ( temp_output_204_0_g432 * 2.0 ) ) * 0.01 ) - ( gustDirection53_g432.xy * time109_g432 * globalGustSpeed105 ) ), 0, 0.0) );
			float gustNoise153_g432 = max( tex2DNode6_g433.r , 0.01 );
			float globalGustStrength106 = max( ( GlobalWindDirectionAndStrength.w * _WindDirectionAndStrength.w ) , 0.01 );
			float temp_output_18_0_g436 = ( gustNoise153_g432 * globalGustStrength106 );
			float clampResult16_g436 = clamp( temp_output_18_0_g436 , 0.1 , 0.9 );
			#ifdef _UVMASK_ON
				float staticSwitch542 = ( v.texcoord.xy.y * 0.2 );
			#else
				float staticSwitch542 = 1.0;
			#endif
			float secondaryFactor452 = v.texcoord1.xy.y;
			#ifdef _BAKEDMASK_ON
				float staticSwitch538 = secondaryFactor452;
			#else
				float staticSwitch538 = staticSwitch542;
			#endif
			float relativeHeightMask101_g432 = staticSwitch538;
			float2 break2_g435 = ( pivotXY50_g432.xy * 10.0 );
			float clampResult8_g435 = clamp( pow( frac( ( break2_g435.x * break2_g435.y ) ) , 2.0 ) , ( 1.0 - _StiffnessVariation ) , 1.0 );
			float randomStiffness90_g432 = clampResult8_g435;
			float vertexMask103_g432 = 1.0;
			float gustStrength105_g432 = ( ( temp_output_18_0_g436 * ( 1.0 - clampResult16_g436 ) * 1.5 ) * relativeHeightMask101_g432 * randomStiffness90_g432 * vertexMask103_g432 );
			float gustStrengthAtPosition118 = gustStrength105_g432;
			float gustStrength25_g438 = gustStrengthAtPosition118;
			float3 scaledGustDirection155_g438 = ( globalGustDirection107.xyz * gustStrength25_g438 );
			float3 positionWithGust169_g438 = ( ase_worldPos + ( scaledGustDirection155_g438 * 10.0 ) );
			float globalShiverSpeed149 = max( ( GlobalShiver.x * _Shiver.x ) , 0.01 );
			float4 tex2DNode11_g437 = tex2Dlod( _ShiverNoise, float4( ( ( pivotXY50_g432.xy + ( temp_output_204_0_g432 * 2.0 ) ) - ( gustDirection53_g432.xy * time109_g432 * globalShiverSpeed149 ) ), 0, 0.0) );
			float4 appendResult6_g437 = (float4(tex2DNode11_g437.r , tex2DNode11_g437.g , tex2DNode11_g437.b , 0.0));
			float4 temp_cast_11 = (0.5).xxxx;
			float4 shiverNoise155_g432 = ( ( appendResult6_g437 - temp_cast_11 ) * 2.0 );
			float temp_output_28_0_g434 = 0.2;
			float3 appendResult32_g434 = (float3(1.0 , temp_output_28_0_g434 , 1.0));
			float3 temp_output_29_0_g434 = ( shiverNoise155_g432.xyz * appendResult32_g434 );
			float2 break10_g434 = gustDirection53_g432.xy;
			float4 appendResult4_g434 = (float4(break10_g434.x , temp_output_28_0_g434 , break10_g434.y , 0.0));
			float temp_output_21_0_g434 = gustStrength105_g432;
			float clampResult45_g434 = clamp( ( ( temp_output_21_0_g434 * 0.8 ) + 0.2 ) , 0.0 , 1.0 );
			float4 lerpResult26_g434 = lerp( float4( temp_output_29_0_g434 , 0.0 ) , appendResult4_g434 , clampResult45_g434);
			float4 shiverDirectionAtPosition132 = lerpResult26_g434;
			float3 shiverDirection29_g438 = shiverDirectionAtPosition132.xyz;
			float temp_output_5_0_g434 = length( temp_output_29_0_g434 );
			float globalShiverStrength141 = max( ( GlobalShiver.y * _Shiver.y ) , 0.01 );
			float temp_output_6_0_g434 = globalShiverStrength141;
			float shiverStrengthAtPosition125 = ( relativeHeightMask101_g432 * ( ( temp_output_21_0_g434 * 0.5 ) + ( temp_output_5_0_g434 * temp_output_6_0_g434 ) ) * vertexMask103_g432 * randomStiffness90_g432 );
			float shiverStrength30_g438 = ( edgeFlutter450 * shiverStrengthAtPosition125 );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float clampResult98_g438 = clamp( ase_vertex3Pos.y , 0.0 , 1.0 );
			float3 shiverPositionOffset170_g438 = ( shiverDirection29_g438 * shiverStrength30_g438 * clampResult98_g438 * 10.0 );
			float4 appendResult87 = (float4(( positionWithGust169_g438 + shiverPositionOffset170_g438 ) , 1.0));
			float4 transform29 = mul(unity_WorldToObject,appendResult87);
			float4 vertexPositionWithWind411 = transform29;
			v.vertex.xyz = vertexPositionWithWind411.xyz;
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

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float2 uv_BumpMap387 = i.uv_texcoord;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap387 ), _BumpScale );
			float _Hue_Instance = UNITY_ACCESS_INSTANCED_PROP(_Hue_arr, _Hue);
			float2 uv_MainTex1 = i.uv_texcoord;
			float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex1 );
			float4 albedo424 = tex2DNode1;
			float3 hsvTorgb527 = RGBToHSV( albedo424.rgb );
			float _Saturation_Instance = UNITY_ACCESS_INSTANCED_PROP(_Saturation_arr, _Saturation);
			float _Lightness_Instance = UNITY_ACCESS_INSTANCED_PROP(_Lightness_arr, _Lightness);
			float3 hsvTorgb531 = HSVToRGB( float3(( _Hue_Instance + hsvTorgb527.x ),( hsvTorgb527.y + _Saturation_Instance ),( hsvTorgb527.z + _Lightness_Instance )) );
			float4 transform15_g307 = mul(unity_ObjectToWorld,float4(0,0,0,1));
			float4 objectPivotInWS113 = transform15_g307;
			float3 ase_worldPos = i.worldPos;
			float edgeFlutter450 = i.vertexColor.g;
			float4 lerpResult590 = lerp( objectPivotInWS113 , float4( ase_worldPos , 0.0 ) , edgeFlutter450);
			float3 break9_g432 = lerpResult590.xyz;
			float4 appendResult11_g432 = (float4(break9_g432.x , break9_g432.z , 0.0 , 0.0));
			float4 pivotXY50_g432 = appendResult11_g432;
			float temp_output_204_0_g432 = 0.0;
			float4 break15_g418 = ( GlobalWindDirectionAndStrength * _WindDirectionAndStrength );
			float4 appendResult3_g418 = (float4(break15_g418.x , 0.0 , break15_g418.y , 0.0));
			float clampResult36_g418 = clamp( ( length( appendResult3_g418 ) * 1000.0 ) , 0.0 , 1.0 );
			float4 lerpResult29_g418 = lerp( float4( float3(0.7,0,0.3) , 0.0 ) , appendResult3_g418 , clampResult36_g418);
			float4 normalizeResult5_g418 = normalize( lerpResult29_g418 );
			float4 globalGustDirection107 = normalizeResult5_g418;
			float3 break12_g432 = globalGustDirection107.xyz;
			float4 appendResult13_g432 = (float4(break12_g432.x , break12_g432.z , 0.0 , 0.0));
			float4 gustDirection53_g432 = appendResult13_g432;
			#ifdef _UVMASK_ON
				float staticSwitch550 = ( i.uv_texcoord.y * 0.1 );
			#else
				float staticSwitch550 = 0.0;
			#endif
			float4 break497 = ( float4( ase_worldPos , 0.0 ) - objectPivotInWS113 );
			float temp_output_493_0 = ( frac( ( ( break497.x + break497.y + break497.z ) * 0.4 ) ) * 0.15 );
			float branchPhase447 = i.vertexColor.r;
			#ifdef _BAKEDMASK_ON
				float staticSwitch537 = ( ( 0.3 * branchPhase447 ) + _Time.y + ( edgeFlutter450 * 0.5 ) + ( temp_output_493_0 * edgeFlutter450 ) );
			#else
				float staticSwitch537 = ( ( _Time.y + staticSwitch550 ) + ( temp_output_493_0 * 0.5 ) );
			#endif
			float time109_g432 = staticSwitch537;
			float globalGustSpeed105 = max( ( GlobalWindDirectionAndStrength.z * _WindDirectionAndStrength.z ) , 0.01 );
			float4 tex2DNode6_g433 = tex2D( _GustNoise, ( ( ( pivotXY50_g432.xy + ( temp_output_204_0_g432 * 2.0 ) ) * 0.01 ) - ( gustDirection53_g432.xy * time109_g432 * globalGustSpeed105 ) ) );
			float gustNoise153_g432 = max( tex2DNode6_g433.r , 0.01 );
			float globalGustStrength106 = max( ( GlobalWindDirectionAndStrength.w * _WindDirectionAndStrength.w ) , 0.01 );
			float temp_output_18_0_g436 = ( gustNoise153_g432 * globalGustStrength106 );
			float clampResult16_g436 = clamp( temp_output_18_0_g436 , 0.1 , 0.9 );
			#ifdef _UVMASK_ON
				float staticSwitch542 = ( i.uv_texcoord.y * 0.2 );
			#else
				float staticSwitch542 = 1.0;
			#endif
			float secondaryFactor452 = i.uv2_texcoord2.y;
			#ifdef _BAKEDMASK_ON
				float staticSwitch538 = secondaryFactor452;
			#else
				float staticSwitch538 = staticSwitch542;
			#endif
			float relativeHeightMask101_g432 = staticSwitch538;
			float2 break2_g435 = ( pivotXY50_g432.xy * 10.0 );
			float clampResult8_g435 = clamp( pow( frac( ( break2_g435.x * break2_g435.y ) ) , 2.0 ) , ( 1.0 - _StiffnessVariation ) , 1.0 );
			float randomStiffness90_g432 = clampResult8_g435;
			float vertexMask103_g432 = 1.0;
			float gustStrength105_g432 = ( ( temp_output_18_0_g436 * ( 1.0 - clampResult16_g436 ) * 1.5 ) * relativeHeightMask101_g432 * randomStiffness90_g432 * vertexMask103_g432 );
			float gustStrengthAtPosition118 = gustStrength105_g432;
			float3 temp_cast_9 = (gustStrengthAtPosition118).xxx;
			#ifdef _DEBUGGUST_ON
				float3 staticSwitch588 = temp_cast_9;
			#else
				float3 staticSwitch588 = hsvTorgb531;
			#endif
			o.Albedo = staticSwitch588;
			float2 uv_MetallicGlossMap593 = i.uv_texcoord;
			float4 tex2DNode593 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap593 );
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch597 = tex2DNode593.r;
			#else
				float staticSwitch597 = _Metallic;
			#endif
			float MetallicMap600 = staticSwitch597;
			o.Metallic = MetallicMap600;
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch598 = (_GlossRemap.x + (tex2DNode593.a - 0.0) * (_GlossRemap.y - _GlossRemap.x) / (1.0 - 0.0));
			#else
				float staticSwitch598 = _Glossiness;
			#endif
			float GlossMap599 = staticSwitch598;
			o.Smoothness = GlossMap599;
			float2 uv_OcclusionMap398 = i.uv_texcoord;
			o.Occlusion = (_OcclusionRemap.x + (tex2D( _OcclusionMap, uv_OcclusionMap398 ).r - 0.0) * (_OcclusionRemap.y - _OcclusionRemap.x) / (1.0 - 0.0));
			float2 uv_TranslucencyMap402 = i.uv_texcoord;
			float4 translucency420 = ( _TranslucencyTint * tex2D( _TranslucencyMap, uv_TranslucencyMap402 ) );
			o.Translucency = translucency420.rgb;
			o.Alpha = 1;
			float alpha425 = tex2DNode1.a;
			clip( alpha425 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "VisualDesignCafe.Nature.Editor.NatureMaterialEditor"
}
/*ASEBEGIN
Version=17500
129;7;960;917;4257.508;1183.766;1.395411;True;False
Node;AmplifyShaderEditor.CommentaryNode;116;-4008.191,1529.564;Inherit;False;672.4673;168.7953;Comment;2;89;113;World Space Object Pivot;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;89;-3958.191,1579.564;Inherit;False;GetPivotInWorldSpace;-1;;307;264e0929a81902742a5a4e0e0a62ac57;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-3578.724,1583.359;Float;False;objectPivotInWS;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;492;-7241.927,2613.734;Inherit;False;113;objectPivotInWS;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;490;-7273.927,2325.735;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;491;-6969.927,2421.735;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;497;-6713.927,2437.734;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;498;-6377.927,2437.734;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;587;-6204.77,2456.149;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;548;-5932.136,2174.471;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;455;-7496.393,3304.655;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;549;-5625.083,2208.728;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;499;-6049.815,2568.766;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;494;-6055.392,2766.154;Float;False;Constant;_Float4;Float 4;14;0;Create;True;0;0;False;0;0.15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;450;-7228.393,3392.655;Float;False;edgeFlutter;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;447;-7242.393,3297.655;Float;False;branchPhase;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;551;-5639.087,2109.32;Float;False;Constant;_Float12;Float 12;21;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;117;-4019.222,1817.138;Inherit;False;1584.851;1651.966;Calculate wind strength at vertex position;22;265;132;125;118;135;110;200;55;202;150;111;469;466;475;142;538;539;541;542;552;570;572;Wind Strength;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;540;-5358.141,2061.155;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;550;-5389.521,2166.455;Float;False;Property;_UVMask;UV Mask;16;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;476;-5661.091,3055.142;Inherit;False;447;branchPhase;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;483;-5634.477,3271.42;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;507;-5802.57,2894.106;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;493;-5781.093,2645.255;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;453;-7495.393,3549.655;Inherit;False;1;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;541;-4070.87,1877.789;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;586;-5459.854,2568.854;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;484;-5314.669,3267.361;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;581;-4815.574,2257.669;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;112;-4004.226,860.4534;Inherit;False;652.8523;550.6835;Global Wind Parameters;5;106;105;107;141;149;Global Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;146;-5619.753,3155.925;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;504;-5537.603,2785.47;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;452;-7202.393,3636.655;Float;False;secondaryFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;552;-3765.556,1959.691;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;518;-3962.294,1033.771;Inherit;False;GlobalWindParameters;20;;418;ef55991c5ff9f3747b20a326fd322e36;0;0;5;FLOAT;11;FLOAT;10;FLOAT;8;FLOAT;6;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;488;-5358.154,3069.787;Inherit;False;2;2;0;FLOAT;0.3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;539;-3774.26,1869.083;Float;False;Constant;_Float9;Float 9;19;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;591;-4349.649,2964.14;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;-4042.756,2101.047;Inherit;False;452;secondaryFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;107;-3613.996,1265.174;Float;False;globalGustDirection;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;585;-5099.753,2455.754;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;-3605.74,1085.918;Float;False;globalGustSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-4330.733,2852.601;Inherit;False;113;objectPivotInWS;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-3621.772,922.121;Float;False;globalShiverSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-3607.401,1174.849;Float;False;globalGustStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;-4874.399,3046.29;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;542;-3577.04,1966.582;Float;False;Property;_UVMask;UV Mask;19;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;584;-4313.979,2497.229;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-3626.28,1007.885;Float;False;globalShiverStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;537;-4424.737,2680.886;Float;False;Property;_BakedMask;Baked Mask;18;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;135;-3839.251,2553.036;Float;True;Global;_ShiverNoise;ShiverNoise;27;0;Create;False;0;0;False;0;None;66dd7d1835f20b8419dbc5544a12688a;False;gray;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-3867.668,2472.236;Inherit;False;141;globalShiverStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-3854.028,2397.19;Inherit;False;105;globalGustSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-3865.651,2908.544;Inherit;False;106;globalGustStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;55;-3833.418,3084.348;Float;True;Global;_GustNoise;_GustNoise;28;0;Create;True;0;0;False;0;None;156c5c844ac14b042b7dacdcfcd0981b;False;black;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.StaticSwitch;538;-3307.836,2040.697;Float;False;Property;_BakedMask;Baked Mask;18;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-3865.564,2825.972;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-3902.15,2214.136;Float;False;Property;_StiffnessVariation;StiffnessVariation;25;0;Create;True;0;0;False;0;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-3868.289,2318.347;Inherit;False;149;globalShiverSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;590;-4074.649,2850.14;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;572;-3384.866,2503.2;Inherit;False;GetWindStrength;-1;;432;81b67046328b6f44f9bbfde7e0fba2b2;0;14;204;FLOAT;0;False;171;FLOAT;0.2;False;165;FLOAT;1;False;164;FLOAT;1;False;114;FLOAT;0;False;60;FLOAT;1;False;55;FLOAT;1;False;49;FLOAT;0.1;False;24;SAMPLER2D;0;False;4;FLOAT;0;False;5;FLOAT3;0,0,0;False;28;FLOAT;1;False;1;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0,0;False;4;FLOAT;138;FLOAT;43;FLOAT4;44;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;120;-2242.677,1836.51;Inherit;False;1849.186;659.9193;Apply wind displacement to vertex position;11;29;87;411;88;86;109;129;133;119;480;485;Wind Displacement;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;423;-3965.596,-228.4894;Inherit;False;748;273;;3;424;1;425;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-2892.449,2458.837;Float;False;shiverStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;481;-1894.376,1754.344;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;-2892.238,2587.098;Float;False;shiverDirectionAtPosition;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;1;-3915.596,-178.4894;Inherit;True;Property;_MainTex;MainTex;4;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;e4e123e1fd77b9843baa3b0b8351235c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;129;-2185.187,1906.63;Inherit;False;125;shiverStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-2894.088,2714.748;Float;False;gustStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;424;-3470.022,-165.6478;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2163.99,2143.075;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-2183.851,1990.018;Inherit;False;132;shiverDirectionAtPosition;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-2148.943,2066.18;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;480;-1776.077,1902.544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;431;2379.381,1852.999;Inherit;False;1952.777;1354.086;;21;387;398;0;534;510;426;588;533;571;532;531;529;530;528;526;524;525;527;523;601;602;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;86;-2076.601,2219.387;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;592;-4024.57,-549.6443;Inherit;False;Property;_GlossRemap;Gloss Remap;30;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;593;-4135.447,-870.8923;Inherit;True;Property;_MetallicGlossMap;Metallic Gloss Map;23;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;523;2424.677,1973.756;Inherit;False;424;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;485;-1530.617,2037.726;Inherit;False;ApplyTreeWindDisplacement;-1;;438;0eb84d03384e16540a3d42d8a1457ec4;0;6;20;FLOAT;1;False;19;FLOAT3;1,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT;0;False;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1316.945,2258.902;Float;False;Constant;_Float2;Float 2;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;422;-3995.063,191.1917;Inherit;False;1055.5;488.6848;;4;402;401;403;420;Translucency;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;594;-3733.262,-725.2172;Inherit;False;Property;_Glossiness;_Glossiness;31;0;Create;True;0;0;False;0;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;525;2601.22,2243.86;Float;False;InstancedProperty;_Lightness;Lightness;3;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;524;2599.334,1889.888;Float;False;InstancedProperty;_Hue;Hue;1;0;Create;True;0;0;False;0;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-1130.095,2089.546;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;401;-3867.631,241.1917;Float;False;Property;_TranslucencyTint;TranslucencyTint;17;0;Create;True;0;0;False;0;0,0,0,0;0.6901961,0.7647059,0.6705883,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;526;2601.769,2142.806;Float;False;InstancedProperty;_Saturation;Saturation;2;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;596;-3646.73,-596.5256;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;402;-3945.063,449.8767;Inherit;True;Property;_TranslucencyMap;TranslucencyMap;19;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;6110e30d8032f4e4985feaed1171231d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;595;-3746.177,-900.4401;Inherit;False;Property;_Metallic;_Metallic;32;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;527;2659.413,1981.257;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;598;-3402.524,-660.2808;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;29;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;597;-3413.684,-836.9415;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;26;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;529;2946.22,2050.86;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;528;2947.22,2168.86;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;530;2946.683,1923.215;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;29;-955.1426,2082.968;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;403;-3496.244,370.9784;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;571;3384.664,1943.682;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;533;2863.478,2775.403;Float;False;Property;_OcclusionRemap;Occlusion Remap;7;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;600;-3098.096,-874.5258;Inherit;False;MetallicMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;425;-3455.624,-44.04722;Float;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;531;3153.854,2001.227;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;599;-3089.546,-618.6511;Inherit;False;GlossMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;-682.4496,2143.209;Float;False;vertexPositionWithWind;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;532;2711.933,2371.125;Float;False;Property;_BumpScale;Bump Scale;5;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;398;2777.196,2556.585;Inherit;True;Property;_OcclusionMap;Occlusion Map;8;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;05f189094ab11444eb08228c5fffd69a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;420;-3184.064,493.4647;Float;False;translucency;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;602;3453.878,2537.164;Inherit;False;599;GlossMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;456;-6413.774,3376.502;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;448;-6745.776,3279.502;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;426;3209.352,2965.344;Inherit;False;425;alpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;458;-6526.774,3532.502;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-2891.923,2329.183;Float;False;gustHighlightAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;510;3184.669,2856.605;Inherit;False;420;translucency;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;449;-6215.774,3394.502;Float;False;vertexFase;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;457;-6756.776,3631.501;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;451;-7204.393,3537.655;Float;False;primaryFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;416;2960.513,3254.859;Inherit;False;411;vertexPositionWithWind;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;534;3189.388,2640.846;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;570;-3792.262,2750.889;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;454;-6729.776,3482.502;Inherit;False;447;branchPhase;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;601;3549.879,2444.364;Inherit;False;600;MetallicMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;588;3645.428,2186.618;Float;False;Property;_DebugGust;Debug Gust;24;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;387;3089.649,2221.875;Inherit;True;Property;_BumpMap;BumpMap;6;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;0900d62a76914014f982fc871dd92766;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;466;-4043.224,2011.918;Inherit;False;451;primaryFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;475;-3750.216,2097.556;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3969.811,2423.591;Float;False;True;-1;2;VisualDesignCafe.Nature.Editor.NatureMaterialEditor;0;0;Standard;Open World Nature/Tree Leaves;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;False;True;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.3;True;True;0;False;TransparentCutout;;AlphaTest;ForwardOnly;14;all;True;True;True;False;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Spherical;False;Absolute;0;;0;9;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;113;0;89;0
WireConnection;491;0;490;0
WireConnection;491;1;492;0
WireConnection;497;0;491;0
WireConnection;498;0;497;0
WireConnection;498;1;497;1
WireConnection;498;2;497;2
WireConnection;587;0;498;0
WireConnection;549;0;548;2
WireConnection;499;0;587;0
WireConnection;450;0;455;2
WireConnection;447;0;455;1
WireConnection;550;1;551;0
WireConnection;550;0;549;0
WireConnection;493;0;499;0
WireConnection;493;1;494;0
WireConnection;586;0;493;0
WireConnection;484;0;483;0
WireConnection;581;0;540;0
WireConnection;581;1;550;0
WireConnection;504;0;493;0
WireConnection;504;1;507;0
WireConnection;452;0;453;2
WireConnection;552;0;541;2
WireConnection;488;1;476;0
WireConnection;107;0;518;0
WireConnection;585;0;581;0
WireConnection;585;1;586;0
WireConnection;105;0;518;8
WireConnection;149;0;518;11
WireConnection;106;0;518;6
WireConnection;477;0;488;0
WireConnection;477;1;146;0
WireConnection;477;2;484;0
WireConnection;477;3;504;0
WireConnection;542;1;539;0
WireConnection;542;0;552;0
WireConnection;141;0;518;10
WireConnection;537;1;585;0
WireConnection;537;0;477;0
WireConnection;538;1;542;0
WireConnection;538;0;469;0
WireConnection;590;0;114;0
WireConnection;590;1;584;0
WireConnection;590;2;591;0
WireConnection;572;165;538;0
WireConnection;572;114;202;0
WireConnection;572;60;150;0
WireConnection;572;55;111;0
WireConnection;572;49;142;0
WireConnection;572;24;135;0
WireConnection;572;4;537;0
WireConnection;572;5;110;0
WireConnection;572;28;200;0
WireConnection;572;1;590;0
WireConnection;572;3;55;0
WireConnection;125;0;572;43
WireConnection;132;0;572;44
WireConnection;118;0;572;0
WireConnection;424;0;1;0
WireConnection;480;0;481;0
WireConnection;480;1;129;0
WireConnection;485;20;480;0
WireConnection;485;19;133;0
WireConnection;485;6;109;0
WireConnection;485;7;119;0
WireConnection;485;1;86;0
WireConnection;87;0;485;0
WireConnection;87;3;88;0
WireConnection;596;0;593;4
WireConnection;596;3;592;1
WireConnection;596;4;592;2
WireConnection;527;0;523;0
WireConnection;598;1;594;0
WireConnection;598;0;596;0
WireConnection;597;1;595;0
WireConnection;597;0;593;1
WireConnection;529;0;527;2
WireConnection;529;1;526;0
WireConnection;528;0;527;3
WireConnection;528;1;525;0
WireConnection;530;0;524;0
WireConnection;530;1;527;1
WireConnection;29;0;87;0
WireConnection;403;0;401;0
WireConnection;403;1;402;0
WireConnection;600;0;597;0
WireConnection;425;0;1;4
WireConnection;531;0;530;0
WireConnection;531;1;529;0
WireConnection;531;2;528;0
WireConnection;599;0;598;0
WireConnection;411;0;29;0
WireConnection;420;0;403;0
WireConnection;456;0;448;0
WireConnection;456;1;458;0
WireConnection;458;0;454;0
WireConnection;458;1;457;0
WireConnection;265;0;572;138
WireConnection;449;0;456;0
WireConnection;451;0;453;1
WireConnection;534;0;398;1
WireConnection;534;3;533;1
WireConnection;534;4;533;2
WireConnection;588;1;531;0
WireConnection;588;0;571;0
WireConnection;387;5;532;0
WireConnection;475;0;466;0
WireConnection;475;1;469;0
WireConnection;0;0;588;0
WireConnection;0;1;387;0
WireConnection;0;3;601;0
WireConnection;0;4;602;0
WireConnection;0;5;534;0
WireConnection;0;7;510;0
WireConnection;0;10;426;0
WireConnection;0;11;416;0
ASEEND*/
//CHKSM=F4F182EF106C4ADCBFCDF1B41A9320D190545BAB