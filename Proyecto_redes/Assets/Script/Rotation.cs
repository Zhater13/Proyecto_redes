using UnityEngine;

public class Rotation : MonoBehaviour
{
    // Velocidad de rotación en grados por segundo
    public float velocidadRotacion = 100f;

    void Update()
    {
        // Rota constantemente sobre el eje Z de forma fluida
        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);
    }
}
