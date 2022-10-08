Shader "Hidden/Radial_Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SampleCount("Sample Count", Range(4, 32)) = 8
        _Strength("Strength", Range(0.0, 5.0)) = 0.5
        _Gray("Gray", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 vertex   : SV_POSITION;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            half _SampleCount;
            half _Strength;
            half _Gray;
 
            /*fixed4 frag (v2f i) : SV_Target
            {
                half4 col           = 0;
                // UVを-0.5～0.5に変換
                half2 symmetryUv    = i.uv - 0.5;
                // 外側に行くほどこの値が大きくなる(0～0.707)
                half distance       = length(symmetryUv);
                for(int j = 0; j < _SampleCount; j++) {
                    // jが大きいほど、画面の外側ほど小さくなる値
                    float uvOffset     = 1 - _Strength * j / _SampleCount * distance;
                    // jが大きくなるにつれてより内側のピクセルをサンプリングしていく
                    // また画面の外側ほどより内側のピクセルをサンプリングする
                    col                 += tex2D(_MainTex, symmetryUv * uvOffset + 0.5);
                }
                col                 /= _SampleCount;
                return col;
            }*/
            fixed4 frag (v2f i) : SV_Target
            {
                 
                half4 col           = 0;
                half2 center        = {0.5,0.75};
                half2 pos           = i.uv - center;
                half dist           = length(pos);
                half factor         = _Strength / (float)_SampleCount * dist * 0.1;
                for(int i = 0; i < _SampleCount; i++)
                {
                    float uvOffset = 1.0 - factor * float(i*3);
                    
                    col.g += tex2D(_MainTex, pos * uvOffset + center).g;
                    col.b += tex2D(_MainTex, pos * uvOffset + center * (1 + 0.02 * _Strength)).b;
                    col.r += tex2D(_MainTex, pos * uvOffset + center * (1 - 0.02 * _Strength)).r;
                    
                }
                col /= float(_SampleCount);

                //col.rgb = dot(col.rgb, fixed3(0.299 + 0.701*(1-_Gray), 0.587+ 0.413*(1-_Gray), 0.114+ 0.886*(1-_Gray)));
                col.rgb = dot(col.rgb, fixed3(0.299, 0.587, 0.114)) *_Gray/1 + col.rgb * (1-_Gray)/1;
                return col;
                
            }
            ENDCG
        }
    }
}
