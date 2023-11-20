using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{

    bool TopDownView = true;
    Vector3 RemainedPostion;
    Quaternion RemainedRotation;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = new Vector3(3f, 1.7f, 3f);
        this.transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
        ChangeToTopDown();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!TopDownView)
        {
            if (Input.GetKey("w"))
            {
                //this.transform.position += new Vector3(0f,0f,0.01f);
                this.transform.position += new Vector3(0.05f * this.transform.forward.x, 0f * this.transform.forward.y, 0.05f * this.transform.forward.z);
            }
            if (Input.GetKey("a"))
            {
                //this.transform.position += new Vector3(0f, 0f, -0.01f);
                this.transform.position += new Vector3(-0.05f * this.transform.right.x, 0f * this.transform.forward.y, -0.05f * this.transform.right.z);
            }
            if (Input.GetKey("s"))
            {
                //this.transform.position += new Vector3(0.01f, 0f, 0f);
                this.transform.position += new Vector3(-0.05f * this.transform.forward.x, 0f * this.transform.forward.y, -0.05f * this.transform.forward.z);
            }
            if (Input.GetKey("d"))
            {
                //this.transform.position += new Vector3(-0.01f, 0f, 0f);
                this.transform.position += new Vector3(0.05f * this.transform.right.x, 0f * this.transform.forward.y, 0.05f * this.transform.right.z);
            }
            if (Input.GetKeyDown("q"))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0,-30, 0)+ this.transform.rotation.eulerAngles);
            }
            if (Input.GetKeyDown("e"))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0, 30, 0) + this.transform.rotation.eulerAngles);
            }
            if (Input.GetKeyDown("r"))
            {
                if (!TopDownView)
                {
                    ChangeToTopDown();
                }
            }
        }
    }

    public void ChangeToTopDown()
    {
        RemainedPostion = this.transform.position;
        RemainedRotation = this.transform.rotation;
        TopDownView = true;
        this.transform.position = new Vector3(3f,5f,3f);
        this.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        this.GetComponent<Camera>().orthographic = true;
    }

    public void ChangeTo3D()
    {
        this.GetComponent<Camera>().orthographic = false;
        TopDownView = false;
        this.transform.position = RemainedPostion;
        this.transform.rotation = RemainedRotation;
    }

    public void ChangeView()
    {
        if (TopDownView)
        {
            ChangeTo3D();
        }
        else
        {
            ChangeToTopDown();
        }
    }

}
