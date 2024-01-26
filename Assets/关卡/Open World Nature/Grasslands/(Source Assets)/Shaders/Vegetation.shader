// Upgrade NOTE: upgraded instancing buffer 'OpenWorldNatureVegetation' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Open World Nature/Vegetation"
{
	Properties
	{
		[Toggle(_BAKEDMASK_ON)] _BakedMask("Baked Mask", Float) = 1
		[Toggle(_UVMASK_ON)] _UVMask("UV Mask", Float) = 0
		[Toggle(_VERTEXPOSITIONMASK_ON)] _VertexPositionMask("Vertex Position Mask", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Hue("Hue", Range( -0.5 , 0.5)) = 0
		_Saturation("Saturation", Range( -1 , 1)) = 0
		_Lightness("Lightness", Range( -1 , 1)) = 0
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		_BumpScale("Bump Scale", Range( 0 , 1)) = 1
		[NoScaleOffset]_BumpMap("Bump Map", 2D) = "bump" {}
		_OcclusionRemap("Occlusion Remap", Vector) = (0,1,0,0)
		[NoScaleOffset]_OcclusionMap("Occlusion Map", 2D) = "white" {}
		_WindDirectionAndStrength("WindDirectionAndStrength", Vector) = (1,1,1,1)
		_Shiver("Shiver", Vector) = (1,1,1,1)
		[NoScaleOffset]_MetallicGlossMap("Metallic Gloss Map", 2D) = "white" {}
		[Toggle(_DEBUGGUST_ON)] _DebugGust("Debug Gust", Float) = 0
		_StiffnessVariation("StiffnessVariation", Range( 0 , 0.99)) = 0
		[Toggle(_METALLICGLOSSMAP_ON)] _METALLICGLOSSMAP("_METALLICGLOSSMAP", Float) = 0
		_GlossRemap("Gloss Remap", Vector) = (0,1,0,0)
		_Glossiness("_Glossiness", Range( 0 , 1)) = 0.2
		_Metallic("_Metallic", Range( 0 , 1)) = 0
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
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _BAKEDMASK_ON
		#pragma shader_feature _UVMASK_ON
		#pragma shader_feature _VERTEXPOSITIONMASK_ON
		#pragma shader_feature _DEBUGGUST_ON
		#pragma shader_feature _METALLICGLOSSMAP_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
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
		uniform float _Cutoff = 0.5;

		UNITY_INSTANCING_BUFFER_START(OpenWorldNatureVegetation)
			UNITY_DEFINE_INSTANCED_PROP(float, _Hue)
#define _Hue_arr OpenWorldNatureVegetation
			UNITY_DEFINE_INSTANCED_PROP(float, _Saturation)
#define _Saturation_arr OpenWorldNatureVegetation
			UNITY_DEFINE_INSTANCED_PROP(float, _Lightness)
#define _Lightness_arr OpenWorldNatureVegetation
		UNITY_INSTANCING_BUFFER_END(OpenWorldNatureVegetation)


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


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
			float4 break15_g376 = ( GlobalWindDirectionAndStrength * _WindDirectionAndStrength );
			float4 appendResult3_g376 = (float4(break15_g376.x , 0.0 , break15_g376.y , 0.0));
			float clampResult36_g376 = clamp( ( length( appendResult3_g376 ) * 1000.0 ) , 0.0 , 1.0 );
			float4 lerpResult29_g376 = lerp( float4( float3(0.7,0,0.3) , 0.0 ) , appendResult3_g376 , clampResult36_g376);
			float4 normalizeResult5_g376 = normalize( lerpResult29_g376 );
			float4 globalGustDirection107 = normalizeResult5_g376;
			float4 transform15_g375 = mul(unity_ObjectToWorld,float4(0,0,0,1));
			float4 objectPivotInWS113 = transform15_g375;
			float3 break9_g592 = objectPivotInWS113.xyz;
			float4 appendResult11_g592 = (float4(break9_g592.x , break9_g592.z , 0.0 , 0.0));
			float4 pivotXY50_g592 = appendResult11_g592;
			#ifdef _UVMASK_ON
				float staticSwitch510 = ( v.texcoord.xy.y * 0.1 );
			#else
				float staticSwitch510 = 0.0;
			#endif
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			#ifdef _BAKEDMASK_ON
				float staticSwitch517 = ( ( 1.0 - v.color.g ) * 0.05 );
			#else
				float staticSwitch517 = ( staticSwitch510 + frac( ( ( ase_worldPos.x + ase_worldPos.z ) * 0.02 ) ) );
			#endif
			float temp_output_204_0_g592 = staticSwitch517;
			float3 break12_g592 = globalGustDirection107.xyz;
			float4 appendResult13_g592 = (float4(break12_g592.x , break12_g592.z , 0.0 , 0.0));
			float4 gustDirection53_g592 = appendResult13_g592;
			float time109_g592 = _Time.y;
			float globalGustSpeed105 = max( ( GlobalWindDirectionAndStrength.z * _WindDirectionAndStrength.z ) , 0.01 );
			float4 tex2DNode6_g593 = tex2Dlod( _GustNoise, float4( ( ( ( pivotXY50_g592.xy + ( temp_output_204_0_g592 * 2.0 ) ) * 0.01 ) - ( gustDirection53_g592.xy * time109_g592 * globalGustSpeed105 ) ), 0, 0.0) );
			float gustNoise153_g592 = max( tex2DNode6_g593.r , 0.01 );
			float globalGustStrength106 = max( ( GlobalWindDirectionAndStrength.w * _WindDirectionAndStrength.w ) , 0.01 );
			float temp_output_18_0_g596 = ( gustNoise153_g592 * globalGustStrength106 );
			float clampResult16_g596 = clamp( temp_output_18_0_g596 , 0.1 , 0.9 );
			#ifdef _BAKEDMASK_ON
				float staticSwitch9_g512 = ( 1.0 - v.color.a );
			#else
				float staticSwitch9_g512 = 1.0;
			#endif
			#ifdef _UVMASK_ON
				float staticSwitch7_g512 = ( 1.0 - v.texcoord.xy.y );
			#else
				float staticSwitch7_g512 = 1.0;
			#endif
			float3 ase_vertex3Pos = v.vertex.xyz;
			float clampResult16_g512 = clamp( ase_vertex3Pos.y , 0.0 , 1.0 );
			#ifdef _VERTEXPOSITIONMASK_ON
				float staticSwitch8_g512 = ( 1.0 - clampResult16_g512 );
			#else
				float staticSwitch8_g512 = 1.0;
			#endif
			float relativeHeightMask101_g592 = max( ( 1.0 - ( staticSwitch9_g512 * staticSwitch7_g512 * staticSwitch8_g512 ) ) , 0.001 );
			float2 break2_g595 = ( pivotXY50_g592.xy * 10.0 );
			float clampResult8_g595 = clamp( pow( frac( ( break2_g595.x * break2_g595.y ) ) , 2.0 ) , ( 1.0 - _StiffnessVariation ) , 1.0 );
			float randomStiffness90_g592 = clampResult8_g595;
			#ifdef _BAKEDMASK_ON
				float staticSwitch486 = max( v.color.r , 0.01 );
			#else
				float staticSwitch486 = 1.0;
			#endif
			float vertexMask103_g592 = staticSwitch486;
			float gustStrength105_g592 = ( ( temp_output_18_0_g596 * ( 1.0 - clampResult16_g596 ) * 1.5 ) * relativeHeightMask101_g592 * randomStiffness90_g592 * vertexMask103_g592 );
			float gustStrengthAtPosition118 = gustStrength105_g592;
			float gustStrength25_g598 = gustStrengthAtPosition118;
			float3 scaledGustDirection155_g598 = ( globalGustDirection107.xyz * gustStrength25_g598 );
			float3 normalizeResult13_g598 = normalize( cross( float3(0,1,0) , scaledGustDirection155_g598 ) );
			float3 verticalAxis159_g598 = normalizeResult13_g598;
			float3 pivot118_g598 = objectPivotInWS113.xyz;
			float3 vertexOffset162_g598 = ( ase_worldPos - pivot118_g598 );
			float dotResult129_g598 = dot( verticalAxis159_g598 , vertexOffset162_g598 );
			float clampResult136_g598 = clamp( ( dotResult129_g598 * 1.0 ) , 0.0 , 1.0 );
			float3 lerpResult142_g598 = lerp( float3(0,1,0) , float3(0,-1,0) , clampResult136_g598);
			float3 horizontalAxis164_g598 = lerpResult142_g598;
			float3 lerpResult144_g598 = lerp( horizontalAxis164_g598 , verticalAxis159_g598 , v.color.b);
			#ifdef _BAKEDMASK_ON
				float3 staticSwitch178_g598 = lerpResult144_g598;
			#else
				float3 staticSwitch178_g598 = verticalAxis159_g598;
			#endif
			float3 rotationAxis166_g598 = staticSwitch178_g598;
			float3 rotatedValue12_g598 = RotateAroundAxis( pivot118_g598, ase_worldPos, rotationAxis166_g598, ( gustStrength25_g598 * 1.0 ) );
			float3 positionWithGust169_g598 = rotatedValue12_g598;
			float globalShiverSpeed149 = max( ( GlobalShiver.x * _Shiver.x ) , 0.01 );
			float4 tex2DNode11_g597 = tex2Dlod( _ShiverNoise, float4( ( ( pivotXY50_g592.xy + ( temp_output_204_0_g592 * 2.0 ) ) - ( gustDirection53_g592.xy * time109_g592 * globalShiverSpeed149 ) ), 0, 0.0) );
			float4 appendResult6_g597 = (float4(tex2DNode11_g597.r , tex2DNode11_g597.g , tex2DNode11_g597.b , 0.0));
			float4 temp_cast_10 = (0.5).xxxx;
			float4 shiverNoise155_g592 = ( ( appendResult6_g597 - temp_cast_10 ) * 2.0 );
			float temp_output_28_0_g594 = 0.2;
			float3 appendResult32_g594 = (float3(1.0 , temp_output_28_0_g594 , 1.0));
			float3 temp_output_29_0_g594 = ( shiverNoise155_g592.xyz * appendResult32_g594 );
			float2 break10_g594 = gustDirection53_g592.xy;
			float4 appendResult4_g594 = (float4(break10_g594.x , temp_output_28_0_g594 , break10_g594.y , 0.0));
			float temp_output_21_0_g594 = gustStrength105_g592;
			float clampResult45_g594 = clamp( ( ( temp_output_21_0_g594 * 0.8 ) + 0.2 ) , 0.0 , 1.0 );
			float4 lerpResult26_g594 = lerp( float4( temp_output_29_0_g594 , 0.0 ) , appendResult4_g594 , clampResult45_g594);
			float4 shiverDirectionAtPosition132 = lerpResult26_g594;
			float3 shiverDirection29_g598 = shiverDirectionAtPosition132.xyz;
			float temp_output_5_0_g594 = length( temp_output_29_0_g594 );
			float globalShiverStrength141 = max( ( GlobalShiver.y * _Shiver.y ) , 0.01 );
			float temp_output_6_0_g594 = globalShiverStrength141;
			float shiverStrengthAtPosition125 = ( relativeHeightMask101_g592 * ( ( temp_output_21_0_g594 * 0.5 ) + ( temp_output_5_0_g594 * temp_output_6_0_g594 ) ) * vertexMask103_g592 * randomStiffness90_g592 );
			float shiverStrength30_g598 = shiverStrengthAtPosition125;
			float3 shiverPositionOffset170_g598 = ( shiverDirection29_g598 * shiverStrength30_g598 );
			float4 appendResult87 = (float4(( positionWithGust169_g598 + shiverPositionOffset170_g598 ) , 1.0));
			float4 transform29 = mul(unity_WorldToObject,appendResult87);
			float4 vertexPositionWithWind411 = transform29;
			v.vertex.xyz = vertexPositionWithWind411.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap387 = i.uv_texcoord;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap387 ), _BumpScale );
			float _Hue_Instance = UNITY_ACCESS_INSTANCED_PROP(_Hue_arr, _Hue);
			float2 uv_MainTex1 = i.uv_texcoord;
			float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex1 );
			float4 albedo424 = tex2DNode1;
			float3 hsvTorgb476 = RGBToHSV( albedo424.rgb );
			float _Saturation_Instance = UNITY_ACCESS_INSTANCED_PROP(_Saturation_arr, _Saturation);
			float _Lightness_Instance = UNITY_ACCESS_INSTANCED_PROP(_Lightness_arr, _Lightness);
			float3 hsvTorgb480 = HSVToRGB( float3(( _Hue_Instance + hsvTorgb476.x ),( hsvTorgb476.y + _Saturation_Instance ),( hsvTorgb476.z + _Lightness_Instance )) );
			float4 transform15_g375 = mul(unity_ObjectToWorld,float4(0,0,0,1));
			float4 objectPivotInWS113 = transform15_g375;
			float3 break9_g592 = objectPivotInWS113.xyz;
			float4 appendResult11_g592 = (float4(break9_g592.x , break9_g592.z , 0.0 , 0.0));
			float4 pivotXY50_g592 = appendResult11_g592;
			#ifdef _UVMASK_ON
				float staticSwitch510 = ( i.uv_texcoord.y * 0.1 );
			#else
				float staticSwitch510 = 0.0;
			#endif
			float3 ase_worldPos = i.worldPos;
			#ifdef _BAKEDMASK_ON
				float staticSwitch517 = ( ( 1.0 - i.vertexColor.g ) * 0.05 );
			#else
				float staticSwitch517 = ( staticSwitch510 + frac( ( ( ase_worldPos.x + ase_worldPos.z ) * 0.02 ) ) );
			#endif
			float temp_output_204_0_g592 = staticSwitch517;
			float4 break15_g376 = ( GlobalWindDirectionAndStrength * _WindDirectionAndStrength );
			float4 appendResult3_g376 = (float4(break15_g376.x , 0.0 , break15_g376.y , 0.0));
			float clampResult36_g376 = clamp( ( length( appendResult3_g376 ) * 1000.0 ) , 0.0 , 1.0 );
			float4 lerpResult29_g376 = lerp( float4( float3(0.7,0,0.3) , 0.0 ) , appendResult3_g376 , clampResult36_g376);
			float4 normalizeResult5_g376 = normalize( lerpResult29_g376 );
			float4 globalGustDirection107 = normalizeResult5_g376;
			float3 break12_g592 = globalGustDirection107.xyz;
			float4 appendResult13_g592 = (float4(break12_g592.x , break12_g592.z , 0.0 , 0.0));
			float4 gustDirection53_g592 = appendResult13_g592;
			float time109_g592 = _Time.y;
			float globalGustSpeed105 = max( ( GlobalWindDirectionAndStrength.z * _WindDirectionAndStrength.z ) , 0.01 );
			float4 tex2DNode6_g593 = tex2D( _GustNoise, ( ( ( pivotXY50_g592.xy + ( temp_output_204_0_g592 * 2.0 ) ) * 0.01 ) - ( gustDirection53_g592.xy * time109_g592 * globalGustSpeed105 ) ) );
			float gustNoise153_g592 = max( tex2DNode6_g593.r , 0.01 );
			float globalGustStrength106 = max( ( GlobalWindDirectionAndStrength.w * _WindDirectionAndStrength.w ) , 0.01 );
			float temp_output_18_0_g596 = ( gustNoise153_g592 * globalGustStrength106 );
			float clampResult16_g596 = clamp( temp_output_18_0_g596 , 0.1 , 0.9 );
			#ifdef _BAKEDMASK_ON
				float staticSwitch9_g512 = ( 1.0 - i.vertexColor.a );
			#else
				float staticSwitch9_g512 = 1.0;
			#endif
			#ifdef _UVMASK_ON
				float staticSwitch7_g512 = ( 1.0 - i.uv_texcoord.y );
			#else
				float staticSwitch7_g512 = 1.0;
			#endif
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float clampResult16_g512 = clamp( ase_vertex3Pos.y , 0.0 , 1.0 );
			#ifdef _VERTEXPOSITIONMASK_ON
				float staticSwitch8_g512 = ( 1.0 - clampResult16_g512 );
			#else
				float staticSwitch8_g512 = 1.0;
			#endif
			float relativeHeightMask101_g592 = max( ( 1.0 - ( staticSwitch9_g512 * staticSwitch7_g512 * staticSwitch8_g512 ) ) , 0.001 );
			float2 break2_g595 = ( pivotXY50_g592.xy * 10.0 );
			float clampResult8_g595 = clamp( pow( frac( ( break2_g595.x * break2_g595.y ) ) , 2.0 ) , ( 1.0 - _StiffnessVariation ) , 1.0 );
			float randomStiffness90_g592 = clampResult8_g595;
			#ifdef _BAKEDMASK_ON
				float staticSwitch486 = max( i.vertexColor.r , 0.01 );
			#else
				float staticSwitch486 = 1.0;
			#endif
			float vertexMask103_g592 = staticSwitch486;
			float gustStrength105_g592 = ( ( temp_output_18_0_g596 * ( 1.0 - clampResult16_g596 ) * 1.5 ) * relativeHeightMask101_g592 * randomStiffness90_g592 * vertexMask103_g592 );
			float gustStrengthAtPosition118 = gustStrength105_g592;
			float3 temp_cast_7 = (gustStrengthAtPosition118).xxx;
			#ifdef _DEBUGGUST_ON
				float3 staticSwitch525 = temp_cast_7;
			#else
				float3 staticSwitch525 = hsvTorgb480;
			#endif
			o.Albedo = staticSwitch525;
			float2 uv_MetallicGlossMap529 = i.uv_texcoord;
			float4 tex2DNode529 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap529 );
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch536 = tex2DNode529.r;
			#else
				float staticSwitch536 = _Metallic;
			#endif
			float MetallicMap530 = staticSwitch536;
			o.Metallic = MetallicMap530;
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch534 = (_GlossRemap.x + (tex2DNode529.a - 0.0) * (_GlossRemap.y - _GlossRemap.x) / (1.0 - 0.0));
			#else
				float staticSwitch534 = _Glossiness;
			#endif
			float GlossMap531 = staticSwitch534;
			o.Smoothness = GlossMap531;
			float2 uv_OcclusionMap398 = i.uv_texcoord;
			o.Occlusion = (_OcclusionRemap.x + (tex2D( _OcclusionMap, uv_OcclusionMap398 ).r - 0.0) * (_OcclusionRemap.y - _OcclusionRemap.x) / (1.0 - 0.0));
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
129;67.5;960;851;3693.074;-10.61243;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;519;-5492.867,2314.957;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;515;-5309.4,2129.138;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;520;-5221.043,2327.681;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;512;-5018.112,2315.978;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.02;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;117;-4019.222,1817.138;Inherit;False;1584.851;1651.966;Calculate wind strength at vertex position;18;440;438;265;132;125;118;135;114;110;142;200;55;146;202;150;111;486;487;Wind Strength;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;514;-4857.743,2524.155;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;507;-5218.957,2051.411;Float;False;Constant;_Float9;Float 9;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;116;-4008.191,1529.564;Inherit;False;672.4673;168.7953;Comment;2;89;113;World Space Object Pivot;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;509;-5060.844,2213.446;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;510;-4839.229,2132.037;Float;False;Property;_UVMask;UV Mask;2;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;518;-4752.581,2301.076;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;112;-4004.226,860.4534;Inherit;False;652.8523;550.6835;Global Wind Parameters;5;106;105;107;141;149;Global Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;452;-3962.294,1033.771;Inherit;False;GlobalWindParameters;16;;376;ef55991c5ff9f3747b20a326fd322e36;0;0;5;FLOAT;11;FLOAT;10;FLOAT;8;FLOAT;6;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;89;-3958.191,1579.564;Inherit;False;GetPivotInWorldSpace;-1;;375;264e0929a81902742a5a4e0e0a62ac57;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;438;-4067.733,1919.095;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;516;-4650.812,2547.191;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-3626.28,1007.885;Float;False;globalShiverStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-3578.724,1583.359;Float;False;objectPivotInWS;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;487;-3839.097,1881.642;Float;False;Constant;_Float1;Float 1;17;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;526;-4445.69,2524.621;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-3621.772,922.121;Float;False;globalShiverSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-3607.401,1174.849;Float;False;globalGustStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;-3605.74,1085.918;Float;False;globalGustSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;513;-4466.498,2214.177;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;440;-3826.545,1971.479;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;107;-3613.996,1265.174;Float;False;globalGustDirection;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;517;-4298.371,2388.069;Float;False;Property;_BakedMask;Baked Mask;9;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;55;-3833.418,3084.348;Float;True;Global;_GustNoise;_GustNoise;28;0;Create;True;0;0;False;0;None;156c5c844ac14b042b7dacdcfcd0981b;False;black;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-3877.668,2473.236;Inherit;False;141;globalShiverStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-3865.564,2825.972;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;495;-3837.048,2139.859;Inherit;False;GetHeightMask;3;;512;d64c2e0885795d34cb76988e77bd8660;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-3854.028,2397.19;Inherit;False;105;globalGustSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;486;-3627.696,1925.742;Float;False;Property;_BakedMask;Baked Mask;19;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3841.733,2996.601;Inherit;False;113;objectPivotInWS;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-3902.15,2214.136;Float;False;Property;_StiffnessVariation;StiffnessVariation;22;0;Create;True;0;0;False;0;0;0.3;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-3865.651,2908.544;Inherit;False;106;globalGustStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-3868.289,2318.347;Inherit;False;149;globalShiverSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;146;-3782.755,2747.397;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;135;-3839.251,2553.036;Float;True;Global;_ShiverNoise;ShiverNoise;29;0;Create;False;0;0;False;0;None;66dd7d1835f20b8419dbc5544a12688a;False;gray;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;423;-3965.596,-228.4894;Inherit;False;748;273;;3;424;1;425;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;528;-3365.866,2424.6;Inherit;False;GetWindStrength;-1;;592;81b67046328b6f44f9bbfde7e0fba2b2;0;14;204;FLOAT;0;False;171;FLOAT;0.2;False;165;FLOAT;1;False;164;FLOAT;1;False;114;FLOAT;0;False;60;FLOAT;1;False;55;FLOAT;1;False;49;FLOAT;0.1;False;24;SAMPLER2D;0;False;4;FLOAT;0;False;5;FLOAT3;0,0,0;False;28;FLOAT;1;False;1;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0,0;False;4;FLOAT;138;FLOAT;43;FLOAT4;44;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;120;-2242.677,1836.51;Inherit;False;1849.186;659.9193;Apply wind displacement to vertex position;10;29;87;411;88;86;109;129;133;115;119;Wind Displacement;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-2892.449,2458.837;Float;False;shiverStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-2894.088,2714.748;Float;False;gustStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;-2892.238,2587.098;Float;False;shiverDirectionAtPosition;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;1;-3915.596,-177.1894;Inherit;True;Property;_MainTex;Albedo;11;1;[NoScaleOffset];Create;False;0;0;False;0;-1;None;e3286ee1934575441b7b2b3399095ba9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;109;-2148.943,2066.18;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-2122.556,2363.043;Inherit;False;113;objectPivotInWS;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;431;2379.381,1852.999;Inherit;False;1795.285;1312.366;;20;0;416;426;398;481;387;480;478;477;479;476;474;469;475;427;482;483;505;538;539;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2163.99,2143.075;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-2183.851,1990.018;Inherit;False;132;shiverDirectionAtPosition;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;86;-2076.601,2219.387;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;424;-3470.022,-165.6478;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-2185.187,1906.63;Inherit;False;125;shiverStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;523;-1495.972,2045.205;Inherit;False;ApplyWindDisplacement;0;;598;739735a3fc284b84ca40e29145dfcbfd;0;6;20;FLOAT;1;False;19;FLOAT3;1,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT;0;False;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector2Node;533;-3860.343,534.8194;Inherit;False;Property;_GlossRemap;Gloss Remap;24;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;529;-3971.221,214.9668;Inherit;True;Property;_MetallicGlossMap;Metallic Gloss Map;20;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;427;2466.197,1982.999;Inherit;False;424;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1316.945,2258.902;Float;False;Constant;_Float2;Float 2;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;537;-3581.949,184.0237;Inherit;False;Property;_Metallic;_Metallic;26;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;469;2660.671,1887.231;Float;False;InstancedProperty;_Hue;Hue;8;0;Create;True;0;0;False;0;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-1130.095,2089.546;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;532;-3482.502,487.9381;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;476;2906.085,1993.399;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;475;2848.442,2154.948;Float;False;InstancedProperty;_Saturation;Saturation;9;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;474;2847.893,2256.001;Float;False;InstancedProperty;_Lightness;Lightness;10;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;535;-3569.034,359.2466;Inherit;False;Property;_Glossiness;_Glossiness;25;0;Create;True;0;0;False;0;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;3193.893,2181.001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;479;3193.355,1935.357;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;536;-3249.456,247.5223;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;23;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;29;-955.1426,2082.968;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;478;3192.893,2063.001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;534;-3238.296,424.183;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;23;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;531;-2925.318,465.8126;Inherit;False;GlossMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;417;-88.11959,2661.995;Inherit;False;2348.442;500.7477;;15;429;384;383;282;428;269;280;270;285;281;283;284;273;267;274;Gust Highlight;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;482;3035.776,2813.884;Float;False;Property;_OcclusionRemap;Occlusion Remap;14;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;414;-214.4057,1844.116;Inherit;False;2449.423;723.1951;Comment;12;415;332;413;331;324;333;412;323;328;327;329;330;Scale Y based on distance from camera;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;505;3781.73,2123.887;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;480;3400.527,2013.369;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;425;-3455.624,-44.04722;Float;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;398;2953.962,2607.665;Inherit;True;Property;_OcclusionMap;Occlusion Map;15;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;921808f136231024aba9ab8b85177107;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;530;-2933.868,209.938;Inherit;False;MetallicMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;481;2882.703,2407.475;Float;False;Property;_BumpScale;Bump Scale;12;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;-682.4496,2143.209;Float;False;vertexPositionWithWind;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;331;1090.333,2062.014;Float;False;Constant;_Vector2;Vector 2;12;0;Create;True;0;0;False;0;1,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;415;1878.953,1968.943;Float;False;scaledVertexPositionWithWind;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;327;-48.41556,2370.635;Float;False;Constant;_Float13;Float 13;12;0;Create;True;0;0;False;0;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;284;415.1168,3080.581;Float;False;Constant;_Float12;Float 12;12;0;Create;True;0;0;False;0;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;412;-164.4057,2272.016;Inherit;False;411;vertexPositionWithWind;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;428;1211.517,2738.598;Inherit;False;424;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;387;3323.612,2252.039;Inherit;True;Property;_BumpMap;Bump Map;13;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;34d05954bc21b6444bf61e2e27f57ac3;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;384;1466.582,2952.339;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;329;748.5842,2308.635;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;429;1896.738,2865.792;Float;False;coloredGustHighlightAtPosition;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;538;3433.65,2575.048;Inherit;False;531;GlossMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;1715.182,2866.14;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;274;112.6065,3017.734;Float;False;Constant;_Float11;Float 11;11;0;Create;True;0;0;False;0;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;281;726.3406,3065.349;Inherit;False;265;gustHighlightAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;330;1402.133,2091.915;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;332;1631.101,1901.984;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;282;1495.38,2835.522;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;333;522.062,2302.018;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;539;3417.224,2469.074;Inherit;False;530;MetallicMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;426;3440.167,2892.226;Inherit;False;425;alpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;324;998.5844,2301.635;Float;False;distanceFromCamera;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;273;377.5147,2951.245;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;525;4247.254,2241.41;Float;False;Property;_DebugGust;Debug Gust;21;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;483;3422.686,2675.327;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-2891.923,2329.183;Float;False;gustHighlightAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;270;712.6763,2711.995;Float;False;Property;_GustHighlightColor;GustHighlightColor;27;1;[HDR];Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;267;-38.1196,2902.234;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;285;786.6329,2901.595;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-48.41556,2460.635;Float;False;Constant;_Float14;Float 14;12;0;Create;True;0;0;False;0;30;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;323;160.7325,2275.633;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;413;1078.301,1894.116;Inherit;False;411;vertexPositionWithWind;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;283;604.1811,2967.315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;416;3348.432,3003.416;Inherit;False;411;vertexPositionWithWind;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;269;1187.444,2851.734;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;280;989.8531,2902.225;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3794.943,2383.357;Float;False;True;-1;2;VisualDesignCafe.Nature.Editor.NatureMaterialEditor;0;0;Standard;Open World Nature/Vegetation;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;False;True;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;ForwardOnly;14;all;True;True;True;False;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Spherical;False;Absolute;0;;7;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;520;0;519;1
WireConnection;520;1;519;3
WireConnection;512;0;520;0
WireConnection;509;0;515;2
WireConnection;510;1;507;0
WireConnection;510;0;509;0
WireConnection;518;0;512;0
WireConnection;516;0;514;2
WireConnection;141;0;452;10
WireConnection;113;0;89;0
WireConnection;526;0;516;0
WireConnection;149;0;452;11
WireConnection;106;0;452;6
WireConnection;105;0;452;8
WireConnection;513;0;510;0
WireConnection;513;1;518;0
WireConnection;440;0;438;1
WireConnection;107;0;452;0
WireConnection;517;1;513;0
WireConnection;517;0;526;0
WireConnection;486;1;487;0
WireConnection;486;0;440;0
WireConnection;528;204;517;0
WireConnection;528;165;495;0
WireConnection;528;164;486;0
WireConnection;528;114;202;0
WireConnection;528;60;150;0
WireConnection;528;55;111;0
WireConnection;528;49;142;0
WireConnection;528;24;135;0
WireConnection;528;4;146;0
WireConnection;528;5;110;0
WireConnection;528;28;200;0
WireConnection;528;1;114;0
WireConnection;528;3;55;0
WireConnection;125;0;528;43
WireConnection;118;0;528;0
WireConnection;132;0;528;44
WireConnection;424;0;1;0
WireConnection;523;20;129;0
WireConnection;523;19;133;0
WireConnection;523;6;109;0
WireConnection;523;7;119;0
WireConnection;523;1;86;0
WireConnection;523;3;115;0
WireConnection;87;0;523;0
WireConnection;87;3;88;0
WireConnection;532;0;529;4
WireConnection;532;3;533;1
WireConnection;532;4;533;2
WireConnection;476;0;427;0
WireConnection;477;0;476;3
WireConnection;477;1;474;0
WireConnection;479;0;469;0
WireConnection;479;1;476;1
WireConnection;536;1;537;0
WireConnection;536;0;529;1
WireConnection;29;0;87;0
WireConnection;478;0;476;2
WireConnection;478;1;475;0
WireConnection;534;1;535;0
WireConnection;534;0;532;0
WireConnection;531;0;534;0
WireConnection;480;0;479;0
WireConnection;480;1;478;0
WireConnection;480;2;477;0
WireConnection;425;0;1;4
WireConnection;530;0;536;0
WireConnection;411;0;29;0
WireConnection;415;0;332;0
WireConnection;387;5;481;0
WireConnection;329;0;333;0
WireConnection;429;0;383;0
WireConnection;383;0;282;0
WireConnection;383;1;384;4
WireConnection;330;0;331;1
WireConnection;330;1;324;0
WireConnection;330;2;331;3
WireConnection;332;0;413;0
WireConnection;332;1;330;0
WireConnection;282;0;428;0
WireConnection;282;1;269;0
WireConnection;333;0;323;0
WireConnection;324;0;329;0
WireConnection;273;0;267;0
WireConnection;273;1;274;0
WireConnection;525;1;480;0
WireConnection;525;0;505;0
WireConnection;483;0;398;1
WireConnection;483;3;482;1
WireConnection;483;4;482;2
WireConnection;265;0;528;138
WireConnection;285;0;283;0
WireConnection;323;2;412;0
WireConnection;323;0;327;0
WireConnection;323;1;328;0
WireConnection;283;0;273;0
WireConnection;283;1;284;0
WireConnection;269;0;270;0
WireConnection;269;1;280;0
WireConnection;280;0;285;0
WireConnection;280;1;281;0
WireConnection;0;0;525;0
WireConnection;0;1;387;0
WireConnection;0;3;539;0
WireConnection;0;4;538;0
WireConnection;0;5;483;0
WireConnection;0;10;426;0
WireConnection;0;11;416;0
ASEEND*/
//CHKSM=83E27804A122BDFC388CD4DD7B9B8764E1850434