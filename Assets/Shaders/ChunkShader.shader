// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ChunkShader" {

Properties {
		_PlanetCenter("Planet Center",Vector) = (0,0,0)
		_GrassTex("Texture", 2D) = "white" {}
		_StoneTex("Texture", 2D) = "white" {}
		_GrassSlopeThreshold("Grass Slope Threshold", Range(0,1)) = .5
		_GrassBlendAmount("Grass Blend Amount", Range(0,1)) = .5
}
SubShader{
	Tags { "RenderType" = "Opaque"}
	LOD 300

	Pass {
			//Parametrage du shader pour éviter de lire, écrire dans le zbuffer, désactiver le culling et le brouillard sur le polygone
			ZWrite On ZTest LEqual
			Fog { Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			float3 _PlanetCenter;

			sampler2D _GrassTex;
			sampler2D _StoneTex;

			half _MaxHeight;
			half _GrassSlopeThreshold;
			half _GrassBlendAmount;

			float4 _GrassTex_ST;

			struct Prog2Vertex {
	            float4 vertex : POSITION; 	//Les "registres" précisés après chaque variable servent
	            float4 tangent : TANGENT; 	//A savoir ce qu'on est censé attendre de la carte graphique.
	            float3 normal : NORMAL;		//(ce n'est pas basé sur le nom des variables).
	            float4 texcoord : TEXCOORD0;  
	            float4 texcoord1 : TEXCOORD1; 
	            fixed4 color : COLOR; 
        	 };
			 
			//Structure servant a transporter des données du vertex shader au pixel shader.
			//C'est au vertex shader de remplir a la main les informations de cette structure.
			struct Vertex2Pixel
			 {
				fixed4 color : COLOR;
           		float4 pos : SV_POSITION;
           		float2 uv : TEXCOORD0;
				float4 worldNormal : TEXCOORD1;

			 };  	 

			Vertex2Pixel vert (Prog2Vertex i)
			{
				Vertex2Pixel o;

				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		        o.pos = UnityObjectToClipPos (i.vertex); //Projection du modèle 3D, cette ligne est obligatoire

		        // o.uv=i.texcoord; //UV de la texture

				o.uv = TRANSFORM_TEX(i.texcoord, _GrassTex);
				o.color.xyz = abs(i.normal);
				UNITY_TRANSFER_FOG(o, o.vertex);
				o.worldNormal = mul(unity_ObjectToWorld, i.normal);
		      	return o;
			}

			inline float angleBetween(float3 normal) 
			{
				return acos(dot(_PlanetCenter, normal) / (distance(_PlanetCenter, normal)));
			}

            float4 frag(Vertex2Pixel i) : COLOR 
            {
				return 0.5f + 0.5f * i.worldNormal;
				// float slope = abs(angleBetween(i.worldNormal.y) * 180 / 3.14);
				// slope /= 90;
				// return lerp(float4(1, 1, 1, 1), float4(0, 0, 0, 1), slope);

				// float slope = 1 - i.worldNormal.y; // slope = 0 when terrain is completely flat
				// float grassBlendHeight = _GrassSlopeThreshold * (1 - _GrassBlendAmount);
				// float grassWeight = 1 - saturate((slope - grassBlendHeight) / (_GrassSlopeThreshold - grassBlendHeight));
				// 
				// float4 grass = tex2D(_GrassTex,i.uv);
				// float4 stone = tex2D(_StoneTex,i.uv);
				// 
				// return grass * grassWeight + stone * (1 - grassWeight);
			}
ENDCG 
	}


	//  Shadow rendering pass
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }

		ZWrite On ZTest LEqual

		CGPROGRAM
		#pragma target 2.0

		#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
		#pragma shader_feature _METALLICGLOSSMAP
		#pragma skip_variants SHADOWS_SOFT
		#pragma multi_compile_shadowcaster

		#pragma vertex vertShadowCaster
		#pragma fragment fragShadowCaster

		#include "UnityStandardShadow.cginc"

		ENDCG
	}
		}

Fallback off

}