using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatController : MonoBehaviour
{
    public List<GameObject> EnvironmentsList;

    float RoomSizeX = 10;
    float RoomSizeY = 10;
    public float MatSize = 0.5f;
    public GameObject MatPrefab;
    GameObject MatHolder;
    public float ValidMapSize = 0.1f;
    public GameObject ValidMapPrefab;
    GameObject ValidMapHolder;

    public Material HittedValidMatMaterial;


    public GameObject BottomLeftPoint;
    //public GameObject BottomRightPoint;
    //public GameObject TopLeftPoint;
    public GameObject TopRightPoint;

    public Color MainColor;
    public void ChangeMainColorInController(Color colorInput)
    {
        MainColor = colorInput;
    }

    int layerMaskforMat = 1 << 5; //Mat is 6

    //private GameObject[,] MatArray;
    // Start is called before the first frame update
    void Start()
    {
        if(EnvironmentsList.Count == 0)
        {
            Debug.LogError("Put the Environment into the list");
        }


        SetUpEnvironmentsList();
        CreateMat();
        layerMaskforMat = ~layerMaskforMat;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        IsRotationFinishedAndAdjustment();
        DynamicAdjustMatMap();
        if (IsProjected)
        {
            AutoAdjustClippinginValidMap();
            AutoDetection();
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskforMat))
            {
                Mat HitMat = hit.collider.gameObject.GetComponent<Mat>();
                if (HitMat != null)
                    HitMat.CustomChangeColor(MainColor);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskforMat))
            {
                Mat HitMat = hit.collider.gameObject.GetComponent<Mat>();
                HitMat.CustomChangeColor(Color.white);
            }
        }
    }


    ///Section For Create MatMap and ValidMap
    //Using To CreateMat
    GameObject[,] MatMap = null;
    Vector2Int NumOfMat;
    Vector3 Origin_location;
    void CreateMat()
    {
        MatHolder = new GameObject("MatHolder");
        MatHolder.transform.position = new Vector3(0f, 0f, 0f);

        RoomSizeX = maxX;
        RoomSizeY = maxZ;

        BottomLeftPoint.transform.position = new Vector3(0f, 1f, 0f);
        //BottomRightPoint.transform.position = new Vector3(maxX, 1f, 0f);
        //TopLeftPoint.transform.position = new Vector3(0f, 1f, maxZ);
        TopRightPoint.transform.position = new Vector3(maxX, 1f, maxZ);

        //Check Parameter Being Good
        if (RoomSizeX <= 0 || RoomSizeY<=0|| MatSize <= 0)
        {
            Debug.LogError("Size Parameter can't less than zero");
        }

        //Create Mat

        int MatXAmout = (int)Mathf.Ceil(RoomSizeX / MatSize);
        int MatYAmout = (int)Mathf.Ceil(RoomSizeY / MatSize);

        MatMap = new GameObject[MatXAmout, MatYAmout];
        NumOfMat = new Vector2Int(MatXAmout, MatYAmout);

        MatPrefab.transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);
        MatPrefab.layer = 6;

        for (int ix = 0; ix < MatXAmout; ix++)
        {
            for (int iy = 0; iy < MatYAmout; iy++)
            {
                MatMap[ix, iy] = Instantiate(MatPrefab, new Vector3(ix * MatSize + MatSize / 2, -1, iy * MatSize + MatSize / 2), Quaternion.identity);
                MatMap[ix, iy].transform.parent = MatHolder.transform;
            }
        }
        Origin_location = new Vector3(0f, 1f, 0f);

    }

    //AutoAdjust Mat
    void DynamicAdjustMatMap()
    {
        //Make sure Mat Map has been create
        if (MatMap == null)
        {
            return;
        }

        //Check Parameter Being Good
        if (MatSize <= 0)
        {
            Debug.LogError("Size Parameter can't less than zero");
        }


        //Adjust Mat Base on parameter 1. size 2.start location 3.NumberofMat
        //Update the Parater
        int MatXAmout = (int)Mathf.Ceil((TopRightPoint.transform.position - BottomLeftPoint.transform.position).x / MatSize);
        int MatYAmout = (int)Mathf.Ceil((TopRightPoint.transform.position - BottomLeftPoint.transform.position).z / MatSize);

        //Detect Mat Size Changed
        if (Mathf.Abs(MatPrefab.transform.localScale.x-MatSize)<0.0001)
        {

            GameObject[,] NewMatMap = new GameObject[MatXAmout, MatYAmout];
            MatPrefab.transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);

            for (int ix = 0; ix < MatXAmout; ix++)
            {
                for (int iy = 0; iy < MatYAmout; iy++)
                {
                    if (ix >= NumOfMat[0] || iy >= NumOfMat[1]) //We now have more mat so need to add mat
                    {
                        NewMatMap[ix, iy] = Instantiate(MatPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        NewMatMap[ix, iy].transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);
                        NewMatMap[ix, iy].transform.parent = MatHolder.transform;
                    }
                    else // Nothing chage
                    {
                        MatMap[ix, iy].transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);
                        NewMatMap[ix, iy] = MatMap[ix, iy];
                    }

                    //Recalulate the position

                    NewMatMap[ix, iy].transform.position = BottomLeftPoint.transform.position + new Vector3(ix * MatSize + MatSize / 2, -1 * BottomLeftPoint.transform.position.y, iy * MatSize + MatSize / 2);

                }
            }
            //Destory the unused mat
            if (NumOfMat[0] > MatXAmout && NumOfMat[1] > MatYAmout)
            {
                for (int ix = MatXAmout; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = 0; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
                for (int ix = 0; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = MatYAmout; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
                Destroy(MatMap[MatXAmout, MatYAmout]);
            }
            else if (NumOfMat[0] > MatXAmout)
            {
                for (int ix = MatXAmout; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = 0; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
            }
            else if (NumOfMat[1] > MatYAmout )
            {
                for (int ix = 0; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = MatYAmout; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
            }
            //Everything Done, save the map
            MatMap = NewMatMap;
            NumOfMat = new Vector2Int(MatXAmout, MatYAmout);
            return;
        }

        //Detect Start Point Changed

        if(Origin_location != BottomLeftPoint.transform.position)
        {
            for (int ix = 0; ix < NumOfMat[0]; ix++)
            {
                for (int iy = 0; iy < NumOfMat[1]; iy++)
                {
                    MatMap[ix, iy].transform.position = BottomLeftPoint.transform.position + new Vector3(ix * MatSize + MatSize / 2, -1 * BottomLeftPoint.transform.position.y, iy * MatSize + MatSize / 2);
                }
            }
            Origin_location = BottomLeftPoint.transform.position;
        }

        //Detect Number of Mat Changed
        if (MatXAmout != NumOfMat[0] || MatYAmout != NumOfMat[1])
        {

            GameObject[,] NewMatMap = new GameObject[MatXAmout, MatYAmout];

            for (int ix = 0; ix < MatXAmout; ix++)
            {
                for (int iy = 0; iy < MatYAmout; iy++)
                {
                    if (ix>= NumOfMat[0] || iy >= NumOfMat[1]) //We now have more mat so need to add mat
                    {
                        NewMatMap[ix, iy] = Instantiate(MatPrefab, new Vector3(0,0,0), Quaternion.identity);
                        NewMatMap[ix, iy].transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);
                        NewMatMap[ix, iy].transform.parent = MatHolder.transform;
                    }
                    else // Nothing chage
                    {
                        MatMap[ix, iy].transform.localScale = new Vector3(MatSize, MatSize / 10, MatSize);
                        NewMatMap[ix, iy] = MatMap[ix, iy];
                    }

                    //Recalulate the position
                    NewMatMap[ix, iy].transform.position = BottomLeftPoint.transform.position + new Vector3(ix * MatSize + MatSize / 2, -1 * BottomLeftPoint.transform.position.y, iy * MatSize + MatSize / 2);
                }
            }
            //Destory the unused mat
            if (NumOfMat[0] > MatXAmout && NumOfMat[1] > MatYAmout)
            {
                for (int ix = MatXAmout; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = 0; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
                for (int ix = 0; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = MatYAmout; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
                Destroy(MatMap[MatXAmout, MatYAmout]);
            }
            else if (NumOfMat[0] > MatXAmout)
            {
                for (int ix = MatXAmout; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = 0; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
            }
            else if (NumOfMat[1] > MatYAmout)
            {
                for (int ix = 0; ix < NumOfMat[0]; ix++)
                {
                    for (int iy = MatYAmout; iy < NumOfMat[1]; iy++)
                    {
                        Destroy(MatMap[ix, iy]);
                    }
                }
            }
            //Everything Done, save the map
            MatMap = NewMatMap;
            NumOfMat = new Vector2Int(MatXAmout, MatYAmout);
            return;
        }
        return;
    }


    public float Covered_Precentage = 0.7f;

    public void AutoDetection() 
    {

        for (int ix=0; ix < NumOfMat[0]; ix++)
        {
            for (int iy = 0; iy < NumOfMat[1]; iy++)
            {
                Vector3 Origin = ValidMap[0, 0].transform.position - new Vector3(ValidMapSize / 2, 0, ValidMapSize / 2);
                Vector3 DifferenceFromOrigin = MatMap[ix, iy].transform.position - Origin;

                int LowerboundX = (int)Mathf.Floor((DifferenceFromOrigin.x - MatSize / 2 ) / ValidMapSize);
                int UpperboundX = (int)Mathf.Ceil((DifferenceFromOrigin.x + MatSize / 2 ) / ValidMapSize);
                int LowerboundY = (int)Mathf.Floor((DifferenceFromOrigin.z - MatSize / 2 ) / ValidMapSize);
                int UpperboundY = (int)Mathf.Ceil((DifferenceFromOrigin.z + MatSize / 2 ) / ValidMapSize);

                if(LowerboundX > NumOfValidMap[0] || UpperboundX<0 ||  LowerboundY > NumOfValidMap[1] || UpperboundY < 0)
                {
                    if (MatMap[ix, iy].GetComponent<Mat>() != null)
                    {
                        MatMap[ix, iy].GetComponent<Mat>().AutoSetAsValid();
                    }
                    continue;
                }
                if (LowerboundX < 0)
                {
                    LowerboundX = 0;
                }
                if (UpperboundX > NumOfValidMap[0])
                {
                    UpperboundX = NumOfValidMap[0];
                }
                if (LowerboundY < 0)
                {
                    LowerboundY = 0;
                }
                if (UpperboundY > NumOfValidMap[1])
                {
                    UpperboundY = NumOfValidMap[1];
                }

                float TotalValidMapinMat = (UpperboundX - LowerboundX) * (UpperboundY - LowerboundY);
                float CoveredValidMap = 0;

                for (int ivx = LowerboundX; ivx< UpperboundX; ivx++)
                {
                    for (int ivy = LowerboundY; ivy < UpperboundY; ivy++)
                    {
                        if (!ValidMap[ivx, ivy].GetComponent<ValidMap>().isValid())
                        {
                            CoveredValidMap ++;
                        }
                    }
                }

                if (CoveredValidMap / TotalValidMapinMat > Covered_Precentage)
                {
                    if (MatMap[ix, iy].GetComponent<Mat>() != null)
                    {
                        MatMap[ix, iy].GetComponent<Mat>().AutoSetAsInValid();
                    }
                }
                else
                {
                    if (MatMap[ix, iy].GetComponent<Mat>() != null)
                    {
                        MatMap[ix, iy].GetComponent<Mat>().AutoSetAsValid();
                    }
                }
            }
        }
        //Finish Detecting the Mat
        

    }
    //Detection Done

    public void SetMatSize(float InputMatSize)
    {
        MatSize = InputMatSize;
    }


    /// <summary>
    /// Following Code is Setting Up the EnvironmentsList to 0,0 and Create ValidMap for Furniture Detection
    /// </summary>
    public float DiscardAbove = 2f;
    public float DiscardBelow = 0.1f;
    public Shader PointCloudshader;
    int layerMaskforValidMap = 1 << 6; //Valid Map is 7
    public float DynamicallyClipAbove = 1f;


    public void SetUpEnvironmentsList()
    {
        EnvironmnetClipByShader();
        FindBoundingBoxAndCenterize();
        CreateVaildMap();
        layerMaskforValidMap = ~layerMaskforValidMap;
        ProjectToValidMap();
    }

    GameObject EnvironmentHolder;
    public void EnvironmnetClipByShader() 
    {
        EnvironmentHolder = new GameObject("EnvironmentHolder");
        EnvironmentHolder.transform.position = new Vector3(0f, 0f, 0f);

        Shader.SetGlobalFloat("ClipAbove", DiscardAbove);
        Shader.SetGlobalFloat("ClipBelow", DiscardBelow);
        Material ChangedMaterial = new Material(PointCloudshader);

        //Loading the Env
        VerticeInWorldPosition = new List<Vector3>();
        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++)
        {
            //Point in parent Object
            EnvironmentsList[num_env].transform.parent = EnvironmentHolder.transform;
            if (EnvironmentsList[num_env].GetComponent<Renderer>() != null)
            {
                EnvironmentsList[num_env].GetComponent<Renderer>().material = ChangedMaterial;

            }
            //Point in Child Object
            for (int ChildsinEnv = 0; ChildsinEnv < EnvironmentsList[num_env].transform.childCount; ChildsinEnv++)
            {
                GameObject Child = EnvironmentsList[num_env].transform.GetChild(ChildsinEnv).gameObject;
                VerticeInWorldPosition = new List<Vector3>();

                if (Child.GetComponent<Renderer>() != null)
                {
                    Child.GetComponent<Renderer>().material = ChangedMaterial;
                }

            }
        }
    }

    float minX, minY, minZ, maxX, maxY, maxZ;
    
    public void FindBoundingBoxAndCenterize()
    {
        minX = Mathf.Infinity;
        minY = Mathf.Infinity;
        minZ = Mathf.Infinity;
        maxX = Mathf.NegativeInfinity;
        maxY = Mathf.NegativeInfinity;
        maxZ = Mathf.NegativeInfinity;

        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++)
        {
            //Point in parent Object
            if (EnvironmentsList[num_env].GetComponent<MeshFilter>() != null)
            {
                Vector3[] tmpVertices;
                tmpVertices = EnvironmentsList[num_env].GetComponent<MeshFilter>().mesh.vertices;
                for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                {
                    SetMinandMax(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                }
            }
            //Point in Child Object
            for (int ChildsinEnv = 0; ChildsinEnv < EnvironmentsList[num_env].transform.childCount; ChildsinEnv++)
            {
                GameObject Child = EnvironmentsList[num_env].transform.GetChild(ChildsinEnv).gameObject;

                if (Child.GetComponent<MeshFilter>() != null)
                {
                    Vector3[] tmpVertices;
                    tmpVertices = Child.GetComponent<MeshFilter>().mesh.vertices;
                    for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                    {
                        SetMinandMax(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                    }
                }
            }
        }


        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++) { 
            EnvironmentsList[num_env].transform.position -= new Vector3(minX , minY, minZ );
            EnvironmentsList[num_env].transform.position += new Vector3(0.1f, 0f, 0.1f);
;
        }

        Vector3 Center = GetEnvironmentsCenter();
        EnvironmentHolder.transform.position = Center;
        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++)
        {
            EnvironmentsList[num_env].transform.position -= EnvironmentHolder.transform.position;
        }


        maxX -= minX - 0.1f;
        maxY -= minY;
        maxZ -= minZ - 0.1f;

        minX = 0f ;
        minY = 0f ;
        minZ = 0f ;
    }

    Vector3 GetEnvironmentsCenter()
    {
        VerticeInWorldPosition = new List<Vector3>();
        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++)
        {
            //Point in parent Object
            if (EnvironmentsList[num_env].GetComponent<MeshFilter>() != null)
            {
                Vector3[] tmpVertices;
                tmpVertices = EnvironmentsList[num_env].GetComponent<MeshFilter>().mesh.vertices;
                for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                {
                    VerticeInWorldPosition.Add(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                }
            }
            //Point in Child Object
            for (int ChildsinEnv = 0; ChildsinEnv < EnvironmentsList[num_env].transform.childCount; ChildsinEnv++)
            {
                GameObject Child = EnvironmentsList[num_env].transform.GetChild(ChildsinEnv).gameObject;

                if (Child.GetComponent<MeshFilter>() != null)
                {
                    Vector3[] tmpVertices;
                    tmpVertices = Child.GetComponent<MeshFilter>().mesh.vertices;
                    for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                    {
                        VerticeInWorldPosition.Add(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                    }
                }
            }
        }

        Vector3 SumOfPoints = new Vector3(0f,0f,0f);
        foreach (Vector3 PointPos in VerticeInWorldPosition)
        {
            SumOfPoints += PointPos;
        }

        return SumOfPoints / VerticeInWorldPosition.Count;

    }

    void SetMinandMax(Vector3 inputPoint)
    {
        if(inputPoint.x < minX)
        {
            minX = inputPoint.x;
        }
        if (inputPoint.y < minY)
        {
            minY = inputPoint.y;
        }
        if (inputPoint.z < minZ)
        {
            minZ = inputPoint.z;
        }
        if (inputPoint.x > maxX)
        {
            maxX = inputPoint.x;
        }
        if (inputPoint.y > maxY)
        {
            maxY = inputPoint.y;
        }
        if (inputPoint.z > maxZ)
        {
            maxZ = inputPoint.z;
        }
    }



    //Using To CreateValidMap
    GameObject[,] ValidMap = null;
    Vector2Int NumOfValidMap;
    void CreateVaildMap()
    {
        ValidMapHolder = new GameObject("ValidMapHolder");
        ValidMapHolder.transform.position = new Vector3(0f, 0f, 0f);

        //Check Parameter Being Good
        if (ValidMapSize <= 0)
        {
            Debug.LogError("Size Parameter can't less than zero");
        }

        //Start from 0,0 to MaxX,MaxZ
        //Create ValidMap

        int ValidMapXAmout = (int)Mathf.Ceil((maxX+0.2f) / ValidMapSize);
        int ValidMapYAmout = (int)Mathf.Ceil((maxZ+0.2f) / ValidMapSize);

        ValidMap = new GameObject[ValidMapXAmout, ValidMapYAmout];
        NumOfValidMap = new Vector2Int(ValidMapXAmout, ValidMapYAmout);

        ValidMapPrefab.transform.localScale = new Vector3(ValidMapSize, ValidMapSize / 10, ValidMapSize);
        ValidMapPrefab.layer = 7;


        for (int ix = 0; ix < ValidMapXAmout; ix++)
        {
            for (int iy = 0; iy < ValidMapYAmout; iy++)
            {
                ValidMap[ix, iy] = Instantiate(ValidMapPrefab, new Vector3(ix * ValidMapSize + ValidMapSize / 2, -5, iy * ValidMapSize + ValidMapSize / 2), Quaternion.identity);
                ValidMap[ix, iy].transform.parent = ValidMapHolder.transform;
            }
        }
    }


    ///Section For Detection Valid Mat
    //Using to do detection

    List<Vector3> VerticeInWorldPosition;

    bool IsProjected = false;
    public void ProjectToValidMap()
    {
        // First, find the all EnvironmentsList be used to detection
        // For each environment Object, only allow parent and parent's child
        VerticeInWorldPosition = new List<Vector3>();
        for (int num_env = 0; num_env < EnvironmentsList.Count; num_env++)
        {
            //Point in parent Object
            if (EnvironmentsList[num_env].GetComponent<MeshFilter>() != null)
            {
                Vector3[] tmpVertices;
                tmpVertices = EnvironmentsList[num_env].GetComponent<MeshFilter>().mesh.vertices;
                for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                {
                    VerticeInWorldPosition.Add(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                }
            }
            //Point in Child Object
            for (int ChildsinEnv = 0; ChildsinEnv < EnvironmentsList[num_env].transform.childCount; ChildsinEnv++)
            {
                GameObject Child = EnvironmentsList[num_env].transform.GetChild(ChildsinEnv).gameObject;                

                if (Child.GetComponent<MeshFilter>() != null)
                {
                    Vector3[] tmpVertices;
                    tmpVertices = Child.GetComponent<MeshFilter>().mesh.vertices;
                    for (int num_vertex = 0; num_vertex < tmpVertices.Length; num_vertex++)
                    {
                        VerticeInWorldPosition.Add(EnvironmentsList[num_env].transform.TransformPoint(tmpVertices[num_vertex]));
                    }
                }

            }
        }
        //Done for finding all the Point to use

        //Detect the Collid for Valid Map
        RaycastHit hit;
        for (int num_vertex = 0; num_vertex < VerticeInWorldPosition.Count; num_vertex++)
        {
            if (VerticeInWorldPosition[num_vertex].y > DiscardAbove || VerticeInWorldPosition[num_vertex].y < DiscardBelow)
            {
                continue;
            }

            if (Physics.Raycast(VerticeInWorldPosition[num_vertex], -1 * transform.up, out hit, Mathf.Infinity, layerMaskforValidMap))
            {
                if(hit.collider.gameObject.GetComponent<ValidMap>()== null)
                {
                    Debug.LogError(hit.collider.name +" Something Worng");
                    continue;
                }
                hit.collider.gameObject.GetComponent<ValidMap>().AutoSetAsInValid();
                hit.collider.gameObject.GetComponent<ValidMap>().SetMinHeight(VerticeInWorldPosition[num_vertex].y);
            }
        }
        //ValidMap Finidsh
        IsProjected = true;

    }

    float PreviousAbove = 0;
    public void AutoAdjustClippinginValidMap()
    {
        if (PreviousAbove != DynamicallyClipAbove)
        {
            for (int ix = 0; ix < NumOfValidMap[0]; ix++)
            {
                for (int iy = 0; iy < NumOfValidMap[1]; iy++)
                {
                    ValidMap[ix, iy].GetComponent<ValidMap>().AutoAdjust(DynamicallyClipAbove);
                }
            }
            PreviousAbove = DynamicallyClipAbove;
        }
    }

    public void RotateEnvironments(float InputRotation)
    {
        EnvironmentHolder.transform.rotation = Quaternion.Euler(0f, InputRotation, 0f);
        RotationFinishCounter = 0;
        CountDownUI.SetActive(true);
    }

    public GameObject WaitingUI;
    public GameObject CountDownUI;
    public int RotateDwellSecond = 300;
    int RotationFinishCounter = 10000; //10000 mean infinity
    public void IsRotationFinishedAndAdjustment()
    {
        CountDownUI.transform.GetChild(0).GetComponent <TMPro.TextMeshProUGUI> ().text = "After " + (RotateDwellSecond-RotationFinishCounter) / 60 + "  second,\nwe will do process";

        if (RotationFinishCounter == RotateDwellSecond - 10)
        {
            CountDownUI.SetActive(false);
            WaitingUI.SetActive(true);
        }
        if (RotationFinishCounter == RotateDwellSecond) // After finishing rotate 5s, it will auto adjust the validmap 
        {
            Destroy(ValidMapHolder);
            IsProjected = false;
            EnvironmentHolder.transform.position = new Vector3(0,0,0);
            FindBoundingBoxAndCenterize();
            CreateVaildMap();
        }
        if (RotationFinishCounter == RotateDwellSecond+10)
        {

            ProjectToValidMap();
            WaitingUI.SetActive(false);
        }
        RotationFinishCounter++;
    }

}
