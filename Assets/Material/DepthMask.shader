Shader "Hidden/DepthMask"
{
    SubShader
    {
        Tags
        {
            "Queue"="Geometry+10"
            "RenderType"="Opaque"
        }

        // Do not render color
        ColorMask 0

        // Write depth so the mask matches boat geometry
        ZWrite On
        ZTest LEqual
        Cull Off

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }
        }
    }
}
