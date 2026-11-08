using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private float _duration = 0.25f;
    [SerializeField]
    private float _magnitude = 0.1f;

    private Vector3 _originalPosition;
    private Coroutine _shakeRoutine;

    void Start()
    {
        _originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
            transform.localPosition = _originalPosition;
        }
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            float strength = _magnitude * (1f - elapsed / _duration);
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            transform.localPosition = _originalPosition + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalPosition;
        _shakeRoutine = null;
    }
}