// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Amazing Assets/Curved World/Mobile/Diffuse" 
{
    Properties 
    {
        [HideInInspector][CurvedWorldBendSettings]	  _CurvedWorldBendSettings("0,5,27,29|1|1", Vector) = (0, 0, 0, 0)

        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
	 
    SubShader
    {
        Tags { "RenderType"="CurvedWorld_Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd vertex:vert addshadow


#pragma shader_feature_local CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE CURVEDWORLD_BEND_TYPE_LITTLEPLANET_Y CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_X_POSITIVE CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_Z_POSITIVE
#define CURVEDWORLD_BEND_ID_1
#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
#include "../../Core/CurvedWorldTransform.cginc" 

        sampler2D _MainTex;

        void vert (inout appdata_full v) 
        {
            #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
                #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
                    CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
                #else
                    CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
                #endif
            #endif
        }

        struct Input 
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) 
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG


         //PassName "ScenePickingPass"
		Pass
        {
            Name "ScenePickingPass"
            Tags { "LightMode" = "Picking" }

            BlendOp Add
            Blend One Zero
            ZWrite On
            Cull Off

            CGPROGRAM
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"


            #pragma target 3.0
            #pragma multi_compile_instancing

            #pragma vertex vertEditorPass
            #pragma fragment fragScenePickingPass


#pragma shader_feature_local CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE CURVEDWORLD_BEND_TYPE_LITTLEPLANET_Y CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_X_POSITIVE CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_Z_POSITIVE
#define CURVEDWORLD_BEND_ID_1
#pragma shader_feature_local CURVEDWORLD_DISABLED_ON


            #include "../../Core/SceneSelection.cginc" 
            ENDCG
        }	//Pass "ScenePickingPass"	

        //PassName "SceneSelectionPass"
		Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }

            BlendOp Add
            Blend One Zero
            ZWrite On
            Cull Off

            CGPROGRAM
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"


            #pragma target 3.0
            #pragma multi_compile_instancing

            #pragma vertex vertEditorPass
            #pragma fragment fragSceneHighlightPass


#pragma shader_feature_local CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE CURVEDWORLD_BEND_TYPE_LITTLEPLANET_Y CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_X_POSITIVE CURVEDWORLD_BEND_TYPE_TWISTEDSPIRAL_Z_POSITIVE
#define CURVEDWORLD_BEND_ID_1
#pragma shader_feature_local CURVEDWORLD_DISABLED_ON


            #include "../../Core/SceneSelection.cginc" 
            ENDCG
        }	//Pass "SceneSelectionPass"	
    }


    FallBack "Hidden/Amazing Assets/Curved World/Fallback/VertexLit"

    CustomEditor "AmazingAssets.CurvedWorldEditor.DefaultShaderGUI"
}
