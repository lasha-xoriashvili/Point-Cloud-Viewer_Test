Shader "Custom/PointRenderWithColor"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 1. განვსაზღვროთ მონაცემთა სტრუქტურა
            struct PointData {
                float3 pos;
                float4 color;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 col : COLOR; // ფერის გადასაცემად ფრაგმენტულ შადერში
                float size : PSIZE;
            };

            // 2. გამოვიყენოთ ჩვენი სტრუქტურა ბუფერში
            StructuredBuffer<PointData> _PointBuffer;
            float _PointSize;
            float4x4 _LocalToWorld;
            v2f vert (uint id : SV_VertexID)
            {
                v2f o;
                // ვიღებთ მონაცემებს ID-ის მიხედვით
                PointData data = _PointBuffer[id];
                float4 worldPos = mul(_LocalToWorld, float4(data.pos, 1.0));
                o.pos = mul(UNITY_MATRIX_VP, worldPos);
                o.col = data.color; // ფერს ვატანთ შემდეგ ეტაპზე
                o.size = _PointSize;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 3. ვაბრუნებთ კონკრეტული წერტილის ფერს
                return i.col;
            }
            ENDCG
        }
    }
}