using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Transform resetTransform;
    [SerializeField] GameObject player;
    [SerializeField] GameObject playerHead;

    [ContextMenu("Reset pos")] 
    public void ResetPos()
    {
        var rotationAngleY = playerHead.transform.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;
                    
        player.transform.Rotate(0, -rotationAngleY, 0);

        var distanceDiffX = resetTransform.position.x - playerHead.transform.position.x;
        var distanceDiffZ = resetTransform.position.z - playerHead.transform.position.z;

        player.transform.position += new Vector3(distanceDiffX, 0, distanceDiffZ);

    }

    void Start()
    {
        ResetPos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
