// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/**
 * A really crappy shader to try and make a circular mask with an outline
 * for the game window.
 * TODO: The outline should ideally be 1px thick all around, but I don't
 *   know how to do that.
 */
Shader "Custom/CircleMask" {
    
    Properties {
        _Color("Color", Color) = (1,1,1,1)
        _StrokeThickness("Stroke Thickness", Float) = 1
        _StrokeColor("Stroke Color", Color) = (1,1,1,1)
        _Radius("Radius", Float) = 0.5
        _NormalizedPosition("Center Position", Vector) = (0,0, 0,0)
        _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _StrokeThickness;
            fixed4 _StrokeColor;
            float _Radius;
            
            float4 _NormalizedPosition;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct fragmentInput {
                float4 pos : SV_POSITION;
                float2 uv : TEXTCOORD0;
            };

            fragmentInput vert(appdata_base v) {
                fragmentInput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //最naive的写法：用归一化的Offset实现
                o.uv = v.texcoord.xy - _NormalizedPosition.xy;
                //o.uv.x *= _ScreenParams.x / _ScreenParams.y;//有用，因为stretch，能知道屏幕宽高比就是uv的宽高比
                //o.uv.x *= _MainTex_TexelSize.z * _MainTex_TexelSize.y;//无用，因为最后的UV和MainTex（一张固有图片）无关
                return o;
            }

            fixed4 frag(fragmentInput i) : SV_Target {
                float dx = ddx(i.uv.x);
                float dy = ddy(i.uv.y);
                float aspect = dy/dx;
                //float aspect = dy;
                i.uv.x *= aspect;//直接通过偏导数的比值求得uv宽高比，性能影响未知。。。
                //float distance = sqrt(pow(i.uv.x, 2) + pow(i.uv.y,2)) * 2;
                const float distance = length(i.uv);
                if (distance < _Radius) {
                    discard;
                }
                if (distance < (_Radius + _StrokeThickness)) {
                    return _StrokeColor;
                }
                return _Color;
            }
            ENDCG
        }   
    }
}
