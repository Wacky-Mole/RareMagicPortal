using UnityEngine;

[ExecuteInEditMode]
public class PortalFX_AxisRotateByTime : MonoBehaviour
{
    public Vector3 RotateAxis = new Vector3(1, 5, 10);

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(RotateAxis * Time.deltaTime);
    }
}