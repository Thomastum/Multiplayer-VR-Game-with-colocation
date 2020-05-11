using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public List<GameObject> characters;
    public float bodyHeight = 1.15f;
    public GameObject head;
    public GameObject body;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject character in characters)
        {
            VRCharacterBodies bodyParts= character.GetComponent<VRCharacterBodies>();
            //head part
            foreach(Transform model in bodyParts.head)
                Destroy(model.gameObject);
            GameObject headInstance = Instantiate(head,bodyParts.head);
            headInstance.transform.localPosition = Vector3.zero;
            //body part
            foreach(Transform model in bodyParts.body)
                Destroy(model.gameObject);
            GameObject bodyInstance = Instantiate(body,bodyParts.body);
            bodyInstance.transform.localPosition = new Vector3(0,bodyHeight,0);

            character.transform.localScale = head.transform.parent.localScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
