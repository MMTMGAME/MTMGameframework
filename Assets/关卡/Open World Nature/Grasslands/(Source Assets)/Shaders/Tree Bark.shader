// Upgrade NOTE: upgraded instancing buffer 'OpenWorldNatureTreeBark' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Open World Nature/Tree Bark"
{
	Properties
	{
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
		[Toggle(_BAKEDMASK_ON)] _BakedMask("Baked Mask", Float) = 1
		_StiffnessVariation("Stiffness Variation", Range( 0 , 0.99)) = 0
		[NoScaleOffset]_MetallicGlossMap("Metallic Gloss Map", 2D) = "white" {}
		[Toggle(_METALLICGLOSSMAP_ON)] _METALLICGLOSSMAP("_METALLICGLOSSMAP", Float) = 0
		_GlossRemap("Gloss Remap", Vector) = (0,1,0,0)
		_Glossiness("_Glossiness", Range( 0 , 1)) = 0.15
		_Metallic("_Metallic", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		ColorMask RGB
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _BAKEDMASK_ON
		#pragma shader_feature _METALLICGLOSSMAP_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
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

		UNITY_INSTANCING_BUFFER_START(OpenWorldNatureTreeBark)
			UNITY_DEFINE_INSTANCED_PROP(float, _Hue)
#define _Hue_arr OpenWorldNatureTreeBark
			UNITY_DEFINE_INSTANCED_PROP(float, _Saturation)
#define _Saturation_arr OpenWorldNatureTreeBark
			UNITY_DEFINE_INSTANCED_PROP(float, _Lightness)
#define _Lightness_arr OpenWorldNatureTreeBark
		UNITY_INSTANCING_BUFFER_END(OpenWorldNatureTreeBark)


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
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 break15_g397 = ( GlobalWindDirectionAndStrength * _WindDirectionAndStrength );
			float4 appendResult3_g397 = (float4(break15_g397.x , 0.0 , break15_g397.y , 0.0));
			float clampResult36_g397 = clamp( ( length( appendResult3_g397 ) * 1000.0 ) , 0.0 , 1.0 );
			float4 lerpResult29_g397 = lerp( float4( float3(0.7,0,0.3) , 0.0 ) , appendResult3_g397 , clampResult36_g397);
			float4 normalizeResult5_g397 = normalize( lerpResult29_g397 );
			float4 globalGustDirection107 = normalizeResult5_g397;
			float4 transform15_g396 = mul(unity_ObjectToWorld,float4(0,0,0,1));
			float4 objectPivotInWS113 = transform15_g396;
			float3 break9_g409 = objectPivotInWS113.xyz;
			float4 appendResult11_g409 = (float4(break9_g409.x , break9_g409.z , 0.0 , 0.0));
			float4 pivotXY50_g409 = appendResult11_g409;
			float temp_output_204_0_g409 = 0.0;
			float3 break12_g409 = globalGustDirection107.xyz;
			float4 appendResult13_g409 = (float4(break12_g409.x , break12_g409.z , 0.0 , 0.0));
			float4 gustDirection53_g409 = appendResult13_g409;
			float branchPhase447 = v.color.r;
			float edgeFlutter450 = v.color.g;
			float time109_g409 = ( ( 0.3 * branchPhase447 ) + _Time.y + ( edgeFlutter450 * 0.5 ) );
			float globalGustSpeed105 = max( ( GlobalWindDirectionAndStrength.z * _WindDirectionAndStrength.z ) , 0.01 );
			float4 tex2DNode6_g410 = tex2Dlod( _GustNoise, float4( ( ( ( pivotXY50_g409.xy + ( temp_output_204_0_g409 * 2.0 ) ) * 0.01 ) - ( gustDirection53_g409.xy * time109_g409 * globalGustSpeed105 ) ), 0, 0.0) );
			float gustNoise153_g409 = max( tex2DNode6_g410.r , 0.01 );
			float globalGustStrength106 = max( ( GlobalWindDirectionAndStrength.w * _WindDirectionAndStrength.w ) , 0.01 );
			float temp_output_18_0_g413 = ( gustNoise153_g409 * globalGustStrength106 );
			float clampResult16_g413 = clamp( temp_output_18_0_g413 , 0.1 , 0.9 );
			float secondaryFactor452 = v.texcoord1.xy.y;
			float relativeHeightMask101_g409 = secondaryFactor452;
			float2 break2_g412 = ( pivotXY50_g409.xy * 10.0 );
			float clampResult8_g412 = clamp( pow( frac( ( break2_g412.x * break2_g412.y ) ) , 2.0 ) , ( 1.0 - _StiffnessVariation ) , 1.0 );
			float randomStiffness90_g409 = clampResult8_g412;
			float vertexMask103_g409 = 1.0;
			float gustStrength105_g409 = ( ( temp_output_18_0_g413 * ( 1.0 - clampResult16_g413 ) * 1.5 ) * relativeHeightMask101_g409 * randomStiffness90_g409 * vertexMask103_g409 );
			float gustStrengthAtPosition118 = gustStrength105_g409;
			float gustStrength25_g415 = gustStrengthAtPosition118;
			float3 scaledGustDirection155_g415 = ( globalGustDirection107.xyz * gustStrength25_g415 );
			float3 positionWithGust169_g415 = ( ase_worldPos + ( scaledGustDirection155_g415 * 10.0 ) );
			float globalShiverSpeed149 = max( ( GlobalShiver.x * _Shiver.x ) , 0.01 );
			float4 tex2DNode11_g414 = tex2Dlod( _ShiverNoise, float4( ( ( pivotXY50_g409.xy + ( temp_output_204_0_g409 * 2.0 ) ) - ( gustDirection53_g409.xy * time109_g409 * globalShiverSpeed149 ) ), 0, 0.0) );
			float4 appendResult6_g414 = (float4(tex2DNode11_g414.r , tex2DNode11_g414.g , tex2DNode11_g414.b , 0.0));
			float4 temp_cast_10 = (0.5).xxxx;
			float4 shiverNoise155_g409 = ( ( appendResult6_g414 - temp_cast_10 ) * 2.0 );
			float temp_output_28_0_g411 = 0.2;
			float3 appendResult32_g411 = (float3(1.0 , temp_output_28_0_g411 , 1.0));
			float3 temp_output_29_0_g411 = ( shiverNoise155_g409.xyz * appendResult32_g411 );
			float2 break10_g411 = gustDirection53_g409.xy;
			float4 appendResult4_g411 = (float4(break10_g411.x , temp_output_28_0_g411 , break10_g411.y , 0.0));
			float temp_output_21_0_g411 = gustStrength105_g409;
			float clampResult45_g411 = clamp( ( ( temp_output_21_0_g411 * 0.8 ) + 0.2 ) , 0.0 , 1.0 );
			float4 lerpResult26_g411 = lerp( float4( temp_output_29_0_g411 , 0.0 ) , appendResult4_g411 , clampResult45_g411);
			float4 shiverDirectionAtPosition132 = lerpResult26_g411;
			float3 shiverDirection29_g415 = shiverDirectionAtPosition132.xyz;
			float temp_output_5_0_g411 = length( temp_output_29_0_g411 );
			float globalShiverStrength141 = max( ( GlobalShiver.y * _Shiver.y ) , 0.01 );
			float temp_output_6_0_g411 = globalShiverStrength141;
			float shiverStrengthAtPosition125 = ( relativeHeightMask101_g409 * ( ( temp_output_21_0_g411 * 0.5 ) + ( temp_output_5_0_g411 * temp_output_6_0_g411 ) ) * vertexMask103_g409 * randomStiffness90_g409 );
			float shiverStrength30_g415 = ( edgeFlutter450 * shiverStrengthAtPosition125 );
			float clampResult98_g415 = clamp( ase_vertex3Pos.y , 0.0 , 1.0 );
			float3 shiverPositionOffset170_g415 = ( shiverDirection29_g415 * shiverStrength30_g415 * clampResult98_g415 * 10.0 );
			float4 appendResult87 = (float4(( positionWithGust169_g415 + shiverPositionOffset170_g415 ) , 1.0));
			float4 transform29 = mul(unity_WorldToObject,appendResult87);
			float4 vertexPositionWithWind411 = transform29;
			#ifdef _BAKEDMASK_ON
				float4 staticSwitch536 = vertexPositionWithWind411;
			#else
				float4 staticSwitch536 = float4( ase_vertex3Pos , 0.0 );
			#endif
			v.vertex.xyz = staticSwitch536.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap387 = i.uv_texcoord;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap387 ), _BumpScale );
			float _Hue_Instance = UNITY_ACCESS_INSTANCED_PROP(_Hue_arr, _Hue);
			float2 uv_MainTex1 = i.uv_texcoord;
			float4 albedo424 = tex2D( _MainTex, uv_MainTex1 );
			float3 hsvTorgb529 = RGBToHSV( albedo424.rgb );
			float _Saturation_Instance = UNITY_ACCESS_INSTANCED_PROP(_Saturation_arr, _Saturation);
			float _Lightness_Instance = UNITY_ACCESS_INSTANCED_PROP(_Lightness_arr, _Lightness);
			float3 hsvTorgb535 = HSVToRGB( float3(( _Hue_Instance + hsvTorgb529.x ),( hsvTorgb529.y + _Saturation_Instance ),( hsvTorgb529.z + _Lightness_Instance )) );
			o.Albedo = hsvTorgb535;
			float2 uv_MetallicGlossMap539 = i.uv_texcoord;
			float4 tex2DNode539 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap539 );
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch543 = tex2DNode539.r;
			#else
				float staticSwitch543 = _Metallic;
			#endif
			float MetallicMap546 = staticSwitch543;
			o.Metallic = MetallicMap546;
			#ifdef _METALLICGLOSSMAP_ON
				float staticSwitch544 = (_GlossRemap.x + (tex2DNode539.a - 0.0) * (_GlossRemap.y - _GlossRemap.x) / (1.0 - 0.0));
			#else
				float staticSwitch544 = _Glossiness;
			#endif
			float GlossMap545 = staticSwitch544;
			o.Smoothness = GlossMap545;
			float2 uv_OcclusionMap398 = i.uv_texcoord;
			o.Occlusion = (_OcclusionRemap.x + (tex2D( _OcclusionMap, uv_OcclusionMap398 ).r - 0.0) * (_OcclusionRemap.y - _OcclusionRemap.x) / (1.0 - 0.0));
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "VisualDesignCafe.Nature.Editor.NatureMaterialEditor"
}
/*ASEBEGIN
Version=17500
129;12.5;960;911;1480.176;271.9045;1;True;False
Node;AmplifyShaderEditor.VertexColorNode;455;-2175.484,2011.764;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;112;-1440.144,869.8115;Inherit;False;652.8523;550.6835;Global Wind Parameters;6;106;105;107;141;149;513;Global Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;-1444.109,1538.922;Inherit;False;672.4673;168.7953;Comment;2;89;113;World Space Object Pivot;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;447;-1921.484,2004.764;Float;False;branchPhase;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;450;-1907.484,2099.764;Float;False;edgeFlutter;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;453;-2174.484,2256.764;Inherit;False;1;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;89;-1394.109,1588.922;Inherit;False;GetPivotInWorldSpace;-1;;396;264e0929a81902742a5a4e0e0a62ac57;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;513;-1398.212,1043.129;Inherit;False;GlobalWindParameters;8;;397;ef55991c5ff9f3747b20a326fd322e36;0;0;5;FLOAT;11;FLOAT;10;FLOAT;8;FLOAT;6;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;476;-2149.694,2642.766;Inherit;False;447;branchPhase;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;483;-2123.08,2859.044;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;488;-1846.756,2657.411;Inherit;False;2;2;0;FLOAT;0.3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;107;-1049.914,1274.532;Float;False;globalGustDirection;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;146;-2108.356,2743.549;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-1014.642,1592.717;Float;False;objectPivotInWS;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;484;-1803.272,2854.985;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-1043.319,1184.207;Float;False;globalGustStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;-1041.658,1095.276;Float;False;globalGustSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-1062.198,1017.243;Float;False;globalShiverStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;117;-1455.14,1826.496;Inherit;False;1584.851;1651.966;Calculate wind strength at vertex position;18;265;132;125;118;135;114;110;200;55;202;150;111;469;466;475;142;515;477;Wind Strength;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;452;-1881.484,2343.764;Float;False;secondaryFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-1057.69,931.479;Float;False;globalShiverSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-1304.207,2327.705;Inherit;False;149;globalShiverSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-1301.482,2835.33;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-1289.946,2406.548;Inherit;False;105;globalGustSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;55;-1269.336,3093.706;Float;True;Global;_GustNoise;Gust Noise;13;0;Create;False;0;0;False;0;None;156c5c844ac14b042b7dacdcfcd0981b;False;black;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-1338.068,2223.494;Float;False;Property;_StiffnessVariation;Stiffness Variation;12;0;Create;True;0;0;False;0;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-1301.569,2917.902;Inherit;False;106;globalGustStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;-1478.674,2030.72;Inherit;False;452;secondaryFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-1277.651,3005.959;Inherit;False;113;objectPivotInWS;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-1303.586,2481.594;Inherit;False;141;globalShiverStrength;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;135;-1275.169,2559.79;Float;True;Global;_ShiverNoise;Shiver Noise;14;0;Create;False;0;0;False;0;None;66dd7d1835f20b8419dbc5544a12688a;False;gray;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;-1440.049,2712.914;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;515;-820.7837,2512.558;Inherit;False;GetWindStrength;-1;;409;81b67046328b6f44f9bbfde7e0fba2b2;0;14;204;FLOAT;0;False;171;FLOAT;0.2;False;165;FLOAT;1;False;164;FLOAT;1;False;114;FLOAT;0;False;60;FLOAT;1;False;55;FLOAT;1;False;49;FLOAT;0.1;False;24;SAMPLER2D;0;False;4;FLOAT;0;False;5;FLOAT3;0,0,0;False;28;FLOAT;1;False;1;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0,0;False;4;FLOAT;138;FLOAT;43;FLOAT4;44;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-328.3668,2468.195;Float;False;shiverStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;120;321.4048,1845.868;Inherit;False;1849.186;659.9193;Apply wind displacement to vertex position;12;29;87;411;88;86;109;129;133;119;480;485;481;Wind Displacement;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;378.8949,2024.788;Inherit;False;125;shiverStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;-328.1559,2596.456;Float;False;shiverDirectionAtPosition;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-330.006,2724.106;Float;False;gustStrengthAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;481;444.106,1922.102;Inherit;False;450;edgeFlutter;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;415.1388,2184.339;Inherit;False;107;globalGustDirection;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;480;772.605,1977.101;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;380.2308,2108.177;Inherit;False;132;shiverDirectionAtPosition;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;423;-1427.334,460.8242;Inherit;False;748;273;;2;424;1;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;86;487.4811,2337.546;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;119;400.0919,2261.234;Inherit;False;118;gustStrengthAtPosition;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;1247.137,2268.26;Float;False;Constant;_Float2;Float 2;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;485;1033.465,2047.084;Inherit;False;ApplyTreeWindDisplacement;-1;;415;0eb84d03384e16540a3d42d8a1457ec4;0;6;20;FLOAT;1;False;19;FLOAT3;1,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT;0;False;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1;-1377.334,510.8242;Inherit;True;Property;_MainTex;Albedo;3;1;[NoScaleOffset];Create;False;0;0;False;0;-1;None;e4e123e1fd77b9843baa3b0b8351235c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;87;1433.987,2098.904;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;424;-931.7611,523.6658;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;431;2378.081,1832.199;Inherit;False;1340.659;1410.796;;19;0;387;519;416;398;520;518;527;528;529;530;531;532;533;534;535;536;547;548;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;538;-1385.396,178.1948;Inherit;False;Property;_GlossRemap;Gloss Remap;18;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;539;-1496.273,-141.6578;Inherit;True;Property;_MetallicGlossMap;Metallic Gloss Map;15;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;540;-1094.088,2.622044;Inherit;False;Property;_Glossiness;_Glossiness;19;0;Create;True;0;0;False;0;0.15;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;541;-1107.002,-172.6009;Inherit;False;Property;_Metallic;_Metallic;20;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;29;1608.939,2092.326;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;542;-1007.555,131.3136;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;527;2456.852,1977.719;Inherit;False;424;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RGBToHSVNode;529;2691.588,1985.22;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;528;2633.395,2247.823;Float;False;InstancedProperty;_Lightness;Lightness;2;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;531;2631.509,1893.851;Float;False;InstancedProperty;_Hue;Hue;0;0;Create;True;0;0;False;0;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;530;2633.944,2146.769;Float;False;InstancedProperty;_Saturation;Saturation;1;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;544;-763.3494,67.55846;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;17;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;543;-774.5093,-109.1023;Inherit;False;Property;_METALLICGLOSSMAP;_METALLICGLOSSMAP;16;0;Create;True;0;0;False;0;0;0;0;True;_METALLICGLOSSMAP;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;1881.632,2152.567;Float;False;vertexPositionWithWind;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;520;2620.074,2877.079;Float;False;Property;_OcclusionRemap;Occlusion Remap;6;0;Create;True;0;0;False;0;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;518;2468.616,2405.135;Float;False;Property;_BumpScale;Bump Scale;4;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;398;2542.905,2656.81;Inherit;True;Property;_OcclusionMap;Occlusion Map;7;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;05f189094ab11444eb08228c5fffd69a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;533;2978.395,2054.823;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;545;-450.3713,109.1881;Inherit;False;GlossMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;537;3032.623,3127.096;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;546;-458.9214,-146.6866;Inherit;False;MetallicMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;532;2979.395,2172.823;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;534;2978.858,1927.178;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;416;2838.725,3014.313;Inherit;False;411;vertexPositionWithWind;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;547;2893.436,2581.313;Inherit;False;546;MetallicMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;535;3186.029,2005.19;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;475;-1161.284,1996.936;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;451;-1883.484,2244.764;Float;False;primaryFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;466;-1473.945,1936.393;Inherit;False;451;primaryFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-327.8409,2338.541;Float;False;gustHighlightAtPosition;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;519;2906.984,2780.522;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;387;2792.257,2370.196;Inherit;True;Property;_BumpMap;Bump Map;5;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;0900d62a76914014f982fc871dd92766;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;536;3316.113,3006.213;Float;False;Property;_BakedMask;Baked Mask;11;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;548;2908.199,2683.646;Inherit;False;545;GlossMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3361.562,2425.545;Float;False;True;-1;2;VisualDesignCafe.Nature.Editor.NatureMaterialEditor;0;0;Standard;Open World Nature/Tree Bark;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.3;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;False;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Spherical;False;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;447;0;455;1
WireConnection;450;0;455;2
WireConnection;488;1;476;0
WireConnection;107;0;513;0
WireConnection;113;0;89;0
WireConnection;484;0;483;0
WireConnection;106;0;513;6
WireConnection;105;0;513;8
WireConnection;141;0;513;10
WireConnection;452;0;453;2
WireConnection;149;0;513;11
WireConnection;477;0;488;0
WireConnection;477;1;146;0
WireConnection;477;2;484;0
WireConnection;515;165;469;0
WireConnection;515;114;202;0
WireConnection;515;60;150;0
WireConnection;515;55;111;0
WireConnection;515;49;142;0
WireConnection;515;24;135;0
WireConnection;515;4;477;0
WireConnection;515;5;110;0
WireConnection;515;28;200;0
WireConnection;515;1;114;0
WireConnection;515;3;55;0
WireConnection;125;0;515;43
WireConnection;132;0;515;44
WireConnection;118;0;515;0
WireConnection;480;0;481;0
WireConnection;480;1;129;0
WireConnection;485;20;480;0
WireConnection;485;19;133;0
WireConnection;485;6;109;0
WireConnection;485;7;119;0
WireConnection;485;1;86;0
WireConnection;87;0;485;0
WireConnection;87;3;88;0
WireConnection;424;0;1;0
WireConnection;29;0;87;0
WireConnection;542;0;539;4
WireConnection;542;3;538;1
WireConnection;542;4;538;2
WireConnection;529;0;527;0
WireConnection;544;1;540;0
WireConnection;544;0;542;0
WireConnection;543;1;541;0
WireConnection;543;0;539;1
WireConnection;411;0;29;0
WireConnection;533;0;529;2
WireConnection;533;1;530;0
WireConnection;545;0;544;0
WireConnection;546;0;543;0
WireConnection;532;0;529;3
WireConnection;532;1;528;0
WireConnection;534;0;531;0
WireConnection;534;1;529;1
WireConnection;535;0;534;0
WireConnection;535;1;533;0
WireConnection;535;2;532;0
WireConnection;475;0;466;0
WireConnection;475;1;469;0
WireConnection;451;0;453;1
WireConnection;265;0;515;138
WireConnection;519;0;398;1
WireConnection;519;3;520;1
WireConnection;519;4;520;2
WireConnection;387;5;518;0
WireConnection;536;1;537;0
WireConnection;536;0;416;0
WireConnection;0;0;535;0
WireConnection;0;1;387;0
WireConnection;0;3;547;0
WireConnection;0;4;548;0
WireConnection;0;5;519;0
WireConnection;0;11;536;0
ASEEND*/
//CHKSM=F401D327B22C6CA7A40975E1985BB41490D93558