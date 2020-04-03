using System.Collections.Generic;
using UnityEngine;

public class Visualizer3D : MonoBehaviour, IVisualizer
{
    public GameObject objectPrefab;
    
    [Header("References")]
    public Transform objectParent;

    string teamName;
    int teamSize;
    bool rightTeam;
    
    Dictionary<string, VisualRcObject> visualObjects = new Dictionary<string, VisualRcObject>();
    
    Dictionary<string, ReferenceFlag> referenceFlags = new Dictionary<string, ReferenceFlag>();

    Vector2 playerPosition;
    
    bool pos1Found = false;
    bool pos2Found = false;
    bool pos3Found = false;

    Vector2 pos1 = Vector2.zero;
    Vector2 pos2 = Vector2.zero;
    Vector2 pos3 = Vector2.zero;

    float dist1 = 0f;
    float dist2 = 0f;
    float dist3 = 0f;
        
    Vector2 relPos1 = Vector2.zero;
    Vector2 relPos2 = Vector2.zero;
    
    public void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects)
    {
        this.teamName = teamName;
        this.teamSize = teamSize;
        this.rightTeam = rightTeam;

        foreach (KeyValuePair<string,RcObject> rcObject in rcObjects)
        {
            CreateVisualObject(rcObject.Value.name, objectPrefab);
        }

        foreach (ReferenceFlag referenceFlag in FindObjectsOfType<ReferenceFlag>())
        {
            referenceFlag.flagName = referenceFlag.name;
            
            Vector3 pos3d = referenceFlag.transform.localPosition;
            referenceFlag.flagPos = new Vector2(pos3d.x, pos3d.z);
            
            referenceFlags.Add(referenceFlag.name, referenceFlag);
        }
    }
    
    void CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        Transform t = obj.transform;
        t.SetParent(objectParent);
        
        VisualRcObject visualObject = new VisualRcObject(t); 
        
        visualObjects.Add(objectName, visualObject);
    }

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        CreateVisualObject(eName, objectPrefab);
    }

    public void ResetVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            rcObject.Value.visible = false;
        }
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].visible = true;
            visualObjects[objectName].relativePos = relativePos;
        }
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        //TODO: do do
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.visible && !rcObject.Key.StartsWith("f"))
            {
                rcObject.Value.t.gameObject.SetActive(true);
                rcObject.Value.t.localPosition = new Vector3(rcObject.Value.relativePos.x, 0f, rcObject.Value.relativePos.y);
            }
            else
            {
                rcObject.Value.t.gameObject.SetActive(false);
            }
        }
        
        TrilateratePlayerPosition();
    }

    void TrilateratePlayerPosition()
    {
        pos1Found = false;
        pos2Found = false;
        pos3Found = false;
        
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.visible)
            {
                if (referenceFlags.ContainsKey(rcObject.Key) && referenceFlags[rcObject.Key].gameObject.activeInHierarchy)
                {
                    playerPosition = -rcObject.Value.relativePos + referenceFlags[rcObject.Key].flagPos;

                    if (!pos1Found)
                    {
                        pos1Found = true;
                        pos1 = referenceFlags[rcObject.Key].flagPos;
                        dist1 = rcObject.Value.relativePos.magnitude;
                        relPos1 = rcObject.Value.relativePos;
                    }
                    else if (!pos2Found)
                    {
                        pos2Found = true;
                        pos2 = referenceFlags[rcObject.Key].flagPos;
                        dist2 = rcObject.Value.relativePos.magnitude;
                        relPos2 = rcObject.Value.relativePos;
                    }
                    else if (!pos3Found)
                    {
                        pos3Found = true;
                        pos3 = referenceFlags[rcObject.Key].flagPos;
                        dist3 = rcObject.Value.relativePos.magnitude;
                        break;
                    }
                }
            }
        }

        if (pos1Found && pos2Found)
        {
            float trueAngle = Vector2.SignedAngle(Vector2.up, pos1 - pos2);
            float relativeAngle = Vector2.SignedAngle(Vector2.up, relPos1 - relPos2);
            
            float angle = relativeAngle - trueAngle;
            objectParent.localRotation = Quaternion.Euler(0, angle, 0);

            if (pos3Found)
            {
                objectParent.localPosition = Trilaterate(pos1, dist1, pos2, dist2, pos3, dist3);
            }
            else
                objectParent.localPosition = Vector3.zero;
        }
        else
            objectParent.localPosition = Vector3.zero;
    }
    
    Vector3 Trilaterate(Vector2 p1, float d1, Vector2 p2, float d2, Vector2 p3, float d3)
    {
        float i1 = p1.x;
        float i2 = p2.x;
        float i3 = p3.x;

        float j1 = p1.y;
        float j2 = p2.y;
        float j3 = p3.y;

        float x,y;
        
        x = (((2*j3-2*j2)*((d1*d1-d2*d2)+(i2*i2-i1*i1)+(j2*j2-j1*j1)) - (2*j2-2*j1)*((d2*d2-d3*d3)+(i3*i3-i2*i2)+(j3*j3-j2*j2)))/
                 ((2*i2-2*i3)*(2*j2-2*j1)-(2*i1-2*i2)*(2*j3-2*j2)));
        y = ((d1*d1-d2*d2)+(i2*i2-i1*i1)+(j2*j2-j1*j1)+x*(2*i1-2*i2))/(2*j2-2*j1);

        return new Vector3(x, 0,y);
    }

    void OnDrawGizmos()
    {
        if (pos2Found)
        {
            Vector3 p1 = new Vector3(pos1.x, 0, pos1.y);
            Vector3 p2 = new Vector3(pos2.x, 0, pos2.y);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(p1, 1f);
            Gizmos.DrawLine(p1, p2);
        }
    }
}

class VisualRcObject
{
    public Transform t;
    public bool visible;
    public Vector2 relativePos;
    public float relativeBodyFacingDir;

    public VisualRcObject(Transform t)
    {
        this.t = t;
        visible = false;
        relativePos = Vector2.zero;
        relativeBodyFacingDir = 0f;
    }
}