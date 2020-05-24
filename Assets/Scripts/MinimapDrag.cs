using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapDrag : MonoBehaviour
{
    Camera viewCamera;
    Camera minimapCamera;
    GameObject drag;

    BoxCollider2D box;
    Vector3 mapSize;

    Vector3 ViewPointToMapPoint(Vector3 viewPoint)
    {
        Vector3 worldViewPoint = viewCamera.ViewportToWorldPoint(viewPoint);
        Vector3 mapViewPoint = minimapCamera.WorldToScreenPoint(worldViewPoint);
        return mapViewPoint;
    }

    // Start is called before the first frame update
    void Start()
    {
        viewCamera = GameObject.Find("/ViewCamera").GetComponent<Camera>();
        minimapCamera = GameObject.Find("/MinimapCamera").GetComponent<Camera>();
        drag = GameObject.Find("MinimapDrag");
        box = GetComponent<BoxCollider2D>();

        Debug.Log(viewCamera.transform.position);
        mapSize = new Vector3(minimapCamera.targetTexture.width,
            minimapCamera.targetTexture.height, 0);
        

        // the following code calibrate the size of minimap-drag
        Vector3 LDViewPoint = new Vector3(0, 0, 0);
        Vector3 RUViewPoint = new Vector3(1, 1, 0);
        LDViewPoint = ViewPointToMapPoint(LDViewPoint);
        RUViewPoint = ViewPointToMapPoint(RUViewPoint);
        Vector3 mapViewScale = RUViewPoint - LDViewPoint;

        Debug.Log("map view scale:"+mapViewScale);

        Rect drect = drag.GetComponent<RectTransform>().rect;
        Debug.Log(drect.width + "," + drect.height);
        float scaleX = mapViewScale.x / drect.width;
        float scaleY = mapViewScale.y / drect.height;
        
        Vector3 currScale = drag.transform.localScale;
        currScale.x = scaleX * currScale.x;
        currScale.y = scaleY * currScale.y;
        drag.transform.localScale = currScale;

        Debug.Log("box:"+box.size);


    }

    private void OnMouseDown()
    {
        Vector3 clickPos = Input.mousePosition;
        Debug.Log("clicked:" + clickPos);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        clickPos.z = screenPosition.z;
        Vector3 worldTarget = Camera.main.ScreenToWorldPoint(clickPos);
        Debug.Log("world target:" + worldTarget);
        Vector3 mapTarget = transform.InverseTransformPoint(worldTarget);
        Debug.Log("map target:" + mapTarget);

        // Vector3 vec = drag.transform.position;
        // Vector3 dragMapPos = transform.InverseTransformPoint(dragPos);

        drag.transform.position = worldTarget;
        // Debug.Log("pos:" + vec);
        /*
        vec.x = target.x;
        vec.y = target.y;
        drag.transform.position = vec;
        */
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.InverseTransformPoint(drag.transform.position);
        // Debug.Log(pos);
        pos.x += box.size.x / 2;
        pos.y += box.size.y / 2;
        Vector3 mapPos = pos;
        // Debug.Log("map:" + mapPos);
        // Debug.Log("mapsize:" + mapSize);
        // Vector3 screenPos = Vector3.Scale(mapPos, mapSize);
        Vector3 screenPos = mapPos;
        // Debug.Log("screen:" + screenPos);
        Vector3 worldPos = minimapCamera.ScreenToWorldPoint(screenPos);
        // Debug.Log("world:" + worldPos);

        Vector3 camPos = viewCamera.transform.position;
        camPos.x = worldPos.x;
        camPos.y = worldPos.y;
        viewCamera.transform.position = camPos;
    }
}
