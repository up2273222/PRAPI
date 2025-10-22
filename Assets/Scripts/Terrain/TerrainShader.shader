Shader "Custom/TerrainShader"
{
  Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }


        Pass
        {
            Cull off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardBRDF.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            

           // StructuredBuffer<float3> TerrainPointsBufferShader;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            

           

            
            
             v2f vert (MeshData v, uint instanceID : SV_INSTANCEID)
            {
                 v2f o;

              
                

                 o.vertex = UnityObjectToClipPos(v.vertex);

                
                 
                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                 

                 return o;
                 
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex,i.uv);
                clip(-(0.5 - col.a));
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                
                return col * ndotl;
                //return float4(i.uv,0,1);
            }
            ENDCG
        }
    }
}