Shader "Unlit/Flash"
{
    Properties
    {
        _FlashColor ("Flash Color", Color) = (1, 0, 0, 1)
        _FlashAmount ("Flash Amount", Range (0, 1)) = 0
        _Emission ("Emission", float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Overlay" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
            };

            float4 _FlashColor;
            float _FlashAmount;
            float _Emission;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 col = _FlashColor * _FlashAmount;

                col.rgb += _Emission;

                return col;
            }
            ENDCG
        }
    }
}