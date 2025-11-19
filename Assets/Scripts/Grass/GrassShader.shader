
Shader "Custom/GrassShader"
{
  Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OldGrassColour ("OldGrassColour", Color) = (0,1,0,1)
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
            #include "Random.cginc"

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
            
            

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;
            
            fixed4 _OldGrassColour;

            

            StructuredBuffer<float4> GrassPositionsBufferShader;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceid : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                uint instanceid : TEXCOORD1;
            };
            

              float3 RotateAroundYAxis (float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }
            


            
            
             v2f vert (MeshData v, uint instanceID : SV_INSTANCEID)
            {
                InitIndirectDrawArgs(0);
                v2f o;

                uint indirectInstanceID = GetIndirectInstanceID(instanceID);                 
                o.instanceid = indirectInstanceID;
                  
                uint grassIndex = o.instanceid / 3;
                uint quadIndex  = o.instanceid % 3;
                
                float3 localPosition  = (RotateAroundYAxis(v.vertex,_Rotation * quadIndex));
                float4 worldPosition = float4(GrassPositionsBufferShader[grassIndex].xyz + localPosition, 1.0f);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                worldPosition.y += o.uv.y * (GrassPositionsBufferShader[grassIndex].w);
                worldPosition.y -= 0.5;
                
                worldPosition.x += o.uv.y * sin((v.vertex.y + _Time * 15) / 0.75) * ((0.15 + GrassPositionsBufferShader[grassIndex].w)/50);
                
               

                  
                
                o.vertex = mul(UNITY_MATRIX_VP, worldPosition);
                  
              
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex,i.uv);
                
                clip(-(0.5 - col.a));
                float3 lightDir = _WorldSpaceLightPos0.xyz;

                uint grassIndex = i.instanceid / 3;

                col = lerp(col,_OldGrassColour,GrassPositionsBufferShader[grassIndex].w);

                
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));

               

    
                return col * ndotl;
                //return float4(i.uv,0,1);
               
            }
            ENDCG
        }
    }
}