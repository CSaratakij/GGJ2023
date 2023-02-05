using UnityEngine;

public class FrameLimit : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}

