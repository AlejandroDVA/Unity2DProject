using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Transform target; // Asigna el transform de tu personaje desde el Inspector
    [SerializeField] private Vector3 offset = new Vector3(); // Offset para la posición de la cámara

    void Update()
    {
        if (target != null)
        {
            // Actualiza la posición de la cámara
            transform.position = target.position + offset;
        }
    }
}
