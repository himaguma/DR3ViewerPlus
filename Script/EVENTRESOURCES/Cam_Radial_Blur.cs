using UnityEngine;

[ExecuteInEditMode]
public class Cam_Radial_Blur : MonoBehaviour
{

    [SerializeField]
    private Shader _shader;
    [SerializeField, Range(4, 32)]
    private int _sampleCount      = 8;
    [SerializeField, Range(0.0f, 1.0f)]
    public float _strength           = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)]
    public float _gray           = 0.0f;

    private Material _material;

    private void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (_material == null) {
            if (_shader == null) {
                Graphics.Blit(source, dest);
                return;
            }
            else {
                _material   = new Material(_shader);
            }
        }
        _material.SetInt("_SampleCount", _sampleCount);
        _material.SetFloat("_Strength", _strength);
        _material.SetFloat("_Gray", _gray);
        Graphics.Blit(source, dest, _material);
    }
}