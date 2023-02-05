using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10.0f;

    private void LateUpdate()
    {
        float dir = (transform.parent.localScale.x > 0.0f) ? -1.0f : 1.0f;
        transform.Rotate(Vector3.forward * dir, rotateSpeed * Time.deltaTime);
    }
}

